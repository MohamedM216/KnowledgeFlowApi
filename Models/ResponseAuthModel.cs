namespace KnowledgeFlowApi.Models
{
    public class ResponseAuthModel
    {
        public string? Message { get; set; }
        public bool IsAuthenticated = false;
        public string? Meta { get; set; }
    }
}