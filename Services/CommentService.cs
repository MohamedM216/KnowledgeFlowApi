using KnowledgeFlowApi.Data;
using KnowledgeFlowApi.DTOs;
using KnowledgeFlowApi.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeFlowApi.Services.CommentServices
{
    public class CommentService
    {
        private readonly IHubContext<CommentsHub> _hubContext;
        private readonly ApplicationDbContext _context;
        public CommentService(IHubContext<CommentsHub> hubContext, ApplicationDbContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        public async Task<GetCommentDto> WriteCommentAsync(AddCommentDto addCommentDto) {
            if (addCommentDto == null)
                return new GetCommentDto { Message = "null request" };

            var file = await _context.FileItems.SingleOrDefaultAsync(f => f.Id == addCommentDto.FileId);
            if (file == null)
                return new GetCommentDto { Message = "this file is not found" };
            
            var comment = new Comment
            {
                Text = addCommentDto.comment,
                UserId = addCommentDto.UserId,
                FileItemId = addCommentDto.FileId,
                CreatedOn = DateTime.UtcNow
            };

            try {
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                // Handle concurrency conflict
                return new GetCommentDto { Message = "Concurrency conflict occurred. Please try again." };
            }

            // Load the User navigation property
            await _context.Entry(comment).Reference(r => r.User).LoadAsync();

            try {
                await _hubContext.Clients.Group($"File_{comment.FileItemId}").SendAsync("ReceivedNewComment", comment.FileItemId, comment.User?.Username ?? "unknownUser", comment.Text);
            } catch (Exception ex) {
                // Log the error
                Console.WriteLine($"SignalR broadcast failed: {ex.Message}");
            }

            return MapCommentToGetCommentDto(comment);
        }


        public async Task<GetCommentDto> WriteReplyAsync(AddCommentDto addCommentDto) {
            if (addCommentDto.ParentId == null)
                return new GetCommentDto { Message = "null parent comment" };

            var parentComment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == addCommentDto.ParentId);
            if (parentComment == null) 
                return new GetCommentDto { Message = "parent comment not found" };

            if (parentComment.ParentCommentId is not null) 
                return new GetCommentDto { Message = "you can't make a reply on a reply" };

            var reply = new Comment
            {
                Text = addCommentDto.comment,
                FileItemId = parentComment.FileItemId,
                UserId = addCommentDto.UserId,
                ParentCommentId = addCommentDto.ParentId,
                CreatedOn = DateTime.UtcNow
            };

            try {
                _context.Comments.Add(reply);
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                // Handle concurrency conflict
                return new GetCommentDto { Message = "Concurrency conflict occurred. Please try again." };
            }

            // Load the User navigation property
            await _context.Entry(reply).Reference(r => r.User).LoadAsync();

            try {
                await _hubContext.Clients.Group($"File_{reply.FileItemId}").SendAsync("ReceivedNewComment", reply.FileItemId, reply.User?.Username ?? "unknownUser", reply.Text);
            } catch (Exception ex) {
                Console.WriteLine($"SignalR broadcast failed: {ex.Message}");
            }

            return MapCommentToGetCommentDto(reply);
        }

        public async Task<GetCommentDto> UpdateCommentAsync(UpdateCommentDto updateCommentDto) {
            var comment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == updateCommentDto.Id);
            if (comment == null) {
                return new GetCommentDto { Message = "comment not found" };
            }

            comment.Text = updateCommentDto.Text;
            _context.Update(comment);
            await _context.SaveChangesAsync();

            return MapCommentToGetCommentDto(comment);
        }

        public async Task<IEnumerable<GetCommentDto>> GetCommentsByFileIdAsync(int fileId, int page = 1, int pageSize = 10) {
            var file = await _context.FileItems
                .Include(f => f.Comments)
                .ThenInclude(c => c.Replies)
                .SingleOrDefaultAsync(f => f.Id == fileId);

            if (file == null) {
                return new List<GetCommentDto>();
            }

            var parentComments = file.Comments
                .Where(c => c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return parentComments.Select(MapCommentToGetCommentDto).ToList();
        }

        public async Task<IEnumerable<GetCommentDto>> GetCommentsByUserIdAsync(int userId, int page = 1, int pageSize = 10) {
            var user = await _context.Users
                .Include(u => u.Comments)
                .ThenInclude(c => c.Replies)
                .SingleOrDefaultAsync(u => u.Id == userId);

            if (user == null) {
                return new List<GetCommentDto>(); 
            }

            // Filter parent comments (comments without a ParentCommentId)
            var parentComments = user.Comments
                .Where(c => c.ParentCommentId == null) 
                .OrderByDescending(c => c.CreatedOn)   
                .Skip((page - 1) * pageSize)           
                .Take(pageSize);                       

            return parentComments.Select(MapCommentToGetCommentDto).ToList();
        }

        public async Task<IEnumerable<GetCommentDto>> GetRepliesByParentIdAsync(int parentId, int page = 1, int pageSize = 10) {
            var parent = await _context.Comments
                .Include(c => c.Replies)
                .SingleOrDefaultAsync(c => c.Id == parentId);

            if (parent == null) {
                return new List<GetCommentDto>(); // Return an empty list if the parent comment is not found
            }

            // Paginate the replies
            var replies = parent.Replies
                .OrderByDescending(r => r.CreatedOn)   
                .Skip((page - 1) * pageSize)           
                .Take(pageSize);                       

            return replies.Select(MapCommentToGetCommentDto).ToList();
        }

        public async Task<bool> DeleteByIdAsync(int id) {
            var comment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == id);
            if (comment == null) {
                return false;
            }

            try {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return true;
            } catch (Exception ex) {
                // Log the error
                Console.WriteLine($"Failed to delete comment: {ex.Message}");
                return false;
            }
        }

        private GetCommentDto MapCommentToGetCommentDto(Comment comment) {
            return new GetCommentDto {
                comment = comment.Text,
                FileId = comment.FileItemId,
                UserId = comment.UserId,
                ParentId = comment.ParentCommentId ?? -1,
                IsCreated = true,
                Replies = comment.Replies?.Select(MapCommentToGetCommentDto).ToList() ?? new List<GetCommentDto>()
            };
        }

    }
}