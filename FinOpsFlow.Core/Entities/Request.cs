using FinOpsFlow.Core.Enums;
using System.Net.Mail;

namespace FinOpsFlow.Core.Entities;

public class Request
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RequestStatus Status { get; set; } = RequestStatus.New;
    public RequestPriority Priority { get; set; } = RequestPriority.Medium;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string CreatedById { get; set; } = string.Empty;
    public ApplicationUser CreatedBy { get; set; } = null!;

    public string? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public DateTime? ClosedAt { get; set; }

    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Attachment> Attachments { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}