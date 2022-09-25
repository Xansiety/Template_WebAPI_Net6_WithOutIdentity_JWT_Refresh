using Template_WebAPI.DTOs;
using Template_WebAPI.Entities.Users;

namespace Template_WebAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<Usuario> GetByRefreshTokenAsync(string refreshToken);
        Task<Usuario> GetByUserNameAsync(string username);
        Task<string> RegisterAsync(RegisterDTO registerDto);
        Task revokeRefreshToken();
        Task<Usuario> ValidateUserAsync(UserCredentials userCredentials);
    }
}
