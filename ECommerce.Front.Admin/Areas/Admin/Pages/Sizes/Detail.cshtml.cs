using ECommerce.Services.IServices;

namespace ECommerce.Front.Admin.Areas.Admin.Pages.Sizes;

public class DetailModel(ISizeService sizeService) : PageModel
{
    public Size Size { get; set; }

    public async Task<IActionResult> OnGet(int id)
    {
        var result = await sizeService.GetById(id);
        if (result.Code == 0)
        {
            Size = result.ReturnData;
            return Page();
        }

        return RedirectToPage("/Sizes/Index",
            new { area = "Admin", message = result.Message, code = result.Code.ToString() });
    }
}