using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;

public record GetAllSalesQuery(int Page = 1, int PageSize = 10, Guid? CustomerId = null) : IRequest<GetAllSalesResult>;
