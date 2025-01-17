using ECommerce.Entities;
using ECommerce.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Front.ArshaHamrah.Areas.Admin.Pages.Tags;

[Authorize(Roles = "Admin,SuperAdmin")]
public class DeleteModel : PageModel
{
    private readonly ITagService _tagService;

    public DeleteModel(ITagService tagService)
    {
        _tagService = tagService;
    }

    public Tag Tag { get; set; }
    [TempData] public string Message { get; set; }

    [TempData] public string Code { get; set; }

    public async Task<IActionResult> OnGet(int id)
    {
        var result = await _tagService.GetById(id);
        if (result.Code == 0)
        {
            Tag = result.ReturnData;
            return Page();
        }

        return RedirectToPage("/Tags/Index",
            new { area = "Admin", message = result.Message, code = result.Code.ToString() });
    }

    public async Task<IActionResult> OnPost(int id)
    {
        if (ModelState.IsValid)
        {
            var result = await _tagService.Delete(id);
            Message = result.Message;
            Code = result.Code.ToString();
            if (result.Code == 0)
                return RedirectToPage("/Tags/Index",
                    new { area = "Admin", message = result.Message, code = result.Code.ToString() });
            Message = result.Message;
            Code = result.Code.ToString();
        }

        return Page();
    }
}