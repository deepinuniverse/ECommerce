using ECommerce.Entities.ViewModel;
using ECommerce.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Front.ArshaHamrah.Areas.Admin.Pages.Users;

[Authorize(Roles = "Admin,SuperAdmin")]
public class IndexModel : PageModel
{
    private readonly IUserService _userService;
    public ServiceResult<List<UserListViewModel>> Users;

    public IndexModel(IUserService userService)
    {
        _userService = userService;
    }

    public string Message { get; set; }

    public string Code { get; set; }

    public async Task OnGet(string search = "", int pageNumber = 0, int pageSize = 10, string message = null,
        string code = null, int userSort = 1, bool? isActive = null, bool? isColleague = null, bool? HasBuying = null)
    {
        Message = message;
        Code = code;
        Users = await _userService.UserList(search, pageNumber, pageSize, userSort, isActive, isColleague, HasBuying);
    }
}