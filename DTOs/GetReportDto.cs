using KnowledgeFlowApi.Enums;

namespace KnowledgeFlowApi.DTOs
{
    public class GetReportDto
    {
        public int ReportedByUserId { get; set; }
        public int ReportedUserId { get; set; }
        public int? ReportedFileItemId { get; set; }
        public string Reason { get; set; }
        public DateTime ReportDate { get; set; }
        public ReportStatus ReportStatus { get; set; }
        public bool IsValid = false;

    }
}