﻿namespace ECommerce.Infrastructure.Repository;

public class PurchaseOrderRepository(SunflowerECommerceDbContext context) : RepositoryBase<PurchaseOrder>(context),
    IPurchaseOrderRepository
{
    public async Task<PurchaseOrder> GetByOrderIdWithInclude(long orderId, CancellationToken cancellationToken)
    {
        var query = context.PurchaseOrders.Where(x => x.OrderId == orderId)
            .Include(d => d.PurchaseOrderDetails)
            .Include(d => d.PaymentMethod)
            .AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PurchaseOrder?> GetByUserAndOrderId(int userId, long orderId, CancellationToken cancellationToken)
    {
        var query = context.PurchaseOrders.Where(x => x.UserId == userId && x.OrderId == orderId)
            .Include(d => d.SendInformation).ThenInclude(c => c.City)
            .Include(d => d.SendInformation).ThenInclude(c => c.State)
            .Include(d => d.PurchaseOrderDetails)
            .Include(d => d.PaymentMethod)
            .AsNoTracking();
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PurchaseOrder?> GetByUser(int id, Status status, CancellationToken cancellationToken)
    {
        return await context.PurchaseOrders
            .Where(x => x.UserId == id && !x.IsPaid && x.Status == status)
            .Include(x => x.PurchaseOrderDetails)
            .Include(x => x.Discount)
            .Include(a => a.SendInformation)
            .ThenInclude(x => x.State)
            .Include(x => x.SendInformation)
            .ThenInclude(x => x.City)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PurchaseOrder?> GetByOrderId(long id, CancellationToken cancellationToken)
    {
        return await context.PurchaseOrders.Where(x => x.OrderId == id && !x.IsPaid)
            .Include(x => x.PurchaseOrderDetails).Include(a => a.SendInformation)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<PurchaseOrderViewModel>?> GetProductListByUserId(int userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var purchaseOrderViewModel = await context.PurchaseOrderDetails
                .Where(x => x.PurchaseOrder!.UserId == userId && !x.PurchaseOrder.IsPaid &&
                            x.PurchaseOrder.Status == Status.New)
                .Select(p => new PurchaseOrderViewModel
                {
                    Id = p.Id,
                    ProductId = p.ProductId,
                    Url = p.Product.Url,
                    Name = p.Product.Name,
                    Price = p.Product.Prices!.First(x => x.Id == p.PriceId),
                    PriceAmount = p.Product.Prices!.First(x => x.Id == p.PriceId).Amount,
                    Discount = p.Product.Prices!.First(x => x.Id == p.PriceId).Discount,
                    PriceId = p.PriceId,
                    ImagePath =
                        $"{p.Product.Images!.FirstOrDefault()!.Path}/{p.Product.Images!.FirstOrDefault()!.Name}",
                    Brand = p.Product.Brand == null ? "بدون برند" : p.Product.Brand.Name,
                    Alt = p.Product.Images!.FirstOrDefault()!.Alt,
                    //Exist = p.Product.Exist,
                    IsColleague = p.PurchaseOrder!.User!.IsColleague,
                    UserId = p.PurchaseOrder.UserId,
                    Quantity = p.Quantity,
                    SumPrice = p.Quantity * p.Product.Prices!.First(x => x.Id == p.PriceId).Amount,
                    ColorName = p.Product.Prices!.First(x => x.Id == p.PriceId).Color!.Name,
                    DiscountAmount = 0
                })
                .ToListAsync(cancellationToken);
            return purchaseOrderViewModel;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<PurchaseOrder> GetPurchaseOrderWithIncludeById(int id, CancellationToken cancellationToken)
    {
        var query = context.PurchaseOrders.Where(x => x.Id == id)
            .Include(d => d.PurchaseOrderDetails).ThenInclude(p => p.Price)
            .Include(d => d.PaymentMethod)
            .AsNoTracking();

        var result = await query.FirstOrDefaultAsync(cancellationToken);
        return result;
    }

    public async Task<PagedList<PurchaseListViewModel>> Search(
        PurchaseFiltreOrderViewModel purchaseFiltreOrderViewModel,
        CancellationToken cancellationToken)
    {
        var query = context.PurchaseOrders.Where(x =>
                x.User.UserName.Contains(purchaseFiltreOrderViewModel.PaginationParameters.Search))
            .Include(d => d.PaymentMethod)
            .Include(d => d.SendInformation)
            .Include(d => d.PurchaseOrderDetails)
            .ThenInclude(pro => pro.Product)
            .ThenInclude(b => b.Images)
            .AsNoTracking();

        if (purchaseFiltreOrderViewModel.IsPaied != null)
            query = query.Where(x => x.IsPaid == purchaseFiltreOrderViewModel.IsPaied);
        if (purchaseFiltreOrderViewModel.UserId > 0)
            query = query.Where(x => x.UserId == purchaseFiltreOrderViewModel.UserId);
        if (purchaseFiltreOrderViewModel.ToCreationDate != null)
            query = query.Where(x => x.CreationDate <= purchaseFiltreOrderViewModel.ToCreationDate);
        if (purchaseFiltreOrderViewModel.FromCreationDate != null)
            query = query.Where(x => x.CreationDate >= purchaseFiltreOrderViewModel.FromCreationDate);
        if (purchaseFiltreOrderViewModel.StatusId != null)
            query = query.Where(x => x.Status == (Status)purchaseFiltreOrderViewModel.StatusId);
        if (purchaseFiltreOrderViewModel.MinimumAmount != null)
            query = query.Where(x => x.Amount >= purchaseFiltreOrderViewModel.MinimumAmount);
        if (purchaseFiltreOrderViewModel.MaximumAmount != null)
            query = query.Where(x => x.Amount <= purchaseFiltreOrderViewModel.MaximumAmount);
        if (purchaseFiltreOrderViewModel.PaymentMethodStatus != null)
            query = query.Where(x =>
                x.PaymentMethod.PaymentMethodStatus == purchaseFiltreOrderViewModel.PaymentMethodStatus);


        var sortedQuery = query.OrderByDescending(x => x.Id);

        switch (purchaseFiltreOrderViewModel.PurchaseSort)
        {
            case PurchaseSort.LowToHighCountBuying:
                sortedQuery = query.OrderBy(x => x.PurchaseOrderDetails.Count);
                break;
            case PurchaseSort.HighToLowCountBuying:
                sortedQuery = query.OrderByDescending(x => x.PurchaseOrderDetails.Count);
                break;
            case PurchaseSort.LowToHighPiceBuying:
                sortedQuery = query.OrderBy(x => x.Amount);
                break;
            case PurchaseSort.HighToLowPriceBuying:
                sortedQuery = query.OrderByDescending(x => x.Amount);
                break;
            case PurchaseSort.LowToHighDateBuying:
                sortedQuery = query.OrderBy(x => x.CreationDate);
                break;
            case PurchaseSort.HighToLowDateBuying:
                sortedQuery = query.OrderByDescending(x => x.CreationDate);
                break;
        }

        var purchaseList = await sortedQuery.Select(p => new PurchaseListViewModel
        {
            Id = p.Id,
            Amount = p.Amount,
            CreationDate = p.CreationDate,
            IsPaied = p.IsPaid,
            Description = p.Description,
            Status = p.Status,
            PaymentMethod = p.PaymentMethod,
            PurchaseOrderDetails = p.PurchaseOrderDetails.ToList(),
            Discount = p.DiscountAmount,
            SendInformation = p.SendInformation,
            UserId = p.UserId,
            UserName = p.User.UserName,
            FBailCode = p.FBailCode,
            OrderId = p.OrderId
        }).ToListAsync(cancellationToken);

        return PagedList<PurchaseListViewModel>.ToPagedList(purchaseList,
            purchaseFiltreOrderViewModel.PaginationParameters.PageNumber,
            purchaseFiltreOrderViewModel.PaginationParameters.PageSize);
    }
}
