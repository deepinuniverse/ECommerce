using ECommerce.Services.IServices;

namespace ECommerce.Front.Admin.Areas.Admin.Pages.Currencies;

public class IndexModel(ICurrencyService currencyService) : PageModel
{
    public ServiceResult<List<Currency>> Currencies { get; set; }

    [TempData] public string Message { get; set; }

    [TempData] public string Code { get; set; }

    public async Task<IActionResult> OnGet(string search = "", int pageNumber = 1, int pageSize = 10,
        string message = null, string code = null)
    {
        Message = message;
        Code = code;
        var result = await currencyService.Load(search, pageNumber, pageSize);
        if (result.Code == ServiceCode.Success)
        {
            result.PaginationDetails.Address = "/Currencies/Index";
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

            Currencies = result;
            return Page();
        }

        return RedirectToPage("/index", new { message = result.Message, code = result.Code.ToString() });
    }
}