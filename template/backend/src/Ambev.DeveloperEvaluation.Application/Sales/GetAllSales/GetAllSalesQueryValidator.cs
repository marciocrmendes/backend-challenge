using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;

public class GetAllSalesQueryValidator : AbstractValidator<GetAllSalesQuery>
{
    public GetAllSalesQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThan(0);
        RuleFor(q => q.PageSize).InclusiveBetween(1, 100);
    }
}
