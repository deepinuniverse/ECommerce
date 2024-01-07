﻿namespace ECommerce.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class SlideShowsController(ISlideShowRepository slideShowRepository, IHolooArticleRepository articleRepository,
        ILogger<SlideShowsController> logger)
    : ControllerBase
{
    private async Task<Product> AddPriceAndExistFromHoloo(Product product)
    {
        foreach (var productPrices in product.Prices)
            if (productPrices.SellNumber != null && productPrices.SellNumber != Price.HolooSellNumber.خالی)
            {
                var article = await articleRepository.GetHolooPrice(productPrices.ArticleCodeCustomer,
                    productPrices.SellNumber!.Value);
                productPrices.Amount = article.price / 10;
                productPrices.Exist = (double)article.exist;
            }

        return product;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        try
        {
            var slideShowsList = await slideShowRepository.GetAllWithInclude(pageNumber, pageSize, cancellationToken);
            var returnSlideShow = new List<SlideShowViewModel>();
            var slideShowViewModelList = slideShowsList.Select(s => new SlideShowViewModel
            {
                Id = s.Id,
                ProductId = s.ProductId,
                Product = s.Product,
                CategoryId = s.CategoryId,
                Category = s.Category,
                Title = s.Title,
                Description = s.Description,
                ImagePath = s.ImagePath
            });
            foreach (var slideShow in slideShowViewModelList)
            {
                if (slideShow.Product != null)
                {
                    var productTemp = await AddPriceAndExistFromHoloo(slideShow.Product);
                    slideShow.Price = productTemp.Prices != null && productTemp.Prices.FirstOrDefault() == null
                        ? 0
                        : productTemp.Prices.FirstOrDefault().Amount;
                }

                returnSlideShow.Add(slideShow);
            }

            return Ok(new ApiResult
            {
                Code = ResultCode.Success,
                ReturnData = returnSlideShow
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
    public async Task<ActionResult<SlideShow>> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await slideShowRepository.GetByIdAsync(cancellationToken, id);
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

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Post(SlideShow slideShow, CancellationToken cancellationToken)
    {
        try
        {
            if (slideShow == null) return BadRequest();
            slideShow.Title = slideShow.Title.Trim();

            var repetitiveTitle = await slideShowRepository.GetByTitle(slideShow.Title, cancellationToken);
            if (repetitiveTitle != null)
                return Ok(new ApiResult
                {
                    Code = ResultCode.Repetitive,
                    Messages = new List<string> { "عنوان اسلاید شو تکراری است" }
                });
            if (slideShow.ProductId != null)
            {
                var repetitiveProduct = slideShowRepository.IsRepetitiveProduct(0, slideShow.ProductId,
                    slideShow.CategoryId, cancellationToken);
                if (repetitiveProduct)
                    return Ok(new ApiResult
                    {
                        Code = ResultCode.Repetitive,
                        Messages = new List<string> { "این کالا قبلا برای یک اسلاید شو دیگر انتخاب شده" }
                    });
            }

            await slideShowRepository.AddAsync(slideShow, cancellationToken);
            return Ok(new ApiResult
            {
                Code = ResultCode.Success
            });
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message);
            return Ok(new ApiResult
                { Code = ResultCode.DatabaseError, Messages = new List<string> { "اشکال در سمت سرور" } });
        }
    }

    [HttpPut]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<bool>> Put(SlideShow slideShow, CancellationToken cancellationToken)
    {
        try
        {
            var repetitiveTitle = await slideShowRepository.GetByTitle(slideShow.Title, cancellationToken);
            if (repetitiveTitle != null && repetitiveTitle.Id != slideShow.Id)
                return Ok(new ApiResult
                {
                    Code = ResultCode.Repetitive,
                    Messages = new List<string> { "عنوان اسلاید شو تکراری است" }
                });
            if (repetitiveTitle != null) slideShowRepository.Detach(repetitiveTitle);

            if (slideShow.ProductId != null)
            {
                var repetitiveProduct = slideShowRepository.IsRepetitiveProduct(slideShow.Id, slideShow.ProductId,
                    slideShow.CategoryId, cancellationToken);
                if (repetitiveProduct)
                    return Ok(new ApiResult
                    {
                        Code = ResultCode.Repetitive,
                        Messages = new List<string> { "این کالا قبلا برای یک اسلاید شو دیگر انتخاب شده" }
                    });
            }

            await slideShowRepository.UpdateAsync(slideShow, cancellationToken);
            return Ok(new ApiResult
            {
                Code = ResultCode.Success
            });
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message);
            return Ok(new ApiResult
                { Code = ResultCode.DatabaseError, Messages = new List<string> { "اشکال در سمت سرور" } });
        }
    }

    [HttpDelete]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await slideShowRepository.DeleteAsync(id, cancellationToken);
            return Ok(new ApiResult
            {
                Code = ResultCode.Success
            });
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message);
            return Ok(new ApiResult
                { Code = ResultCode.DatabaseError, Messages = new List<string> { "اشکال در سمت سرور" } });
        }
    }
}