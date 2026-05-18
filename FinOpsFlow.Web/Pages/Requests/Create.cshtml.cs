using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FinOpsFlow.Core.DTOs;
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
public class CreateModel : PageModel
{
    private readonly IRequestService _requestService;
    private readonly ApplicationDbContext _db;

    public CreateModel(IRequestService requestService, ApplicationDbContext db)
    {
        _requestService = requestService;
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Categories { get; set; } = null!;

    public class InputModel
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        public RequestPriority Priority { get; set; } = RequestPriority.Medium;

        [Required(ErrorMessage = "Please select a category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var dto = new CreateRequestDto(
            Input.Title,
            Input.Description,
            Input.Priority,
            Input.CategoryId,
            Input.DueDate
        );

        var request = await _requestService.CreateAsync(dto, userId);
        TempData["Success"] = "Request created successfully.";
        return RedirectToPage("./Details", new { id = request.Id });
    }

    private async Task LoadCategoriesAsync()
    {
        var cats = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        Categories = new SelectList(cats, "Id", "Name");
    }
}