using FinOpsFlow.Core.Entities;
using FinOpsFlow.Core.Enums;
using FinOpsFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinOpsFlow.Web.Pages.Admin;

[Authorize(Roles = "Admin,Manager")]
public class AuditLogModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private const int PageSize = 25;

    public AuditLogModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<AuditLog> Logs { get; set; } = [];
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public AuditAction? ActionFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? UserFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int? RequestIdFilter { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }

    public SelectList Actions { get; set; } = null!;
    public SelectList Users { get; set; } = null!;

    public async Task OnGetAsync()
    {
        var query = _db.AuditLogs
            .Include(a => a.User)
            .Include(a => a.Request)
            .AsQueryable();

        if (ActionFilter.HasValue)
            query = query.Where(a => a.Action == ActionFilter.Value);

        if (!string.IsNullOrWhiteSpace(UserFilter))
            query = query.Where(a => a.UserId == UserFilter);

        if (RequestIdFilter.HasValue)
            query = query.Where(a => a.RequestId == RequestIdFilter.Value);

        if (DateFrom.HasValue)
            query = query.Where(a => a.Timestamp >= DateFrom.Value);

        if (DateTo.HasValue)
            query = query.Where(a => a.Timestamp < DateTo.Value.AddDays(1));

        TotalCount = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
        CurrentPage = Math.Clamp(CurrentPage, 1, Math.Max(1, TotalPages));

        Logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        Actions = new SelectList(Enum.GetValues<AuditAction>());
        var users = await _db.Users.OrderBy(u => u.FirstName).ToListAsync();
        Users = new SelectList(users, "Id", "FullName");
    }
}