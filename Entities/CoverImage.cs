
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeFlowApi.Entities
{
    public class CoverImage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime UploadedOn { get; set; }  
        public FileItem FileItem { get; set; }
        [ForeignKey("FileItem")]
        public int FileItemId { get; set; }
    }
}