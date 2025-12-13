using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GatewayService.Clients;

public class FileStorageClient : IFileStorageClient
{
    private readonly HttpClient _httpClient;

    public FileStorageClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<FileUploadResult> UploadAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();

        await using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        form.Add(fileContent, "file", file.FileName);

        // Отправляем запрос в FileStorageService
        var response = await _httpClient.PostAsync("api/files", form, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // Если FileStorage вернул 4xx/5xx - подробную ошибку
            throw new HttpRequestException(
                $"FileStorageService returned {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
        }

      

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var id = root.GetProperty("id").GetGuid();
        var originalFileName = root.GetProperty("originalFileName").GetString() ?? string.Empty;
        var contentType = root.GetProperty("contentType").GetString() ?? "application/octet-stream";
        var createdAt = root.GetProperty("createdAt").GetDateTime();

        return new FileUploadResult(
            id,
            originalFileName,
            contentType,
            createdAt
        );
    }
}
