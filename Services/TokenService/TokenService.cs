using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using KairosWebAPI.Models.ResponseResults.Login;
using Microsoft.IdentityModel.Tokens;
using RestSharp;

namespace KairosWebAPI.Services.TokenService
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly RestClient _restClient;
        private readonly string _apiKey;
        private readonly string _authorizationKey;
        public TokenService(IConfiguration config)
        {
            _configuration = config;
            _restClient = new RestClient(_configuration["kairos:API_Url"] ?? string.Empty);
            _apiKey = _configuration["kairos:API_KEY"] ?? string.Empty;
            _authorizationKey = _configuration["kairos:AUTHORIZATION_KEY"] ?? string.Empty;
        }
        //public async Task<string> GenerateToken(AppUser user)
        //{
        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s: Configuration["JWT:Key"] ?? ""));
        //    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        //    var claims = new[] {
        //        new Claim("userID", user.UserId ?? ""),
        //        new Claim("password", user.Password ?? "")

        //    };

        //    var token = new JwtSecurityToken(
        //        Configuration["JWT:Issuer"],
        //        Configuration["JWT:Issuer"],
        //        claims,
        //        expires: DateTime.Now.AddMinutes(Convert.ToInt32(Configuration["JWT:TokenExpiryTime"])),
        //        signingCredentials: credentials
        //    );

        //    var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        //    return await Task.FromResult(jwtToken);
        //}
        public async Task<string> GenerateToken(AppUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s: _configuration["JWT:Key"] ?? ""));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var kLoginResponseDto = await GetUserDetailsFromEpicor(user);
            if (kLoginResponseDto == null) return "Cannot find the user in Epicor";
            List<Claim> claims = new List<Claim>();
            PropertyInfo[] propertiesUserFile = typeof(UserFile).GetProperties();
            foreach (var rr in propertiesUserFile)
            {
                string name = rr.Name;
                var val = Convert.ToString(rr.GetValue(kLoginResponseDto.returnObj!.UserFile![0]));
                claims.Add(new Claim(name, value: val ?? "null"));
            }
            PropertyInfo[] propertiesUserComp = typeof(UserComp).GetProperties();
            foreach (var rr in propertiesUserComp)
            {
                string name = rr.Name;
                if (name == "CurPlant" || name == "PlantList" || name == "CompanyName")
                {
                    var val = Convert.ToString(rr.GetValue(kLoginResponseDto.returnObj!.UserComp![0]));
                    claims.Add(new Claim(name, value: val ?? "null"));
                }

            }

            claims.Add(new Claim("Password", user.Password ?? ""));

            var token = new JwtSecurityToken(
                _configuration["JWT:Issuer"],
                _configuration["JWT:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JWT:TokenExpiryTime"])),
                signingCredentials: credentials
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return await Task.FromResult(jwtToken);
        }
        public async Task<Dictionary<string, string>> DecodeToken(string token)
        {
            var tokenInfo = new Dictionary<string, string>();

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var claims = jwtSecurityToken.Claims.ToList();

            foreach (var claim in claims)
            {
                tokenInfo.Add(claim.Type, claim.Value);
            }

            return await Task.FromResult(tokenInfo);
        }

        private async Task<KLoginResponseDto?> GetUserDetailsFromEpicor(AppUser appUser)
        {

            var request = new RestRequest($"Ice.BO.UserFileSvc/GetByID?userID={appUser.UserId}")
            {
                Timeout = -1
            };
            request.AddHeader("Authorization", "Basic " + _authorizationKey);
            request.AddHeader("X-API-Key", _apiKey);
            request.AddHeader("Content-Type", "application/json");


            var response = await _restClient.ExecuteGetAsync<KLoginResponseDto>(request);
            return response is { IsSuccessful: true, Data: not null } ? response.Data : null;
        }

    }

}
