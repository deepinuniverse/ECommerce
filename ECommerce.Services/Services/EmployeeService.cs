﻿namespace ECommerce.Services.Services;

public class EmployeeService(IHttpService http) : EntityService<Employee>(http), IEmployeeService
{
    private const string Url = "api/Employees";

    public async Task<ServiceResult<List<Employee>>> Load(string search = "", int pageNumber = 0, int pageSize = 10)
    {
        var result = await ReadList(Url, "GetAll");
        return Return(result);
    }

    public async Task<ServiceResult<List<Employee>>> GetAll(string search = "", int pageNumber = 0, int pageSize = 10)
    {
        var result = await ReadList(Url,
            $"Get?PageNumber={pageNumber}&Search={search}&PageSize={pageSize}");
        return Return(result);
    }

    public async Task<ServiceResult<Dictionary<int, string>>> LoadDictionary()
    {
        var result = await ReadList(Url, "GetAll");
        if (result.Code == ResultCode.Success)
            return new ServiceResult<Dictionary<int, string>>
            {
                Code = ServiceCode.Success,
                ReturnData = result.ReturnData.ToDictionary(item => item.Id, item => item.Name),
                Message = result.Messages?.FirstOrDefault()
            };
        return new ServiceResult<Dictionary<int, string>>
        {
            Code = ServiceCode.Error,
            Message = result.GetBody()
        };
    }


    public async Task<ServiceResult> Add(Employee employee)
    {
        var result = await Create(Url, employee);
        return Return(result);
    }

    public async Task<ServiceResult> Edit(Employee employee)
    {
        var result = await Update(Url, employee);
        return Return(result);
    }

    public async Task<ServiceResult> Delete(int id)
    {
        //    var result = await Delete(Url, id);
        //    return Return(result);
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

    public async Task<ServiceResult<Employee>> GetById(int id)
    {
        var result = await http.GetAsync<Employee>(Url, $"GetById?id={id}");
        return Return(result);
    }
}