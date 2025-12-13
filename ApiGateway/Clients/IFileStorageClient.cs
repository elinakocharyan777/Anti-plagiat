using Microsoft.AspNetCore.Http;

namespace GatewayService.Clients;

public record FileUploadResult(Guid FileId, string FileName, string ContentType, DateTime CreatedAt);

public interface IFileStorageClient
{
    Task<FileUploadResult> UploadAsync(IFormFile file, CancellationToken cancellationToken = default);
}
