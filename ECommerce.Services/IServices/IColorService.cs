﻿using Entities;
using Entities.Helper;

namespace Services.IServices;

public interface IColorService : IEntityService<Color>
{
    Task<ServiceResult<List<Color>>> Filtering(string filter);
    Task<ServiceResult<List<Color>>> Load(string search = "", int pageNumber = 0, int pageSize = 10);
    Task<ServiceResult> Add(Color color);
    Task<ServiceResult> Edit(Color color);
    Task<ServiceResult> Delete(int id);
    Task<ServiceResult<Color>> GetById(int id);
}