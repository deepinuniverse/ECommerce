﻿using ECommerce.Domain.Entities.HolooEntity;

namespace ECommerce.Infrastructure.Repository;

public class ProductRepository(SunflowerECommerceDbContext context, HolooDbContext holooDbContext)
    : RepositoryBase<Product>(context), IProductRepository
{
    public async Task<Product?> GetByName(string name, CancellationToken cancellationToken)
    {
        return await context.Products.Where(x => x.Name == name).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Product?> GetByUrl(string url, CancellationToken cancellationToken)
    {
        return await context.Products
            .Where(x => x.Url == url && x.Prices!.Any())
            .Include(x => x.Brand)
            .Include(x => x.Keywords)
            .Include(t => t.Tags)
            .Include(x => x.Images)
            .Include(x => x.Prices).ThenInclude(x => x.Discount)
            .Include(x => x.Prices).ThenInclude(x => x.Color)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task AddAll(IEnumerable<Product?> products)
    {
        return context.Products.AddRangeAsync(products);
    }

    //public async Task<IEnumerable<Product>> GetAllHolooProducts(CancellationToken cancellationToken) => await context.Products.Where(x => x.ArticleCode != null).ToListAsync(cancellationToken);

    public async Task<Product> AddWithRelations(ProductViewModel productViewModel, CancellationToken cancellationToken)
    {
        Product? product = productViewModel;
        //product.Prices = productViewModel.Prices;
        product.Keywords = new List<Keyword>();
        foreach (var id in productViewModel.KeywordsId) product.Keywords.Add(context.Keywords.First(x => x.Id == id));
        product.Tags = new List<Tag>();
        foreach (var id in productViewModel.TagsId) product.Tags.Add(context.Tags.First(x => x.Id == id));
        product.ProductCategories = new List<Category>();
        foreach (var id in productViewModel.CategoriesId)
            product.ProductCategories.Add(context.Categories.First(x => x.Id == id));

        //product.AttributeGroupProducts = new List<ProductAttributeGroup>();
        //foreach (var id in productViewModel.Attributes.Select(x => x.AttributeGroupId).Distinct())
        //{
        //    product.AttributeGroupProducts.Add(await context.ProductAttributeGroups.FindAsync(id, cancellationToken));
        //}

        //product.AttributeValues = new List<ProductAttributeValue>();
        //foreach (var productAttribute in productViewModel.Attributes.Where(a => a.AttributeValue.Count > 0).Select(x => x.AttributeValue))
        //{
        //    product.AttributeValues.Add(productAttribute.FirstOrDefault());
        //}

       var newProduct = await context.Products.AddAsync(product, cancellationToken);
       return newProduct.Entity;
    }

    public async Task<Product?> EditWithRelations(ProductViewModel productViewModel,
        CancellationToken cancellationToken)
    {
        var product = await context.Products.Where(x => x.Id == productViewModel.Id)
            .Include(nameof(Product.ProductCategories)).Include(nameof(Product.Tags)).Include(nameof(Product.Keywords))
            .Include(nameof(Product.Images)).Include(nameof(Product.AttributeValues))
            .Include(nameof(Product.AttributeGroupProducts)).FirstAsync(cancellationToken);
        product.Name = productViewModel.Name.Trim();
        //product.ArticleCode = productViewModel.ArticleCode;
        //product.ArticleCodeCustomer = productViewModel.ArticleCodeCustomer;
        product.Description = productViewModel.Description?.Trim();
        //product.Exist = productViewModel.Exist;
        product.MinOrder = productViewModel.MinOrder;
        product.MaxOrder = productViewModel.MaxOrder;
        product.MinInStore = productViewModel.MinInStore;
        product.ReorderingLevel = productViewModel.ReorderingLevel;
        product.IsDiscontinued = productViewModel.IsDiscontinued;
        product.IsActive = productViewModel.IsActive;
        product.Url = productViewModel.Url.Trim();
        //product.DiscountId = productViewModel.DiscountId;
        product.HolooCompanyId = productViewModel.HolooCompanyId;
        product.BrandId = productViewModel.BrandId != 0 ? productViewModel.BrandId : null;
        product.SupplierId = productViewModel.SupplierId;
        product.StoreId = productViewModel.StoreId;
        product.Review = productViewModel.Review?.Trim();

        foreach (var productKeyword in product.Keywords) product.Keywords.Remove(productKeyword);
        foreach (var id in productViewModel.KeywordsId)
            product.Keywords.Add(await context.Keywords.FirstAsync(x => x.Id == id));

        var tags = product.Tags;
        foreach (var tag in tags) product.Tags.Remove(tag);
        foreach (var id in productViewModel.TagsId) product.Tags.Add(await context.Tags.FirstAsync(x => x.Id == id));

        foreach (var category in product.ProductCategories) product.ProductCategories.Remove(category);
        foreach (var id in productViewModel.CategoriesId)
            product.ProductCategories.Add(await context.Categories.FirstAsync(x => x.Id == id));

        //if (productViewModel.Attributes != null)
        //{
        //    foreach (var attributeValues in productViewModel.شفف)
        //    {
        //        product.AttributeValues.Remove(attributeValues);
        //    }

        //    foreach (var productAttribute in productViewModel.Attributes.Where(a => a.AttributeValue.Count > 0)
        //        .Select(x => x.AttributeValue))
        //    {
        //        product.AttributeValues.Add(productAttribute.FirstOrDefault());
        //    }
        //}

        context.Entry(product).State = EntityState.Modified;
        return product;
    }

    public IQueryable<Product> GetWithInclude(CancellationToken cancellationToken)
    {
        return context.Products.AsNoTracking().Include(x => x.ProductCategories).Include(x => x.Prices)
            .Include(x => x.Brand).Include(x => x.Images);
    }

    public IQueryable<Product> GetProductByIdWithInclude(int productId)
    {
        return context.Products.AsNoTracking().Where(x => x.Id == productId)
            .Include(c => c.ProductCategories)
            .Include(u => u.ProductUserRanks)
            .Include(p => p.Prices)
            .Include(t => t.Tags)
            .Include(k => k.Keywords)
            .Include(b => b.Brand)
            .Include(su => su.Supplier)
            .Include(st => st.Store)
            .Include(i => i.Images);
    }

    public int GetCategoryProductCount(int categoryId, CancellationToken cancellationToken)
    {
        return context.Products.Count(x => x.ProductCategories.Any(i => i.Id == categoryId));
    }

    public async Task<List<ProductIndexPageViewModel>> GetProductList(List<int> productIdList,
        CancellationToken cancellationToken)
    {
        var productIndexPageViewModel = new List<ProductIndexPageViewModel>();
        var products = await context.Products
            .Where(x => productIdList.Contains(x.Id))
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.ProductUserRanks)
            .Include(x => x.Prices)
            .ThenInclude(c => c.Color)
            .ToListAsync(cancellationToken);
        productIndexPageViewModel.AddRange(products.Select(product => (ProductIndexPageViewModel)product));
        return productIndexPageViewModel;
    }

    public IEnumerable<ProductCompareViewModel> GetProductListWithAttribute(List<int> productIdList)
    {
        var group = context.ProductAttributeGroups.AsNoTracking()
            .Include(a => a.Attribute)
            .ToList();

        var productCompareViewModel = new List<ProductCompareViewModel>();


        //foreach (var product in products)
        //{
        //    product.AttributeValues = await context.ProductAttributeValues.Where(x => x.ProductId == product.Id)
        //        .ToListAsync(cancellationToken);
        //}

        var productValues = context.ProductAttributeValues.Where(x => productIdList.Contains(x.ProductId.Value))
            .ToList();

        var products = context.Products.AsNoTracking()
            .Where(x => productIdList.Contains(x.Id))
            .Include(x => x.ProductCategories)
            .Include(x => x.Prices)
            .Include(x => x!.AttributeValues)!
            .ThenInclude(x => x.ProductAttribute)
            .Select(x => new ProductCompareViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Prices.FirstOrDefault(y => !y.IsColleague && y.MinQuantity == 1).Amount,
                Prices = x.Prices,
                ImagePath = $"{x.Images.FirstOrDefault().Path}/{x.Images.FirstOrDefault().Name}",
                Url = x.Url,
                Brand = x.Brand.Name,
                Alt = x.Images.FirstOrDefault().Alt,
                ProductCategories = x.ProductCategories.Select(x => x.Id)
            })
            .ToList();

        foreach (var product in products)
        {
            var newGroup = new List<ProductAttributeGroup>();
            foreach (var productAttributeGroup in group)
            {
                var newProductAttributeGroup = new ProductAttributeGroup
                {
                    Id = productAttributeGroup.Id,
                    Name = productAttributeGroup.Name,
                    Products = productAttributeGroup.Products,
                    Attribute = new List<ProductAttribute>()
                };
                foreach (var attribute in productAttributeGroup.Attribute)
                {
                    var productAttribute = new ProductAttribute
                    {
                        Title = attribute.Title,
                        AttributeType = attribute.AttributeType,
                        AttributeGroupId = attribute.AttributeGroupId,
                        AttributeGroup = attribute.AttributeGroup
                    };
                    var value = productValues.FirstOrDefault(x =>
                        x.ProductAttributeId == attribute.Id && x.ProductId == product.Id);
                    if (value != null)
                        productAttribute.AttributeValue = new List<ProductAttributeValue> { value };
                    else
                        productAttribute.AttributeValue = new List<ProductAttributeValue>();
                    newProductAttributeGroup.Attribute.Add(productAttribute);
                }

                newGroup.Add(newProductAttributeGroup);
            }

            product.AttributeGroupProducts = newGroup;
            //yield return product;
            productCompareViewModel.Add(product);
        }

        return productCompareViewModel;
        //productCompareViewModel.AddRange(products.Select(product => product));
    }

    public async Task<List<Product>> GetByCategoryId(int categoryId, CancellationToken cancellationToken)
    {
        var products = await context.Products.Where(x => x.ProductCategories.Any(y => y.Id == categoryId))
            .OrderByDescending(x => x.Id)
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.Prices)
            .Include(x => x.ProductUserRanks)
            .ToListAsync(cancellationToken);

        return products;
    }

    /// <summary>
    /// </summary>
    /// <param name="brandsId"></param>
    /// <param name="starsCount"></param>
    /// <param name="tagsId"></param>
    /// <returns></returns>
    public IQueryable<Product?> GetProducts(List<int> categoriesId, List<int>? brandsId, List<int>? starsCount,
        List<int>? tagsId)
    {
        var products = context.Products.Where(x => x.Prices!.Any()).AsQueryable();

        if (categoriesId.Any(x => x != 0))
            products = products.Where(x =>
                x.ProductCategories.Any(cat => categoriesId.Any(categoryId => cat.Id == categoryId)));

        if (brandsId is { Count: > 0 }) products = products.Where(x => brandsId.Contains((int)x.BrandId));

        if (starsCount is { Count: > 0 })
            products = products.Where(x =>
                starsCount.Contains(x.ProductUserRanks.Sum(s => s.Stars) / x.ProductUserRanks.Count));
        if (tagsId is { Count: > 0 })
            products = tagsId.Aggregate(products,
                (current, tagId) => current.Where(x => x.Tags.Any(t => t.Id == tagId)));

        var result = products.Include(x => x.Prices).ThenInclude(y => y.Discount);
        return result;
    }

    public IQueryable<Product?> GetAllProducts()
    {
        var products = context.Products.Where(x => x.Prices!.Any()).AsQueryable();
        var result = products.Include(x => x.Prices).ThenInclude(y => y.Discount);
        return result;
    }

    public async Task<Product?> GetProductById(int id, CancellationToken cancellationToken)
    {
        return await context.Products
            .Where(x => x.Id == id)
            .Include(x => x.Brand)
            .Include(x => x.Images)
            .Include(x => x.ProductUserRanks)
            .Include(x => x.Prices)
            .ThenInclude(c => c.Color)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedList<ProductIndexPageViewModel>> Search(PaginationParameters paginationParameters,
        CancellationToken cancellationToken)
    {
        return PagedList<ProductIndexPageViewModel>.ToPagedList(
            await context.Products.Where(x => x.Name.Contains(paginationParameters.Search))
                .AsNoTracking()
                .OrderByDescending(on => on.Id)
                .Select(p => new ProductIndexPageViewModel
                {
                    Prices = p.Prices!,
                    Alt = p.Images!.First().Alt,
                    Brand = p.Brand!.Name,
                    Name = p.Name,
                    Description = p.Description,
                    Id = p.Id,
                    ImagePath = $"{p.Images!.First().Path}/{p.Images!.First().Name}",
                    Stars = p.Star,
                    Url = p.Url
                })
                .ToListAsync(cancellationToken),
            paginationParameters.PageNumber,
            paginationParameters.PageSize);
    }

    #region Tops

    public async Task<List<ProductIndexPageViewModel>> TopNew(int count, int start, string? topCategory,
        CancellationToken cancellationToken)
    {
        var products = await context.Products.Where(x => x.Images!.Count > 0 && x.Prices!.Any())
            .OrderByDescending(x => x.Id).Skip(start).Take(count)
            .Include(x => x.Prices).ThenInclude(c => c.Discount)
            .Select(p => new ProductIndexPageViewModel
            {
                Prices = p.Prices!,
                Alt = p.Images!.First().Alt,
                Brand = p.Brand!.Name,
                Name = p.Name,
                Description = p.Description,
                Id = p.Id,
                ImagePath = $"{p.Images!.First().Path}/{p.Images!.First().Name}",
                Stars = p.Star,
                Url = p.Url,
                TopCategory = topCategory
            })
            .ToListAsync(cancellationToken);

        return products;
    }

    public async Task<List<ProductIndexPageViewModel>> TopPrices(int count, int start, string? topCategory,
        CancellationToken cancellationToken)
    {
        var products = await context.Prices.OrderByDescending(x => x.Amount)
            .Where(x => x.Product.Images.Count > 0).Include(x => x.Discount)
            .Skip(start).Take(count)
            .Select(p => new ProductIndexPageViewModel
            {
                Prices = p.Product!.Prices!,
                Alt = p.Product.Images!.First().Alt,
                Brand = p.Product.Brand!.Name,
                Name = p.Product.Name,
                Description = p.Product.Description,
                Id = p.Product.Id,
                ImagePath = $"{p.Product.Images!.First().Path}/{p.Product.Images!.First().Name}",
                Stars = p.Product.Star,
                Url = p.Product.Url,
                TopCategory = topCategory
            })
            .ToListAsync(cancellationToken);
        return products;
    }

    private Price selectPrice(Price productPrices, HolooArticle article)
    {
        productPrices.Amount = productPrices.SellNumber switch
        {
            Price.HolooSellNumber.Sel_Price => Convert.ToDecimal(article.Sel_Price) / 10,
            Price.HolooSellNumber.Sel_Price2 => Convert.ToDecimal(article.Sel_Price2) / 10,
            Price.HolooSellNumber.Sel_Price3 => Convert.ToDecimal(article.Sel_Price3) / 10,
            Price.HolooSellNumber.Sel_Price4 => Convert.ToDecimal(article.Sel_Price4) / 10,
            Price.HolooSellNumber.Sel_Price5 => Convert.ToDecimal(article.Sel_Price5) / 10,
            Price.HolooSellNumber.Sel_Price6 => Convert.ToDecimal(article.Sel_Price6) / 10,
            Price.HolooSellNumber.Sel_Price7 => Convert.ToDecimal(article.Sel_Price7) / 10,
            Price.HolooSellNumber.Sel_Price8 => Convert.ToDecimal(article.Sel_Price8) / 10,
            Price.HolooSellNumber.Sel_Price9 => Convert.ToDecimal(article.Sel_Price9) / 10,
            Price.HolooSellNumber.Sel_Price10 => Convert.ToDecimal(article.Sel_Price10) / 10,
            _ => productPrices.Amount
        };

        var temp = holooDbContext.ARTICLE.Where(
            x => article.A_Code_C == x.A_Code_C);
        var bolooryFlag = holooDbContext.Customer.First().C_Name == "گروه تجهيزات صنعتي بلوري";
        var articlesWithSameCode = temp.Where(x => Convert.ToInt32(x.A_Code) < 3400000 || !bolooryFlag).ToList();
        productPrices.Exist = articlesWithSameCode.Sum(x => x.Exist) ?? 0;
        return productPrices;
    }

    public async Task<List<ProductIndexPageViewModel>> TopChip(int count, CancellationToken cancellationToken)
    {
        var products = await context.Prices.OrderBy(x => x.Amount)
            .Where(x => x.Product!.Images!.Count > 0).Include(x => x.Discount)
            .Take(count)
            .Select(p => new ProductIndexPageViewModel
            {
                Prices = p.Product!.Prices!,
                Alt = p.Product.Images!.First().Alt,
                Brand = p.Product.Brand!.Name,
                Name = p.Product.Name,
                Description = p.Product.Description,
                Id = p.Product.Id,
                ImagePath = $"{p.Product.Images!.First().Path}/{p.Product.Images!.First().Name}",
                Stars = p.Product.Star,
                Url = p.Product.Url
            })
            .ToListAsync(cancellationToken);
        return products;
    }

    //public async Task<List<ProductIndexPageViewModel?>> TopDiscounts(int count, CancellationToken cancellationToken)
    //{
    //    var products = new List<ProductIndexPageViewModel?>();
    //    var discounts = context.Discounts.OrderByDescending(x => x.Amount).Select(x => x.Products).Take(count);
    //    var i = 0;
    //    foreach (var discount in discounts)
    //        foreach (var product in discount)
    //        {
    //            products.Add(await context.Products
    //                .Where(x => x.Id == product.Id && x.Images!.Count > 0 && x.Prices!.Any())
    //                .Select(p => new ProductIndexPageViewModel
    //                {
    //                    Prices = p.Prices!,
    //                    Alt = p.Images!.First().Alt,
    //                    Brand = p.Brand!.Name,
    //                    Name = p.Name,
    //                    Description = p.Description,
    //                    Id = p.Id,
    //                    ImagePath = $"{p.Images!.First().Path}/{p.Images!.First().Name}",
    //                    Stars = p.Star,
    //                    Url = p.Url
    //                })
    //                .FirstOrDefaultAsync(cancellationToken));
    //            i++;
    //            if (i == count) break;
    //        }

    //    //if (productIndexPageViewModel.Count < 5)
    //    //{
    //    //    productIndexPageViewModel.AddRange(await TopNew(count - productIndexPageViewModel.Count, cancellationToken));
    //    //}
    //    return products;
    //}

    public async Task<List<ProductIndexPageViewModel>> TopStars(int count, int start, string? topCategory,
        CancellationToken cancellationToken)
    {
        var products = await context.ProductUserRanks.OrderByDescending(x => x.Stars)
            .Where(x => x.Product!.Images!.Count > 0 && x.Product.Prices!.Any()).Include(x => x.Product)
            .ThenInclude(x => x.Prices).ThenInclude(x => x.Discount)
            .Skip(start).Take(count)
            .Select(p => new ProductIndexPageViewModel
            {
                Prices = p.Product!.Prices!,
                Alt = p.Product.Images!.First().Alt,
                Brand = p.Product.Brand!.Name,
                Name = p.Product.Name,
                Description = p.Product.Description,
                Id = p.Product.Id,
                ImagePath = $"{p.Product.Images!.First().Path}/{p.Product.Images!.First().Name}",
                Stars = p.Product.Star,
                Url = p.Product.Url,
                TopCategory = topCategory
            })
            .ToListAsync(cancellationToken);

        return products;
    }

    public async Task<List<ProductIndexPageViewModel>> TopSells(int count, CancellationToken cancellationToken)
    {
        var products = await context.ProductSellCounts.OrderByDescending(x => x.Count)
            .Where(x => x.Product!.Images!.Count > 0 && x.Product.Prices!.Any()).Include(x => x.Product)
            .ThenInclude(x => x.Prices).ThenInclude(x => x.Discount)
            .Take(count)
            .Select(p => new ProductIndexPageViewModel
            {
                Prices = p.Product!.Prices!,
                Alt = p.Product.Images!.First().Alt,
                Brand = p.Product.Brand!.Name,
                Name = p.Product.Name,
                Description = p.Product.Description,
                Id = p.Product.Id,
                ImagePath = $"{p.Product.Images!.First().Path}/{p.Product.Images!.First().Name}",
                Stars = p.Product.Star,
                Url = p.Product.Url
            })
            .ToListAsync(cancellationToken);

        return products;
    }

    public async Task<List<ProductIndexPageViewModel>> TopRelatives(int productId, int count,
        CancellationToken cancellationToken)
    {
        //var categories = await context.Categories
        //    .Where(y => y.Products.Any(x => x.Id == productId && x.Prices.Any()))
        //    .ToListAsync(cancellationToken);

        var product = await context.Products.Include(i => i.Tags)
            .FirstAsync(x => x.Id == productId, cancellationToken);

        var products = new List<ProductIndexPageViewModel>();
        //var countPerCategory = count;
        //if (categories.Count > 1)
        //{
        //    countPerCategory = count / categories.Count;
        //    if (countPerCategory < 1) countPerCategory = 1;
        //}
        var rnd = new Random();
        var selectedProductId = new List<int>();
        var j = 0;
        foreach (var tag in product.Tags)
        {
            var selectedProduct = await context.Products
                .Where(x => x.Tags.Contains(tag) && x.Id != productId && !selectedProductId.Contains(x.Id))
                .Skip(rnd.Next(0, tag.Products.Count - 1))
                .Select(p => new ProductIndexPageViewModel
                {
                    Prices = p.Prices!,
                    Alt = p.Images!.First().Alt,
                    Brand = p.Brand!.Name,
                    Name = p.Name,
                    Description = p.Description,
                    Id = p.Id,
                    ImagePath = $"{p.Images!.First().Path}/{p.Images!.First().Name}",
                    Stars = p.Star,
                    Url = p.Url
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (selectedProduct != null)
            {
                products.Add(selectedProduct);
                selectedProductId.Add(selectedProduct.Id);
            }
            else
            {
                j++;
                if (j == count)
                    break;
            }
        }

        j = 0;
        if (products.Count == 0)
        {
            var category = await context.Categories
                .FirstAsync(y => y.Products.Any(x => x.Id == productId && x.Prices.Any()));

            var categoryProductCount =
                context.Products.Count(x => x.ProductCategories.Any(c => c.Id == category.Id)) - 1;
            if (categoryProductCount <= 1) return products;
            for (var i = 1; i <= count; i++)
            {
                var selectedProductByCategory = await context.Products
                    .Where(x => x.ProductCategories.Any(c => c.Id == category.Id) && x.Id != productId &&
                                x.Id != productId && !selectedProductId.Contains(x.Id))
                    .Skip(rnd.Next(0, categoryProductCount))
                    .Select(p => new ProductIndexPageViewModel
                    {
                        Prices = p.Prices!,
                        Alt = p.Images!.First().Alt,
                        Brand = p.Brand!.Name,
                        Name = p.Name,
                        Description = p.Description,
                        Id = p.Id,
                        ImagePath = $"{p.Images!.First().Path}/{p.Images!.First().Name}",
                        Stars = p.Star,
                        Url = p.Url
                    })
                    .FirstOrDefaultAsync(cancellationToken);
                if (selectedProductByCategory != null)
                {
                    products.Add(selectedProductByCategory);
                    selectedProductId.Add(selectedProductByCategory.Id);
                }
                else
                {
                    j++;
                    if (j == count)
                        break;
                }
            }
        }

        return products.Take(count).ToList();
    }

    public IQueryable<Product> GetAllWithInclude(CancellationToken cancellationToken)
    {
        var products = context.Products.Include(x => x.Prices).Include(x => x.Brand)
            .Include(x => x.Images).Include(x => x.ProductUserRanks);
        return products;
    }

    public List<int> GetProductsIdWithCategories(int categoryId)
    {
        var ProductsIds = context.Products
            .Include(x => x.ProductCategories)
            .Where(x => x.ProductCategories.Any(c => c.Id == categoryId)).Select(c => c.Id)
            .ToList();

        return ProductsIds;
    }

    #endregion
}
