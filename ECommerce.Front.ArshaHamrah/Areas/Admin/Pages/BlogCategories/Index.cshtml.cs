using ECommerce.Entities;
using ECommerce.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Front.ArshaHamrah.Areas.Admin.Pages.BlogCategories;

[Authorize(Roles = "Admin,SuperAdmin")]
public class IndexModel : PageModel
{
    private readonly IBlogCategoryService _blogCategoryService;

    public IndexModel(IBlogCategoryService blogCategoryService)
    {
        _blogCategoryService = blogCategoryService;
    }

    public ServiceResult<List<BlogCategory>> BlogCategories { get; set; }

    [TempData] public string Message { get; set; }

    [TempData] public string Code { get; set; }

    public async Task<IActionResult> OnGet(string search = "", int pageNumber = 1, int pageSize = 10,
        string message = null, string code = null)
    {
        Message = message;
        Code = code;
        var result = await _blogCategoryService.GetAll(search, pageNumber, pageSize);
        if (result.Code == ServiceCode.Success)
        {
            Message = result.Message;
            Code = result.Code.ToString();
            BlogCategories = result;
            return Page();
        }

        return RedirectToPage("/index", new { message = result.Message, code = result.Code.ToString() });
    }
}