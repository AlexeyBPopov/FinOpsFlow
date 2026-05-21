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
    private readonly ICommentService _commentService;
    private readonly IAttachmentService _attachmentService;
    private readonly IFileStorageService _fileStorage;
    private readonly ApplicationDbContext _db;

    private static readonly string[] AllowedExtensions =
        [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".txt", ".csv"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public DetailsModel(
        IRequestService requestService,
        ICommentService commentService,
        IAttachmentService attachmentService,
        IFileStorageService fileStorage,
        ApplicationDbContext db)
    {
        _requestService = requestService;
        _commentService = commentService;
        _attachmentService = attachmentService;
        _fileStorage = fileStorage;
        _db = db;
    }

    public Request RequestDetail { get; set; } = null!;
    public SelectList Statuses { get; set; } = null!;
    public SelectList Users { get; set; } = null!;

    [BindProperty] public RequestStatus NewStatus { get; set; }
    [BindProperty] public string? AssignedToId { get; set; }
    [BindProperty] public string CommentBody { get; set; } = string.Empty;
    [BindProperty] public IFormFile? UploadedFile { get; set; }

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

    public async Task<IActionResult> OnPostAddCommentAsync(int id)
    {
        if (string.IsNullOrWhiteSpace(CommentBody))
        {
            TempData["Error"] = "Comment cannot be empty.";
            return RedirectToPage(new { id });
        }

        if (CommentBody.Length > 2000)
        {
            TempData["Error"] = "Comment cannot exceed 2000 characters.";
            return RedirectToPage(new { id });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _commentService.AddAsync(id, userId, CommentBody.Trim());
        TempData["Success"] = "Comment added.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostUploadFileAsync(int id)
    {
        if (UploadedFile is null || UploadedFile.Length == 0)
        {
            TempData["Error"] = "Please select a file.";
            return RedirectToPage(new { id });
        }

        if (UploadedFile.Length > MaxFileSizeBytes)
        {
            TempData["Error"] = "File size cannot exceed 10MB.";
            return RedirectToPage(new { id });
        }

        var ext = Path.GetExtension(UploadedFile.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            TempData["Error"] = $"File type '{ext}' is not allowed.";
            return RedirectToPage(new { id });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        using var stream = UploadedFile.OpenReadStream();
        var storagePath = await _fileStorage.SaveAsync(stream, UploadedFile.FileName, $"requests/{id}");

        await _attachmentService.SaveMetadataAsync(
            requestId: id,
            uploadedById: userId,
            fileName: UploadedFile.FileName,
            storedFileName: Path.GetFileName(storagePath),
            contentType: UploadedFile.ContentType,
            fileSizeBytes: UploadedFile.Length,
            storagePath: storagePath);

        TempData["Success"] = "File uploaded.";
        return RedirectToPage(new { id });
    }

    private async Task LoadSelectListsAsync()
    {
        Statuses = new SelectList(Enum.GetValues<RequestStatus>());
        var users = await _db.Users.OrderBy(u => u.FirstName).ToListAsync();
        Users = new SelectList(users, "Id", "FullName");
    }
}