﻿namespace ECommerce.Services.Services;

public class UnitService(IHttpService http) : EntityService<Unit>(http), IUnitService
{
    private const string Url = "api/Units";
    private List<Unit> _units;

    public async Task<ServiceResult<List<Unit>>> Load(string search = "", int pageNumber = 0, int pageSize = 10)
    {
        var result = await ReadList(Url, $"Get?PageNumber={pageNumber}&PageSize={pageSize}&Search={search}");
        return Return(result);
    }

    public async Task<ServiceResult<List<Unit>>> Filtering(string filter)
    {
        if (_units == null)
        {
            var units = await Load();
            if (units.Code > 0) return units;
            _units = units.ReturnData;
        }

        var result = _units.Where(x => x.Name.Contains(filter)).ToList();
        if (result.Count == 0)
            return new ServiceResult<List<Unit>> { Code = ServiceCode.Info, Message = "واحدی یافت نشد" };
        return new ServiceResult<List<Unit>>
        {
            Code = ServiceCode.Success,
            ReturnData = result
        };
    }

    public async Task<ServiceResult> Add(Unit unit)
    {
        var result = await Create(Url, unit);
        _units = null;
        return Return(result);
    }

    public async Task<ServiceResult> Edit(Unit unit)
    {
        var result = await Update(Url, unit);
        _units = null;
        return Return(result);
    }

    public async Task<ServiceResult> Delete(int id)
    {
        //var result = await Delete(Url, id);
        //_units = null;
        //return Return(result);
        var result = await http.DeleteAsync(Url, id);
        _units = null;
        if (result.Code == ResultCode.Success)
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "با موفقیت حذف شد"
            };
        return new ServiceResult
            { Code = ServiceCode.Error, Message = "به علت وابستگی با عناصر دیگر امکان حذف وجود ندارد" };
    }

    public async Task<ServiceResult> ConvertHolooUnits()
    {
        //await SweetMessage("ذخیره اتوماتیک", "لطفا جند لحظه صبر کنید", "warning");
        var result = await Create($"{Url}/ConvertHolooToSunflower", null);
        return Return(result);
    }

    public async Task<ServiceResult<Unit>> GetById(int id)
    {
        var result = await http.GetAsync<Unit>(Url, $"GetById?id={id}");
        return Return(result);
    }
}