using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events.Consumers;

public class ItemCancelledConsumer : IHandleMessages<ItemCancelledEvent>
{
    private readonly ILogger<ItemCancelledConsumer> _logger;

    public ItemCancelledConsumer(ILogger<ItemCancelledConsumer> logger)
    {
        _logger = logger;
    }

    public Task Handle(ItemCancelledEvent message)
    {
        _logger.LogInformation("[Consumer] ItemCancelled received: SaleId={SaleId}, ItemId={ItemId}, Product={ProductName}",
            message.Sale.Id,
            message.CancelledItem.Id,
            message.CancelledItem.ProductName);

        return Task.CompletedTask;
    }
}
