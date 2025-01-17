﻿using ECommerce.Domain.Entities.HolooEntity;
using ECommerce.Services.IServices;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Front.Admin.Areas.Admin.Pages.Units;

public class CreateModel(IUnitService unitService, IHolooUnitService holooUnitService)
    : PageModel
{
    [BindProperty] public Unit Unit { get; set; }
    public SelectList HolooUnits { get; set; }
    [TempData] public string Message { get; set; }

    [TempData] public string Code { get; set; }

    public async Task OnGet()
    {
        var result = await holooUnitService.Load();
        HolooUnits = new SelectList(result.ReturnData, nameof(HolooUnit.Unit_Code), nameof(HolooUnit.Unit_Name));
    }

    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid)
        {
            var result = await unitService.Add(Unit);
            if (result.Code == 0)
                return RedirectToPage("/Units/Index",
                    new { area = "Admin", message = result.Message, code = result.Code.ToString() });
            Message = result.Message;
            Code = result.Code.ToString();
            ModelState.AddModelError("", result.Message);
        }

        return Page();
    }
}