using System.IdentityModel.Tokens.Jwt;

namespace KairosWebAPI.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Retrieve the token from the request headers or query parameters
            string token = context.Request.Headers["Authorization"].ToString();

            // Validate the token and check for specific claims
            if (IsValidToken(token))
            {
                await _next(context); // Token is valid, proceed to the controller
            }
            else
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Unauthorized"); // Return an error message
            }
        }
        private bool IsValidToken(string token)
        {
            // Perform the token validation logic here
            // In a real-world scenario, you would use a JWT library to validate and decode the token

            // Dummy implementation for testing purposes
            // In this example, the token is expected to be a valid JWT
            // Modify this logic according to your token validation requirements
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Retrieve the userId and email claims from the token
            string? userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "userID")?.Value;
            string? email = jwtToken.Claims.FirstOrDefault(c => c.Type == "password")?.Value;

            // Check if the token's claims match the required values
            // Modify this logic according to your validation requirements
            if (userId == "kairosdev1" && email == "K@irosPass99")
            {
                return true;
            }

            return false;
        }

    }
}
