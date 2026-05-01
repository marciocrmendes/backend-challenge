using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem
{
    public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ILogger<CancelSaleItemHandler> _logger;

        public CancelSaleItemHandler(ISaleRepository saleRepository, ILogger<CancelSaleItemHandler> logger)
        {
            _saleRepository = saleRepository;
            _logger = logger;
        }

        public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken) ??
                throw new KeyNotFoundException($"Sale with ID {command.SaleId} not found.");

            if (sale.IsCancelled)
                throw new DomainException("Cannot cancel an item on a cancelled sale.");

            var item = sale.Items.FirstOrDefault(i => i.Id == command.ItemId)
                ?? throw new KeyNotFoundException($"SaleItem with ID {command.ItemId} not found.");

            sale.CancelItem(command.ItemId);
            await _saleRepository.UpdateAsync(sale, cancellationToken);

            _logger.LogInformation("ItemCancelled: {@Event}", new ItemCancelledEvent(sale, item));

            return new CancelSaleItemResult { Success = true };
        }
    }
}
