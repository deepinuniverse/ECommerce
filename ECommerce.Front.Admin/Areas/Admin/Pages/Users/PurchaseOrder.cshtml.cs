using ECommerce.Services.IServices;

namespace ECommerce.Front.Admin.Areas.Admin.Pages.Users;

public class PurchaseOrderModel(IPurchaseOrderService purchaseOrderService) : PageModel
{
    [BindProperty] public PurchaseOrder PurchaseOrder { get; set; }
    [TempData] public string Message { get; set; }
    [TempData] public string Code { get; set; }

    public async Task<IActionResult> OnGet(int id, string message = null, string code = null)
    {
        Message = message;
        Code = code;
        var result = await purchaseOrderService.GetPurchaseOrderWithIncludeById(id);
        if (result.Code == ServiceCode.Success)
        {
            Message = result.Message;
            Code = result.Code.ToString();
            PurchaseOrder = result.ReturnData;
            return Page();
        }

        return RedirectToPage("/index", new { message = result.Message, code = result.Code.ToString() });
    }
}
