using ECommerce.Services.IServices;

namespace ECommerce.Front.Admin.Areas.Admin.Pages.Users;

public class PurchaseModel(IPurchaseOrderService purchaseOrderService, IUserService userService)
    : PageModel
{
    [BindProperty] public ServiceResult<List<PurchaseListViewModel>> PurchaseOrders { get; set; } = new();
    [BindProperty] public decimal? MaximumAmount { get; set; } = null;
    [BindProperty] public decimal? MinimumAmount { get; set; } = null;
    [BindProperty] public int? IsPaid { get; set; } = null;
    [BindProperty] public string Username { get; set; } = "";
    [BindProperty] public int UserId { get; set; }
    [BindProperty] public Status? Status { get; set; } = null;
    [BindProperty] public PurchaseSort PurchaseSort { get; set; } = PurchaseSort.HighToLowDateBuying;

    [BindProperty] public ServiceResult<List<UserListViewModel>>? Users { get; set; }

    [TempData] public string Message { get; set; }
    [TempData] public string Code { get; set; }

    public async Task<IActionResult> OnGet(int userId = 0, string userName = "", string search = "", int pageNumber = 1,
        int pageSize = 10,
        string message = null, string code = null, bool? isPaid = null, decimal? minimumAmount = null,
        decimal? maximumAmount = null,
        Status status = Domain.Entities.Status.New, PurchaseSort purchaseSort = PurchaseSort.HighToLowDateBuying)
    {
        Message = message;
        Code = code;
        /*  Users = await _userService.UserList(pageSize: 200); */ // It should be corrected in the next task 
        var result = await purchaseOrderService.PurchaseList(userId, userName, pageNumber, pageSize,
            isPaied: isPaid, maximumAmount: maximumAmount, minimumAmount: minimumAmount, statusId: (int)status,
            purchaseSort: (int)purchaseSort);
        if (result.Code == ServiceCode.Success)
        {
            Message = result.Message;
            Code = result.Code.ToString();
            PurchaseOrders = result;
            PurchaseOrders.PaginationDetails.UserId = userId;
            PurchaseOrders.PaginationDetails.Username = userName;
            PurchaseOrders.PaginationDetails.Search = search;
            PurchaseOrders.PaginationDetails.IsPaid = isPaid;
            PurchaseOrders.PaginationDetails.MinPrice = minimumAmount;
            PurchaseOrders.PaginationDetails.MaxPrice = maximumAmount;
            PurchaseOrders.PaginationDetails.Status = status;
            PurchaseOrders.PaginationDetails.PurchaseSort = purchaseSort;
            PurchaseOrders.PaginationDetails.PageSize = pageSize;
            return Page();
        }

        return RedirectToPage("/index", new { message = result.Message, code = result.Code.ToString() });
    }

    public async Task<IActionResult> OnPost(ServiceResult<List<PurchaseListViewModel>> PurchaseOrders)
    {
        try
        {
            return RedirectToPage("/Users/Purchases",
                new
                {
                    area = "Admin",
                    isPaid = PurchaseOrders.PaginationDetails.IsPaid,
                    minimumAmount = PurchaseOrders.PaginationDetails.MinPrice,
                    maximumAmount = PurchaseOrders.PaginationDetails.MaxPrice,
                    status = PurchaseOrders.PaginationDetails.Status,
                    purchaseSort = PurchaseOrders.PaginationDetails.PurchaseSort,
                    userName = PurchaseOrders.PaginationDetails.Username,
                    userId = PurchaseOrders.PaginationDetails.UserId,
                    pageSize = PurchaseOrders.PaginationDetails.PageSize
                });
        }
        catch (Exception ex)
        {
            return Page();
        }
    }

    public async Task<JsonResult> OnGetEditPurchaseStatus(int id, Status status)
    {
        var result = await purchaseOrderService.SetStatusById(id, status);
        return new JsonResult(result);
    }

    public async Task<JsonResult> OnGetUserListBySearch(string search)
    {
        var result = await userService.UserList(search);
        List<string> users = result.ReturnData.Select(x => x.Username).ToList();
        return new JsonResult(users);
    }
}
