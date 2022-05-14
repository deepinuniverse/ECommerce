﻿using Entities.Helper;
using Entities.ViewModel;

namespace Services.IServices;

public interface IWishListService
{
    Task<ServiceResult<List<WishListViewModel>>> Load();
    Task<ServiceResult> Add(int productId);
    Task<ServiceResult> Delete(int wishListId);
}