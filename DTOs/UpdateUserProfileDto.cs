using System.ComponentModel.DataAnnotations;

namespace KnowledgeFlowApi.DTOs
{
    public class UpdateUserProfileDto
    {
        [Required]
        public int Id { get; set; } // userid
        [MaxLength(20)]
        public string? Username { get; set; }
        [MaxLength(10000)]
        public string? Bio { get; set; }
        public string? ContactEmail { get; set; }
        public IFormFile? newProfileImage { get; set; }
        public string? oldImagePath { get; set; }
        
    }
}