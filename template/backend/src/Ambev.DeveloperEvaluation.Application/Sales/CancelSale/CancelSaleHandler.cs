using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale
{
    public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResult>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ILogger<CancelSaleHandler> _logger;

        public CancelSaleHandler(ISaleRepository saleRepository, ILogger<CancelSaleHandler> logger)
        {
            _saleRepository = saleRepository;
            _logger = logger;
        }

        public async Task<CancelSaleResult> Handle(CancelSaleCommand command, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
            if (sale is null)
                throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

            sale.Cancel();
            await _saleRepository.UpdateAsync(sale, cancellationToken);

            _logger.LogInformation("SaleCancelled: {@Event}", new SaleCancelledEvent(sale));

            return new CancelSaleResult { Success = true };
        }
    }
}
