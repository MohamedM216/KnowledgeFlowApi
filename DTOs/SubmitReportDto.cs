namespace KnowledgeFlowApi.DTOs
{
    public class SubmitReportDto
    {
        public int ReportedByUserId { get; set; }
        public int ReportedUserId { get; set; }
        public int? ReportedFileItemId { get; set; }
        public string Reason { get; set; }
    }
}