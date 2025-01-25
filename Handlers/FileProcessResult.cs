namespace KnowledgeFlowApi.Handlers;

public class FileProcessResult
{
    public bool Success { get; set; }
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public string ErrorMessage { get; set; }
}
