using KairosWebAPI.Models.Dto;

namespace KairosWebAPI.Services.KairosService
{
    public interface IKairosService
    {
        Task<bool> ValidateUser(LoginDto loginDto);
    }
}
