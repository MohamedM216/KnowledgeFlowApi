using System.ComponentModel.DataAnnotations;

namespace KnowledgeFlowApi.DTOs
{
    public class FileResponseDto
    {
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public long? FileSize { get; set; }
        public string? CoverImagePath { get; set; }
        public DateTime? UploadedOn { get; set; }
        public bool IsValid = false;
        public string  ErrorMessage { get; set; }
    }
}