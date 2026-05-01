using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.Functional.TestInfrastructure;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public class ApiScenariosTests
{
    [Fact(DisplayName = "Given valid user When creating and authenticating Then returns created and ok")]
    public async Task UsersAndAuth_WithValidData_CreateAndAuthenticate()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var password = "P@ssw0rd!123";
        var email = $"customer-{Guid.NewGuid():N}@example.com";

        var createResponse = await client.PostAsJsonAsync("/api/Users", new
        {
            Username = "customer-user",
            Password = password,
            Phone = "+5511999999999",
            Email = email,
            Status = UserStatus.Active,
            Role = UserRole.Customer
        });
        var authResponse = await client.PostAsJsonAsync("/api/Auth", new
        {
            Email = email,
            Password = password
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        authResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authJson = await ReadJsonAsync(authResponse);
        authJson.RootElement.GetProperty("data").GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Given anonymous request When listing sales Then returns unauthorized")]
    public async Task Sales_WhenAnonymous_ReturnsUnauthorized()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/Sales");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Given customer role When creating sale Then uses authenticated customer identity")]
    public async Task Sales_CreateAsCustomer_UsesAuthenticatedCustomer()
    {
        await using var factory = new CustomWebApplicationFactory();
        var customerId = Guid.NewGuid();
        var client = factory.CreateAuthenticatedClient(UserRole.Customer.ToString(), customerId, "Customer From Token");

        var response = await client.PostAsJsonAsync("/api/Sales", new
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = "Ignored Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items = new[]
            {
                new
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    Quantity = 5,
                    UnitPrice = 10m,
                    Currency = "BRL"
                }
            }
        });
        var sales = await factory.GetSalesAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        sales.Should().ContainSingle();
        sales.Single().CustomerId.Should().Be(customerId);
        sales.Single().CustomerName.Should().Be("Customer From Token");
        sales.Single().TotalAmount.Amount.Should().Be(45m);
    }

    [Fact(DisplayName = "Given invalid sale item When creating sale Then returns bad request")]
    public async Task Sales_CreateWithInvalidItem_ReturnsBadRequest()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateAuthenticatedClient(UserRole.Manager.ToString());

        var response = await client.PostAsJsonAsync("/api/Sales", new
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items = new[]
            {
                new
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    Quantity = 21,
                    UnitPrice = 10m,
                    Currency = "BRL"
                }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await ReadJsonAsync(response);
        json.RootElement.GetProperty("message").GetString().Should().Be("Validation Failed");
    }

    [Fact(DisplayName = "Given customer role When listing sales Then returns only scoped customer sales")]
    public async Task Sales_ListAsCustomer_ReturnsOnlyCustomerSales()
    {
        await using var factory = new CustomWebApplicationFactory();
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        await factory.AddSaleAsync(CreateSale(customerId));
        await factory.AddSaleAsync(CreateSale(otherCustomerId));
        var client = factory.CreateAuthenticatedClient(UserRole.Customer.ToString(), customerId);

        var response = await client.GetAsync("/api/Sales");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await ReadJsonAsync(response);
        var data = json.RootElement.GetProperty("data");
        data.GetProperty("totalCount").GetInt32().Should().Be(1);
        data.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact(DisplayName = "Given different customer When getting sale Then returns forbidden")]
    public async Task Sales_GetAsDifferentCustomer_ReturnsForbidden()
    {
        await using var factory = new CustomWebApplicationFactory();
        var sale = CreateSale(Guid.NewGuid());
        await factory.AddSaleAsync(sale);
        var client = factory.CreateAuthenticatedClient(UserRole.Customer.ToString(), Guid.NewGuid());

        var response = await client.GetAsync($"/api/Sales/{sale.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Given non admin role When listing users Then returns forbidden")]
    public async Task Users_ListAsCustomer_ReturnsForbidden()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateAuthenticatedClient(UserRole.Customer.ToString());

        var response = await client.GetAsync("/api/Users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static Sale CreateSale(Guid customerId)
    {
        var sale = new Sale(DateTime.UtcNow, customerId, "Customer", Guid.NewGuid(), "Branch");
        sale.AddItem(Guid.NewGuid(), "Product A", 4, new Money(10m));
        return sale;
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(stream);
    }
}
