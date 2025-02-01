using System.Security.Claims;
using KnowledgeFlowApi.Models;
using KnowledgeFlowApi.Requests.UserRequests;
using KnowledgeFlowApi.Services.UserServices;
using LibraryManagementSystemAPI.Services.SendEmailServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeFlowApi.Controllers.User
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthUserController : ControllerBase
    {
        private readonly AuthUserService _authUserService;
        private readonly SendEmailService _sendEmailService;

        public AuthUserController(AuthUserService authUserService, SendEmailService sendEmailService) {
            _authUserService = authUserService;
            _sendEmailService = sendEmailService;
        }


        [HttpPost]
        [Route("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");

            ResponseAuthModel response = await _authUserService.SignUpAsync(request);
            if (response == null || !response.IsAuthenticated)
                return Unauthorized(response?.Message);

            return Ok("Signed up successfully");
        }

        [HttpPost]
        [Route("sendEmail/{userId}")]
        public async Task<ActionResult> SendEmail(int userId) {
            if (!ModelState.IsValid)
                return BadRequest();

            var sendEmailResponse = await _authUserService.GetUsernameAndEmail(userId);
            if (!sendEmailResponse.IsRetreved)
                return  BadRequest(sendEmailResponse.Message);
            
            await _sendEmailService.SendWelcomeEmailAsync(sendEmailResponse.ToEmail, sendEmailResponse.ToUsername);
            return Ok("Email sent successfully");
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> GetTokenAsync([FromBody]AuthRequest request) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");

            var response = await _authUserService.GetTokenAsync(request);
            if (response == null || !response.IsAuthenticated)
                return Unauthorized(response?.Message);

            if (!string.IsNullOrEmpty(response.RefreshToken))
                SetRefreshTokenInCookie(response.RefreshToken, response.RefreshTokenExpiration);

            return Ok(response);
        }

        [HttpPost]
        [Route("refreshToken")]
        [Authorize(Roles = Role.Admin)]
        [Authorize(Roles = Role.User)]
        public async Task<IActionResult> GetRefreshToken([FromBody]string token) {
            var refreshToken = Request.Cookies["userRefreshToken"] ?? token;

            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new JwtAuthModel { Message = "token is required" });

            var response = await _authUserService.RefreshTokenAsync(refreshToken);

            if (response == null || !response.IsAuthenticated)
                return BadRequest(new { Message = response?.Message ?? "Invalid token" });

            SetRefreshTokenInCookie(response.RefreshToken, response.RefreshTokenExpiration);
            return Ok(response);
        }

        [HttpPost]
        [Route("revokeToken")]
        [Authorize(Roles = Role.Admin)]
        [Authorize(Roles = Role.User)]
        public async Task<IActionResult> RevokeToken([FromBody]string token) {
            var refreshToken = token ?? Request.Cookies["userRefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("token is required");

            var result = await _authUserService.RevokeRefreshTokenAsync(token);

            if (!result)
                return BadRequest("token is invalid");
            
            return Ok();
        }

        private void SetRefreshTokenInCookie(string refreshToken, DateTime expiresOn) {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expiresOn.ToLocalTime()
            };
            Response.Cookies.Append("userRefreshToken", refreshToken, cookieOptions);
        }
    }
}