using FinOpsFlow.Core.Entities;

namespace FinOpsFlow.Core.Interfaces;

public interface ICommentService
{
    Task<Comment> AddAsync(int requestId, string authorId, string body);
}