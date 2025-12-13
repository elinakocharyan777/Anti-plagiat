using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileStorageService.Models;
using Microsoft.AspNetCore.Http;

namespace FileStorageService.Services
{
    public interface IFileStorageService
    {
        Task<FileMetadata> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);

        Task<(FileMetadata metadata, Stream content)?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
