using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResult>
{
    private readonly ISaleRepository _saleRepository;

    public CancelSaleHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<CancelSaleResult> Handle(CancelSaleCommand command, CancellationToken cancellationToken = default)
    {
        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken) ??
            throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        sale.Cancel();

        await _saleRepository.UpdateAsync(sale, cancellationToken);

        return new CancelSaleResult { Success = true };
    }
}
