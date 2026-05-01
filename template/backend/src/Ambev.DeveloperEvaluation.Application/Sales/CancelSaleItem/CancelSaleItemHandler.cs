using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;

    public CancelSaleItemHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken = default)
    {
        var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken) ??
            throw new KeyNotFoundException($"Sale with ID {command.SaleId} not found.");

        if (sale.IsCancelled)
            throw new DomainException("Cannot cancel an item on a cancelled sale.");

        sale.CancelItem(command.ItemId);
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        return new CancelSaleItemResult { Success = true };
    }
}
