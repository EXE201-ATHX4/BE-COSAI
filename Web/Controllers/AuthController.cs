using Contract.Repositories.Entity;
using Contract.Services.Interface;
using Core.Utils;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ModelViews.AuthModelViews;
using Newtonsoft.Json.Linq;
using Services.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly TokenService _tokenService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICartService _cartService;
        public AuthController(IUserService userService, TokenService tokenService, ILogger<AuthController> logger, UserManager<User> userManager, IConfiguration configuration, IEmailSender emailSender, ICartService cartService)
        {
            _userManager = userManager;
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger;
            _configuration = configuration;
            _emailSender = emailSender;
            _cartService = cartService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateAccountModel model)
        {
            _logger.LogInformation("Received registration request for email: {Email}", model.Email);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null && existingUser.EmailConfirmed)
            {
                _logger.LogWarning("Email already registered: {Email}", model.Email);
                return BadRequest("Email is already registered.");
            }

            var user = new User
            {
                UserName = model.Email.Split('@')[0],
                Email = model.Email,
                CreatedTime = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedBy = model.Email,
                LastUpdatedBy = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create account: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(result.Errors);
            }

            await _userService.AddRoleToAccountAsync(user.Id, "Customer");

            // Generate email confirmation token
            SendEmail(user);
            return Ok("Registration successful! Please check your email to confirm your account.");
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return BadRequest("Invalid email confirmation request.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
                return Content("Email confirmed successfully! You may now log in.");
            else
                return BadRequest("Email confirmation failed.");
        }

        private async Task SendEmail(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenEncoded = Uri.EscapeDataString(token);
            var confirmUrl = $"{_configuration["App:BaseUrl"]}auth/confirm-email?userId={user.Id}&code={tokenEncoded}";

            var body = $@"<p>Welcome {user.Email},</p><p>Click the button below to confirm your email:</p><a href='{confirmUrl}' style='padding:10px 20px;background:#28a745;color:white;border-radius:5px;text-decoration:none;'>Confirm Email</a>
";
            await _emailSender.SendEmailAsync(user.Email, "Confirm your account", body);
            _logger.LogInformation("Account created. Confirmation email sent to: {Email}", user.Email);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelView model)
        {
            _logger.LogInformation("Received login request for username: {Username}", model.EmailAddress);

            // Authenticate user
            User account = await _userService.AuthenticateAsync(model);
            if (account != null && !account.EmailConfirmed)
            {
                SendEmail(account);
                return BadRequest("Your account is not comfirmed");
            }
            if (account == null)
            {
                _logger.LogWarning("Invalid credentials for username: {Username}", model.EmailAddress);
                return Unauthorized("Invalid credentials");
            }
            var anonymousId = CartHelper.GetCartId(HttpContext);

            // Gộp giỏ hàng
            await _cartService.MergeCartsOnLoginAsync(anonymousId!, account.Id.ToString());

            // Xóa cookie CartId tạm (nếu muốn)
            HttpContext.Response.Cookies.Delete("CartId");
            var token = await _tokenService.GenerateJwtTokenAsync(account);
            var refreshToken = _tokenService.GenerateRefreshToken(account);
            _logger.LogInformation("Login successful for user: {UserName}", account.UserName);

            return Ok(new
            {
                AccessToken = token,
                RefreshToken = refreshToken.Result
            });
        }

        [HttpPost("firebase-login")]
        public async Task<IActionResult> FirebaseLogin([FromBody] string request)
        {
            try
            {
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request);
                string email = decodedToken.Claims["email"].ToString();
                UserRecord us = await FirebaseAuth.DefaultInstance.GetUserAsync(decodedToken.Uid); // get user by firebase to get full name
                string fullName = us.DisplayName ?? "No name";
                Console.WriteLine("NAME:" + fullName);
                string uid = decodedToken.Uid;
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        
                        UserName = email,
                        Email = email,
                        CreatedBy = fullName,
                        LastUpdatedBy = fullName,
                    };
                    var result = await _userManager.CreateAsync(user);
                    await _userService.AddRoleToAccountAsync(user.Id, "Customer");
                    if (!result.Succeeded)
                    {
                        return BadRequest(result.Errors);
                    }
                }

                // Tạo JWT token để gửi về frontend

                var token = await _tokenService.GenerateJwtTokenAsync(user);
                var refreshToken = await _tokenService.GenerateRefreshToken(user);
                var role = await _userManager.GetRolesAsync(user);

                return Ok(new { Message = "Login successful", AccessToken = token, RefreshToken = refreshToken, Email = email, UserId = user.Id, Role = role });
            }
            catch
            {
                return Unauthorized(new { Message = "Invalid token" });
            }
        }
        [Authorize] // Bắt buộc có JWT Token
        [HttpGet("validation")]
        public IActionResult ValidateToken()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null && identity.IsAuthenticated)
            {
                var claims = identity.Claims.Select(c => new { c.Type, c.Value }).ToList();
                return Ok(new
                {
                    IsValid = true,
                    User = claims
                });
            }

            return Unauthorized(new { IsValid = false, Message = "Invalid token" });
        }
        //[Authorize]
        [HttpPost("user-role")]
        public async Task<IActionResult> AddRoleToUser([FromBody] AddRoleModel model)
        {
            _logger.LogInformation("Received request to add role: {RoleName} to user: {UserId}", model.RoleName, model.UserId);

            var result = await _userService.AddRoleToAccountAsync(model.UserId, model.RoleName);
            if (!result)
            {
                _logger.LogWarning("Failed to add role: {RoleName} to user: {UserId}", model.RoleName, model.UserId);
                return BadRequest("Failed to add role to user or user/role does not exist.");
            }

            _logger.LogInformation("Role: {RoleName} added to user: {UserId} successfully", model.RoleName, model.UserId);

            return Ok("Role added to user successfully.");
        }

        [HttpPost("role-claim")]
        public async Task<IActionResult> AddClaimToRole([FromBody] AddClaimToRoleModel model)
        {
            _logger.LogInformation("Received request to add claim to role: {RoleId}", model.RoleId);

            var result = await _userService.AddClaimToRoleAsync(model.RoleId, model.ClaimType, model.ClaimValue, model.CreatedBy);
            if (!result)
            {
                _logger.LogWarning("Failed to add claim to role: {RoleId}", model.RoleId);
                return BadRequest("Failed to add claim to role or role does not exist.");
            }

            _logger.LogInformation("Claim added to role: {RoleId} successfully", model.RoleId);

            return Ok("Claim added to role successfully.");
        }

        [HttpPost("user-claim")]
        public async Task<IActionResult> AddClaimToUser([FromBody] AddClaimModel model)
        {
            _logger.LogInformation("Received request to add claim to user: {UserId}", model.UserId);

            var result = await _userService.AddClaimToUserAsync(model.UserId, model.ClaimType, model.ClaimValue, model.CreatedBy);
            if (!result)
            {
                _logger.LogWarning("Failed to add claim to user: {UserId}", model.UserId);
                return BadRequest("Failed to add claim.");
            }

            _logger.LogInformation("Claim added to user: {UserId} successfully", model.UserId);

            return Ok("Claim added successfully.");
        }

        [HttpGet("user-claims/{userId}")]
        public async Task<IActionResult> GetUserClaims(int userId)
        {
            _logger.LogInformation("Received request to get claims for user: {UserId}", userId);

            var claims = await _userService.GetUserClaimsAsync(userId);

            _logger.LogInformation("Retrieved claims for user: {UserId}", userId);

            return Ok(claims);
        }

        [HttpPut("claims")]
        public async Task<IActionResult> UpdateClaim([FromBody] UpdateClaimModel model)
        {
            _logger.LogInformation("Received request to update claim: {ClaimId}", model.ClaimId);

            var result = await _userService.UpdateClaimAsync(model.ClaimId, model.ClaimType, model.ClaimValue, model.UpdatedBy);
            if (!result)
            {
                _logger.LogWarning("Failed to update claim: {ClaimId}", model.ClaimId);
                return BadRequest("Failed to update claim.");
            }

            _logger.LogInformation("Claim: {ClaimId} updated successfully", model.ClaimId);

            return Ok("Claim updated successfully.");
        }

        [HttpDelete("claims/{claimId}")]
        public async Task<IActionResult> SoftDeleteClaim(int claimId, [FromBody] string deletedBy)
        {
            _logger.LogInformation("Received request to soft delete claim: {ClaimId}", claimId);

            var result = await _userService.SoftDeleteClaimAsync(claimId, deletedBy);
            if (!result)
            {
                _logger.LogWarning("Failed to soft delete claim: {ClaimId}", claimId);
                return BadRequest("Failed to delete claim.");
            }

            _logger.LogInformation("Claim: {ClaimId} soft deleted successfully", claimId);

            return Ok("Claim deleted successfully.");
        }
    }
}
