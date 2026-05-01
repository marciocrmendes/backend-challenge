using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events.Consumers;

public class SaleCreatedConsumer : IHandleMessages<SaleCreatedEvent>
{
    private readonly ILogger<SaleCreatedConsumer> _logger;

    public SaleCreatedConsumer(ILogger<SaleCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleCreatedEvent message)
    {
        _logger.LogInformation("[Consumer] SaleCreated received: SaleId={SaleId}, Number={SaleNumber}, Customer={CustomerId}",
            message.Sale.Id,
            message.Sale.SaleNumber,
            message.Sale.CustomerId);

        return Task.CompletedTask;
    }
}
