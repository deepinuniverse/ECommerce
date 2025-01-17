﻿using ECommerce.Services.IServices;

namespace ECommerce.Front.Admin.Areas.Admin.Pages.Sizes;

public class CreateModel(ISizeService sizService) : PageModel
{
    [BindProperty] public Size Size { get; set; }


    [TempData] public string Message { get; set; }

    [TempData] public string Code { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid)
        {
            var result = await sizService.Add(Size);
            if (result.Code == 0)
                return RedirectToPage("/Sizes/Index",
                    new { area = "Admin", message = result.Message, code = result.Code.ToString() });
            Message = result.Message;
            Code = result.Code.ToString();
            ModelState.AddModelError("", result.Message);
        }

        return Page();
    }
}