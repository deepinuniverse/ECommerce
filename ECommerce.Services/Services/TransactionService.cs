﻿namespace ECommerce.Services.Services;

public class TransactionService(IHttpService http) : EntityService<Transaction>(http), ITransactionService
{
    private const string Url = "api/Transactions";

    public async Task<ServiceResult<List<Transaction>>> Load(int userId = 0, string userName = "", string search = "",
        int pageNumber = 1, int pageSize = 10,
        string message = null, string code = null, decimal? minimumAmount = null, decimal? maximumAmount = null,
        Status status = Status.New, PurchaseSort purchaseSort = PurchaseSort.HighToLowDateBuying)
    {
        var command = "Get?" +
                      $"PaginationParameters.PageNumber={pageNumber}&" +
                      $"PaginationParameters.PageSize={pageSize}&";
        if (!string.IsNullOrEmpty(search)) command += $"PaginationParameters.Search={search}&";
        if (userId > 0) command += $"UserId={userId}&";
        if (minimumAmount != null) command += $"MinimumAmount={minimumAmount}&";
        if (maximumAmount != null) command += $"MaximumAmount={maximumAmount}&";
        command += $"PurchaseSort={purchaseSort}";
        var result = await ReadList(Url, command);
        return Return(result);
    }

    public async Task<ServiceResult<List<Transaction>>> GetAll()
    {
        var result = await ReadList(Url, "GetAll");
        return Return(result);
    }

    public async Task<ServiceResult> Add(Transaction transaction)
    {
        var result = await Create(Url, transaction);
        return Return(result);
    }

    public async Task<ServiceResult> Edit(Transaction transaction)
    {
        var result = await Update(Url, transaction);
        return Return(result);
    }

    public async Task<ServiceResult> Delete(int id)
    {
        var result = await Delete(Url, id);
        return Return(result);
    }

    public async Task<ServiceResult<Transaction>> GetById(int id)
    {
        var result = await http.GetAsync<Transaction>(Url, $"GetById?id={id}");
        return Return(result);
    }
}