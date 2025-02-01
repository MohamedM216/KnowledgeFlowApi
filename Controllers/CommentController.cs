using KnowledgeFlowApi.Attributes;
using KnowledgeFlowApi.DTOs;
using KnowledgeFlowApi.Services.CommentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeFlowApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [BannedUser]
    public class CommentController : ControllerBase
    {
        private readonly CommentService _commentService;

        public CommentController(CommentService commentService) {
            _commentService = commentService;
        }

        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        [Authorize(Roles = Role.User)]
        public async Task<IActionResult> CreateComment([FromBody] AddCommentDto request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _commentService.WriteCommentAsync(request);
            if (!response.IsCreated)
                return BadRequest(response.Message);
            return Ok(response);
        }

        [HttpPost("reply")]
        [Authorize(Roles = Role.Admin)]
        [Authorize(Roles = Role.User)]
        public async Task<IActionResult> CreateReply([FromBody] AddCommentDto request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _commentService.WriteReplyAsync(request);
            if (!response.IsCreated)
                return BadRequest(response.Message);
            return Ok(response);
        }

        [HttpGet("file/{fileId}")]
        public async Task<IActionResult> GetCommentsByFileId(int fileId, [FromQuery] int page = 1, 
                                                        [FromQuery] int pageSize = 10) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comments = await _commentService.GetCommentsByFileIdAsync(fileId, page, pageSize);
            if (!comments.Any())
                return NotFound("No comments found for this file.");
            return Ok(comments);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = Role.Admin)]
        [Authorize(Roles = Role.User)]
        public async Task<IActionResult> GetCommentsByUserId(int userId, [FromQuery] int page = 1,
                                                        [FromQuery] int pageSize = 10)
        {
            var comments = await _commentService.GetCommentsByUserIdAsync(userId, page, pageSize);

            if (!comments.Any())
            {
                return NotFound("No comments found for this user."); 
            }

            return Ok(comments); 
        }


        [HttpGet("replies/{parentId}")]
        public async Task<IActionResult> GetRepliesByParentId(int parentId, [FromQuery] int page = 1,
                                                             [FromQuery] int pageSize = 10)
        {
            var replies = await _commentService.GetRepliesByParentIdAsync(parentId, page, pageSize);

            if (!replies.Any())
            {
                return NotFound("No replies found for this comment."); // Return 404 if no replies are found
            }

            return Ok(replies); // Return the paginated replies
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Role.Admin)]
        [Authorize(Roles = Role.User)]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var result = await _commentService.DeleteByIdAsync(id);

            if (!result)
            {
                return NotFound("Comment not found or could not be deleted."); 
            }
            return Ok(); 
        }

        [HttpPut]
        [Authorize(Roles = Role.Admin)]
        [Authorize(Roles = Role.User)]
        public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentDto updateCommentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            var result = await _commentService.UpdateCommentAsync(updateCommentDto);

            if (!result.IsCreated)
            {
                return NotFound(result.Message);
            }

            return Ok(result); 
        }

    }
}