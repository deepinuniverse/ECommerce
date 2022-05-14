﻿using Entities;

namespace API.Interface;

public interface ISlideShowRepository : IAsyncRepository<SlideShow>
{
    bool IsRepetitiveProduct(int id, CancellationToken cancellationToken);

    Task<SlideShow> GetByTitle(string title, CancellationToken cancellationToken);

    Task<IEnumerable<SlideShow>> GetAllWithInclude(CancellationToken cancellationToken);
}