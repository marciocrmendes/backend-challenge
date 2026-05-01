using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class SalesValidatorsTests
{
    [Fact(DisplayName = "Given valid create sale command When validating Then succeeds")]
    public void CreateSaleValidator_WithValidCommand_Succeeds()
    {
        var validator = new CreateSaleCommandValidator();

        var result = validator.Validate(CreateSaleCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given invalid create sale command When validating Then returns expected errors")]
    public void CreateSaleValidator_WithInvalidCommand_Fails()
    {
        var command = CreateSaleCommand();
        command.CustomerId = Guid.Empty;
        command.CustomerName = string.Empty;
        command.Items[0].Quantity = 21;
        command.Items[0].UnitPrice = 0m;
        var validator = new CreateSaleCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName).Should().Contain(["CustomerId", "CustomerName", "Items[0].Quantity", "Items[0].UnitPrice"]);
    }

    [Fact(DisplayName = "Given valid update sale command When validating Then succeeds")]
    public void UpdateSaleValidator_WithValidCommand_Succeeds()
    {
        var validator = new UpdateSaleCommandValidator();

        var result = validator.Validate(UpdateSaleCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Given invalid update sale command When validating Then returns expected errors")]
    public void UpdateSaleValidator_WithInvalidCommand_Fails()
    {
        var command = UpdateSaleCommand();
        command.Id = Guid.Empty;
        command.SaleNumber = string.Empty;
        command.Items.Clear();
        var validator = new UpdateSaleCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName).Should().Contain(["Id", "SaleNumber", "Items"]);
    }

    [Theory(DisplayName = "Given query paging values When validating get all sales Then follows range rules")]
    [InlineData(1, 1, true)]
    [InlineData(1, 100, true)]
    [InlineData(0, 10, false)]
    [InlineData(1, 101, false)]
    public void GetAllSalesValidator_ValidatesPaging(int page, int pageSize, bool expectedValid)
    {
        var validator = new GetAllSalesQueryValidator();

        var result = validator.Validate(new GetAllSalesQuery(page, pageSize));

        result.IsValid.Should().Be(expectedValid);
    }

    [Fact(DisplayName = "Given empty get sale id When validating Then fails")]
    public void GetSaleValidator_WithEmptyId_Fails()
    {
        var validator = new GetSaleQueryValidator();

        var result = validator.Validate(new GetSaleQuery(Guid.Empty));

        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Given empty cancel sale id When validating Then fails")]
    public void CancelSaleValidator_WithEmptyId_Fails()
    {
        var validator = new CancelSaleCommandValidator();

        var result = validator.Validate(new CancelSaleCommand(Guid.Empty));

        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "Given empty cancel sale item ids When validating Then fails")]
    public void CancelSaleItemValidator_WithEmptyIds_Fails()
    {
        var validator = new CancelSaleItemCommandValidator();

        var result = validator.Validate(new CancelSaleItemCommand(Guid.Empty, Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName).Should().Contain(["SaleId", "ItemId"]);
    }

    private static CreateSaleCommand CreateSaleCommand() => new()
    {
        CustomerId = Guid.NewGuid(),
        CustomerName = "Customer",
        BranchId = Guid.NewGuid(),
        BranchName = "Branch",
        Items =
        [
            new CreateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product",
                Quantity = 5,
                UnitPrice = 10m,
                Currency = "BRL"
            }
        ]
    };

    private static UpdateSaleCommand UpdateSaleCommand() => new()
    {
        Id = Guid.NewGuid(),
        SaleNumber = "AMB-20260501-0000000001",
        SaleDate = DateTime.UtcNow,
        CustomerId = Guid.NewGuid(),
        CustomerName = "Customer",
        BranchId = Guid.NewGuid(),
        BranchName = "Branch",
        Items =
        [
            new UpdateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product",
                Quantity = 5,
                UnitPrice = 10m,
                Currency = "BRL"
            }
        ]
    };
}
