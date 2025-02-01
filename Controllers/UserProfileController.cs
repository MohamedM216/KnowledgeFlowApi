using KnowledgeFlowApi.Attributes;
using KnowledgeFlowApi.DTOs;
using KnowledgeFlowApi.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace KnowledgeFlowApi.Controllers.User
{
    [ApiController]
    [Route("api/[controller]")]
    [BannedUser]
    public class UserProfileController : ControllerBase
    {
        private readonly UserProfileService _userProfileService;

        public UserProfileController(UserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [HttpPost]
        [Route("uploadProfileImage")]
        [Authorize(Roles = Role.User)]
        public async Task<ActionResult> UploadProfileImage([FromForm]UploadUserProfileImageDto request) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid credentials");
            
            if (request == null)
                return BadRequest("null request object");

            var response = await _userProfileService.UploadUserProfileImageAsync(request);
            if (!response.IsSuccedded)
                return BadRequest("uploading image failed");
            
            return Ok(response.ProfileImagePath);
        }

        [HttpPut]   
        [Route("update")]
        [Authorize(Roles = Role.User)]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileDto updateUserProfileDto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid credentials");

            if (updateUserProfileDto == null)
                return BadRequest("null request object");

            var updatedProfile = await _userProfileService.UpdateProfileAsync(updateUserProfileDto);
            if (updatedProfile == null)
                return BadRequest("User not found");
            return Ok(updatedProfile);
        }

        [HttpGet]
        [Route("get/{userId}")] 
        [Authorize(Roles = Role.User)]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            var userProfile = await _userProfileService.GetUserProfileAsync(userId);
            if (userProfile == null)
                return BadRequest("User not found");
            return Ok(userProfile);
        }

        [HttpDelete]    
        [Route("delete/{userId}")]
        [Authorize(Roles = Role.User)]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteUserAccount(int userId)
        {
            var response = await _userProfileService.DeleteUserAccountById(userId);
            if (!response)
                return BadRequest("User not found");
            return Ok("User account deleted successfully");
        }

        [HttpGet("search-by-name/{word}")]
        public async Task<IActionResult> SearchFilesByName(string word) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");
            var result = await _userProfileService.SearchUsersByNameAsync(word);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet("order-by-rating")]  // higher to lower
        public async Task<IActionResult> OrderByRating() {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");
            var result = await _userProfileService.OrderByRatingAsync();
            if (result == null)
                return NotFound();
            return Ok(result);
        }
        
    }
}