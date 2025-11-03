using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Services.KairosService;
using KairosWebAPI.Services.EpicorAuthService;

namespace KairosWebAPI.AuthorizationFilter
{
    public class KAuthorizationFilter : IAuthorizationFilter
    {
        private readonly IKairosService _kairosService;
        private readonly IEpicorAuthService _epicorAuthService;

        public KAuthorizationFilter(IKairosService kairosService, IEpicorAuthService epicorAuthService)
        {
            _kairosService = kairosService;
            _epicorAuthService = epicorAuthService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Retrieve the token from the request headers or query parameters
            string token = context.HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Validate the token and check for specific claims
            token = token.Replace("Bearer ", "");
            if (!IsValidToken(token).Result)
            {
                context.Result = new UnauthorizedResult(); // Return 401 Unauthorized
            }
        }

        private async Task<bool> IsValidToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Retrieve the userId and password claims from the token
            string? userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;
            string? password = jwtToken.Claims.FirstOrDefault(c => c.Type == "Password")?.Value;

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(password))
            {
                // STEP 1: Try original ValidatePassword first
                var isValidKairosUser = await _kairosService.ValidateUser(new LoginDto()
                {
                    UserId = userId,
                    Password = password
                });

                if (isValidKairosUser)
                {
                    // Success with ValidatePassword
                    return true;
                }

                // STEP 2: If ValidatePassword fails, try UserAuthentication as fallback
                try
                {
                    var epicorResponse = await _epicorAuthService.AuthenticateUser(userId, password);
                    if (epicorResponse != null && epicorResponse.isAuthenticated)
                    {
                        // Success with UserAuthentication
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // Log error if needed, but don't throw - just return false
                    Console.WriteLine($"[KAuthorizationFilter] Error during UserAuthentication: {ex.Message}");
                }
            }

            return false;
        }
    }
}