using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.WebApi.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Ambev.DeveloperEvaluation.WebApi.Authorization.Handlers;

public class SaleOwnerAuthorizationHandler
    : AuthorizationHandler<SaleOwnerRequirement, GetSaleResult>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SaleOwnerRequirement requirement,
        GetSaleResult resource)
    {
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (role == UserRole.Manager.ToString() || role == UserRole.Admin.ToString())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (role == UserRole.Customer.ToString())
        {
            var rawId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(rawId, out var userId) && resource.CustomerId == userId)
                context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
