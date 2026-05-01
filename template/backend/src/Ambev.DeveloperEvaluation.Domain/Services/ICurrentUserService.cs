using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Services;

public interface ICurrentUserService
{
    Guid Id { get; }
    string Name { get; }
    bool IsInRole(UserRole role);
    Guid? GetScopedCustomerId();
}
