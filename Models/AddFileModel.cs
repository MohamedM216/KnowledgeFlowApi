using System.ComponentModel.DataAnnotations;

namespace KnowledgeFlowApi.Models
{
    public class AddFileModel
    {
        
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        [Required]
        public IFormFile File { get; set; }
        public IFormFile? CoverImage { get; set; }
    }
}