using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale
{
    public class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
    {
        public UpdateSaleCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty();
            RuleFor(c => c.SaleNumber).NotEmpty().MaximumLength(50);
            RuleFor(c => c.SaleDate).NotEmpty();
            RuleFor(c => c.CustomerId).NotEmpty();
            RuleFor(c => c.CustomerName).NotEmpty().MaximumLength(100);
            RuleFor(c => c.BranchId).NotEmpty();
            RuleFor(c => c.BranchName).NotEmpty().MaximumLength(100);
            RuleFor(c => c.Items).NotEmpty().WithMessage("At least one item is required.");
            RuleForEach(c => c.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty();
                item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(200);
                item.RuleFor(i => i.Quantity).InclusiveBetween(1, 20);
                item.RuleFor(i => i.UnitPrice).GreaterThan(0);
                item.RuleFor(i => i.Currency).NotEmpty().MaximumLength(3);
            });
        }
    }
}
