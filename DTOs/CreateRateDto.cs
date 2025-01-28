namespace KnowledgeFlowApi.DTOs
{
    public class CreateRateDto
    {
        public int RaterId { get; set; }
        public int RatedId { get; set; }
        public decimal Value { get; set; }

        public string? Review { get; set; } 
        public string? Message { get; set; }
        public bool IsFound = false;
    }
}