﻿using ECommerce.Services.IServices;

namespace ECommerce.Front.Admin.Areas.Admin.Pages.Discounts;

public class EditModel(IDiscountService discountService) : PageModel
{
    [BindProperty] public Discount Discount { get; set; }
    [BindProperty] public bool WithPrice { get; set; }
    [TempData] public string Message { get; set; }
    [TempData] public string Code { get; set; }

    public async Task OnGet(int id)
    {
       await Initial(id);
    }

    private async Task Initial(int id)
    {
        var result = await discountService.GetById(id);
        Discount = result.ReturnData;
        if (!Discount.Amount.HasValue) WithPrice = false;
        else if (!Discount.Percent.HasValue) WithPrice = true;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!Discount.StartDate.HasValue) ModelState.AddModelError(string.Empty, "تاریخ شروع نباید خالی باشد.");
        if (!Discount.EndDate.HasValue) ModelState.AddModelError(string.Empty, "تاریخ پایان نباید خالی باشد.");
        if (Discount.StartDate > Discount.EndDate)
            ModelState.AddModelError(string.Empty, "تاریخ پایان نباید قبل از تاریخ شروع باشد.");
        if (Discount.MinOrder > Discount.MaxOrder)
            ModelState.AddModelError(string.Empty, "حداقل تعداد سفارش باید کم تر از حداکثر آن باشد.");
       
        if (ModelState.IsValid)
        {
            if (WithPrice) Discount.Percent = null;
            else Discount.Amount = null;
            var result = await discountService.Edit(Discount);
            Message = result.Message;
            Code = result.Code.ToString();
            if (result.Code == 0)
                return RedirectToPage("/Discounts/Index",
                    new { area = "Admin", message = result.Message, code = result.Code.ToString() });
            Message = result.Message;
            Code = result.Code.ToString();
            ModelState.AddModelError("", result.Message);
        }
        await Initial(Discount.Id);
        return Page();
    }
}