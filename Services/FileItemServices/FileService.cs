using KnowledgeFlowApi.Data;
using KnowledgeFlowApi.DTOs;
using KnowledgeFlowApi.Entities;
using KnowledgeFlowApi.Handlers;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeFlowApi.Services.FileItemServices
{
    public class FileService
    {
        private readonly ApplicationDbContext _context;
        private readonly FileHandler _fileHandler;

        public FileService(ApplicationDbContext context, FileHandler fileHandler)
        {
            _context = context;
            _fileHandler = fileHandler;
        }

        public async Task<FileResponseDto> UploadFileItemAsync(FileUploadDto fileUploadDto)
        {
            // file processing
            var validationResult = _fileHandler.ValidateFile(fileUploadDto.File, FileType.Document);
            if (!validationResult.IsValid)
                return new FileResponseDto { ErrorMessage = validationResult.ErrorMessage };
            
            var fileProcessResult = await _fileHandler.SaveFileAsync(fileUploadDto.File, "documents", FileType.Document);
            if (!fileProcessResult.Success)
                return new FileResponseDto { ErrorMessage = fileProcessResult.ErrorMessage };
            
            // cover image processing
            var ImagevalidationResult = _fileHandler.ValidateFile(fileUploadDto.CoverImage, FileType.Image);
            if (!ImagevalidationResult.IsValid)
                return new FileResponseDto { ErrorMessage = ImagevalidationResult.ErrorMessage };
            
            var ImageFileProcessResult = await _fileHandler.SaveFileAsync(fileUploadDto.CoverImage, "images/books", FileType.Image);
            if (!ImageFileProcessResult.Success)
                return new FileResponseDto { ErrorMessage = ImageFileProcessResult.ErrorMessage };

            using (var transaction = await _context.Database.BeginTransactionAsync()) {
                try {

                    var fileItem = new FileItem
                    {
                        Title = fileUploadDto.Title,
                        Description = fileUploadDto.Description,
                        UploadedOn = DateTime.UtcNow,
                        Name = fileProcessResult.FileName,
                        Path = fileProcessResult.FilePath,
                        Size = fileProcessResult.FileSize,
                        UserId = fileUploadDto.UserId
                    };
                    _context.Add(fileItem);
                    await _context.SaveChangesAsync();

                    var coverImage = new CoverImage
                    {
                        Name = ImageFileProcessResult.FileName,
                        Path = ImageFileProcessResult.FilePath,
                        Size = ImageFileProcessResult.FileSize,
                        FileItemId = fileItem.Id,
                        UploadedOn = DateTime.UtcNow,
                    };
                    _context.Add(coverImage);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    
                    return MapToFileResponseDto(fileItem);
                }
                catch (Exception ex) {
                    transaction.Rollback();
                    Console.WriteLine($"{ex.Message} --- {ex.InnerException} --- {ex.Source}");
                }
            }
            return new FileResponseDto { ErrorMessage = "An error happend while uploading the file or image" };
        }

        public async Task<FileResponseDto> GetFileItemAsyncByUserId(int fileId)
        {
            var file = await _context.Set<FileItem>()
                .Include(f => f.CoverImage)
                .FirstOrDefaultAsync(f => f.Id == fileId);

            return file == null ? new FileResponseDto { ErrorMessage = "file not found" } : MapToFileResponseDto(file);
        }

        public async Task<bool> DeleteFileAsync(int fileId)
        {
            var file = await _context.Set<FileItem>()
                .Include(f => f.CoverImage).FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
                return false;

            var isFileRemoved = await _fileHandler.DeleteFileAsync(file.Path);

            bool isCoverImageRemoved = false;
            if (isFileRemoved && file.CoverImage != null && !string.IsNullOrEmpty(file.CoverImage.Path))
                isCoverImageRemoved = await _fileHandler.DeleteFileAsync(file.CoverImage.Path);

            if (isFileRemoved && isCoverImageRemoved)
            {
                _context.Remove(file);
                await _context.SaveChangesAsync();
                return true;    
            }
            return false;
        }

        public async Task<IEnumerable<FileResponseDto>> GetAllFileItemsAsyncByUserId(int userId)
        {
            
            var files = await _context.Set<FileItem>()
                .Include(f => f.CoverImage) 
                .Where(f => f.UserId == userId)
                .ToListAsync();

            
            if (files == null || !files.Any())
                return null;

            return files.Select(MapToFileResponseDto);
        }


        public async Task<IEnumerable<FileResponseDto>> GetAllFileItemsAsync()
        {
            var files = await _context.Set<FileItem>()
                .Include(f => f.CoverImage)
                .ToListAsync();

            if (files == null || !files.Any())
                return null;

            return files.Select(MapToFileResponseDto);
        }

        private FileResponseDto MapToFileResponseDto(FileItem file)
        {
            return new FileResponseDto
            {
                Title = file.Title,
                Description = file.Description,
                FileName = file.Name,
                FilePath = file.Path,
                FileSize = file.Size,
                UploadedOn = file.UploadedOn,
                CoverImagePath = file.CoverImage?.Path,
                IsValid = true,
            };
        }

    }
}