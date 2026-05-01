using Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;
using Ambev.DeveloperEvaluation.Common.Validation;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common;

public class ValidationBehaviorTests
{
    [Fact(DisplayName = "Given no validators When handling request Then invokes next delegate")]
    public async Task Handle_WithNoValidators_InvokesNext()
    {
        var behavior = new ValidationBehavior<GetAllSalesQuery, GetAllSalesResult>([]);
        var nextWasCalled = false;

        var result = await behavior.Handle(
            new GetAllSalesQuery(),
            () =>
            {
                nextWasCalled = true;
                return Task.FromResult(new GetAllSalesResult { Page = 1, PageSize = 10 });
            },
            CancellationToken.None);

        nextWasCalled.Should().BeTrue();
        result.Page.Should().Be(1);
    }

    [Fact(DisplayName = "Given valid request When handling with validators Then invokes next delegate")]
    public async Task Handle_WithValidRequest_InvokesNext()
    {
        var behavior = new ValidationBehavior<GetAllSalesQuery, GetAllSalesResult>([new GetAllSalesQueryValidator()]);

        var result = await behavior.Handle(
            new GetAllSalesQuery(2, 20),
            () => Task.FromResult(new GetAllSalesResult { Page = 2, PageSize = 20 }),
            CancellationToken.None);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(20);
    }

    [Fact(DisplayName = "Given invalid request When handling with validators Then throws validation exception")]
    public async Task Handle_WithInvalidRequest_ThrowsValidationException()
    {
        var behavior = new ValidationBehavior<GetAllSalesQuery, GetAllSalesResult>([new GetAllSalesQueryValidator()]);

        Func<Task> act = () => behavior.Handle(
            new GetAllSalesQuery(0, 101),
            () => Task.FromResult(new GetAllSalesResult()),
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Select(e => e.PropertyName).Should().Contain(["Page", "PageSize"]);
    }
}
