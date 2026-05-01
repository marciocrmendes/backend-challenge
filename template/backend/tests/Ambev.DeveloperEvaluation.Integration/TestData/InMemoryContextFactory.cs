using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.Integration.TestData;

internal static class InMemoryContextFactory
{
    public static DefaultContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new DefaultContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }

    public static DefaultContext CreateContext(DbContextOptions<DefaultContext> options)
    {
        var context = new DefaultContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static DbContextOptions<DefaultContext> CreateOptions(string? databaseName = null) =>
        new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;
}
