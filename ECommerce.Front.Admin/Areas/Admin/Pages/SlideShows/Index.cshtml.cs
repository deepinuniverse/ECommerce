using ECommerce.Services.IServices;

namespace ECommerce.Front.Admin.Areas.Admin.Pages.SlideShows;

public class IndexModel(ISlideShowService slideShowService) : PageModel
{
    public List<SlideShowViewModel> SlideShows { get; set; }

    [TempData] public string Message { get; set; }

    [TempData] public string Code { get; set; }

    public async Task<IActionResult> OnGet(string message = null, string code = null)
    {
        Message = message;
        Code = code;
        var result = await slideShowService.Load();
        if (result.Code == ServiceCode.Success)
        {
            if (Message != null)
            {
                Message = Message;
                Code = Code;
            }
            else
            {
                Message = result.Message;
                Code = result.Code.ToString();
            }

            SlideShows = result.ReturnData;
            return Page();
        }

        return RedirectToPage("/index", new { message = result.Message, code = result.Code.ToString() });
    }
}