using KairosWebAPI.Models.Dto;

namespace KairosWebAPI.Services.EpicorAuthService
{
    public interface IEpicorAuthService
    {
        Task<EpicorAuthResponseDto> AuthenticateUser(string empId, string password);
    }
}