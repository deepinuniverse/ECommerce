﻿using ECommerce.Entities;
using ECommerce.Entities.ViewModel;
using ECommerce.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.Front.ArshaHamrah.Areas.Admin.Pages.SlideShows;

[Authorize(Roles = "Admin,SuperAdmin")]
public class CreateModel : PageModel
{
    private readonly IHostEnvironment _environment;
    private readonly IImageService _imageService;
    private readonly IProductService _productService;
    private readonly ISlideShowService _slideShowService;

    public CreateModel(ISlideShowService slideShowService, IImageService imageService, IHostEnvironment environment,
        IProductService productService)
    {
        _slideShowService = slideShowService;
        _imageService = imageService;
        _environment = environment;
        _productService = productService;
    }

    [BindProperty] public SlideShow SlideShow { get; set; }

    [BindProperty] public IFormFile Upload { get; set; }

    //public PaginationViewModel PaginationViewModel { get; set; }
    public ServiceResult<List<ProductIndexPageViewModel>> Products { get; set; }
    [TempData] public string Message { get; set; }

    [TempData] public string Code { get; set; }

    public async Task OnGet(string search)
    {
        var result = await _productService.Search(search, 1, 30);
        if (result.Code == ServiceCode.Success)
        {
            Message = result.Message;
            Code = result.Code.ToString();
            Products = result;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        var resultProduct = await _productService.Search("", 1, 30);
        if (resultProduct.Code == ServiceCode.Success)
        {
            Message = resultProduct.Message;
            Code = resultProduct.Code.ToString();
            Products = resultProduct;
        }

        if (Upload == null)
        {
            Message = "لطفا عکس را انتخاب کنید";
            Code = ServiceCode.Error.ToString();
            return Page();
        }

        var fileName = (await _imageService.Upload(Upload, "Images/SlideShows", _environment.ContentRootPath))
            .ReturnData;
        SlideShow.ImagePath = $"/{fileName[0]}/{fileName[1]}/{fileName[2]}";
        ModelState.Remove("SlideShow.ImagePath");

        if (ModelState.IsValid)
        {
            var result = await _slideShowService.Add(SlideShow);
            if (result.Code == 0)
                return RedirectToPage("/SlideShows/Index",
                    new { area = "Admin", message = result.Message, code = result.Code.ToString() });
            Message = result.Message;
            Code = result.Code.ToString();
            ModelState.AddModelError("", result.Message);
        }

        return Page();
    }

    public async Task<JsonResult> OnGetReturnProducts(string search = "")
    {
        var result = await _productService.Search(search, 1, 30);
        if (result.Code == ServiceCode.Success)
        {
            Message = result.Message;
            Code = result.Code.ToString();
        }

        return new JsonResult(result.ReturnData);
    }
}