using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleItemTests
{
    [Fact(DisplayName = "Given valid data When creating sale item Then calculates total amount")]
    public void Constructor_WithValidData_CalculatesTotalAmount()
    {
        var item = new SaleItem(Guid.NewGuid(), "Product A", 5, new Money(20m), new Money(10m));

        item.Id.Should().NotBeEmpty();
        item.Quantity.Should().Be(5);
        item.TotalAmount.Should().Be(new Money(90m));
        item.IsCancelled.Should().BeFalse();
    }

    [Theory(DisplayName = "Given invalid quantity When creating or updating sale item Then throws")]
    [InlineData(0)]
    [InlineData(-1)]
    public void QuantityRules_WithInvalidQuantity_Throw(int quantity)
    {
        Action create = () => new SaleItem(Guid.NewGuid(), "Product A", quantity, new Money(20m), new Money(0m));
        var item = new SaleItem(Guid.NewGuid(), "Product A", 1, new Money(20m), new Money(0m));
        Action update = () => item.UpdateQuantity(quantity);

        create.Should().Throw<ArgumentException>();
        update.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Given null money values When creating or updating sale item Then throws")]
    public void MoneyArguments_WhenNull_Throw()
    {
        Action nullUnitPrice = () => new SaleItem(Guid.NewGuid(), "Product A", 1, null!, new Money(0m));
        Action nullDiscount = () => new SaleItem(Guid.NewGuid(), "Product A", 1, new Money(10m), null!);
        var item = new SaleItem(Guid.NewGuid(), "Product A", 1, new Money(10m), new Money(0m));
        Action updateDiscount = () => item.UpdateDiscount(null!);

        nullUnitPrice.Should().Throw<ArgumentNullException>();
        nullDiscount.Should().Throw<ArgumentNullException>();
        updateDiscount.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Given blank product name When creating sale item Then throws")]
    public void Constructor_WithBlankProductName_Throws()
    {
        Action act = () => new SaleItem(Guid.NewGuid(), " ", 1, new Money(10m), new Money(0m));

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "productName");
    }

    [Fact(DisplayName = "Given existing item When updating price, quantity and discount Then recalculates total")]
    public void UpdatePriceQuantityAndDiscount_RecalculatesTotal()
    {
        var item = new SaleItem(Guid.NewGuid(), "Product A", 2, new Money(50m), new Money(0m));

        item.UpdateUnitPrice(new Money(60m));
        item.UpdateQuantity(4);
        item.UpdateDiscount(new Money(20m));

        item.UnitPrice.Should().Be(new Money(60m));
        item.TotalAmount.Should().Be(new Money(220m));
    }

    [Fact(DisplayName = "Given active item When cancelling Then marks cancelled")]
    public void Cancel_MarksItemAsCancelled()
    {
        var item = new SaleItem(Guid.NewGuid(), "Product A", 1, new Money(10m), new Money(0m));

        item.Cancel();

        item.IsCancelled.Should().BeTrue();
    }
}
