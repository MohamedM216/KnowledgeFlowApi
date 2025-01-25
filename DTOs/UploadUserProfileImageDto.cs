namespace KnowledgeFlowApi.DTOs
{
    public class UploadUserProfileImageDto
    {
        public int UserId { get; set; }
        public IFormFile ProfileImage { get; set; }
    }
}