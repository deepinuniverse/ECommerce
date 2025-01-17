﻿using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces;

public interface ISlideShowRepository : IRepositoryBase<SlideShow>
{
    bool IsRepetitiveProduct(int id, int? productId, int? categoryId, CancellationToken cancellationToken);

    Task<SlideShow?> GetByTitle(string title, CancellationToken cancellationToken);

    Task<IEnumerable<SlideShow>> GetAllWithInclude(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
