using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeFlowApi.Entities
{
    public class FileItem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        public string Name { get; set; } // name.ext
        public string Path { get; set; }
        public long Size { get; set; }
        public decimal? TotalRating { get; set; }    
        [Required]
        public DateTime UploadedOn { get; set; }
        public CoverImage? CoverImage { get; set; }
        public ICollection<FileRating> FileRatings { get; set; }
        
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        // public ICollection<FileRating> FileRatings { get; set; }

    }
}