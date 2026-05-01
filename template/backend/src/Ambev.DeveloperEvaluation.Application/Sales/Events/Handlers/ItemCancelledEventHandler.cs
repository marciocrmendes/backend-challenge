using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events.Handlers;

public class ItemCancelledEventHandler : INotificationHandler<ItemCancelledEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    public ItemCancelledEventHandler(IBus bus, ILogger<ItemCancelledEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(ItemCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Init ItemCancelled at {Time}: {@Event}", DateTime.UtcNow, notification);
        await _bus.Publish(notification);
    }
}
