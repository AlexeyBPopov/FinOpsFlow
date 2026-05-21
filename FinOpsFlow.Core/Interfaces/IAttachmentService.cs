using FinOpsFlow.Core.Entities;

namespace FinOpsFlow.Core.Interfaces;

public interface IAttachmentService
{
    Task<Attachment> SaveMetadataAsync(
        int requestId, string uploadedById,
        string fileName, string storedFileName,
        string contentType, long fileSizeBytes, string storagePath);

    Task<Attachment?> GetByIdAsync(int id);
}