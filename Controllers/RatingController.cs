using KnowledgeFlowApi.DTOs;
using KnowledgeFlowApi.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeFlowApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingController : ControllerBase
    {
        private readonly RatingService _ratingService;

        public RatingController(RatingService ratingService) {
            _ratingService = ratingService;
        }

        [HttpPost("rate-user")]
        [Authorize]
        public async Task<IActionResult> RateUser([FromBody] CreateRateDto request) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");

            var result = await _ratingService.RateUserAsync(request);
            if (result == null || !result.IsFound)
                return BadRequest(result?.Message);
            return Ok(result);
        }

        [HttpPost("rate-file")]
        [Authorize]
        public async Task<IActionResult> RateFile([FromBody] CreateRateDto request) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");

            var result = await _ratingService.RateFileAsync(request);
            if (result == null || !result.IsFound)
                return BadRequest(result?.Message);
            return Ok(result);
        }


        [HttpDelete("delete-user-rate")]
        [Authorize]
        public async Task<IActionResult> DeleteUserRate([FromBody] DeleteRateDto request) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");

            var result = await _ratingService.DeleteUserRateAsync(request);
            if (result == null || !result.IsFound)
                return BadRequest(result?.Message);
            return Ok(result);
        }

        [HttpDelete("delete-file-rate")]
        [Authorize]
        public async Task<IActionResult> DeleteFileRate([FromBody] DeleteRateDto request) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");

            var result = await _ratingService.DeleteFileRateAsync(request);
            if (result == null || !result.IsFound)
                return BadRequest(result?.Message);
            return Ok(result);
        }

        [HttpGet("get-user-rate/{userId}")]
        public async Task<IActionResult> GetUserRate(int userId) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");

            var result = await _ratingService.GetUserRateAsync(userId);
            if (result == null || !result.IsFound)
                return BadRequest(result?.Message);
            return Ok(result);
        }

        [HttpGet("get-file-rate/{fileId}")]
        public async Task<IActionResult> GetFileRate(int fileId) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");

            var result = await _ratingService.GetFileRateAsync(fileId);
            if (result == null || !result.IsFound)
                return BadRequest(result?.Message);
            return Ok(result);
        }

        [HttpGet("get-users-user-rated/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetGivenRatedUsers(int userId) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");

            var result = _ratingService.GetAllUsersTheUserHasRatedAsync(userId);
            if (result == null)
                return NotFound("user or ratings not found");
            return Ok(result);
        }

        [HttpGet("get-files-user-rated/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetGivenRatedFiles(int userId) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");

            var result = _ratingService.GetAllFilesTheUserHasRatedAsync(userId);
            if (result == null)
                return NotFound("user or ratings not found");
            return Ok(result);
        }

        [HttpPut("update-user-rate")]
        [Authorize]
        public async Task<IActionResult> UpdateUserRate([FromBody] CreateRateDto request) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");

            var result = await _ratingService.UpdateUseRateAsync(request);
            if (result == null || !result.IsFound)
                return NotFound(result?.Message);
            return Ok(result);
        }

        [HttpPut("update-file-rate")]
        [Authorize]
        public async Task<IActionResult> UpdateFileRate([FromBody] CreateRateDto request) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");

            var result = await _ratingService.UpdateFileRateAsync(request);
            if (result == null || !result.IsFound)
                return NotFound(result?.Message);
            return Ok(result);
        }
        
    }
}