using Ambev.DeveloperEvaluation.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Ambev.DeveloperEvaluation.Domain.Services;

public interface ICurrentUserService
{
    Guid Id { get; }
    string Name { get; }
    bool IsInRole(UserRole role);
    Guid? GetScopedCustomerId();
}


public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("No HTTP context available.");

    public Guid Id =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new InvalidOperationException("User ID claim not found."));

    public string Name =>
        User.FindFirst(ClaimTypes.Name)?.Value
        ?? throw new InvalidOperationException("User name claim not found.");

    public bool IsInRole(UserRole role) => User.IsInRole(role.ToString());

    public Guid? GetScopedCustomerId() => IsInRole(UserRole.Customer) ? Id : null;
}
