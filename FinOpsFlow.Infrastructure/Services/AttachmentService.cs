using FinOpsFlow.Core.Entities;
using FinOpsFlow.Core.Enums;
using FinOpsFlow.Core.Interfaces;
using FinOpsFlow.Infrastructure.Data;

namespace FinOpsFlow.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly ApplicationDbContext _db;

    public AttachmentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Attachment> SaveMetadataAsync(
        int requestId, string uploadedById,
        string fileName, string storedFileName,
        string contentType, long fileSizeBytes, string storagePath)
    {
        var attachment = new Attachment
        {
            RequestId = requestId,
            UploadedById = uploadedById,
            FileName = fileName,
            StoredFileName = storedFileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            StoragePath = storagePath
        };

        _db.Attachments.Add(attachment);

        _db.AuditLogs.Add(new AuditLog
        {
            RequestId = requestId,
            UserId = uploadedById,
            Action = AuditAction.AttachmentAdded,
            NewValue = fileName
        });

        await _db.SaveChangesAsync();
        return attachment;
    }

    public async Task<Attachment?> GetByIdAsync(int id)
        => await _db.Attachments.FindAsync(id);
}