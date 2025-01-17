﻿using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class CategoriesController(IUnitOfWork unitOfWork, ILogger<CategoriesController> logger)
    : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository = unitOfWork.GetRepository<CategoryRepository, Category>();

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] PaginationParameters paginationParameters,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(paginationParameters.Search)) paginationParameters.Search = "";
            var entity = await _categoryRepository.Search(paginationParameters, cancellationToken);
            var paginationDetails = new PaginationDetails
            {
                TotalCount = entity.TotalCount,
                PageSize = entity.PageSize,
                CurrentPage = entity.CurrentPage,
                TotalPages = entity.TotalPages,
                HasNext = entity.HasNext,
                HasPrevious = entity.HasPrevious,
                Search = paginationParameters.Search
            };
            return Ok(new ApiResult
            {
                PaginationDetails = paginationDetails,
                Code = ResultCode.Success,
                ReturnData = entity
            });
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message);
            return Ok(new ApiResult { Code = ResultCode.DatabaseError });
        }
    }

    [HttpGet]
    public async Task<ActionResult<Category>> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _categoryRepository.GetByIdAsync(cancellationToken, id);
            if (result == null)
                return Ok(new ApiResult
                {
                    Code = ResultCode.NotFound
                });

            return Ok(new ApiResult
            {
                Code = ResultCode.Success,
                ReturnData = result
            });
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message);
            return Ok(new ApiResult { Code = ResultCode.DatabaseError });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetParents(int productId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _categoryRepository.Parents(productId, cancellationToken);
            if (result == null)
                return Ok(new ApiResult
                {
                    Code = ResultCode.NotFound
                });

            return Ok(new ApiResult
            {
                Code = ResultCode.Success,
                ReturnData = result
            });
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message);
            return Ok(new ApiResult
                { Code = ResultCode.DatabaseError, Messages = new List<string> { "اشکال در سمت سرور" } });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Search(string searchKeyword, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _categoryRepository.Search(searchKeyword, cancellationToken);
            if (result == null)
                return Ok(new ApiResult
                {
                    Code = ResultCode.NotFound
                });

            return Ok(new ApiResult
            {
                Code = ResultCode.Success,
                ReturnData = result
            });
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message);
            return Ok(new ApiResult
                { Code = ResultCode.DatabaseError, Messages = new List<string> { "اشکال در سمت سرور" } });
        }
    }


    [HttpGet]
    public async Task<ActionResult<CategoryViewModel>> GetByUrl(string url, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _categoryRepository.GetByUrl(url, cancellationToken);
            if (result == null)
                return Ok(new ApiResult
                {
                    Code = ResultCode.NotFound
                });

            return Ok(new ApiResult
            {
                Code = ResultCode.Success,
                ReturnData = result
            });
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message);
            return Ok(new ApiResult
                { Code = ResultCode.DatabaseError, Messages = new List<string> { "اشکال در سمت سرور" } });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCategoriesByProduct(int productId, CancellationToken cancellationToken)
    {
        try
        {
            var categoryList = _categoryRepository.GetAll("Products")
                .Where(x => x.Products != null && x.Products.Any(p => p.Id == productId));
            return Ok(await categoryList.Select(x => new CategoryViewModel
            {
                Id = x.Id,
                Name = x.Name,
                ProductsId = x.Products!.Select(product => product.Id).ToList(),
                ParentId = x.ParentId,
                Parent = x.Parent,
                Categories = x.Categories!.Select(category => category.Id).ToList()
            }).ToListAsync(cancellationToken));
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message);
            return Ok(new ApiResult { Code = ResultCode.DatabaseError });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Post(Category? category, CancellationToken cancellationToken)
    {
        try
        {
            if (category == null)
                return Ok(new ApiResult
                {
                    Code = ResultCode.BadRequest
                });
            category.Name = category.Name.Trim();

            var repetitiveCategory =
                await _categoryRepository.GetByName(category.Name, cancellationToken, category.ParentId);
            if (repetitiveCategory != null)
                return Ok(new ApiResult
                {
                    Code = ResultCode.Repetitive,
                    Messages = new List<string> { "نام دسته تکراری است" }
                });

            _categoryRepository.Add(category);
            await unitOfWork.SaveAsync(cancellationToken);

            return Ok(new ApiResult
            {
                Code = ResultCode.Success
            });
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message);
            return Ok(new ApiResult { Code = ResultCode.DatabaseError });
        }
    }

    [HttpPut]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<bool>> Put(Category category, CancellationToken cancellationToken)
    {
        try
        {
            _categoryRepository.Update(category);
            await unitOfWork.SaveAsync(cancellationToken);

            return Ok(new ApiResult
            {
                Code = ResultCode.Success
            });
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message);
            return Ok(new ApiResult { Code = ResultCode.DatabaseError });
        }
    }

    [HttpDelete]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _categoryRepository.DeleteById(id, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);

            return Ok(new ApiResult
            {
                Code = ResultCode.Success
            });
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message);
            return Ok(new ApiResult { Code = ResultCode.DatabaseError });
        }
    }
}