﻿using ECommerce.Services.IServices;

namespace ECommerce.Front.BolouriGroup.ViewComponents;

public class BlogCategoriesMenuViewComponent(IBlogCategoryService categoryService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var result = (await categoryService.GetParents()).ReturnData;
        return View(result);
    }
}