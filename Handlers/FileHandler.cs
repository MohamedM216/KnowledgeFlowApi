// Helpers/FileHandler.cs
namespace KnowledgeFlowApi.Handlers;

public enum FileType
{
    Image,
    Document
}

public class FileHandler
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileHandler> _logger;

    // Supported file types configuration
    private readonly Dictionary<FileType, string[]> _allowedFileTypes = new()
    {
        { FileType.Image, new[] { ".jpg", ".jpeg", ".png" } },
        { FileType.Document, new[] { ".pdf", ".doc", ".docx", ".txt", ".rtf" } }
    };

    // File size limits (in bytes)
    private readonly Dictionary<FileType, long> _fileSizeLimits = new()
    {
        { FileType.Image, 5 * 1024 * 1024 },     // 5MB for images
        { FileType.Document, 100 * 1024 * 1024 }   // 100MB for documents
    };

    public FileHandler(IWebHostEnvironment environment, ILogger<FileHandler> logger)
    {
        _environment = environment;
        _logger = logger;
    }


    public FileValidationResult ValidateFile(IFormFile file, FileType fileType, long? customSizeLimit = null)
    {
        try
        {
            if (file == null || file.Length == 0)
                return new FileValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = "No file was provided." 
                };

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedFileTypes[fileType].Contains(extension))
                return new FileValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = $"File type not allowed. Allowed types are: {string.Join(", ", _allowedFileTypes[fileType])}" 
                };

            // Check file size
            var sizeLimit = customSizeLimit ?? _fileSizeLimits[fileType];
            if (file.Length > sizeLimit)
                return new FileValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = $"File size exceeds the limit of {sizeLimit / 1024 / 1024} MB" 
                };

            return new FileValidationResult { IsValid = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file");
            return new FileValidationResult 
            { 
                IsValid = false, 
                ErrorMessage = "An error occurred while validating the file." 
            };
        }
    }

    public async Task<FileProcessResult> SaveFileAsync(
        IFormFile file, 
        string subDirectory, 
        FileType fileType,
        string customFileName = null)
    {
        try
        {
            // Validate file
            var validationResult = ValidateFile(file, fileType);
            if (!validationResult.IsValid)
                return new FileProcessResult 
                { 
                    Success = false, 
                    ErrorMessage = validationResult.ErrorMessage 
                };

            // Create directory if it doesn't exist
            var uploadPath = Path.Combine(_environment.WebRootPath, subDirectory);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Generate file name
            var fileName = customFileName ?? 
                $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // Generate paths
            var filePath = Path.Combine(subDirectory, fileName);
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);

            // Save file
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new FileProcessResult
            {
                Success = true,
                FilePath = filePath.Replace("\\", "/"), // Normalize path separators
                FileName = fileName,
                FileSize = file.Length,
                ContentType = file.ContentType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file");
            return new FileProcessResult 
            { 
                Success = false, 
                ErrorMessage = "An error occurred while saving the file." 
            };
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var fullPath = Path.Combine(_environment.WebRootPath, filePath);
            if (!File.Exists(fullPath))
                return false;

            await Task.Run(() => File.Delete(fullPath));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<FileProcessResult> ReplaceFileAsync(
        IFormFile newFile, 
        string oldFilePath, 
        string subDirectory, 
        FileType fileType)
    {
        try
        {
            // Delete old file if exists
            if (!string.IsNullOrEmpty(oldFilePath))
            {
                await DeleteFileAsync(oldFilePath);
            }

            // Save new file
            return await SaveFileAsync(newFile, subDirectory, fileType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replacing file");
            return new FileProcessResult 
            { 
                Success = false, 
                ErrorMessage = "An error occurred while replacing the file." 
            };
        }
    }

    public bool FileExists(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        var fullPath = Path.Combine(_environment.WebRootPath, filePath);
        return File.Exists(fullPath);
    }

    public long GetFileSize(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return 0;

        var fullPath = Path.Combine(_environment.WebRootPath, filePath);
        if (!File.Exists(fullPath))
            return 0;

        var fileInfo = new FileInfo(fullPath);
        return fileInfo.Length;
    }
}