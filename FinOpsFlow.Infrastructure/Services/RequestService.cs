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

            if (!string.IsNullOrWhiteSpace(filter.AssignedToId))
                query = query.Where(r => r.AssignedToId == filter.AssignedToId);

            if (filter.CategoryId.HasValue)
                query = query.Where(r => r.CategoryId == filter.CategoryId.Value);

            if (filter.CreatedFrom.HasValue)
                query = query.Where(r => r.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(r => r.CreatedAt < filter.CreatedTo.Value.AddDays(1));

            if (filter.OverdueOnly)
                query = query.Where(r =>
                    r.DueDate.HasValue &&
                    r.DueDate < DateTime.UtcNow &&
                    r.Status != RequestStatus.Completed &&
                    r.Status != RequestStatus.Rejected);
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
        await _db.SaveChangesAsync(); // DbContext auto-logs Created
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

        await _db.SaveChangesAsync(); // DbContext auto-logs field changes
    }

    public async Task ChangeStatusAsync(int id, RequestStatus newStatus, string userId)
    {
        var request = await _db.Requests.FindAsync(id)
            ?? throw new KeyNotFoundException($"Request {id} not found");

        request.Status = newStatus;
        request.UpdatedAt = DateTime.UtcNow;

        if (newStatus is RequestStatus.Completed or RequestStatus.Rejected)
            request.ClosedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(); // DbContext auto-logs StatusChanged
    }
}