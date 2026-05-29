using FinOpsFlow.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinOpsFlow.Web.Pages.Requests;

[Authorize]
public class DownloadModel : PageModel
{
    private readonly IAttachmentService _attachmentService;
    private readonly IFileStorageService _fileStorage;

    public DownloadModel(IAttachmentService attachmentService, IFileStorageService fileStorage)
    {
        _attachmentService = attachmentService;
        _fileStorage = fileStorage;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attachment = await _attachmentService.GetByIdAsync(id);
        if (attachment is null) return NotFound();

        var stream = await _fileStorage.GetStreamAsync(attachment.StoragePath);
        return File(stream, attachment.ContentType, attachment.FileName);
    }
}