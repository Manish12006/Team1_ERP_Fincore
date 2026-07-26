using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fincore.Application;
using Fincore.Application.DTO;
using Fincore.Application.Interfaces;
using Fincore.Domain.Models;
using Fincore.Infrastructure.Data;
using Fincore.Infrastructure.Seed.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto.Generators;

namespace Fincore.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext db;
        private readonly IJwtTokenHelper jwtHelper;
        private readonly ITwoFactorHelper twoFactorHelper;
        private readonly JwtSettings jwtSettings;

        public AuthService(AppDbContext db, IJwtTokenHelper jwtHelper,
            ITwoFactorHelper twoFactorHelper, IOptions<JwtSettings> options)
        {
            this.db = db;
            this.jwtHelper = jwtHelper;
            this.twoFactorHelper = twoFactorHelper;
            this.jwtSettings = options.Value;
        }

        public async Task<string> DisableTwoFactor(int userId)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user == null)
                throw new Exception("User not found.");

            user.Is2FAEnabled = false;
            user.TwoFactorSecretKey = null;
            await db.SaveChangesAsync();

            return "2FA disabled successfully.";
        }

        private async Task<LoginResponseDTO> GenerateLoginResponse(User user)
        {
            var accessToken = jwtHelper.GenerateAccessToken(user);
            var refreshToken = jwtHelper.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.LastLogin = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return new LoginResponseDTO
            {
                Requires2FA = false,
                UserId = user.UserId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                FullName = user.FullName,
                RoleName = user.Role.RoleName
            };
        }


        public async Task<LoginResponseDTO> Login(LoginDTO dto)
        {
            var user = await db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null || user.IsActive == 0)
            {
                throw new Exception("Invalid Credentials");
            }
            bool isPasswordValid = VerifyPassword(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
                throw new Exception("Invalid password.");

            if (user.Is2FAEnabled)
            {
                return new LoginResponseDTO
                {
                    Requires2FA = true,
                    UserId = user.UserId
                };
            }

            return await GenerateLoginResponse(user);
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return inputPassword == storedHash;
            }
        }

        public async Task<LoginResponseDTO> VerifyOtpAndLogin(Verify2FADTO dto)
        {
            var user = await db.Users.Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == dto.UserId);

            if (user == null || !user.Is2FAEnabled || string.IsNullOrEmpty(user.TwoFactorSecretKey))
                throw new Exception("2FA not configured for this user.");

            bool isValidOtp = twoFactorHelper.VerifyOtp(user.TwoFactorSecretKey, dto.OtpCode);
            if (!isValidOtp)
                throw new Exception("Invalid or expired OTP.");

            return await GenerateLoginResponse(user);
        }



        public async Task<LoginResponseDTO> RefreshToken(RefreshTokenRequestDTO dto)
        {
            var user = await db.Users.Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == dto.UserId);

            if (user == null || string.IsNullOrEmpty(user.RefreshToken) || user.RefreshToken != dto.RefreshToken)
                throw new Exception("Invalid refresh token.");

            return await GenerateLoginResponse(user);
        }


        public async Task<Enable2FAResponseDTO> GenerateTwoFactorSetup(int userId)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user == null)
                throw new Exception("User not found.");

            var secretKey = twoFactorHelper.GenerateSecretKey();
            var qrUri = twoFactorHelper.GenerateQrCodeUri(user.Email, secretKey);

            user.TwoFactorSecretKey = secretKey;
            await db.SaveChangesAsync();

            return new Enable2FAResponseDTO
            {
                SecretKey = secretKey,
                QrCodeUri = qrUri
            };
        }


        public async Task<string> ConfirmTwoFactorSetup(Confirm2FARequestDTO dto)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.UserId == dto.UserId);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecretKey))
                throw new Exception("2FA setup not initiated for this user.");

            bool isValidOtp = twoFactorHelper.VerifyOtp(user.TwoFactorSecretKey, dto.OtpCode);
            if (!isValidOtp)
                throw new Exception("Invalid OTP. 2FA setup failed.");

            user.Is2FAEnabled = true;
            await db.SaveChangesAsync();

            return "2FA enabled successfully.";
        }

        public async Task<RegisterResponseDTO> Register(RegisterDTO dto)
        {
            var existing = await db.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (existing != null)
            {
                throw new Exception("Email Already Registered!");
            }

            var now = DateTime.UtcNow;

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = dto.RoleId,
                UserCategory = dto.UserCategory,
                Phone = dto.Phone,
                RefreshToken = string.Empty,
                IsActive = 1,
                CreatedAt = now,
                ModifiedAt = now,

                CreatedBy = RoleSeeder.BootstrapUserId,
                ModifiedBy = RoleSeeder.BootstrapUserId
            };
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            user.CreatedBy = user.UserId;
            user.ModifiedBy = user.UserId;
            await db.SaveChangesAsync();

            var role = await db.Roles.FirstOrDefaultAsync(x => x.RoleId == user.RoleId);

            return new RegisterResponseDTO
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                RoleName = role?.RoleName,
                UserCategory = user.UserCategory,
                Is2FAEnabled = user.Is2FAEnabled,
                CreatedAt = user.CreatedAt
            };
        }


    }
}
