using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.ValueObjects;

public class SaleNumberTests
{
    [Fact(DisplayName = "Given valid value When creating sale number Then stores value")]
    public void Constructor_WithValidValue_StoresValue()
    {
        var saleNumber = new SaleNumber("AMB-20260501-0000000001");

        saleNumber.Value.Should().Be("AMB-20260501-0000000001");
        saleNumber.ToString().Should().Be("AMB-20260501-0000000001");
        ((string)saleNumber).Should().Be("AMB-20260501-0000000001");
    }

    [Theory(DisplayName = "Given blank value When creating sale number Then throws")]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithBlankValue_Throws(string value)
    {
        Action act = () => new SaleNumber(value);

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "value");
    }

    [Fact(DisplayName = "Given sale numbers When comparing Then equality uses value")]
    public void Equality_UsesValue()
    {
        var left = new SaleNumber("AMB-20260501-0000000001");
        var same = new SaleNumber("AMB-20260501-0000000001");
        var different = new SaleNumber("AMB-20260501-0000000002");

        (left == same).Should().BeTrue();
        (left != same).Should().BeFalse();
        (left == different).Should().BeFalse();
        left.Equals((object)same).Should().BeTrue();
        left.GetHashCode().Should().Be(same.GetHashCode());
    }
}
