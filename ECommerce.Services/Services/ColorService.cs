﻿namespace ECommerce.Services.Services;

public class ColorService(IHttpService http) : EntityService<Color>(http), IColorService
{
    private const string Url = "api/Colors";
    private List<Color> _colors;

    public async Task<ServiceResult<List<Color>>> Load()
    {
        var result = await ReadList(Url, "GetAll");
        return Return(result);
    }

    public async Task<ServiceResult<List<Color>>> GetAll(string search = "", int pageNumber = 0, int pageSize = 10)
    {
        var result = await ReadList(Url,
            $"GetAllWithPagination?PageNumber={pageNumber}&Search={search}&PageSize={pageSize}");
        return Return(result);
    }

    public async Task<ServiceResult<List<Color>>> Filtering(string filter)
    {
        if (_colors == null)
        {
            var colors = await Load();
            if (colors.Code > 0) return colors;
            _colors = colors.ReturnData;
        }

        var result = _colors.Where(x => x.Name.Contains(filter)).ToList();
        if (result.Count == 0)
            return new ServiceResult<List<Color>> { Code = ServiceCode.Info, Message = "رنگ یافت نشد" };
        return new ServiceResult<List<Color>>
        {
            Code = ServiceCode.Success,
            ReturnData = result
        };
    }

    public async Task<ServiceResult> Add(Color color)
    {
        var result = await Create(Url, color);
        _colors = null;
        return Return(result);
    }

    public async Task<ServiceResult> Edit(Color color)
    {
        var result = await Update(Url, color);
        _colors = null;
        return Return(result);
    }

    public async Task<ServiceResult> Delete(int id)
    {
        //var result = await Delete(Url, id);
        //_colors = null;
        //return Return(result);
        var result = await http.DeleteAsync(Url, id);
        if (result.Code == ResultCode.Success)
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "با موفقیت حذف شد"
            };
        return new ServiceResult
            { Code = ServiceCode.Error, Message = "به علت وابستگی با عناصر دیگر امکان حذف وجود ندارد" };
    }

    public async Task<ServiceResult<Color>> GetById(int id)
    {
        var result = await http.GetAsync<Color>(Url, $"GetById?id={id}");
        return Return(result);
    }
}