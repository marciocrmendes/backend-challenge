using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleAdditionalTests
{
    [Fact(DisplayName = "Given new sale When created Then initializes identity, sale number and created event")]
    public void Constructor_WithValidData_InitializesSaleAndRaisesCreatedEvent()
    {
        var sale = CreateSale();

        sale.Id.Should().NotBeEmpty();
        sale.SaleNumber.Value.Should().StartWith("AMB-");
        sale.TotalAmount.Amount.Should().Be(0m);
        sale.IsCancelled.Should().BeFalse();
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCreatedEvent);
    }

    [Fact(DisplayName = "Given sale with active items When cancelling one item Then total ignores cancelled item and event is raised")]
    public void CancelItem_WithExistingItem_RecalculatesTotalAndRaisesEvent()
    {
        var sale = CreateSale();
        var first = sale.AddItem(Guid.NewGuid(), "Product A", 2, new Money(10m));
        sale.AddItem(Guid.NewGuid(), "Product B", 5, new Money(10m));

        sale.CancelItem(first.Id);

        sale.TotalAmount.Amount.Should().Be(45m);
        sale.UpdatedAt.Should().NotBeNull();
        sale.DomainEvents.Should().Contain(e => e is ItemCancelledEvent);
    }

    [Fact(DisplayName = "Given unknown item When cancelling or updating Then throws")]
    public void ItemOperations_WithUnknownItem_Throw()
    {
        var sale = CreateSale();

        Action cancel = () => sale.CancelItem(Guid.NewGuid());
        Action update = () => sale.UpdateItem(Guid.NewGuid(), 2, new Money(10m));

        cancel.Should().Throw<KeyNotFoundException>();
        update.Should().Throw<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing item When updating quantity and price Then recalculates discount and sale total")]
    public void UpdateItem_WithValidData_RecalculatesDiscountAndTotal()
    {
        var sale = CreateSale();
        var item = sale.AddItem(Guid.NewGuid(), "Product A", 2, new Money(10m));

        sale.UpdateItem(item.Id, 10, new Money(20m));

        item.Quantity.Should().Be(10);
        item.UnitPrice.Should().Be(new Money(20m));
        item.Discount.Amount.Should().Be(40m);
        sale.TotalAmount.Amount.Should().Be(160m);
        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given quantity above limit When updating item Then throws domain exception")]
    public void UpdateItem_WhenQuantityExceedsLimit_ThrowsDomainException()
    {
        var sale = CreateSale();
        var item = sale.AddItem(Guid.NewGuid(), "Product A", 1, new Money(10m));

        Action act = () => sale.UpdateItem(item.Id, 21, new Money(10m));

        act.Should().Throw<DomainException>()
            .WithMessage("*20*");
    }

    [Fact(DisplayName = "Given sale When updating header Then changes header and raises modified event")]
    public void UpdateHeader_WithValidData_UpdatesHeaderAndRaisesEvent()
    {
        var sale = CreateSale();
        var customerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var saleDate = DateTime.UtcNow.AddDays(-1);

        sale.UpdateHeader(saleDate, customerId, "New Customer", branchId, "New Branch");

        sale.SaleDate.Should().Be(saleDate);
        sale.CustomerId.Should().Be(customerId);
        sale.CustomerName.Should().Be("New Customer");
        sale.BranchId.Should().Be(branchId);
        sale.BranchName.Should().Be("New Branch");
        sale.UpdatedAt.Should().NotBeNull();
        sale.DomainEvents.Should().Contain(e => e is SaleModifiedEvent);
    }

    [Fact(DisplayName = "Given sale When cancelling sale Then marks cancelled and raises event")]
    public void Cancel_MarksSaleCancelledAndRaisesEvent()
    {
        var sale = CreateSale();

        sale.Cancel();

        sale.IsCancelled.Should().BeTrue();
        sale.UpdatedAt.Should().NotBeNull();
        sale.DomainEvents.Should().Contain(e => e is SaleCancelledEvent);
    }

    [Fact(DisplayName = "Given sale with domain events When clearing events Then removes all events")]
    public void ClearEvents_RemovesDomainEvents()
    {
        var sale = CreateSale();

        sale.ClearEvents();

        sale.DomainEvents.Should().BeEmpty();
    }

    private static Sale CreateSale() =>
        new(DateTime.UtcNow, Guid.NewGuid(), "Test Customer", Guid.NewGuid(), "Test Branch");
}
