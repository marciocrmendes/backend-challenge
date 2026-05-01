using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events.Consumers;

public class SaleCancelledConsumer : IHandleMessages<SaleCancelledEvent>
{
    private readonly ILogger<SaleCancelledConsumer> _logger;

    public SaleCancelledConsumer(ILogger<SaleCancelledConsumer> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCancelledEvent message)
    {
        _logger.LogInformation("[Consumer] SaleCancelled received: SaleId={SaleId}, Number={SaleNumber}",
            message.Sale.Id,
            message.Sale.SaleNumber);

        return Task.CompletedTask;
    }
}
