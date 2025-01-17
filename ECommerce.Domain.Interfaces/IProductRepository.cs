﻿using ECommerce.Application.ViewModels;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Entities.Helper;
using ECommerce.Domain.Interfaces.Utilities;

namespace ECommerce.Domain.Interfaces;

public interface IProductRepository : IRepositoryBase<Product>
{
    Task<Product?> GetByName(string name, CancellationToken cancellationToken);

    Task<Product?> GetByUrl(string url, CancellationToken cancellationToken);

    Task AddAll(IEnumerable<Product?> products);

    //Task<IEnumerable<Product?>> GetAllHolooProducts(CancellationToken cancellationToken);

    Task<Product?> AddWithRelations(ProductViewModel productViewModel, CancellationToken cancellationToken);

    Task<Product?> EditWithRelations(ProductViewModel productViewModel, CancellationToken cancellationToken);

    IQueryable<Product> GetWithInclude(CancellationToken cancellationToken);

    int GetCategoryProductCount(int categoryId, CancellationToken cancellationToken);

    Task<List<ProductIndexPageViewModel>> GetProductList(List<int> productIdList, CancellationToken cancellationToken);

    IEnumerable<ProductCompareViewModel> GetProductListWithAttribute(List<int> productIdList);
    List<int> GetProductsIdWithCategories(int categoryId);

    IQueryable<Product> GetProductByIdWithInclude(int productId);

    IQueryable<Product?> GetProducts(List<int> categoriesId, List<int>? brandsId, List<int>? starsCount,
        List<int>? tagsId);

    IQueryable<Product?> GetAllProducts();

    Task<List<Product>> GetByCategoryId(int categoryId, CancellationToken cancellationToken);

    Task<PagedList<ProductIndexPageViewModel>> Search(PaginationParameters paginationParameters,
        CancellationToken cancellationToken);

    Task<Product?> GetProductById(int id, CancellationToken cancellationToken);

    #region Tops

    Task<List<ProductIndexPageViewModel>> TopNew(int count, int start, string? topCategory,
        CancellationToken cancellationToken);

    Task<List<ProductIndexPageViewModel>> TopPrices(int count, int start, string? topCategory,
        CancellationToken cancellationToken);

    Task<List<ProductIndexPageViewModel>> TopChip(int count, CancellationToken cancellationToken);

    //Task<List<ProductIndexPageViewModel?>> TopDiscounts(int count, CancellationToken cancellationToken);

    Task<List<ProductIndexPageViewModel>> TopStars(int count, int start, string? topCategory,
        CancellationToken cancellationToken);

    Task<List<ProductIndexPageViewModel>> TopSells(int count, CancellationToken cancellationToken);

    Task<List<ProductIndexPageViewModel>> TopRelatives(int productId, int count, CancellationToken cancellationToken);
    IQueryable<Product> GetAllWithInclude(CancellationToken cancellationToken);

    #endregion
}
