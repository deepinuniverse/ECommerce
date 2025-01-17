﻿namespace ECommerce.Services.Services;

public class KeywordService(IHttpService http) : EntityService<Keyword>(http), IKeywordService
{
    private const string Url = "api/Keywords";
    private List<Keyword> _keywords;

    public async Task<ServiceResult<List<Keyword>>> Load(string search = "", int pageNumber = 0, int pageSize = 10)
    {
        var result = await ReadList(Url, $"Get?PageNumber={pageNumber}&PageSize={pageSize}&Search={search}");
        return Return(result);
    }

    public async Task<ServiceResult<List<Keyword>>> GetAll()
    {
        var result = await ReadList(Url, "GetAll");
        return Return(result);
    }

    public async Task<ServiceResult<Dictionary<int, string>>> LoadDictionary()
    {
        var result = await ReadList(Url);
        if (result.Code == ResultCode.Success)
            return new ServiceResult<Dictionary<int, string>>
            {
                Code = ServiceCode.Success,
                ReturnData = result.ReturnData.ToDictionary(item => item.Id, item => item.KeywordText),
                Message = result.Messages?.FirstOrDefault()
            };
        return new ServiceResult<Dictionary<int, string>>
        {
            Code = ServiceCode.Error,
            Message = result.GetBody()
        };
    }

    public async Task<ServiceResult<List<Keyword>>> Filtering(string filter)
    {
        if (_keywords == null)
        {
            var keywords = await Load();
            if (keywords.Code > 0) return keywords;
            _keywords = keywords.ReturnData;
        }

        var result = _keywords.Where(x => x.KeywordText.Contains(filter)).ToList();
        if (result.Count == 0)
            return new ServiceResult<List<Keyword>> { Code = ServiceCode.Info, Message = "کلمه کلیدی یافت نشد" };
        return new ServiceResult<List<Keyword>>
        {
            Code = ServiceCode.Success,
            ReturnData = result
        };
    }

    public async Task<ServiceResult> Add(Keyword keyword)
    {
        var result = await Create(Url, keyword);
        _keywords = null;
        return Return(result);
    }

    public async Task<ServiceResult> Edit(Keyword keyword)
    {
        var result = await Update(Url, keyword);
        _keywords = null;
        return Return(result);
    }

    public async Task<ServiceResult> Delete(int id)
    {
        //var result = await Delete(Url, id);
        //_keywords = null;
        //return Return(result);
        var result = await http.DeleteAsync(Url, id);
        if (result.Code == ResultCode.Success)
        {
            _keywords = null;
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "با موفقیت حذف شد"
            };
        }

        _keywords = null;
        return new ServiceResult
            { Code = ServiceCode.Error, Message = "به علت وابستگی با عناصر دیگر امکان حذف وجود ندارد" };
    }

    public async Task<ServiceResult<List<Keyword>>> GetKeywordsByProductId(int productId)
    {
        var result = await ReadList(Url, $"GetKeywordsByProductId?id={productId}");
        return Return(result);
        //_keywordProductId ??= (await _keywordViewModelEntityService.ReadList(Url,$"GetKeywordsWithProductId")).ReturnData;
        //return _keywordProductId.Where(x => x.ProductsId.Any(y => y == productId)).ToList();
    }

    public async Task<ServiceResult<Keyword>> GetById(int id)
    {
        var result = await http.GetAsync<Keyword>(Url, $"GetById?id={id}");
        return Return(result);
    }
}