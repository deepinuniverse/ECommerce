﻿using ECommerce.Application.ViewModels;

namespace ECommerce.Services.Services;

public class BlogService(IHttpService http) : EntityService<Blog>(http), IBlogService
{
    private const string Url = "api/Blogs";
    private List<Blog> _blogs;

    public async Task<ServiceResult<List<Blog>>> Load(string search = "", int pageNumber = 0, int pageSize = 10)
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
                ReturnData = result.ReturnData.ToDictionary(item => item.Id, item => item.Title),
                Message = result.Messages?.FirstOrDefault()
            };
        return new ServiceResult<Dictionary<int, string>>
        {
            Code = ServiceCode.Error,
            Message = result.GetBody()
        };
    }

    public async Task<ServiceResult<List<Blog>>> Filtering(string filter)
    {
        if (_blogs == null)
        {
            var blogs = await Load();
            if (blogs.Code > 0) return blogs;
            _blogs = blogs.ReturnData;
        }

        var result = _blogs.Where(x => x.Title.Contains(filter)).ToList();
        if (result.Count == 0)
            return new ServiceResult<List<Blog>> { Code = ServiceCode.Info, Message = "برندی یافت نشد" };
        return new ServiceResult<List<Blog>>
        {
            Code = ServiceCode.Success,
            ReturnData = result
        };
    }

    //public async Task<ServiceResult> Add(Blog blog)
    //{
    //    var result = await Create(Url, blog);
    //    _blogs = null;
    //    return Return(result);
    //}

    public async Task<ServiceResult<BlogViewModel>> Add(BlogViewModel blogViewModel)
    {
        //if (blogViewModel.BrandId == 0) blogViewModel.BrandId = null;
        //var result = await Create<BlogViewModel>(Url, blogViewModel);
        var result = await http.PostAsync<BlogViewModel, BlogViewModel>(Url, blogViewModel);
        return Return(result);
    }

    public async Task<ServiceResult> Edit(BlogViewModel blogViewModel)
    {
        var result = await http.PutAsync(Url, blogViewModel);
        _blogs = null;
        return Return(result);
    }

    public async Task<ServiceResult> Delete(int id)
    {
        //var result = await Delete(Url, id);
        //_blogs = null;
        //return Return(result);
        var result = await http.DeleteAsync(Url, id);
        if (result.Code == ResultCode.Success)
        {
            _blogs = null;
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "با موفقیت حذف شد"
            };
        }

        _blogs = null;
        return new ServiceResult
            { Code = ServiceCode.Error, Message = "به علت وابستگی با عناصر دیگر امکان حذف وجود ندارد" };
    }

    public async Task<ServiceResult<BlogViewModel>> GetById(int id)
    {
        var result = await http.GetAsync<BlogViewModel>(Url, $"GetById?id={id}");
        return Return(result);
    }

    public async Task<ServiceResult<List<BlogViewModel>>> TopBlogs(string CategoryId = null, string search = "",
        int pageNumber = 0, int pageSize = 3, int blogSort = 1)
    {
        var command = "Get?" +
                      $"PaginationParameters.PageNumber={pageNumber}&" +
                      $"PaginationParameters.PageSize={pageSize}&";
        if (!string.IsNullOrEmpty(search)) command += $"PaginationParameters.Search={search}&";
        if (!string.IsNullOrEmpty(CategoryId)) command += $"PaginationParameters.CategoryId={CategoryId}&";

        command += $"BlogSort={blogSort}";
        var result = await http.GetAsync<List<BlogViewModel>>(Url, command);
        return Return(result);
    }

    public async Task<ServiceResult<List<BlogViewModel>>> TopBlogsByTagText(string CategoryId = "", string TagText = "",
        int pageNumber = 0, int pageSize = 3, int blogSort = 1)
    {
        var command = "GetByTagText?" +
                      $"PaginationParameters.PageNumber={pageNumber}&" +
                      $"PaginationParameters.PageSize={pageSize}&";
        if (!string.IsNullOrEmpty(TagText)) command += $"PaginationParameters.TagText={TagText}&";
        if (!string.IsNullOrEmpty(CategoryId)) command += $"PaginationParameters.CategoryId={CategoryId}&";

        command += $"BlogSort={blogSort}";
        var result = await http.GetAsync<List<BlogViewModel>>(Url, command);
        return Return(result);
    }

    public async Task<ServiceResult<BlogDetailsViewModel>> GetByUrl(string blogUrl)
    {
        var result = await http.GetAsync<BlogDetailsViewModel>(Url, $"GetByUrl?blogUrl={blogUrl}");
        return Return(result);
    }
}