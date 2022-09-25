using Template_WebAPI.DTOs;

namespace Template_WebAPI.Services.Interfaces
{
    public interface ITokenService
    {
        Task<ResponseAuth> GenerateAccessTokenAsync(UserCredentials model);
        Task<ResponseAuth> RefreshTokenAsync(string refreshToken);
    }
}
