using System;

namespace FileStorageService.Models
{
    public class FileMetadata
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public string StoragePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
