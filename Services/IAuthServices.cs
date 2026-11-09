using Raceday.API.DTOs;

namespace RacedayAPI.Services
{
    public interface IAuthService
    {
        Task<UserResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<UserResponseDto?> LoginAsync(LoginDto loginDto);
        Task<UserResponseDto?> GetUserByIdAsync(int userId);
    }
}