using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events.Handlers;

public class SaleCreatedEventHandler : INotificationHandler<SaleCreatedEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(IBus bus, ILogger<SaleCreatedEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(SaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Init SaleCreated at {Time}: {@Event}", DateTime.UtcNow, notification);
        await _bus.Publish(notification);
    }
}
