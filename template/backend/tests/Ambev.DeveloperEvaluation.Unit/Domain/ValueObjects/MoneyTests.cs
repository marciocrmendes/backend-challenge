using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.ValueObjects;

public class MoneyTests
{
    [Fact(DisplayName = "Given valid amount When creating money Then stores amount and default currency")]
    public void Constructor_WithValidAmount_StoresAmountAndDefaultCurrency()
    {
        var money = new Money(25.5m);

        money.Amount.Should().Be(25.5m);
        money.Currency.Should().Be("BRL");
        money.ToString().Should().Be("BRL 25.50");
    }

    [Theory(DisplayName = "Given invalid money data When creating money Then throws")]
    [InlineData(-1, "BRL", "amount")]
    [InlineData(1, "", "currency")]
    [InlineData(1, " ", "currency")]
    public void Constructor_WithInvalidData_Throws(decimal amount, string currency, string parameterName)
    {
        Action act = () => new Money(amount, currency);

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == parameterName);
    }

    [Fact(DisplayName = "Given same currency When adding money Then returns summed amount")]
    public void Add_WithSameCurrency_ReturnsSum()
    {
        var result = new Money(10m, "USD").Add(new Money(15m, "USD"));

        result.Should().Be(new Money(25m, "USD"));
    }

    [Fact(DisplayName = "Given different currencies When adding money Then throws")]
    public void Add_WithDifferentCurrencies_Throws()
    {
        Action act = () => new Money(10m, "BRL").Add(new Money(10m, "USD"));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact(DisplayName = "Given same currency When subtracting money Then returns difference")]
    public void Subtract_WithSameCurrency_ReturnsDifference()
    {
        var result = new Money(20m).Subtract(new Money(7.5m));

        result.Should().Be(new Money(12.5m));
    }

    [Fact(DisplayName = "Given subtraction would be negative When subtracting money Then throws")]
    public void Subtract_WhenResultWouldBeNegative_Throws()
    {
        Action act = () => new Money(5m).Subtract(new Money(6m));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*negative*");
    }

    [Fact(DisplayName = "Given negative multiplier When multiplying money Then throws")]
    public void Multiply_WithNegativeMultiplier_Throws()
    {
        Action act = () => new Money(5m).Multiply(-1m);

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "multiplier");
    }

    [Fact(DisplayName = "Given equal and different money values When comparing Then equality operators reflect value")]
    public void EqualityOperators_CompareByAmountAndCurrency()
    {
        var left = new Money(10m, "BRL");
        var same = new Money(10m, "BRL");
        var different = new Money(10m, "USD");

        (left == same).Should().BeTrue();
        (left != same).Should().BeFalse();
        (left == different).Should().BeFalse();
        left.Equals((object)same).Should().BeTrue();
        left.GetHashCode().Should().Be(same.GetHashCode());
    }
}
