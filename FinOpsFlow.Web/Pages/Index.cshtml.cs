using System.Security.Claims;
using FinOpsFlow.Core.DTOs;
using FinOpsFlow.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinOpsFlow.Web.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IDashboardService _dashboardService;

    public IndexModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public DashboardDto Dashboard { get; set; } = null!;

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isManager = User.IsInRole("Admin") || User.IsInRole("Manager");
        Dashboard = await _dashboardService.GetAsync(userId, isManager);
    }
}