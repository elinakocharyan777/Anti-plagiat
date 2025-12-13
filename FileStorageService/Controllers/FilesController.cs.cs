using System;
using System.Threading;
using System.Threading.Tasks;
using FileStorageService.Models;
using FileStorageService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileStorageService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public FilesController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        // Загрузка файла в файловое хранилищ
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<FileMetadata>> Upload(
            [FromForm] IFormFile file,
            CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "Файл не передан или пустой" });
            }

            var metadata = await _fileStorageService.SaveAsync(file, cancellationToken);

            
            return Ok(metadata);
        }

        //Получить файл по id
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var result = await _fileStorageService.GetAsync(id, cancellationToken);

            if (result == null)
            {
                return NotFound(new { error = "Файл не найден" });
            }

            var (metadata, contentStream) = result.Value;

            return File(contentStream, metadata.ContentType, metadata.OriginalFileName);
        }
    }
}
