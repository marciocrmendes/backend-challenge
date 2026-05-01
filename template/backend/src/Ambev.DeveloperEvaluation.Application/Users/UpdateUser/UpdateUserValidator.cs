using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50);

        RuleFor(x => x.Email)
            .SetValidator(new EmailValidator());

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$");

        RuleFor(x => x.Password)
            .SetValidator(new PasswordValidator()!)
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.Status)
            .NotEqual(UserStatus.Unknown);

        RuleFor(x => x.Role)
            .NotEqual(UserRole.None);
    }
}
