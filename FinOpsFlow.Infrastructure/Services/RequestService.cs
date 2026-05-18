using FinOpsFlow.Core.DTOs;
using FinOpsFlow.Core.Entities;
using FinOpsFlow.Core.Enums;
using FinOpsFlow.Core.Interfaces;
using FinOpsFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinOpsFlow.Infrastructure.Services;

public class RequestService : IRequestService
{
    private readonly ApplicationDbContext _db;

    public RequestService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Request>> GetAllAsync(RequestFilterDto? filter = null)
    {
        var query = _db.Requests
            .Include(r => r.Category)
            .Include(r => r.CreatedBy)
            .Include(r => r.AssignedTo)
            .AsQueryable();

        if (filter is not null)
        {
            if (filter.Status.HasValue)
                query = query.Where(r => r.Status == filter.Status.Value);

            if (filter.Priority.HasValue)
                query = query.Where(r => r.Priority == filter.Priority.Value);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(r =>
                    r.Title.Contains(filter.Keyword) ||
                    r.Description.Contains(filter.Keyword));
        }

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<Request?> GetByIdAsync(int id)
    {
        return await _db.Requests
            .Include(r => r.Category)
            .Include(r => r.CreatedBy)
            .Include(r => r.AssignedTo)
            .Include(r => r.Comments).ThenInclude(c => c.Author)
            .Include(r => r.Attachments).ThenInclude(a => a.UploadedBy)
            .Include(r => r.AuditLogs).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Request> CreateAsync(CreateRequestDto dto, string userId)
    {
        var request = new Request
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            CategoryId = dto.CategoryId,
            DueDate = dto.DueDate,
            CreatedById = userId,
            Status = RequestStatus.New
        };

        _db.Requests.Add(request);
        await _db.SaveChangesAsync();

        _db.AuditLogs.Add(new AuditLog
        {
            RequestId = request.Id,
            UserId = userId,
            Action = AuditAction.Created,
            NewValue = request.Title
        });
        await _db.SaveChangesAsync();

        return request;
    }

    public async Task UpdateAsync(int id, UpdateRequestDto dto, string userId)
    {
        var request = await _db.Requests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Request {id} not found");

        request.Title = dto.Title;
        request.Description = dto.Description;
        request.Priority = dto.Priority;
        request.CategoryId = dto.CategoryId;
        request.DueDate = dto.DueDate;
        request.AssignedToId = string.IsNullOrEmpty(dto.AssignedToId) ? null : dto.AssignedToId;
        request.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            RequestId = id,
            UserId = userId,
            Action = AuditAction.FieldUpdated,
            FieldName = "Request",
            NewValue = dto.Title
        });

        await _db.SaveChangesAsync();
    }

    public async Task ChangeStatusAsync(int id, RequestStatus newStatus, string userId)
    {
        var request = await _db.Requests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Request {id} not found");

        var oldStatus = request.Status;
        request.Status = newStatus;
        request.UpdatedAt = DateTime.UtcNow;

        if (newStatus is RequestStatus.Completed or RequestStatus.Rejected)
            request.ClosedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            RequestId = id,
            UserId = userId,
            Action = AuditAction.StatusChanged,
            FieldName = "Status",
            OldValue = oldStatus.ToString(),
            NewValue = newStatus.ToString()
        });

        await _db.SaveChangesAsync();
    }
}