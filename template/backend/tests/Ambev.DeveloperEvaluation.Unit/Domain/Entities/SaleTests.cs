using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Theory(DisplayName = "Given quantity below 4 When adding item Then no discount is applied")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void AddItem_WhenQuantityIsBelowFour_ThenNoDiscountIsApplied(int quantity)
    {
        // Arrange
        var sale = CreateSale();
        var unitPrice = new Money(100m);

        // Act
        sale.AddItem(Guid.NewGuid(), "Product A", quantity, unitPrice);

        // Assert
        var item = sale.Items.Single();
        item.Discount.Amount.Should().Be(0m);
        item.TotalAmount.Amount.Should().Be(100m * quantity);
    }

    [Theory(DisplayName = "Given quantity between 4 and 9 When adding item Then 10% discount is applied")]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(9)]
    public void AddItem_WhenQuantityIsBetweenFourAndNine_ThenTenPercentDiscountIsApplied(int quantity)
    {
        // Arrange
        var sale = CreateSale();
        var unitPrice = new Money(100m);

        // Act
        sale.AddItem(Guid.NewGuid(), "Product A", quantity, unitPrice);

        // Assert
        var item = sale.Items.Single();
        var expectedDiscount = 100m * quantity * 0.10m;
        var expectedTotal = 100m * quantity - expectedDiscount;

        item.Discount.Amount.Should().Be(expectedDiscount);
        item.TotalAmount.Amount.Should().Be(expectedTotal);
    }

    [Theory(DisplayName = "Given quantity between 10 and 20 When adding item Then 20% discount is applied")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void AddItem_WhenQuantityIsBetweenTenAndTwenty_ThenTwentyPercentDiscountIsApplied(int quantity)
    {
        // Arrange
        var sale = CreateSale();
        var unitPrice = new Money(100m);

        // Act
        sale.AddItem(Guid.NewGuid(), "Product A", quantity, unitPrice);

        // Assert
        var item = sale.Items.Single();
        var expectedDiscount = 100m * quantity * 0.20m;
        var expectedTotal = 100m * quantity - expectedDiscount;

        item.Discount.Amount.Should().Be(expectedDiscount);
        item.TotalAmount.Amount.Should().Be(expectedTotal);
    }

    [Fact(DisplayName = "Given quantity above 20 When adding item Then throws DomainException")]
    public void AddItem_WhenQuantityExceedsTwenty_ThenThrowsDomainException()
    {
        // Arrange
        var sale = CreateSale();

        // Act
        Action act = () => sale.AddItem(Guid.NewGuid(), "Product A", 21, new Money(100m));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*20*");
    }

    [Fact(DisplayName = "Given quantity exactly 4 When adding item Then 10% discount applied at lower boundary")]
    public void AddItem_WhenQuantityIsExactlyFour_ThenTenPercentDiscountIsApplied()
    {
        // Arrange
        var sale = CreateSale();
        var unitPrice = new Money(200m);
        const int quantity = 4;

        // Act
        sale.AddItem(Guid.NewGuid(), "Product A", quantity, unitPrice);

        // Assert
        var item = sale.Items.Single();
        item.Discount.Amount.Should().Be(200m * quantity * 0.10m);
    }

    [Fact(DisplayName = "Given quantity exactly 10 When adding item Then 20% discount applied at lower boundary")]
    public void AddItem_WhenQuantityIsExactlyTen_ThenTwentyPercentDiscountIsApplied()
    {
        // Arrange
        var sale = CreateSale();
        var unitPrice = new Money(200m);
        const int quantity = 10;

        // Act
        sale.AddItem(Guid.NewGuid(), "Product A", quantity, unitPrice);

        // Assert
        var item = sale.Items.Single();
        item.Discount.Amount.Should().Be(200m * quantity * 0.20m);
    }

    [Fact(DisplayName = "Given sale with items across all discount tiers When computing total Then total reflects correct discounts")]
    public void TotalAmount_WhenSaleHasItemsAcrossAllDiscountTiers_ThenReflectsAllDiscountsCorrectly()
    {
        // Arrange
        var sale = CreateSale();

        // qty 3  → no discount:  3 × 50        = 150.00
        // qty 5  → 10% discount: 5 × 80 × 0.90 = 360.00
        // qty 10 → 20% discount: 10 × 120 × 0.80 = 960.00

        // Act
        sale.AddItem(Guid.NewGuid(), "Product A", 3, new Money(50m));
        sale.AddItem(Guid.NewGuid(), "Product B", 5, new Money(80m));
        sale.AddItem(Guid.NewGuid(), "Product C", 10, new Money(120m));

        // Assert
        var expectedTotal = (50m * 3) + (80m * 5 * 0.90m) + (120m * 10 * 0.80m);
        sale.TotalAmount.Amount.Should().Be(expectedTotal);
    }

    [Fact(DisplayName = "Given sale with no items When checking total Then total is zero")]
    public void TotalAmount_WhenSaleHasNoItems_ThenIsZero()
    {
        // Arrange / Act
        var sale = CreateSale();

        // Assert
        sale.TotalAmount.Amount.Should().Be(0m);
    }

    private static Sale CreateSale() =>
        new(DateTime.UtcNow, Guid.NewGuid(), "Test Customer", Guid.NewGuid(), "Test Branch");
}
