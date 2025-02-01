using KnowledgeFlowApi.Attributes;
using KnowledgeFlowApi.Data;
using KnowledgeFlowApi.DTOs;
using KnowledgeFlowApi.Entities;
using KnowledgeFlowApi.Models;
using KnowledgeFlowApi.Services.FileItemServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeFlowApi.Controllers.FileItems
{
    [ApiController]
    [Route("api/[controller]")]
    [BannedUser]
    
    public class FileItemController : ControllerBase
    {
        private readonly FileService _fileService;
        private readonly ILogger<FileItemController> _logger;
        private readonly long maxAllowedImageSizeInMB = 5 * 1024 * 1024;
        private readonly long maxAllowedFileSizeInMB = 100 * 1024 * 1024;

        public FileItemController(FileService fileService, ILogger<FileItemController> logger) {
            _fileService = fileService;
            _logger = logger;
        }

        [HttpPost]
        [Route("create")]
        [Authorize(Roles = Role.User)]
        public async Task<IActionResult> CreateFile([FromForm] FileUploadDto model) {
            if (model == null)
                return BadRequest("null or empty request");

            if (model.CoverImage?.Length > maxAllowedImageSizeInMB) {
                return BadRequest("max allowed image size is 5 MB");
            }
            if (model.File?.Length > maxAllowedFileSizeInMB) {
                return BadRequest("max allowed file size is 1 GB");
            }

            try {
                var response = await _fileService.UploadFileItemAsync(model);
                if (response.IsValid)
                    return Ok("new file created successfully");
                return BadRequest(response.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("download/{fileId}")]
        [Authorize(Roles = Role.User)]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DownloadFile(int fileId) {
            try
            {
                var fileStreamResult = await _fileService.DownloadFile(fileId);
                if (fileStreamResult == null)
                    return NotFound();
                return Ok(fileStreamResult);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new FileResponseDto 
                { 
                    IsValid = false, 
                    ErrorMessage = ex.Message 
                });
            }
        }

        [HttpGet("download/image/{fileId}")]
        [Authorize(Roles = Role.User)]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DownloadImage(int fileId) {
            try
            {
                var fileStreamResult = await _fileService.DownloadFile(fileId, true);
                if (fileStreamResult == null)
                    return NotFound();
                return Ok(fileStreamResult);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new FileResponseDto 
                { 
                    IsValid = false, 
                    ErrorMessage = ex.Message 
                });
            }
        }

        [HttpDelete]
        [Route("delete/{id}")]
        [Authorize(Roles = Role.User)]
        public async Task<IActionResult> DeleteFile(int id)
        {
            try
            {
                var response = await _fileService.DeleteFileAsync(id);
                if (response)
                    return Ok("File removed successfully");
                return BadRequest("File not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("get/{id}")]
        public async Task<IActionResult> GetFile(int id) {
            var response = await _fileService.GetFileItemAsyncByFileId(id);
            if (response.IsValid)
                return Ok(response);
            return NotFound("file not found");
        }

        [HttpGet]
        [Route("get-all-for-one-user/{userId}")]
        public async Task<IActionResult> GetAllFilesByUserId(int userId) {
            var response = await _fileService.GetAllFileItemsAsyncByUserId(userId);
            if (response != null)
                return Ok(response);
            return NotFound("empty, no files uploaded yet");
        }

        [HttpGet]
        [Route("get-all/")]
        public async Task<IActionResult> GetAllFiles() {
            var response = await _fileService.GetAllFileItemsAsync();
            if (response != null)
                return Ok(response);
            return NotFound("empty, no files uploaded yet");
        }

        [HttpGet("search-by-name/{word}")]
        public async Task<IActionResult> SearchFilesByName(string word) {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");
            var result = await _fileService.SearchFilesByNameAsync(word);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet("order-by-time")]  // recent to last
        public async Task<IActionResult> OrderByTime() {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");
            var result = _fileService.OrderByTimeAsync();
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet("order-by-rating")]  // higher to lower
        public async Task<IActionResult> OrderByRating() {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Credentials");
            var result = _fileService.OrderByRatingAsync();
            if (result == null)
                return NotFound();
            return Ok(result);
        }

    }
}