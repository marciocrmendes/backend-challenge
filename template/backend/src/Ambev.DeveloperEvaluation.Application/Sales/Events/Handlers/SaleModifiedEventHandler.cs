using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events.Handlers;

public class SaleModifiedEventHandler : INotificationHandler<SaleModifiedEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    public SaleModifiedEventHandler(IBus bus, ILogger<SaleModifiedEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(SaleModifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Init SaleModified at {Time}: {@Event}", DateTime.UtcNow, notification);
        await _bus.Publish(notification);
    }
}
