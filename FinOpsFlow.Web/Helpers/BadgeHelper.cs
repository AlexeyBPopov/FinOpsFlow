using FinOpsFlow.Core.Enums;

namespace FinOpsFlow.Web.Helpers;

public static class BadgeHelper
{
    public static string StatusBadge(RequestStatus status) => status switch
    {
        RequestStatus.New => "bg-secondary",
        RequestStatus.InReview => "bg-info",
        RequestStatus.WaitingForInfo => "bg-warning text-dark",
        RequestStatus.Approved => "bg-success",
        RequestStatus.Rejected => "bg-danger",
        RequestStatus.Completed => "bg-dark",
        _ => "bg-secondary"
    };

    public static string PriorityBadge(RequestPriority priority) => priority switch
    {
        RequestPriority.Low => "bg-success",
        RequestPriority.Medium => "bg-warning text-dark",
        RequestPriority.High => "bg-danger",
        RequestPriority.Critical => "bg-dark",
        _ => "bg-secondary"
    };

    public static string FormatFileSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024):F1} MB"
    };
}