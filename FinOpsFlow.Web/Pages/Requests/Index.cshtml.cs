using FinOpsFlow.Core.DTOs;
using FinOpsFlow.Core.Entities;
using FinOpsFlow.Core.Enums;
using FinOpsFlow.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinOpsFlow.Web.Pages.Requests;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IRequestService _requestService;

    public IndexModel(IRequestService requestService)
    {
        _requestService = requestService;
    }

    public List<Request> Requests { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public RequestStatus? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public RequestPriority? PriorityFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    public async Task OnGetAsync()
    {
        var filter = new RequestFilterDto(StatusFilter, PriorityFilter, Keyword);
        Requests = await _requestService.GetAllAsync(filter);
    }
}