using System.Security.Claims;
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
public class DetailsModel : PageModel
{
    private readonly IRequestService _requestService;
    private readonly ApplicationDbContext _db;

    public DetailsModel(IRequestService requestService, ApplicationDbContext db)
    {
        _requestService = requestService;
        _db = db;
    }

    public Request RequestDetail { get; set; } = null!;
    public SelectList Statuses { get; set; } = null!;
    public SelectList Users { get; set; } = null!;

    [BindProperty]
    public RequestStatus NewStatus { get; set; }

    [BindProperty]
    public string? AssignedToId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var request = await _requestService.GetByIdAsync(id);
        if (request is null) return NotFound();

        RequestDetail = request;
        NewStatus = request.Status;
        AssignedToId = request.AssignedToId;
        await LoadSelectListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostChangeStatusAsync(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _requestService.ChangeStatusAsync(id, NewStatus, userId);
        TempData["Success"] = "Status updated.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAssignAsync(int id)
    {
        var request = await _db.Requests.FindAsync(id);
        if (request is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        request.AssignedToId = string.IsNullOrEmpty(AssignedToId) ? null : AssignedToId;
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Assignee updated.";
        return RedirectToPage(new { id });
    }

    private async Task LoadSelectListsAsync()
    {
        Statuses = new SelectList(Enum.GetValues<RequestStatus>());
        var users = await _db.Users.OrderBy(u => u.FirstName).ToListAsync();
        Users = new SelectList(users, "Id", "FullName");
    }
}