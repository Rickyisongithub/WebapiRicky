
namespace KairosWebAPI.Services.TokenService
{
    public interface ITokenService
    {
        Task<string> GenerateToken(AppUser user);
        Task<Dictionary<string, string>> DecodeToken(string token);
    }


    public class AppUser
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
    }
}