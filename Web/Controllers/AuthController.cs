using Contract.Repositories.Entity;
using Contract.Services.Interface;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ModelViews.AuthModelViews;
using Services.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        private readonly ILogger<AuthController> _logger;
        public AuthController(IUserService userService, TokenService tokenService, ILogger<AuthController> logger, UserManager<User> userManager)
        {
            _userManager = userManager;
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger;
        }

        //[HttpPost("login")]
        //[AllowAnonymous]
        //public async Task<IActionResult> GoogleSignIn([FromBody] string request)
        //{
        //    Account acc = new Account();
        //    try
        //    {
        //        var payload = await GoogleJsonWebSignature.ValidateAsync(request);

        //        if (payload == null)
        //            return BadRequest("Invalid Google token");
        //        Console.WriteLine(payload.Email);
        //        var user = await _userService.GetUserByEmail(payload.Email);
        //        //check user don't exist create user
        //        if (user == null)
        //        {

        //            user = new Account
        //            {
        //                UserName = payload.Email.Split("@")[0],
        //                Email = payload.Email,
        //                FullName = payload.Name,
        //                CreatedBy = payload.Name,
        //                CreatedTime = DateTime.UtcNow,
        //                LastUpdatedBy = payload.Name,
        //                LastUpdatedTime = DateTime.UtcNow

        //            };

        //            user = await _userService.CreateAccountAsync(user);
        //            var role = await _userService.AddRoleToAccountAsync(user.Id, "Patient"); 
        //        }
        //        Console.WriteLine("User ID" + user.Id.ToString());
        //        var jwtToken = _tokenService.GenerateJwtTokenAsync(user);
        //        return Ok(new { token = jwtToken, user });
        //    }
        //    catch
        //    {
        //        return BadRequest("Google authentication failed");
        //    }
        //}
        [HttpPost("firebase-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] string request)
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
                        
                        CreatedBy = fullName,
                        LastUpdatedBy = fullName,
                    };
                    var result = await _userManager.CreateAsync(user);
                    await _userService.AddRoleToAccountAsync(user.Id, "Patient");
                    if (!result.Succeeded)
                    {
                        return BadRequest(result.Errors);
                    }
                }

                // Tạo JWT token để gửi về frontend

                var token = await _tokenService.GenerateJwtTokenAsync(user);
                var role = await _userManager.GetRolesAsync(user);

                return Ok(new { Message = "Login successful", Token = token, Email = email, UserId = user.Id, Role = role });
            }
            catch
            {
                return Unauthorized(new { Message = "Invalid token" });
            }
        }
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("hoqee0yaZ7jg1kBiii75VNA4fAuWPTA0A9pVY5W+XKV8IAf+99yvEMjIWLGAWYOU2iFrGL+Ct7FupbTX2LYzXQ=="));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var token = new JwtSecurityToken(
                issuer: "yourdomain.com",
                audience: "yourdomain.com",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
