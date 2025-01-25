using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeFlowApi.Entities
{
    public class UserRefreshToken
    {
        public int Id { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
        public DateTime CreatedOn { get; set; }
        public DateTime RevokedOn { get; set; }
        public bool IsActive => RevokedOn == null && !IsExpired;
    }
}