using ECommerce.Entities;
using ECommerce.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Front.ArshaHamrah.Areas.Admin.Pages.PaymentMethods;

[Authorize(Roles = "Admin,SuperAdmin")]
public class IndexModel : PageModel
{
    private readonly IPaymentMethodService _paymentMethodService;

    public IndexModel(IPaymentMethodService paymentMethodService)
    {
        _paymentMethodService = paymentMethodService;
    }

    public ServiceResult<List<PaymentMethod>> PaymentMethods { get; set; }

    [TempData] public string Message { get; set; }

    [TempData] public string Code { get; set; }

    public async Task<IActionResult> OnGet(string search = "", int pageNumber = 1, int pageSize = 10,
        string message = null, string code = null)
    {
        Message = message;
        Code = code;
        var result = await _paymentMethodService.Load(search, pageNumber, pageSize);
        if (result.Code == ServiceCode.Success)
        {
            Message = result.Message;
            Code = result.Code.ToString();
            PaymentMethods = result;
            return Page();
        }

        return RedirectToPage("/index", new { message = result.Message, code = result.Code.ToString() });
    }
}