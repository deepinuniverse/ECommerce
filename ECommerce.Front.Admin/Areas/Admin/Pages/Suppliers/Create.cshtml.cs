﻿using ECommerce.Services.IServices;

namespace ECommerce.Front.Admin.Areas.Admin.Pages.Suppliers;

public class CreateModel(ISupplierService supplierService) : PageModel
{
    [BindProperty] public Supplier Supplier { get; set; }

    [TempData] public string Message { get; set; }

    [TempData] public string Code { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid)
        {
            var result = await supplierService.Add(Supplier);
            if (result.Code == 0)
                return RedirectToPage("/Suppliers/Index",
                    new { area = "Admin", message = result.Message, code = result.Code.ToString() });
            Message = result.Message;
            Code = result.Code.ToString();
            ModelState.AddModelError("", result.Message);
        }

        return Page();
    }
}