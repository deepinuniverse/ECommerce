﻿using ECommerce.Application.ViewModels;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Services.Services;

public class CartService(IHttpService http, ICookieService cookieService, IProductService productService,
        IPriceService priceService)
    : EntityService<PurchaseOrderViewModel>(http), ICartService
{
    private const string Url = "api/PurchaseOrders";
    private readonly IHttpService _http = http;
    private readonly string _key = "Cart";
    private readonly IPriceService _priceService = priceService;


    public async Task<ServiceResult<List<PurchaseOrderViewModel>>> Load(HttpContext context,
        bool shouldUpdatePurchaseOrderDetails = false)
    {
        var currentUser = cookieService.GetCurrentUser();
        var carts = await ReadFromCookies(context);

        if (currentUser.Id != 0)
        {
            if (carts.Count > 0)
                foreach (var cart in carts)
                {
                    await Add(context, cart.ProductId, cart.PriceId, cart.Quantity);
                    await Delete(context, cart.Id, cart.ProductId, cart.PriceId, true);
                }

            var result = await ReadList(Url,
                $"UserCart?userId={currentUser.Id}&shouldUpdatePurchaseOrderDetails={shouldUpdatePurchaseOrderDetails}");
            return Return(result);
        }

        //if(cart.Count==0)
        //    return new ServiceResult<List<PurchaseOrderViewModel>>
        //    { Code = ServiceCode.Error, Message = "کالای مورد نظر یافت نشد"};

        return new ServiceResult<List<PurchaseOrderViewModel>>
        {
            Code = ServiceCode.Success,
            ReturnData = carts
        };
    }

    public async Task<ServiceResult<List<PurchaseOrderViewModel>>> CartListFromServer(
        bool shouldUpdatePurchaseOrderDetails = false)
    {
        var currentUser = cookieService.GetCurrentUser();
        if (currentUser.Id != 0)
        {
            var result = await ReadList(Url,
                $"UserCart?userId={currentUser.Id}&shouldUpdatePurchaseOrderDetails={shouldUpdatePurchaseOrderDetails}");
            return Return(result);
        }

        return new ServiceResult<List<PurchaseOrderViewModel>>
        {
            Code = ServiceCode.Error
        };
    }

    public async Task<ServiceResult> Add(HttpContext context, int productId, int priceId, int count)
    {
        var productResult = await productService.ProductsWithIdsForCart(new List<int> { productId });
        var productFromServer = productResult.ReturnData[0];

        var exist = productFromServer.Prices.First(x => x.Id == priceId).Exist;
        var maxOrder = productFromServer.MaxOrder;

        var currentUser = cookieService.GetCurrentUser();

        if (currentUser.Id == 0)
        {
            var product = cookieService.GetCookie(context, $"{_key}-{productId}-{priceId}", false);
            var newCount = product.FirstOrDefault()!.Value + count;

            if (newCount > exist)
                return new ServiceResult
                {
                    Code = ServiceCode.Error,
                    Message = "تعداد کالا بیشتر از موجودی است"
                };

            if (newCount > maxOrder && maxOrder > 0)
                return new ServiceResult
                {
                    Code = ServiceCode.Error,
                    Message = "تعداد کالا بیشتر از حد مجاز است"
                };

            cookieService.SetCookie(context, new CookieData($"{_key}-{productId}-{priceId}", newCount));
            if (newCount > 1)
                return new ServiceResult
                {
                    Code = ServiceCode.Info,
                    Message = "کالا با موفقیت به سبد خرید اضافه شد"
                };
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
            Quantity = Convert.ToUInt16(count),
            ProductId = productId,
            PriceId = priceId,
            DiscountAmount = 0
        };
        var result = await Create(Url, purchaseOrderViewModel);
        if (result.Code == ResultCode.Repetitive)
            return new ServiceResult
            {
                Code = ServiceCode.Info,
                Message = result.Messages?.FirstOrDefault()
            };
        return Return(result);
    }

    public async Task<ServiceResult> Decrease(HttpContext context, int id, int productId, int priceId)
    {
        var currentUser = cookieService.GetCurrentUser();
        if (currentUser.Id == 0)
        {
            var product = cookieService.GetCookie(context, $"{_key}-{productId}-{priceId}", false);
            if (product.Any())
            {
                var count = product.FirstOrDefault()!.Value - 1;
                if (count <= 0)
                {
                    cookieService.Remove(context, new CookieData($"{_key}-{productId}-{priceId}", productId));
                    return new ServiceResult
                    {
                        Code = ServiceCode.Success,
                        Message = "کالا با موفقیت از سبد شما حذف شد"
                    };
                }

                cookieService.SetCookie(context, new CookieData($"{_key}-{productId}-{priceId}", count));
                return new ServiceResult
                {
                    Code = ServiceCode.Success,
                    Message = "کالا با موفقیت از سبد شما کم شد"
                };
            }
        }

        var success = await Update(Url, new PurchaseOrderViewModel { Id = id }, "Decrease");
        return Return(success);
    }

    public async Task<ServiceResult> Delete(HttpContext context, int id, int productId, int priceId,
        bool deleteFromCookie = false)
    {
        var currentUser = cookieService.GetCurrentUser();
        if (currentUser.Id == 0 || deleteFromCookie)
        {
            var product = cookieService.GetCookie(context, $"{_key}-{productId}-{priceId}", false);
            if (product.Any())
            {
                cookieService.Remove(context, new CookieData($"{_key}-{productId}-{priceId}", productId));
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

    private async Task<List<PurchaseOrderViewModel>> ReadFromCookies(HttpContext context)
    {
        var carts = new List<PurchaseOrderViewModel>();

        var productList = new List<CookiesProduct>();

        var cookies = cookieService.GetCookie(context, _key);
        foreach (var cookie in cookies.OrderBy(x => x.Key))
        {
            var temp = cookie.Key.Split("-");
            var productCode = Convert.ToInt32(temp[1]);
            var productCount = Convert.ToUInt16(cookie.Value);
            var priceId = Convert.ToInt32(temp[2]);
            if (productCode <= 0 || productCount <= 0 || priceId <= 0) continue;
            productList.Add(new CookiesProduct
            {
                ProductId = productCode,
                ProductNumber = productCount,
                ProductPrice = priceId
            });
        }

        var responseProduct =
            await productService.ProductsWithIdsForCart(productList.Select(x => x.ProductId).ToList());
        if (responseProduct.Code > 0)
            return carts;

        for (var i = 0; i < responseProduct.ReturnData.Count; i++)
        {
            var product = productList.First(x => x.ProductId == responseProduct.ReturnData[i].Id);
            var priceId = product.ProductPrice;
            var price = responseProduct.ReturnData[i].Prices.Where(x => x.Id == priceId).FirstOrDefault();
            if (price == null)
                continue;
            var quantity = responseProduct.ReturnData[i].MaxOrder < product.ProductNumber &&
                           responseProduct.ReturnData[i].MaxOrder > 0
                ? responseProduct.ReturnData[i].MaxOrder
                : product.ProductNumber;
            var tempPurchaseOrderDetail = new PurchaseOrderViewModel
            {
                ProductId = responseProduct.ReturnData[i].Id,
                Quantity = quantity,
                Name = responseProduct.ReturnData[i].Name,
                Price = price,
                Url = responseProduct.ReturnData[i].Url,
                ImagePath = responseProduct.ReturnData[i].ImagePath,
                Alt = responseProduct.ReturnData[i].Alt,
                Brand = responseProduct.ReturnData[i].Brand,
                SumPrice = price.Amount * quantity,
                PriceAmount = price.Amount,
                PriceId = priceId,
                ColorName = price.Color.Name,
                Id = responseProduct.ReturnData[i].Id,
                Exist = price.Exist
            };

            carts.Add(tempPurchaseOrderDetail);
        }

        return carts;
    }

    private struct CookiesProduct
    {
        public int ProductId { get; set; }
        public ushort ProductNumber { get; set; }
        public int ProductPrice { get; set; }
    }
}