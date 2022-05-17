﻿using Entities.Helper;
using Entities.ViewModel;
using Microsoft.AspNetCore.Http;
using Services.IServices;

namespace Services.Services;

public class CartService : EntityService<PurchaseOrderViewModel>, ICartService
{
    private const string Url = "api/PurchaseOrders";
    private readonly ICookieService _cookieService;
    private readonly IHttpService _http;
    private readonly string _key = "Cart";
    private readonly IProductService _productService;


    public CartService(IHttpService http, ICookieService cookieService, IProductService productService) : base(http)
    {
        _http = http;
        _cookieService = cookieService;
        _productService = productService;
    }

    public async Task<ServiceResult<List<PurchaseOrderViewModel>>> Load(HttpContext context)
    {
        var currentUser = _cookieService.GetCurrentUser();
        if (currentUser.Id != 0)
        {
            var result = await ReadList(Url, $"UserCart?userId={currentUser.Id}");
            return Return(result);
        }

        var productIdList = new List<int>();
        var productNumberList = new List<int>();
        var cookies = _cookieService.GetCookie(context, _key);
        foreach (var cookie in cookies.OrderBy(x => x.Key))
        {
            var temp = cookie.Key.Split("-");
            productIdList.Add(Convert.ToInt32(temp[1]));
            productNumberList.Add(Convert.ToInt32(cookie.Value));
        }

        var responseProduct = await _productService.ProductsWithIdsForCart(productIdList);
        if (responseProduct.Code > 0)
            return new ServiceResult<List<PurchaseOrderViewModel>>
            { Code = ServiceCode.Error, Message = responseProduct.Message };
        var cart = new List<PurchaseOrderViewModel>();
        for (var i = 0; i < responseProduct.ReturnData.Count; i++)
        {
            var tempPurchaseOrderDetail = new PurchaseOrderViewModel
            {
                ProductId = responseProduct.ReturnData[i].Id,
                Quantity = productNumberList[i],
                Name = responseProduct.ReturnData[i].Name,
                //Price = responseProduct.ReturnData[i].Price,
                Url = responseProduct.ReturnData[i].Url,
                ImagePath = responseProduct.ReturnData[i].ImagePath,
                Alt = responseProduct.ReturnData[i].Alt,
                Brand = responseProduct.ReturnData[i].Brand
                //SumPrice = responseProduct.ReturnData[i].Price* productNumberList[i]
            };

            cart.Add(tempPurchaseOrderDetail);
        }

        return new ServiceResult<List<PurchaseOrderViewModel>>
        {
            Code = ServiceCode.Success,
            ReturnData = cart
        };
    }

    public async Task<ServiceResult> Add(HttpContext context, int productId, int priceId)
    {
        var currentUser = _cookieService.GetCurrentUser();
        if (currentUser.Id == 0)
        {
            var product = _cookieService.GetCookie(context, $"{_key}-{productId}-{priceId}", false);
            var count = product.FirstOrDefault()!.Value + 1;

            _cookieService.SetCookie(context, new CookieData($"{_key}-{productId}-{priceId}", count));
            return new ServiceResult
            {
                Code = ServiceCode.Success,
                Message = "کالا با موفقیت به سبد خرید اضافه شد"
            };
        }

        var purchaseOrderViewModel = new PurchaseOrderViewModel
        {
            IsColleague = currentUser.IsColleague,
            UserId = currentUser.Id,
            Quantity = 1,
            ProductId = productId,
            PriceId = priceId
        };
        var result = await Create(Url, purchaseOrderViewModel);
        //var result = await _http.PostAsync(Url, purchaseOrderViewModel);
        return Return(result);
    }

    public async Task<ServiceResult> Decrease(HttpContext context, int id, int productId, int priceId)
    {
        var currentUser = _cookieService.GetCurrentUser();
        if (currentUser.Id == 0)
        {
            var count = 0;
            var product = _cookieService.GetCookie(context, $"{_key}-{productId}-{priceId}", false);
            if (product != null)
            {
                count = product.FirstOrDefault().Value - 1;
                if (count == 0)
                {
                    _cookieService.Remove(context, new CookieData($"{_key}-{productId}-{priceId}", productId));
                    return new ServiceResult
                    {
                        Code = ServiceCode.Success,
                        Message = "کالا با موفقیت از سبد شما کم شد"
                    };
                }

                _cookieService.SetCookie(context, new CookieData($"{_key}-{productId}-{priceId}", count));
                return new ServiceResult
                {
                    Code = ServiceCode.Success
                };
            }
        }

        var success = await Update(Url, new PurchaseOrderViewModel{ Id = id}, "Decrease");
        return Return(success);
    }

    public Task<Guid> PreFactor(int orderId, string refId, int amount)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> PreFactor(HttpContext context)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult> Delete(HttpContext context, int id, int productId, int priceId)
    {
        var currentUser = _cookieService.GetCurrentUser();
        if (currentUser.Id == 0)
        {
            var product = _cookieService.GetCookie(context, $"{_key}-{productId}-{priceId}", false);
            if (product != null)
            {
                _cookieService.Remove(context, new CookieData($"{_key}-{productId}-{priceId}", productId));
                return new ServiceResult
                {
                    Code = ServiceCode.Success,
                    Message = "کالا با موفقیت از سبد شما حذف شد"
                };
            }
        }

        var success = await Delete(Url, id);
        return Return(success);
    }

   
}