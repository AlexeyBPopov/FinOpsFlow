using FinOpsFlow.Core.Enums;

namespace FinOpsFlow.Core.DTOs;

public class DashboardDto
{
    public int TotalOpen { get; init; }
    public int TotalOverdue { get; init; }
    public int CompletedThisMonth { get; init; }
    public int MyOpenRequests { get; init; }
    public Dictionary<RequestStatus, int> ByStatus { get; init; } = [];
    public Dictionary<RequestPriority, int> ByPriority { get; init; } = [];
    public List<RecentActivityItem> RecentActivity { get; init; } = [];
    public List<AssigneeLoadItem> ByAssignee { get; init; } = [];
}

public record RecentActivityItem(
    int RequestId,
    string RequestTitle,
    string UserName,
    AuditAction Action,
    DateTime Timestamp
);

public record AssigneeLoadItem(string Name, int OpenCount);