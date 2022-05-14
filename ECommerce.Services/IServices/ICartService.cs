﻿using Entities.Helper;
using Entities.ViewModel;
using Microsoft.AspNetCore.Http;

namespace Services.IServices;

public interface ICartService
{
    Task<ServiceResult<List<PurchaseOrderViewModel>>> Load(HttpContext context);
    Task<ServiceResult> Add(HttpContext context, int productId);
    Task<ServiceResult> Delete(HttpContext context, int productId);
    Task<int> Count(HttpContext context);
}