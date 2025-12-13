using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileStorageService.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace FileStorageService.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _storageRoot;

        private static readonly ConcurrentDictionary<Guid, FileMetadata> _files =
            new ConcurrentDictionary<Guid, FileMetadata>();

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;

            // Путь <ContentRoot>/StoredFiles
            _storageRoot = Path.Combine(_environment.ContentRootPath, "StoredFiles");

            if (!Directory.Exists(_storageRoot))
            {
                Directory.CreateDirectory(_storageRoot);
            }
        }

        public async Task<FileMetadata> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Файл пустой", nameof(file));

            var id = Guid.NewGuid();
            var fileNameOnDisk = id.ToString("N");
            var fullPath = Path.Combine(_storageRoot, fileNameOnDisk);

            await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var metadata = new FileMetadata
            {
                Id = id,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType ?? "application/octet-stream",
                StoragePath = fullPath,
                CreatedAt = DateTime.UtcNow
            };

            _files[id] = metadata;
            return metadata;
        }

        public async Task<(FileMetadata metadata, Stream content)?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (!_files.TryGetValue(id, out var metadata))
                return null;

            if (!File.Exists(metadata.StoragePath))
                return null;

            var stream = new FileStream(metadata.StoragePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            await Task.CompletedTask; 

            return (metadata, stream);
        }
    }
}
