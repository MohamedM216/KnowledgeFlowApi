using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        public string Role { get; set; }
        [MaxLength(10000)]
        public string? Bio { get; set; }
        public string? ContactEmail { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(255)")]
        public string Password { get; set; }    // hashed password
        [Required]
        public DateTime MembershipDate { get; set; }
        public decimal? TotalRating { get; set; }
         public bool IsBanned { get; set; } // Indicates if the user is currently banned
        

        public UserProfileImage? UserProfileImage { get; set; }
        public ICollection<FileItem>? FileItems { get; set; }
        public ICollection<FileRating> FileRatings { get; set; }
        public ICollection<UserRating> UserRatings { get; set; }
        public ICollection<UserRefreshToken>? UserRefreshTokens { get; set; }

        // A user can rate multiple users
        [JsonIgnore]
        public ICollection<UserRating> GivenUserRatings { get; set; } = new List<UserRating>();
        // A user can receive multiple ratings
        public ICollection<UserRating> ReceivedUserRatings { get; set; } = new List<UserRating>();
        public ICollection<Comment> Comments { get; set; }

        public ICollection<Report> ReportsSubmitted { get; set; }
        public ICollection<Report> ReportsAgainst { get; set; }
        public ICollection<UserViolation> Violations { get; set; }
        public ICollection<Ban> Bans { get; set; }
        
    }
}

