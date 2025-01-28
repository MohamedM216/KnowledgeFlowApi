namespace KnowledgeFlowApi.DTOs
{
    public class GetRateDto
    {
        public bool IsFound = false;
        public decimal? Value { get; set; }
        public string? Message { get; set; }

    }
}