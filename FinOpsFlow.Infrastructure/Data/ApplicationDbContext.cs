using System.Security.Claims;
using FinOpsFlow.Core.Entities;
using FinOpsFlow.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinOpsFlow.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Request> Requests => Set<Request>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        ChangeTracker.DetectChanges();

        // Capture added requests before save (Id not yet assigned)
        var addedRequests = ChangeTracker.Entries<Request>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        // Capture field changes on modified requests before save
        var fieldChangeLogs = new List<AuditLog>();

        if (userId is not null)
        {
            var tracked = new HashSet<string> { "Title", "Description", "Status", "Priority", "CategoryId", "AssignedToId", "DueDate" };

            foreach (var entry in ChangeTracker.Entries<Request>().Where(e => e.State == EntityState.Modified))
            {
                foreach (var prop in entry.Properties.Where(p => p.IsModified && tracked.Contains(p.Metadata.Name)))
                {
                    var oldVal = prop.OriginalValue?.ToString();
                    var newVal = prop.CurrentValue?.ToString();

                    if (oldVal == newVal) continue;

                    fieldChangeLogs.Add(new AuditLog
                    {
                        RequestId = entry.Entity.Id,
                        UserId = userId,
                        Action = GetAuditAction(prop.Metadata.Name),
                        FieldName = prop.Metadata.Name,
                        OldValue = oldVal,
                        NewValue = newVal
                    });
                }
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        // Now added requests have Ids — log creation
        if (userId is not null && addedRequests.Any())
        {
            foreach (var req in addedRequests)
            {
                AuditLogs.Add(new AuditLog
                {
                    RequestId = req.Id,
                    UserId = userId,
                    Action = AuditAction.Created,
                    NewValue = req.Title
                });
            }
        }

        if (fieldChangeLogs.Any())
            AuditLogs.AddRange(fieldChangeLogs);

        if (addedRequests.Any() || fieldChangeLogs.Any())
            await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    private static AuditAction GetAuditAction(string propertyName) => propertyName switch
    {
        "Status" => AuditAction.StatusChanged,
        "AssignedToId" => AuditAction.AssigneeChanged,
        "Priority" => AuditAction.PriorityChanged,
        _ => AuditAction.Updated
    };
}
