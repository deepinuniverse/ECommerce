﻿using ECommerce.Application.ViewModels;

namespace ECommerce.Services.Services;

public class SlideShowService(IHttpService http) : EntityService<SlideShowViewModel>(http), ISlideShowService
{
    private const string Url = "api/SlideShows";
    private List<SlideShowViewModel> _slideShows;

    public async Task<ServiceResult<List<SlideShowViewModel>>> Load(int pageNumber = 1, int pageSize = 10)
    {
        var result = await ReadList(Url, $"Get?PageNumber={pageNumber}&PageSize={pageSize}");
        return Return(result);
    }

    public async Task<ServiceResult<List<SlideShowViewModel>>> TopSlideShow(int top)
    {
        if (_slideShows == null)
        {
            var slideShows = await Load(1, 5);
            if (slideShows.Code > 0) return slideShows;
            _slideShows = slideShows.ReturnData;
        }


        var result = _slideShows.OrderBy(x => x.DisplayOrder).Take(top).ToList();
        if (result.Count == 0)
            return new ServiceResult<List<SlideShowViewModel>>
                { Code = ServiceCode.Info, Message = "اسلایدشویی یافت نشد" };
        return new ServiceResult<List<SlideShowViewModel>>
        {
            Code = ServiceCode.Success,
            ReturnData = result
        };
    }

    public async Task<ServiceResult<List<SlideShowViewModel>>> Filtering(string filter)
    {
        if (_slideShows == null)
        {
            var slideShows = await Load();
            if (slideShows.Code > 0) return slideShows;
            _slideShows = slideShows.ReturnData;
        }

        var result = _slideShows.Where(x => x.Title.Contains(filter)).ToList();
        if (result.Count == 0)
            return new ServiceResult<List<SlideShowViewModel>>
                { Code = ServiceCode.Info, Message = "اسلایدشویی یافت نشد" };
        return new ServiceResult<List<SlideShowViewModel>>
        {
            Code = ServiceCode.Success,
            ReturnData = result
        };
    }

    public async Task<ServiceResult> Add(SlideShowViewModel slideShowViewModel)
    {
        var result = await Create(Url, slideShowViewModel);
        _slideShows = null;
        return Return(result);
    }

    public async Task<ServiceResult> Edit(SlideShowViewModel slideShowViewModel)
    {
        var result = await Update(Url, slideShowViewModel);
        _slideShows = null;
        return Return(result);
    }

    public async Task<ServiceResult> Delete(int id)
    {
        //var result = await Delete(Url, id);
        //_slideShows = null;
        //return Return(result);
        var result = await http.DeleteAsync(Url, id);
        _slideShows = null;
        if (result.Code == ResultCode.Success)
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "با موفقیت حذف شد"
            };
        return new ServiceResult
            { Code = ServiceCode.Error, Message = "به علت وابستگی با عناصر دیگر امکان حذف وجود ندارد" };
    }

    public async Task<ServiceResult<SlideShowViewModel>> GetById(int id)
    {
        var result = await http.GetAsync<SlideShowViewModel>(Url, $"GetById?id={id}");
        return Return(result);
    }
}