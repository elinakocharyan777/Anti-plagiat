using FileStorageService.Services;
using FileStorageService.Swagger;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FileStorageService",
        Version = "v1"
    });

    c.OperationFilter<FileUploadOperationFilter>();
});

builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthorization();
app.MapControllers();

app.Run();
