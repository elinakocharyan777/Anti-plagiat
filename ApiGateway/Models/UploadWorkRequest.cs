using Microsoft.AspNetCore.Http;

namespace GatewayService.Models
{
    public class UploadWorkRequest
    {
        public string StudentId { get; set; } = string.Empty;

        public string AssignmentId { get; set; } = string.Empty;

        public IFormFile File { get; set; } = default!;
    }
}
