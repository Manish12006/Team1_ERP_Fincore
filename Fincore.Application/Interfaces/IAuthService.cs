using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fincore.Application.DTO;

namespace Fincore.Application.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponseDTO> Register(RegisterDTO dto);
        Task<LoginResponseDTO> Login(LoginDTO dto);
        Task<LoginResponseDTO> VerifyOtpAndLogin(Verify2FADTO dto);
        Task<LoginResponseDTO> RefreshToken(RefreshTokenRequestDTO dto);

        Task<Enable2FAResponseDTO> GenerateTwoFactorSetup(int userId);
        Task<string> ConfirmTwoFactorSetup(Confirm2FARequestDTO dto);
        Task<string> DisableTwoFactor(int userId);
    }
}
