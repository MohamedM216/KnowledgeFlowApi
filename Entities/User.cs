using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeFlowApi.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [MaxLength(10000)]
        public string? Bio { get; set; }
        public string? ContactEmail { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string Password { get; set; }    // hashed password
        [Required]
        public DateTime MembershipDate { get; set; }
        public UserProfileImage? UserProfileImage { get; set; }
        public ICollection<FileItem>? FileItems { get; set; }
        // public ICollection<FileRating>? FileRatings { get; set; }
        public ICollection<UserRefreshToken>? UserRefreshTokens { get; set; }
    }
}