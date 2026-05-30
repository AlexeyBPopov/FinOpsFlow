namespace FinOpsFlow.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string originalFileName, string subfolder);
    Task<Stream> GetStreamAsync(string storagePath);
    void Delete(string relativePath);
}