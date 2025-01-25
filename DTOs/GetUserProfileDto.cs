namespace KnowledgeFlowApi.DTOs
{
    public class GetUserProfileDto
    {
        public string Username { get; set; }
        public string Bio { get; set; }
        public string ContactEmail { get; set; } 
        public string? ProfileImagePath { get; set; }
    }
}