using KnowledgeFlowApi.Data;
using KnowledgeFlowApi.DTOs;
using KnowledgeFlowApi.Entities;
using KnowledgeFlowApi.Enums;
using KnowledgeFlowApi.Handlers;
using KnowledgeFlowApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeFlowApi.Services.UserServices
{
    public class UserProfileService
    {
        private readonly FileHandler _fileHandler;
        private readonly ApplicationDbContext _context;

        public UserProfileService(FileHandler fileHandler, ApplicationDbContext context)
        {
            _fileHandler = fileHandler;
            _context = context;
        }

        // if the user has no profile image
        public async Task<UploadUserProfileImageResponse> UploadUserProfileImageAsync(UploadUserProfileImageDto uploadUserProfileImageDto) {
            if (uploadUserProfileImageDto == null)
                return new UploadUserProfileImageResponse { ProfileImagePath = "not found"};
            
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == uploadUserProfileImageDto.UserId);
            if (user == null)
                return new UploadUserProfileImageResponse { ProfileImagePath = "not found"};
            
            var result = await _fileHandler.SaveFileAsync(uploadUserProfileImageDto.ProfileImage, "images/profiles", FileType.Image, $"{uploadUserProfileImageDto.UserId}");
            if (!result.Success)
                return new UploadUserProfileImageResponse { ProfileImagePath = "couldn't save the profile image"};
            
            var profileImage = new UserProfileImage
            {
                Name = $"{uploadUserProfileImageDto.UserId}",
                Path = result.FilePath,
                Size = result.FileSize,
                UploadedOn = DateTime.UtcNow,
                UserId = uploadUserProfileImageDto.UserId
            };
            _context.Add(profileImage);
            await _context.SaveChangesAsync();

            return new UploadUserProfileImageResponse { IsSuccedded = true, ProfileImagePath = result.FilePath };
        }

        // update all user info without updating password
        public async Task<bool> UpdateProfileAsync(UpdateUserProfileDto updateUserProfileDto)
        {
            var user = await _context.Users.Include(u => u.UserProfileImage).FirstOrDefaultAsync(x => x.Id == updateUserProfileDto.Id);
            if (user == null)
                return false;

            user.Username = updateUserProfileDto.Username ?? user.Username;
            user.Bio = updateUserProfileDto.Bio ?? user.Bio ?? "";
            user.ContactEmail = updateUserProfileDto.ContactEmail ?? user.ContactEmail ?? "";
            
            FileProcessResult fileProcessResult = new();
            if (updateUserProfileDto.oldImagePath != null && updateUserProfileDto.newProfileImage != null)
                fileProcessResult = await _fileHandler.ReplaceFileAsync(updateUserProfileDto.newProfileImage, updateUserProfileDto.oldImagePath, "images/profiles", FileType.Image);
            
            if (fileProcessResult.Success) {
                
                using (var transaction = await _context.Database.BeginTransactionAsync()) {
                    try {
                        user.UserProfileImage.Path = fileProcessResult.FilePath;
                        user.UserProfileImage.Size = fileProcessResult.FileSize;
                        user.UserProfileImage.UploadedOn = DateTime.UtcNow;
                        _context.Update(user.UserProfileImage);
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return true;
                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        Console.WriteLine($"{ex.Message} --- {ex.InnerException} --- {ex.Source}");
                    }
                }
            }
            return true;
        }

        public async Task<GetUserProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users.Include(u => u.UserProfileImage).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return null;

            return new GetUserProfileDto
            {
                Username = user.Username,
                Bio = user.Bio,
                ContactEmail = user.ContactEmail,
                ProfileImagePath = user.UserProfileImage.Path,
            };
        }

        public async Task<bool> DeleteUserAccountById(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return false;

            _context.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<GetUserProfileDto>> SearchUsersByNameAsync(string word) {
            var users = _context.Users.Where(u => u.Username.Contains(word)).ToList();
            return users == null ? null : users.Select(MapToUserProfileDto).ToList();
        }
        public async Task<IEnumerable<GetUserProfileDto>> OrderByRatingAsync() {
            var users = _context.Users.OrderByDescending(u => u.TotalRating);
            return users == null ? null : users.Select(MapToUserProfileDto).ToList();
        }

        private GetUserProfileDto MapToUserProfileDto(User user) {
            return new GetUserProfileDto
            {
                Username = user.Username,
                Bio = user?.Bio ?? "",
                ContactEmail = user?.ContactEmail ?? "",
                ProfileImagePath = user.UserProfileImage?.Path ?? "",
            };
        }
    }
}