using Entities.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.IServices;

namespace ArshaHamrah.Areas.Admin.Pages.Products;

public class IndexModel : PageModel
{
    private readonly IProductService _productService;

    public IndexModel(IProductService productService)
    {
        _productService = productService;
    }

    public PaginationViewModel PaginationViewModel { get; set; }

    [TempData] public string Message { get; set; }

    [TempData] public string Code { get; set; }

    public async Task OnGet(string search = "", int pageIndex = 1, int quantityPerPage = 10, string message = null,
        string code = null)
    {
        Message = message;
        Code = code;
        var result = await _productService.Search(search, pageIndex, quantityPerPage);
        PaginationViewModel = result.ReturnData;
    }
}