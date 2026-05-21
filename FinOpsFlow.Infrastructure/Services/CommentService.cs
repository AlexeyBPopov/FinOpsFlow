using FinOpsFlow.Core.Entities;
using FinOpsFlow.Core.Enums;
using FinOpsFlow.Core.Interfaces;
using FinOpsFlow.Infrastructure.Data;

namespace FinOpsFlow.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _db;

    public CommentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Comment> AddAsync(int requestId, string authorId, string body)
    {
        var comment = new Comment
        {
            RequestId = requestId,
            AuthorId = authorId,
            Body = body
        };

        _db.Comments.Add(comment);

        _db.AuditLogs.Add(new AuditLog
        {
            RequestId = requestId,
            UserId = authorId,
            Action = AuditAction.CommentAdded,
            NewValue = body.Length > 100 ? body[..100] + "…" : body
        });

        await _db.SaveChangesAsync();
        return comment;
    }
}