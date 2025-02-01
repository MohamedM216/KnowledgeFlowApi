using KnowledgeFlowApi.Enums;

namespace KnowledgeFlowApi.DTOs
{
    public class GetReviewReportDto
    {
        public bool IsValid = false;
        public string? Message { get; set; }
        public AdminAction? AdminAction { get; set; }
    }
}