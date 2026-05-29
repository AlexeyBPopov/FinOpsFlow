using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FinOpsFlow.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FinOpsFlow.Infrastructure.Services;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "attachments";

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage")
            ?? throw new InvalidOperationException("AzureBlobStorage connection string not configured.");

        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> SaveAsync(Stream content, string originalFileName, string subfolder)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        var blobName = $"{subfolder}/{Guid.NewGuid()}{ext}";
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(content, new BlobHttpHeaders
        {
            ContentType = GetContentType(ext)
        });

        return blobName;
    }

    public async Task<Stream> GetStreamAsync(string storagePath)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(storagePath);
        var response = await blobClient.DownloadStreamingAsync();
        return response.Value.Content;
    }

    public void Delete(string storagePath)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        containerClient.GetBlobClient(storagePath).DeleteIfExists();
    }

    private static string GetContentType(string ext) => ext switch
    {
        ".pdf" => "application/pdf",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".xls" => "application/vnd.ms-excel",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        _ => "application/octet-stream"
    };
}