﻿using ECommerce.Application.ViewModels;

namespace ECommerce.Services.IServices;

public interface ISlideShowService : IEntityService<SlideShowViewModel>
{
    Task<ServiceResult<List<SlideShowViewModel>>> Filtering(string filter);
    Task<ServiceResult<List<SlideShowViewModel>>> Load(int pageNumber = 1, int pageSize = 10);
    Task<ServiceResult> Add(SlideShowViewModel slideShowViewModel);
    Task<ServiceResult> Edit(SlideShowViewModel slideShowViewModel);
    Task<ServiceResult> Delete(int id);
    Task<ServiceResult<SlideShowViewModel>> GetById(int id);
    Task<ServiceResult<List<SlideShowViewModel>>> TopSlideShow(int top);
}