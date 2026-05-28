using FinOpsFlow.Core.DTOs;
using FinOpsFlow.Core.Entities;
using FinOpsFlow.Core.Enums;
using FinOpsFlow.Core.Interfaces;
using FinOpsFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FinOpsFlow.Web.Pages.Requests;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IRequestService _requestService;
    private readonly ApplicationDbContext _db;

    public IndexModel(IRequestService requestService, ApplicationDbContext db)
    {
        _requestService = requestService;
        _db = db;
    }

    public List<Request> Requests { get; set; } = [];

    [BindProperty(SupportsGet = true)] public RequestStatus? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public RequestPriority? PriorityFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? Keyword { get; set; }
    [BindProperty(SupportsGet = true)] public string? AssignedToFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryFilter { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? CreatedFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? CreatedTo { get; set; }
    [BindProperty(SupportsGet = true)] public bool OverdueOnly { get; set; }

    public SelectList Categories { get; set; } = null!;
    public SelectList Users { get; set; } = null!;

    public async Task OnGetAsync()
    {
        var filter = new RequestFilterDto(
            Status: StatusFilter,
            Priority: PriorityFilter,
            Keyword: Keyword,
            AssignedToId: AssignedToFilter,
            CategoryId: CategoryFilter,
            CreatedFrom: CreatedFrom,
            CreatedTo: CreatedTo,
            OverdueOnly: OverdueOnly
        );

        Requests = await _requestService.GetAllAsync(filter);

        var cats = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        Categories = new SelectList(cats, "Id", "Name");

        var users = await _db.Users.OrderBy(u => u.FirstName).ToListAsync();
        Users = new SelectList(users, "Id", "FullName");
    }
}