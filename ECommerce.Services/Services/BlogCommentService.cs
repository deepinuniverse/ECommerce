﻿namespace ECommerce.Services.Services;

public class BlogCommentService(IHttpService http) : EntityService<BlogComment>(http), IBlogCommentService
{
    private const string Url = "api/BlogComments";
    private List<BlogComment> _blogComments;

    public async Task<ServiceResult<List<BlogComment>>> Load(string search = "", int pageNumber = 0, int pageSize = 10)
    {
        var result = await ReadList(Url, $"Get?PageNumber={pageNumber}&PageSize={pageSize}&Search={search}");
        return Return(result);
    }

    public async Task<ServiceResult<Dictionary<int, string>>> LoadDictionary()
    {
        var result = await ReadList(Url);
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

    public async Task<ServiceResult<List<BlogComment>>> Filtering(string filter)
    {
        if (_blogComments == null)
        {
            var blogComments = await Load();
            if (blogComments.Code > 0) return blogComments;
            _blogComments = blogComments.ReturnData;
        }

        var result = _blogComments.Where(x => x.Name.Contains(filter)).ToList();
        if (result.Count == 0)
            return new ServiceResult<List<BlogComment>> { Code = ServiceCode.Info, Message = "برندی یافت نشد" };
        return new ServiceResult<List<BlogComment>>
        {
            Code = ServiceCode.Success,
            ReturnData = result
        };
    }

    public async Task<ServiceResult> Add(BlogComment blogComment)
    {
        blogComment.IsAccepted = false;
        blogComment.IsRead = false;
        blogComment.IsAnswered = false;
        blogComment.DateTime = DateTime.Now;
        var result = await CreateWithoutToken(Url, blogComment);
        return Return(result);
    }

    public async Task<ServiceResult> Edit(BlogComment blogComment)
    {
        var result = await Update(Url, blogComment);
        _blogComments = null;
        return Return(result);
    }

    public async Task<ServiceResult> Accept(BlogComment blogComment)
    {
        var result = await Update(Url, blogComment);
        if (result.Code == ResultCode.Success && blogComment.IsAccepted)
        {
            _blogComments = null;
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "با موفقیت تایید شد"
            };
        }

        if (result.Code == ResultCode.Success && blogComment.IsAccepted == false)
        {
            _blogComments = null;
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "وضعیت پیام به عدم تایید تغییر یافت"
            };
        }

        _blogComments = null;
        return Return(result);
    }

    public async Task<ServiceResult> Delete(int id)
    {
        //var result = await Delete(Url, id);
        //_blogComments = null;
        //return Return(result);
        var result = await http.DeleteAsync(Url, id);
        if (result.Code == ResultCode.Success)
        {
            _blogComments = null;
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "با موفقیت حذف شد"
            };
        }

        _blogComments = null;
        return new ServiceResult
            { Code = ServiceCode.Error, Message = "به علت وابستگی با عناصر دیگر امکان حذف وجود ندارد" };
    }

    public async Task<ServiceResult<BlogComment>> GetById(int id)
    {
        var result = await http.GetAsync<BlogComment>(Url, $"GetById?id={id}");
        return Return(result);
    }

    public async Task<ServiceResult<List<BlogComment>>> GetAllAcceptedComments(string search = "", int pageNumber = 0,
        int pageSize = 10)
    {
        var result = await ReadList(Url,
            $"GetAllAcceptedComments?PageNumber={pageNumber}&PageSize={pageSize}&Search={search}");
        return Return(result);
    }
}