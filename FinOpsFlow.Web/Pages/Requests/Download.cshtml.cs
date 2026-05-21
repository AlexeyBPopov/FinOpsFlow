using FinOpsFlow.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinOpsFlow.Web.Pages.Requests;

[Authorize]
public class DownloadModel : PageModel
{
    private readonly IAttachmentService _attachmentService;
    private readonly IWebHostEnvironment _env;

    public DownloadModel(IAttachmentService attachmentService, IWebHostEnvironment env)
    {
        _attachmentService = attachmentService;
        _env = env;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attachment = await _attachmentService.GetByIdAsync(id);
        if (attachment is null) return NotFound();

        var fullPath = Path.Combine(
            _env.WebRootPath,
            attachment.StoragePath.Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(fullPath)) return NotFound();

        var stream = System.IO.File.OpenRead(fullPath);
        return File(stream, attachment.ContentType, attachment.FileName);
    }
}