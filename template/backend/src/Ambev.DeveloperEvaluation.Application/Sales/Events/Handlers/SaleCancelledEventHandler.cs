using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events.Handlers;

public class SaleCancelledEventHandler : INotificationHandler<SaleCancelledEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    public SaleCancelledEventHandler(IBus bus, ILogger<SaleCancelledEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Init SaleCancelled at {Time}: {@Event}", DateTime.UtcNow, notification);
        await _bus.Publish(notification);
    }
}
