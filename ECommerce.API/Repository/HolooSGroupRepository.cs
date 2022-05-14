﻿using API.DataContext;
using API.Interface;
using Entities.HolooEntity;
using Microsoft.EntityFrameworkCore;

namespace API.Repository;

public class HolooSGroupRepository : HolooRepository<HolooSGroup>, IHolooSGroupRepository
{
    private readonly HolooDbContext _context;

    public HolooSGroupRepository(HolooDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HolooSGroup>> GetSGroupByMCode(string mCode, CancellationToken cancellationToken)
    {
        return await _context.S_GROUP.Where(x => x.M_groupcode == mCode).ToListAsync(cancellationToken);
    }
}