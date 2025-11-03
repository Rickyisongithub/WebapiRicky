using KairosWebAPI.Helpers;
using Microsoft.AspNetCore.Mvc;
using KairosWebAPI.Models.Dto;
using KairosWebAPI.Services.KairosService;
using KairosWebAPI.Services.TokenService;
using KairosWebAPI.Services.EpicorAuthService;

namespace KairosWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IKairosService _kairosService;
        private readonly IEpicorAuthService _epicorAuthService;
        private readonly ILogger<AccountController> _logger;  // Add this

        public AccountController(
            ITokenService tokenService,
            IKairosService kairosService,
            IEpicorAuthService epicorAuthService,
            ILogger<AccountController> logger)  // Add logger parameter
        {
            this._tokenService = tokenService;
            this._kairosService = kairosService;
            this._epicorAuthService = epicorAuthService;
            this._logger = logger;  // Initialize logger
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation($"Login attempt from {Request.Headers["User-Agent"]} for user: {loginDto.UserId}");

                if (string.IsNullOrEmpty(loginDto.UserId) || string.IsNullOrEmpty(loginDto.Password))
                {
                    _logger.LogWarning("Login attempt with missing credentials");
                    return BadRequest("Username and/or Password not specified");
                }

                // STEP 1: Try original Kairos validation first
                _logger.LogInformation("Attempting Kairos validation...");
                var isValidKairosUser = await _kairosService.ValidateUser(loginDto);
                _logger.LogInformation($"Kairos validation result: {isValidKairosUser}");

                if (isValidKairosUser)
                {
                    _logger.LogInformation("Successfully authenticated via Kairos");
                    var token = await _tokenService.GenerateToken(new AppUser()
                    {
                        UserId = loginDto.UserId,
                        Password = loginDto.Password
                    });
                    return Ok(ServiceResponse<string>.ReturnResultWith200(token));
                }

                // STEP 2: If Kairos validation failed, try Epicor authentication
                _logger.LogInformation("Kairos failed, attempting Epicor authentication...");
                var epicorResponse = await _epicorAuthService.AuthenticateUser(loginDto.UserId, loginDto.Password);
                _logger.LogInformation($"Epicor authentication result: {epicorResponse?.isAuthenticated}, Message: {epicorResponse?.responseMessage}");

                if (epicorResponse != null && epicorResponse.isAuthenticated)
                {
                    _logger.LogInformation("Successfully authenticated via Epicor");
                    var token = await _tokenService.GenerateToken(new AppUser()
                    {
                        UserId = loginDto.UserId,
                        Password = loginDto.Password
                    });
                    return Ok(ServiceResponse<string>.ReturnResultWith200(token));
                }

                // Both methods failed
                _logger.LogWarning($"Both authentication methods failed for user: {loginDto.UserId}");
                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login error for user: {loginDto.UserId}");
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }
    }
}