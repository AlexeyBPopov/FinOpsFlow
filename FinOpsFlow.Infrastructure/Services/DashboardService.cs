using FinOpsFlow.Core.DTOs;
using FinOpsFlow.Core.Enums;
using FinOpsFlow.Core.Interfaces;
using FinOpsFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinOpsFlow.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;

    private static readonly RequestStatus[] OpenStatuses =
        [RequestStatus.New, RequestStatus.InReview, RequestStatus.WaitingForInfo, RequestStatus.Approved];

    private static readonly RequestStatus[] ClosedStatuses =
        [RequestStatus.Completed, RequestStatus.Rejected];

    public DashboardService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardDto> GetAsync(string userId, bool isManager)
    {
        var now = DateTime.UtcNow;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalOpen = await _db.Requests
            .CountAsync(r => OpenStatuses.Contains(r.Status));

        var totalOverdue = await _db.Requests
            .CountAsync(r => !ClosedStatuses.Contains(r.Status) && r.DueDate.HasValue && r.DueDate < now);

        var completedThisMonth = await _db.Requests
            .CountAsync(r => r.Status == RequestStatus.Completed && r.ClosedAt >= firstOfMonth);

        var myOpenRequests = await _db.Requests
            .CountAsync(r => r.AssignedToId == userId && OpenStatuses.Contains(r.Status));

        // ByStatus — ensure all statuses present even if count = 0
        var byStatusRaw = await _db.Requests
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var byStatus = Enum.GetValues<RequestStatus>()
            .ToDictionary(s => s, s => byStatusRaw.FirstOrDefault(x => x.Status == s)?.Count ?? 0);

        // ByPriority — open requests only
        var byPriorityRaw = await _db.Requests
            .Where(r => OpenStatuses.Contains(r.Status))
            .GroupBy(r => r.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync();

        var byPriority = Enum.GetValues<RequestPriority>()
            .ToDictionary(p => p, p => byPriorityRaw.FirstOrDefault(x => x.Priority == p)?.Count ?? 0);

        // Recent activity
        var recentRaw = await _db.AuditLogs
            .Include(a => a.User)
            .Include(a => a.Request)
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .ToListAsync();

        var recentActivity = recentRaw
            .Select(a => new RecentActivityItem(
                a.RequestId,
                a.Request?.Title ?? $"Request #{a.RequestId}",
                a.User?.FullName ?? "Unknown",
                a.Action,
                a.Timestamp))
            .ToList();

        // Assignee workload (Manager/Admin only)
        var byAssignee = new List<AssigneeLoadItem>();
        if (isManager)
        {
            var assignedOpen = await _db.Requests
                .Include(r => r.AssignedTo)
                .Where(r => r.AssignedToId != null && OpenStatuses.Contains(r.Status))
                .ToListAsync();

            byAssignee = assignedOpen
                .GroupBy(r => r.AssignedTo!.FullName)
                .Select(g => new AssigneeLoadItem(g.Key, g.Count()))
                .OrderByDescending(x => x.OpenCount)
                .ToList();
        }

        return new DashboardDto
        {
            TotalOpen = totalOpen,
            TotalOverdue = totalOverdue,
            CompletedThisMonth = completedThisMonth,
            MyOpenRequests = myOpenRequests,
            ByStatus = byStatus,
            ByPriority = byPriority,
            RecentActivity = recentActivity,
            ByAssignee = byAssignee
        };
    }
}