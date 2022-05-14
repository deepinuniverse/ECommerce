﻿using API.Utilities;
using Entities;
using Entities.Helper;

namespace API.Interface;

public interface IProductAttributeGroupRepository : IAsyncRepository<ProductAttributeGroup>
{
    Task<PagedList<ProductAttributeGroup>> Search(PaginationParameters paginationParameters,
        CancellationToken cancellationToken);

    Task<ProductAttributeGroup> GetByName(string name, CancellationToken cancellationToken);

    Task<List<ProductAttributeGroup>> GetWithInclude(CancellationToken cancellationToken);

    Task<IEnumerable<ProductAttributeGroup>> GetAllAttributeWithProductId(int productId,
        CancellationToken cancellationToken);

    Task<List<ProductAttributeGroup>> AddWithAttributeValue(List<ProductAttributeGroup> productAttributeGroups,
        int productId,
        CancellationToken cancellationToken);
}