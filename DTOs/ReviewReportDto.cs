using KnowledgeFlowApi.Enums;

namespace KnowledgeFlowApi.DTOs
{
    public class ReviewReportDto
    {
        public int ReportId { get; set; }
        public AdminAction AdminAction { get; set; }
        public ViolationType ViolationType { get; set; }
        public string AdminComment { get; set; }
    }
}