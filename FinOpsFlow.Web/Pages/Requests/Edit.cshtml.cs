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

[Authorize(Roles = "Admin,Manager")]
public class EditModel : PageModel
{
    private readonly IRequestService _requestService;
    private readonly ApplicationDbContext _db;

    public EditModel(IRequestService requestService, ApplicationDbContext db)
    {
        _requestService = requestService;
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Categories { get; set; } = null!;
    public SelectList Users { get; set; } = null!;
    public int RequestId { get; set; }

    public class InputModel
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        public RequestPriority Priority { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        public string? AssignedToId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var request = await _requestService.GetByIdAsync(id);
        if (request is null) return NotFound();

        RequestId = id;
        Input = new InputModel
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            CategoryId = request.CategoryId,
            DueDate = request.DueDate,
            AssignedToId = request.AssignedToId
        };

        await LoadSelectListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            RequestId = id;
            await LoadSelectListsAsync();
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var dto = new UpdateRequestDto(
            Input.Title,
            Input.Description,
            Input.Priority,
            Input.CategoryId,
            Input.DueDate,
            Input.AssignedToId
        );

        await _requestService.UpdateAsync(id, dto, userId);
        TempData["Success"] = "Request updated.";
        return RedirectToPage("./Details", new { id });
    }

    private async Task LoadSelectListsAsync()
    {
        var cats = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        Categories = new SelectList(cats, "Id", "Name");

        var users = await _db.Users.OrderBy(u => u.FirstName).ToListAsync();
        Users = new SelectList(users, "Id", "FullName");
    }
}