using ECommerce.Entities;
using ECommerce.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Front.ArshaHamrah.Areas.Admin.Pages.Products;

[Authorize(Roles = "Admin,SuperAdmin")]
public class DetailModel : PageModel
{
    private readonly IProductService _productService;


    public DetailModel(IProductService productService)
    {
        _productService = productService;
    }

    [BindProperty] public Product Product { get; set; }
    [BindProperty] public IFormFile Upload { get; set; }

    public async Task<IActionResult> OnGet(int id)
    {
        var result = await _productService.GetById(id);
        if (result.Code == 0)
        {
            Product = result.ReturnData;
            return Page();
        }

        return RedirectToPage("/Products/Index",
            new { area = "Admin", message = result.Message, code = result.Code.ToString() });
    }
}