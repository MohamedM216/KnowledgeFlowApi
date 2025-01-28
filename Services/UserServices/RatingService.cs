using System.Numerics;
using KnowledgeFlowApi.Data;
using KnowledgeFlowApi.DTOs;
using KnowledgeFlowApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeFlowApi.Services.UserServices
{
    public class RatingService(ApplicationDbContext context)
    {
        public async Task<GetRateDto> RateUserAsync(CreateRateDto createRateDto) {
            if (!(createRateDto.Value >= 1 && createRateDto.Value <= 5))
                return new GetRateDto { Message = "value must be between 1 and 5" };

            var rating = await context.UserRatings.SingleOrDefaultAsync(f => f.UserId == createRateDto.RaterId && f.RatedUserId == createRateDto.RatedId);
            if (rating != null)
                return new GetRateDto { Message = "user has rated this user before" };

            var rater = await context.Users.SingleOrDefaultAsync(u => u.Id == createRateDto.RaterId);
            var rated = await context.Users.Include(u => u.ReceivedUserRatings).SingleOrDefaultAsync(u => u.Id == createRateDto.RatedId);
            if (rater == null || rated == null)
                return new GetRateDto { Message = "either the rater or the rated are not found" };
            
            using (var transaction = await context.Database.BeginTransactionAsync()) {
                try {
                    var userRating = new UserRating
                    {
                        UserId = createRateDto.RaterId,
                        RatedUserId = createRateDto.RatedId,
                        Value = createRateDto.Value,
                        RatedOn = DateTime.UtcNow
                    };
                    context.Add(userRating);

                    var len = rated.ReceivedUserRatings.ToList().Count;
                    var actualRate = rated.ReceivedUserRatings.Sum(r => r.Value);
                    var total = actualRate / len;
                    rated.TotalRating = total;
                    context.Update(rated);

                    await context.SaveChangesAsync();

                    transaction.Commit();
                }
                catch (Exception ex) {
                    transaction.Rollback();
                    return new GetRateDto { Message = $"problem to rate a user, ERROR: {ex.Message}, SOURCE: {ex.Source} " };
                }
            }
            
            // update total rating for the rated user
            return new GetRateDto {
                IsFound = true,
                Message = "success",
                Value = rated.TotalRating
            };
        }
        public async Task<GetRateDto> RateFileAsync(CreateRateDto createRateDto) {
            if (!(createRateDto.Value >= 1 && createRateDto.Value <= 5))
                return new GetRateDto { Message = "value must be between 1 and 5" };

            var rating = await context.FileRatings.SingleOrDefaultAsync(f => f.UserId == createRateDto.RaterId && f.FileItemId == createRateDto.RatedId);
            if (rating != null)
                return new GetRateDto { Message = "user has rated this file before" };

            var rater = await context.Users.SingleOrDefaultAsync(u => u.Id == createRateDto.RaterId);
            var rated = await context.FileItems.Include(f => f.FileRatings).SingleOrDefaultAsync(u => u.Id == createRateDto.RatedId);
            if (rater == null || rated == null)
                return new GetRateDto { Message = "either the rater or the rated are not found" };
            
            using (var transaction = await context.Database.BeginTransactionAsync()) {
                try {
                    var fileRating = new FileRating
                    {
                        UserId = createRateDto.RaterId,
                        FileItemId = createRateDto.RatedId,
                        Value = createRateDto.Value,
                        Review = createRateDto.Review ?? "",
                        RatedOn = DateTime.UtcNow
                    };
                    context.Add(fileRating);

                    var len = rated.FileRatings.ToList().Count;
                    var actualRate = rated.FileRatings.Sum(r => r.Value);
                    var total = actualRate / len;
                    rated.TotalRating = total;
                    context.Update(rated);

                    await context.SaveChangesAsync();   // 

                    transaction.Commit();
                }
                catch (Exception ex) {
                    transaction.Rollback();
                    return new GetRateDto { Message = $"problem to rate a file, ERROR: {ex.Message}, SOURCE: {ex.Source} " };
                }
            }

            // update total rating for the rated user
            return new GetRateDto {
                IsFound = true,
                Message = "success",
                Value = rated.TotalRating
            };
        }

        public async Task<GetRateDto> GetUserRateAsync(int userId) {
            var user = await context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return new GetRateDto { Message = "user not found" };
            
            return new GetRateDto { Message = "Rate returned Successfully" , IsFound = true, Value = user.TotalRating };
            
        }
        public async Task<GetRateDto> GetFileRateAsync(int fileId) {
            var file = await context.FileItems.SingleOrDefaultAsync(u => u.Id == fileId);
            if (file == null)
                return new GetRateDto { Message = "file not found" };
            
            return new GetRateDto { Message = "Rate returned Successfully" , IsFound = true, Value = file.TotalRating };
        }

        public async Task<GetRateDto> DeleteUserRateAsync(DeleteRateDto deleteRateDto) {
            var rating = await context.UserRatings.SingleOrDefaultAsync(f => f.UserId == deleteRateDto.RaterId && f.RatedUserId == deleteRateDto.RatedId);
            if (rating == null)
                return new GetRateDto { Message = "user hasn't rated this user before" };

            var rated = await context.Users.Include(u => u.ReceivedUserRatings).SingleOrDefaultAsync(u => u.Id == deleteRateDto.RatedId);

            using (var transaction = await context.Database.BeginTransactionAsync()) {
                try {
                    context.Remove(rating);
                    context.SaveChanges();

                    var len = rated.ReceivedUserRatings.ToList().Count;
                    var actualRate = rated.ReceivedUserRatings.Sum(r => r.Value);
                    decimal total = 0;
                    if (len > 0)
                        total = actualRate / len;
                    rated.TotalRating = total;
                    context.Update(rated);

                    await context.SaveChangesAsync();

                    transaction.Commit();
                }
                catch (Exception ex) {
                    transaction.Rollback();
                    return new GetRateDto { Message = $"problem to delete a user rating, ERROR: {ex.Message}, SOURCE: {ex.Source} " };
                }
            }
            
            // update total rating for the rated user
            return new GetRateDto {
                IsFound = true,
                Message = "success",
                Value = rated.TotalRating
            };   
        }

        public async Task<GetRateDto> DeleteFileRateAsync(DeleteRateDto deleteRateDto) {
            var rating = await context.FileRatings.SingleOrDefaultAsync(f => f.UserId == deleteRateDto.RaterId && f.FileItemId == deleteRateDto.RatedId);
            if (rating == null)
                return new GetRateDto { Message = "user hasn't rated this file before" };

            var rated = await context.FileItems.Include(f => f.FileRatings).SingleOrDefaultAsync(u => u.Id == deleteRateDto.RatedId);

            using (var transaction = await context.Database.BeginTransactionAsync()) {
                try {
                    context.Remove(rating);
                    context.SaveChanges();

                    var len = rated.FileRatings.ToList().Count;
                    var actualRate = rated.FileRatings.Sum(r => r.Value);
                    decimal total = 0;
                    if (len > 0)
                        total = actualRate / len;
                    rated.TotalRating = total;
                    context.Update(rated);

                    await context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex) {
                    transaction.Rollback();
                    return new GetRateDto { Message = $"problem to delete a file rating, ERROR: {ex.Message}, SOURCE: {ex.Source} " };
                }
            }

            // update total rating for the rated user
            return new GetRateDto {
                IsFound = true,
                Message = "success",
                Value = rated.TotalRating
            };
        }

        public async Task<IEnumerable<UserRating>> GetAllUsersTheUserHasRatedAsync(int userId) {
            var ratings = context.UserRatings.Where(r => r.UserId == userId);
            if (ratings == null)
                return null;
            return ratings.ToList();
        }

        public async Task<IEnumerable<FileRating>> GetAllFilesTheUserHasRatedAsync(int userId) {
            var ratings = context.FileRatings.Where(f => f.UserId == userId);
            if (ratings == null)
                return null;
            return ratings.ToList();
        }

        public async Task<GetRateDto> UpdateUseRateAsync(CreateRateDto createRateDto) {
            if (!(createRateDto.Value >= 1 && createRateDto.Value <= 5))
                return new GetRateDto { Message = "value must be between 1 and 5" };

            var rating = await context.UserRatings.SingleOrDefaultAsync(f => f.UserId == createRateDto.RaterId && f.RatedUserId == createRateDto.RatedId);
            if (rating == null)
                return new GetRateDto { Message = "user hasn't rated this user before" };

            User rated = new User();

            using (var transaction = await context.Database.BeginTransactionAsync()) {
                try {
                    rating.Value = createRateDto.Value;
                    context.Update(rating);

                    rated = await context.Users.Include(u => u.ReceivedUserRatings).SingleOrDefaultAsync(u => u.Id == createRateDto.RatedId);

                    var len = rated.ReceivedUserRatings.ToList().Count;
                    var actualRate = rated.ReceivedUserRatings.Sum(r => r.Value);
                    var total = actualRate / len;
                    rated.TotalRating = total;
                    context.Update(rated);

                    await context.SaveChangesAsync();

                    transaction.Commit();
                }
                catch (Exception ex) {
                    transaction.Rollback();
                    return new GetRateDto { Message = $"problem to rate a user, ERROR: {ex.Message}, SOURCE: {ex.Source} " };
                }
            }
            
            // update total rating for the rated user
            return new GetRateDto {
                IsFound = true,
                Message = "success",
                Value = rated.TotalRating
            };
        }

        public async Task<GetRateDto> UpdateFileRateAsync(CreateRateDto createRateDto) {
            if (!(createRateDto.Value >= 1 && createRateDto.Value <= 5))
                return new GetRateDto { Message = "value must be between 1 and 5" };

            var rating = await context.FileRatings.SingleOrDefaultAsync(f => f.UserId == createRateDto.RaterId && f.FileItemId == createRateDto.RatedId);
            if (rating == null)
                return new GetRateDto { Message = "user hasn't rated this file before" };

            FileItem rated = new();
            using (var transaction = await context.Database.BeginTransactionAsync()) {
                try {
                    rating.Value = createRateDto.Value;
                    context.Update(rating);
                    
                    rated = await context.FileItems.Include(f => f.FileRatings).SingleOrDefaultAsync(u => u.Id == createRateDto.RatedId);
                    var len = rated.FileRatings.ToList().Count;
                    var actualRate = rated.FileRatings.Sum(r => r.Value);
                    var total = actualRate / len;
                    rated.TotalRating = total;
                    context.Update(rated);

                    await context.SaveChangesAsync();   // 

                    transaction.Commit();
                }
                catch (Exception ex) {
                    transaction.Rollback();
                    return new GetRateDto { Message = $"problem to rate a file, ERROR: {ex.Message}, SOURCE: {ex.Source} " };
                }
            }

            // update total rating for the rated user
            return new GetRateDto {
                IsFound = true,
                Message = "success",
                Value = rated.TotalRating
            };
        }

        
    }
}