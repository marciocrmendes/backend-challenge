using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Ambev.DeveloperEvaluation.Functional.TestInfrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<DbContextOptions<DefaultContext>>();
            services.RemoveAll<DbContext>();

            services.AddDbContext<DefaultContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<DefaultContext>());

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        });
    }

    public HttpClient CreateAuthenticatedClient(string role, Guid? userId = null, string userName = "Test User")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, (userId ?? Guid.NewGuid()).ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserNameHeader, userName);
        return client;
    }

    public async Task AddSaleAsync(Sale sale)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await context.Sales.AddAsync(sale);
        await context.SaveChangesAsync();
    }

    public async Task<List<Sale>> GetSalesAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        return await context.Sales.Include(s => s.Items).ToListAsync();
    }
}
