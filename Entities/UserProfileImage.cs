

using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeFlowApi.Entities
{
    public class UserProfileImage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime UploadedOn { get; set; }  
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
    }
}