using Ambev.DeveloperEvaluation.Application.Sales.Events.Handlers;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class SaleEventHandlersTests
{
    private readonly IBus _bus = Substitute.For<IBus>();

    [Fact(DisplayName = "Given sale created event When handling Then publishes event")]
    public async Task SaleCreatedEventHandler_PublishesEvent()
    {
        var notification = new SaleCreatedEvent(CreateSale());
        var handler = new SaleCreatedEventHandler(_bus, Substitute.For<ILogger<SaleCreatedEventHandler>>());

        await handler.Handle(notification, CancellationToken.None);

        await _bus.Received(1).Publish(notification);
    }

    [Fact(DisplayName = "Given sale modified event When handling Then publishes event")]
    public async Task SaleModifiedEventHandler_PublishesEvent()
    {
        var notification = new SaleModifiedEvent(CreateSale());
        var handler = new SaleModifiedEventHandler(_bus, Substitute.For<ILogger<SaleModifiedEventHandler>>());

        await handler.Handle(notification, CancellationToken.None);

        await _bus.Received(1).Publish(notification);
    }

    [Fact(DisplayName = "Given sale cancelled event When handling Then publishes event")]
    public async Task SaleCancelledEventHandler_PublishesEvent()
    {
        var notification = new SaleCancelledEvent(CreateSale());
        var handler = new SaleCancelledEventHandler(_bus, Substitute.For<ILogger<SaleCancelledEventHandler>>());

        await handler.Handle(notification, CancellationToken.None);

        await _bus.Received(1).Publish(notification);
    }

    [Fact(DisplayName = "Given item cancelled event When handling Then publishes event")]
    public async Task ItemCancelledEventHandler_PublishesEvent()
    {
        var sale = CreateSale();
        var item = sale.AddItem(Guid.NewGuid(), "Product", 1, new(10m));
        var notification = new ItemCancelledEvent(sale, item);
        var handler = new ItemCancelledEventHandler(_bus, Substitute.For<ILogger<ItemCancelledEventHandler>>());

        await handler.Handle(notification, CancellationToken.None);

        await _bus.Received(1).Publish(notification);
        notification.CancelledItem.Should().BeSameAs(item);
    }

    private static Sale CreateSale() =>
        new(DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");
}
