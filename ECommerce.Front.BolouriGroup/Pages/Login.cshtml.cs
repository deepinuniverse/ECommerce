using Ecommerce.Entities.ViewModel;
using ECommerce.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Front.BolouriGroup.Pages;

public class LoginModel : PageModel
{
    private readonly IUserService _userService;

    public LoginModel(IUserService userService)
    {
        _userService = userService;
    }

    [BindProperty] public RegisterViewModel RegisterViewModel { get; set; }

    [BindProperty] public LoginViewModel LoginViewModel { get; set; }

    [TempData] public string Message { get; set; }

    [TempData] public string Code { get; set; }

    [TempData] public string ReturnUrl { get; set; }

    public void OnGet(string returnUrl = null)
    {
        if (string.IsNullOrEmpty(returnUrl)) returnUrl = "/";
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostSubmit()
    {
        var s = TempData["ReturnUrl"];
        //ReturnUrl = "/Shop/Coffee/shop/equipment/Hot.bar/Coffee.makers";
        var result = await _userService.Login(LoginViewModel);
        if (result.Code == 0)
            return RedirectToPage(ReturnUrl == "/" ? "/Index" : ReturnUrl);

        Message = result.Message;
        Code = result.Code.ToString();

        return Page();
    }

    public async Task<IActionResult> OnPostRegister()
    {
        if (!ModelState.IsValid) return Page();
        var result = await _userService.Register(RegisterViewModel);
        Message = result.Message;
        Code = result.Code.ToString();
        if (result.Code == 0) return RedirectToPage("Index");
        return Page();
    }

    public async Task<IActionResult> OnPostSendSms()
    {        
        Random randomCode = new Random();
        int code = randomCode.Next(100000000);
        if (code < 10000000) code = code + 10000000;
        var result = await _userService.SetConfirmCodeByUsername(LoginViewModel.Username, code, DateTime.Now);
        if (result.Status==200)
        {
            await _userService.SendAuthenticationSms(LoginViewModel.Username, code);
        }
        else
        {

        }       
        return null;
    }
}