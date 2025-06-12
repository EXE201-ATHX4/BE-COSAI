using Contract.Repositories.Entity;
using Contract.Repositories.Interface;
using Core.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.Service
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public TokenService(IConfiguration configuration, UserManager<User> userManager, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            // Các claims của token
            var claims = new List<Claim>
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim("userId", user.Id.ToString()),

            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            // Tạo khóa bí mật để ký token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Tạo token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Thêm token vào bảng ApplicationUserTokens bằng cách sử dụng UnitOfWork
            Console.WriteLine("USER ID:" + user.Id);
            await AddTokenToDatabaseAsync(user.Id, tokenString);

            return tokenString;
        }
        public async Task<string> GenerateRefreshToken(User user)
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var refreshToken = Convert.ToBase64String(randomBytes);
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            var tokenRepo = _unitOfWork.GetRepository<ApplicationUserTokens>();
            var existing = await tokenRepo.Entities
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (existing != null)
            {
                existing.RefreshToken = refreshToken;
                existing.RefreshTokenExpiryTime = refreshTokenExpiry;
                existing.LastUpdatedBy = user.UserName;
                existing.LastUpdatedTime = DateTime.UtcNow;
                await tokenRepo.UpdateAsync(existing);
            }
            else
            {
                await tokenRepo.InsertAsync(new ApplicationUserTokens
                {
                    UserId = user.Id,
                    LoginProvider = "CustomLoginProvider",
                    Name = "RefreshToken",
                    RefreshToken = refreshToken,
                    RefreshTokenExpiryTime = refreshTokenExpiry,
                    CreatedBy = user.UserName,
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedBy = user.UserName,
                    LastUpdatedTime = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveAsync();
            return refreshToken;
        }

        private async Task AddTokenToDatabaseAsync(int userId, string tokenString)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var refreshToken = Guid.NewGuid().ToString(); // Use a secure random generator in production
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            Console.WriteLine("UserID token" + user.Id);
            if (user != null)
            {
                var tokenRepository = _unitOfWork.GetRepository<ApplicationUserTokens>();

                try
                {
                    // Bắt đầu giao dịch
                    //_unitOfWork.BeginTransaction();

                    var existingToken = await tokenRepository.Entities
                        .FirstOrDefaultAsync(t => t.UserId.Equals(userId) && t.LoginProvider == "CustomLoginProvider");

                    if (existingToken != null)
                    {
                        // Cập nhật token hiện tại
                        existingToken.Value = tokenString;
                        existingToken.RefreshToken = refreshToken;
                        existingToken.RefreshTokenExpiryTime = refreshTokenExpiry;
                        existingToken.LastUpdatedBy = user.UserName;
                        existingToken.LastUpdatedTime = CoreHelper.SystemTimeNow;

                        await tokenRepository.UpdateAsync(existingToken);
                    }
                    else
                    {
                        // Thêm token mới
                        var userToken = new ApplicationUserTokens
                        {
                            UserId = userId,
                            LoginProvider = "CustomLoginProvider",
                            Name = "JWT",
                            Value = tokenString,
                            RefreshToken = refreshToken,
                            RefreshTokenExpiryTime = refreshTokenExpiry,
                            CreatedBy = user.UserName,
                            CreatedTime = CoreHelper.SystemTimeNow,
                            LastUpdatedBy = user.UserName,
                            LastUpdatedTime = CoreHelper.SystemTimeNow
                        };

                        await tokenRepository.InsertAsync(userToken);
                    }

                    // Lưu và commit giao dịch
                    await _unitOfWork.SaveAsync();
                    //_unitOfWork.CommitTransaction();
                }
                catch (Exception)
                {
                    //// Rollback giao dịch nếu có lỗi xảy ra
                    //_unitOfWork.RollBack();
                    throw;
                }
            }
        }
    }
}
