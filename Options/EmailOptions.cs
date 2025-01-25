namespace KnowledgeFlowApi.Options
{
    public class EmailOptions
    {
        public string SMTPServer { get; set; }
        public int SMTPProt { get; set; }
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
    }
}