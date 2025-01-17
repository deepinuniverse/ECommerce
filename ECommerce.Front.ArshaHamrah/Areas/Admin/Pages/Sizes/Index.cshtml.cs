using ECommerce.Entities;
using ECommerce.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Front.ArshaHamrah.Areas.Admin.Pages.Sizes;

[Authorize(Roles = "Admin,SuperAdmin")]
public class IndexModel : PageModel
{
    private readonly ISizeService _sizService;

    public IndexModel(ISizeService sizeService)
    {
        _sizService = sizeService;
    }

    public ServiceResult<List<Size>> Sizes { get; set; }

    [TempData] public string Message { get; set; }

    [TempData] public string Code { get; set; }

    public async Task<IActionResult> OnGet(string search = "", int pageNumber = 1, int pageSize = 10,
        string message = null, string code = null)
    {
        Message = message;
        Code = code;
        var result = await _sizService.Load(search, pageNumber, pageSize);
        if (result.Code == ServiceCode.Success)
        {
            Message = result.Message;
            Code = result.Code.ToString();
            Sizes = result;
            return Page();
        }

        return RedirectToPage("/index", new { message = result.Message, code = result.Code.ToString() });
    }
}