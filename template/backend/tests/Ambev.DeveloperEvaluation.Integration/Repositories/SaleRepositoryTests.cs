using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.Integration.TestData;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

public class SaleRepositoryTests
{
    [Fact(DisplayName = "Given sale with items When creating Then can retrieve full aggregate")]
    public async Task CreateAsync_WhenSaleHasItems_PersistsAggregateWithCalculatedTotals()
    {
        var databaseName = Guid.NewGuid().ToString();
        var options = InMemoryContextFactory.CreateOptions(databaseName);
        Guid saleId;

        await using (var context = InMemoryContextFactory.CreateContext(options))
        {
            var repository = new SaleRepository(context);
            var sale = CreateSale(customerId: Guid.NewGuid(), saleDate: DateTime.UtcNow);
            sale.AddItem(Guid.NewGuid(), "Product A", 5, new Money(10m));
            sale.AddItem(Guid.NewGuid(), "Product B", 10, new Money(20m));

            var created = await repository.CreateAsync(sale);
            saleId = created.Id;
        }

        await using (var context = InMemoryContextFactory.CreateContext(options))
        {
            var repository = new SaleRepository(context);
            var stored = await repository.GetByIdAsync(saleId);

            stored.Should().NotBeNull();
            stored!.Items.Should().HaveCount(2);
            stored.TotalAmount.Amount.Should().Be(205m);
            stored.Items.Select(i => i.Discount.Amount).Should().BeEquivalentTo([5m, 40m]);
        }
    }

    [Fact(DisplayName = "Given sales from different customers When listing with customer filter Then returns scoped page")]
    public async Task GetAllAsync_WithCustomerFilter_ReturnsOnlyMatchingSales()
    {
        await using var context = InMemoryContextFactory.CreateContext();
        var repository = new SaleRepository(context);
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        var older = CreateSale(customerId, DateTime.UtcNow.AddDays(-2));
        var newer = CreateSale(customerId, DateTime.UtcNow.AddDays(-1));
        var other = CreateSale(otherCustomerId, DateTime.UtcNow);
        await repository.CreateAsync(older);
        await repository.CreateAsync(newer);
        await repository.CreateAsync(other);

        var (items, totalCount) = await repository.GetAllAsync(page: 1, pageSize: 10, customerId);

        totalCount.Should().Be(2);
        items.Select(s => s.Id).Should().ContainInOrder(newer.Id, older.Id);
        items.Should().OnlyContain(s => s.CustomerId == customerId);
    }

    [Fact(DisplayName = "Given sales page When listing Then applies pagination")]
    public async Task GetAllAsync_WithPagination_ReturnsRequestedPage()
    {
        await using var context = InMemoryContextFactory.CreateContext();
        var repository = new SaleRepository(context);
        var first = CreateSale(Guid.NewGuid(), DateTime.UtcNow.AddDays(-3));
        var second = CreateSale(Guid.NewGuid(), DateTime.UtcNow.AddDays(-2));
        var third = CreateSale(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1));
        await repository.CreateAsync(first);
        await repository.CreateAsync(second);
        await repository.CreateAsync(third);

        var (items, totalCount) = await repository.GetAllAsync(page: 2, pageSize: 1);

        totalCount.Should().Be(3);
        items.Should().ContainSingle(s => s.Id == second.Id);
    }

    [Fact(DisplayName = "Given existing sale When updating Then persists cancellation and item changes")]
    public async Task UpdateAsync_WhenSaleChanges_PersistsAggregateChanges()
    {
        var databaseName = Guid.NewGuid().ToString();
        var options = InMemoryContextFactory.CreateOptions(databaseName);
        Guid saleId;
        Guid itemId;

        await using (var context = InMemoryContextFactory.CreateContext(options))
        {
            var repository = new SaleRepository(context);
            var sale = CreateSale(Guid.NewGuid(), DateTime.UtcNow);
            var item = sale.AddItem(Guid.NewGuid(), "Product A", 2, new Money(50m));
            saleId = sale.Id;
            itemId = item.Id;
            await repository.CreateAsync(sale);
        }

        await using (var context = InMemoryContextFactory.CreateContext(options))
        {
            var repository = new SaleRepository(context);
            var sale = await repository.GetByIdAsync(saleId);
            sale!.UpdateItem(itemId, 4, new Money(50m));
            sale.Cancel();
            await repository.UpdateAsync(sale);
        }

        await using (var context = InMemoryContextFactory.CreateContext(options))
        {
            var repository = new SaleRepository(context);
            var stored = await repository.GetByIdAsync(saleId);

            stored.Should().NotBeNull();
            stored!.IsCancelled.Should().BeTrue();
            stored.Items.Single().Quantity.Should().Be(4);
            stored.Items.Single().Discount.Amount.Should().Be(20m);
            stored.TotalAmount.Amount.Should().Be(180m);
        }
    }

    [Fact(DisplayName = "Given sale id When deleting Then removes aggregate and reports missing deletes")]
    public async Task DeleteAsync_RemovesExistingSaleAndReturnsFalseForMissing()
    {
        await using var context = InMemoryContextFactory.CreateContext();
        var repository = new SaleRepository(context);
        var sale = await repository.CreateAsync(CreateSale(Guid.NewGuid(), DateTime.UtcNow));

        var deleted = await repository.DeleteAsync(sale.Id);
        var missingDelete = await repository.DeleteAsync(sale.Id);
        var stored = await repository.GetByIdAsync(sale.Id);

        deleted.Should().BeTrue();
        missingDelete.Should().BeFalse();
        stored.Should().BeNull();
    }

    private static Sale CreateSale(Guid customerId, DateTime saleDate) =>
        new(saleDate, customerId, "Customer", Guid.NewGuid(), "Branch");
}
