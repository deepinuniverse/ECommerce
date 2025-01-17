﻿using ECommerce.Entities;
using ECommerce.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Front.ArshaHamrah.Areas.Admin.Pages.Colors;

[Authorize(Roles = "Admin,SuperAdmin")]
public class EditModel : PageModel
{
    private readonly IColorService _colorService;

    public EditModel(IColorService colorService)
    {
        _colorService = colorService;
    }

    [BindProperty] public Color Color { get; set; }
    [TempData] public string Message { get; set; }
    [TempData] public string Code { get; set; }

    public async Task OnGet(int id)
    {
        var result = await _colorService.GetById(id);
        Color = result.ReturnData;
    }

    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid)
        {
            var result = await _colorService.Edit(Color);
            Message = result.Message;
            Code = result.Code.ToString();
            if (result.Code == 0)
                return RedirectToPage("/Colors/Index",
                    new { area = "Admin", message = result.Message, code = result.Code.ToString() });
            Message = result.Message;
            Code = result.Code.ToString();
            ModelState.AddModelError("", result.Message);
        }

        return Page();
    }
}