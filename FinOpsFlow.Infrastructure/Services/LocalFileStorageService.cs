using FinOpsFlow.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace FinOpsFlow.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _webRootPath;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _webRootPath = env.WebRootPath;
    }

    public async Task<string> SaveAsync(Stream content, string originalFileName, string subfolder)
    {
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        var storedName = $"{Guid.NewGuid()}{ext}";
        var folderPath = Path.Combine(_webRootPath, "uploads", subfolder);

        Directory.CreateDirectory(folderPath);

        var fullPath = Path.Combine(folderPath, storedName);
        using var fileStream = new FileStream(fullPath, FileMode.Create);
        await content.CopyToAsync(fileStream);

        return $"uploads/{subfolder}/{storedName}";
    }

    public void Delete(string relativePath)
    {
        var fullPath = Path.Combine(_webRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}