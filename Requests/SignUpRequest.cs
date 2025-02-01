using System.ComponentModel.DataAnnotations;

namespace KnowledgeFlowApi.Requests.UserRequests
{
    public class SignUpRequest
    {
        [Required]
        [MaxLength(20)]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [MaxLength(10000)]
        public string? Bio { get; set; }
        public string? ContactEmail { get; set; }
        [Required]
        [MaxLength(20)]
        public string Password { get; set; }    // hashed password
        public string Role { get; set; }
        public string? AdminKey { get; set; }
    }
}