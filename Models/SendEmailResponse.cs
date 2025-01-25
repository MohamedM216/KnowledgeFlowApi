namespace KnowledgeFlowApi.Models
{
    public class SendEmailResponse
    {
        
        public bool IsRetreved = false;
        public string ToEmail { get; set; }
        public string ToUsername { get; set; }
        public string Message { get; set; }
        
    }
}