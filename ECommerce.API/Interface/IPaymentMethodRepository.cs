﻿using API.Utilities;
using Entities;
using Entities.Helper;

namespace API.Interface;

public interface IPaymentMethodRepository : IAsyncRepository<PaymentMethod>
{
    Task<PagedList<PaymentMethod>> Search(PaginationParameters paginationParameters,
        CancellationToken cancellationToken);

    Task<PaymentMethod> GetByAccountNumber(string name, CancellationToken cancellationToken);

    Task<int> AddAll(IEnumerable<PaymentMethod> paymentMethods, CancellationToken cancellationToken);
}