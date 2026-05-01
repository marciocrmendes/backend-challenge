using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.WebApi.Authorization.Handlers;
using Ambev.DeveloperEvaluation.WebApi.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

namespace Ambev.DeveloperEvaluation.WebApi.Authorization;

public static class AuthorizationPoliciesExtension
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.CanCreateSale, policy =>
                policy.RequireRole(
                    UserRole.Customer.ToString(),
                    UserRole.Manager.ToString(),
                    UserRole.Admin.ToString()))
            .AddPolicy(Policies.CanListSales, policy =>
                policy.RequireRole(
                    UserRole.Customer.ToString(),
                    UserRole.Manager.ToString(),
                    UserRole.Admin.ToString()))
            .AddPolicy(Policies.CanViewSale, policy =>
                policy.Requirements.Add(new SaleOwnerRequirement()))
            .AddPolicy(Policies.CanManageSales, policy =>
                policy.RequireRole(UserRole.Manager.ToString(), UserRole.Admin.ToString()))
            .AddPolicy(Policies.CanManageUsers, policy =>
                policy.RequireRole(UserRole.Admin.ToString()));

        services.AddSingleton<IAuthorizationHandler, SaleOwnerAuthorizationHandler>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
