﻿namespace ECommerce.Services.Services;

public class SizeService(IHttpService http) : EntityService<Size>(http), ISizeService
{
    private const string Url = "api/Sizes";
    private List<Size> _sizes;


    public async Task<ServiceResult<List<Size>>> Load(string search = "", int pageNumber = 0, int pageSize = 10)
    {
        var result = await ReadList(Url, $"Get?PageNumber={pageNumber}&PageSize={pageSize}&Search={search}");
        return Return(result);
    }

    public async Task<ServiceResult<List<Size>>> Filtering(string filter)
    {
        if (_sizes == null)
        {
            var sizes = await Load();
            if (sizes.Code > 0) return sizes;
            _sizes = sizes.ReturnData;
        }

        var result = _sizes.Where(x => x.Name.Contains(filter)).ToList();
        if (result.Count == 0)
            return new ServiceResult<List<Size>> { Code = ServiceCode.Info, Message = "سایزی یافت نشد" };
        return new ServiceResult<List<Size>>
        {
            Code = ServiceCode.Success,
            ReturnData = result
        };
    }

    public async Task<ServiceResult> Add(Size size)
    {
        var result = await Create(Url, size);
        _sizes = null;
        return Return(result);
    }

    public async Task<ServiceResult> Edit(Size size)
    {
        var result = await Update(Url, size);
        _sizes = null;
        return Return(result);
    }

    public async Task<ServiceResult> Delete(int id)
    {
        //var result = await Delete(Url, id);
        //_sizes = null;
        //return Return(result);
        var result = await http.DeleteAsync(Url, id);
        if (result.Code == ResultCode.Success)
        {
            _sizes = null;
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "با موفقیت حذف شد"
            };
        }

        _sizes = null;
        return new ServiceResult
            { Code = ServiceCode.Error, Message = "به علت وابستگی با عناصر دیگر امکان حذف وجود ندارد" };
    }

    public async Task<ServiceResult<Size>> GetById(int id)
    {
        var result = await http.GetAsync<Size>(Url, $"GetById?id={id}");
        return Return(result);
    }
}