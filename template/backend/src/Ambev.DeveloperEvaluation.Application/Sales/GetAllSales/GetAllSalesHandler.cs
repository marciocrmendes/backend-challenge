using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;

public class GetAllSalesHandler : IRequestHandler<GetAllSalesQuery, GetAllSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public GetAllSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<GetAllSalesResult> Handle(GetAllSalesQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _saleRepository.GetAllAsync(query.Page, query.PageSize, query.CustomerId, cancellationToken);

        return new GetAllSalesResult
        {
            Items = _mapper.Map<IEnumerable<GetSaleResult>>(items),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
}
