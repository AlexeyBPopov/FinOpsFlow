namespace FinOpsFlow.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string originalFileName, string subfolder);
    void Delete(string relativePath);
}