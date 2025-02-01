using KnowledgeFlowApi.Enums;

namespace KnowledgeFlowApi.Entities
{
    public class Report
    {
        public int Id { get; set; }
        public int ReportedByUserId { get; set; }
        public int? ReportedUserId { get; set; }
        public int? ReportedFileItemId { get; set; }
        public string Reason { get; set; }
        public DateTime ReportDate { get; set; }
        public ReportStatus Status { get; set; } // Enum: Pending, Reviewed, Resolved

        public User ReportedByUser { get; set; }
        public User ReportedUser { get; set; }
        public FileItem ReportedFileItem { get; set; }
    }
}