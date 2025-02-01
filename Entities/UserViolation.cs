using KnowledgeFlowApi.Enums;

namespace KnowledgeFlowApi.Entities
{
    public class UserViolation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ViolationType ViolationType { get; set; } // Enum: SexualContent, Other
        public DateTime ViolationDate { get; set; }
        public User User { get; set; }
    }
}