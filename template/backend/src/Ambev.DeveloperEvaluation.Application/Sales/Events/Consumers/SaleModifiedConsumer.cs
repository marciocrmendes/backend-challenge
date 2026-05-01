using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events.Consumers;

public class SaleModifiedConsumer : IHandleMessages<SaleModifiedEvent>
{
    private readonly ILogger<SaleModifiedConsumer> _logger;

    public SaleModifiedConsumer(ILogger<SaleModifiedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Handle(SaleModifiedEvent message)
    {
        _logger.LogInformation("[Consumer] SaleModified received: SaleId={SaleId}, Total={Total}",
            message.Sale.Id,
            message.Sale.TotalAmount);

        return Task.CompletedTask;
    }
}
