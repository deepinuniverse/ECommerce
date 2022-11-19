﻿using API.DataContext;
using API.Interface;
using API.Utilities;
using Ecommerce.Entities.ViewModel;
using Entities;
using Entities.Helper;
using Microsoft.EntityFrameworkCore;

namespace API.Repository;

public class UserRepository : AsyncRepository<User>, IUserRepository
{
    public UserRepository(SunflowerECommerceDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<PagedList<UserListViewModel>> Search(UserFilterdParameters userFilterdParameters,
        CancellationToken cancellationToken)
    {
        var query = TableNoTracking.Where(x => x.UserName.Contains(userFilterdParameters.PaginationParameters.Search)).AsNoTracking();

        if (userFilterdParameters.IsActive != null) query = query.Where(x => x.IsActive == userFilterdParameters.IsActive);
        if (userFilterdParameters.IsColleauge != null) query = query.Where(x => x.IsColleague == userFilterdParameters.IsColleauge);
        if (userFilterdParameters.HasBuying != null)
        {

            query = userFilterdParameters.HasBuying == true ? query.Where(x => x.PurchaseOrders.Count > 0): query.Where(x => x.PurchaseOrders.Count == 0);
        }

        var sortedQuery = query.OrderByDescending(x => x.Id).ToList();

        switch (userFilterdParameters.UserSort)
        {
            case UserSort.LowToHighCountBuying:
                sortedQuery = query.OrderBy(x => x.PurchaseOrders.Count).ToList();
                break;
            case UserSort.HighToLowCountBuying:
                sortedQuery = query.OrderByDescending(x => x.PurchaseOrders.Count).ToList();
                break;
            case UserSort.LowToHighPiceBuying:
                sortedQuery = query.OrderBy(x => x.PurchaseOrders.Sum(p=>p.Amount)).ToList();
                break;
            case UserSort.HighToLowPriceBuying:
                sortedQuery = query.OrderByDescending(x => x.PurchaseOrders.Sum(p => p.Amount)).ToList();
                break;
        }

        var userList = await query.Select(u=> new UserListViewModel
        {
            Id=u.Id,
            BuyingAmount= u.PurchaseOrders.Sum(s=>s.Amount),
            City= u.City.Name,
            State=u.State.Name,
            IsActive=u.IsActive,
            IsColleague=u.IsColleague,
            RegisterDate=u.RegisterDate,
            Username=u.UserName,
            UserRole=u.UserRole.Name
        }).ToListAsync(cancellationToken);
        return PagedList<UserListViewModel>.ToPagedList(userList,
            userFilterdParameters.PaginationParameters.PageNumber,
            userFilterdParameters.PaginationParameters.PageSize);
    }

    public async Task<User?> GetByEmailOrUserName(string input, CancellationToken cancellationToken)
    {
        return await TableNoTracking.Where(p => p.Email == input || p.PhoneNumber == input || p.UserName == input)
            .Include(x => x.UserRole).AsNoTracking().SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<User> GetByPhoneNumber(string phone, CancellationToken cancellationToken)
    {
        return await TableNoTracking.Where(p => p.PhoneNumber == phone).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> Exists(int id, string email, string phoneNumber, CancellationToken cancellationToken)
    {
        return await TableNoTracking.AnyAsync(p => p.Id != id && (p.Email == email || p.PhoneNumber == phoneNumber),
            cancellationToken);
    }

    public async Task<List<UserRole>> GetUserRoles(int id, CancellationToken cancellationToken)
    {
        var userRoles = await DbContext.UserRoles.AsNoTracking().Where(q => q.UserId == id).Select(p => p.RoleId)
            .ToListAsync(cancellationToken);
        return await DbContext.Roles.Where(p => userRoles.Contains(p.Id)).ToListAsync(cancellationToken);
    }

    public async Task<List<UserRole>> GetApplicationRoles(CancellationToken cancellationToken)
    {
        return await DbContext.Roles.ToListAsync(cancellationToken);
    }

    public async Task AddLoginHistory(int userId, string token, string ipAddress, DateTime expirationDate)
    {
        await DbContext.LoginHistories.AddAsync(new LoginHistory
        {
            CreationDate = DateTime.Now,
            ExpirationDate = expirationDate,
            IpAddress = ipAddress,
            Token = token,
            UserId = userId
        });
        await DbContext.SaveChangesAsync();
    }
}