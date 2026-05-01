using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class SalesHandlersTests
{
    private readonly ISaleRepository _saleRepository = Substitute.For<ISaleRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();

    [Fact(DisplayName = "Given valid create sale command When handling Then persists sale with calculated totals")]
    public async Task CreateSaleHandler_WithValidCommand_PersistsSaleWithCalculatedTotals()
    {
        Sale? capturedSale = null;
        var handler = new CreateSaleHandler(_saleRepository, _mapper);
        var command = CreateSaleCommand();

        _saleRepository.CreateAsync(Arg.Do<Sale>(sale => capturedSale = sale), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>())
            .Returns(call =>
            {
                var sale = call.Arg<Sale>();
                return new CreateSaleResult { Id = sale.Id, TotalAmount = sale.TotalAmount.Amount, Currency = sale.TotalAmount.Currency };
            });

        var result = await handler.Handle(command, CancellationToken.None);

        capturedSale.Should().NotBeNull();
        capturedSale!.CustomerId.Should().Be(command.CustomerId);
        capturedSale.Items.Should().HaveCount(2);
        capturedSale.TotalAmount.Amount.Should().Be(245m);
        result.TotalAmount.Should().Be(245m);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given missing sale When getting sale Then throws")]
    public async Task GetSaleHandler_WhenSaleDoesNotExist_Throws()
    {
        var id = Guid.NewGuid();
        var handler = new GetSaleHandler(_saleRepository, _mapper);
        _saleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        Func<Task> act = () => handler.Handle(new GetSaleQuery(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact(DisplayName = "Given existing sale When getting sale Then maps result")]
    public async Task GetSaleHandler_WhenSaleExists_ReturnsMappedResult()
    {
        var sale = CreateSale();
        var expected = new GetSaleResult { Id = sale.Id, TotalAmount = sale.TotalAmount.Amount };
        var handler = new GetSaleHandler(_saleRepository, _mapper);

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(expected);

        var result = await handler.Handle(new GetSaleQuery(sale.Id), CancellationToken.None);

        result.Should().BeSameAs(expected);
    }

    [Fact(DisplayName = "Given sales page query When handling Then returns mapped page data")]
    public async Task GetAllSalesHandler_WithPageQuery_ReturnsMappedPage()
    {
        var sales = new[] { CreateSale(), CreateSale() };
        var mapped = sales.Select(s => new GetSaleResult { Id = s.Id }).ToArray();
        var handler = new GetAllSalesHandler(_saleRepository, _mapper);
        var customerId = Guid.NewGuid();

        _saleRepository.GetAllAsync(2, 5, customerId, Arg.Any<CancellationToken>())
            .Returns((sales, 12));
        _mapper.Map<IEnumerable<GetSaleResult>>(sales).Returns(mapped);

        var result = await handler.Handle(new GetAllSalesQuery(2, 5, customerId), CancellationToken.None);

        result.Items.Should().BeEquivalentTo(mapped);
        result.TotalCount.Should().Be(12);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    [Fact(DisplayName = "Given existing sale When cancelling sale Then updates repository")]
    public async Task CancelSaleHandler_WhenSaleExists_CancelsAndUpdatesSale()
    {
        var sale = CreateSale();
        var handler = new CancelSaleHandler(_saleRepository);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Sale>());

        var result = await handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        result.Success.Should().BeTrue();
        sale.IsCancelled.Should().BeTrue();
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given missing sale When cancelling sale Then throws")]
    public async Task CancelSaleHandler_WhenSaleDoesNotExist_Throws()
    {
        var id = Guid.NewGuid();
        var handler = new CancelSaleHandler(_saleRepository);
        _saleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        Func<Task> act = () => handler.Handle(new CancelSaleCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given existing sale item When cancelling item Then cancels item and updates sale")]
    public async Task CancelSaleItemHandler_WhenSaleAndItemExist_CancelsItem()
    {
        var sale = CreateSale();
        var item = sale.Items.First();
        var handler = new CancelSaleItemHandler(_saleRepository);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Sale>());

        var result = await handler.Handle(new CancelSaleItemCommand(sale.Id, item.Id), CancellationToken.None);

        result.Success.Should().BeTrue();
        item.IsCancelled.Should().BeTrue();
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given cancelled sale When cancelling item Then throws domain exception")]
    public async Task CancelSaleItemHandler_WhenSaleIsCancelled_ThrowsDomainException()
    {
        var sale = CreateSale();
        sale.Cancel();
        var handler = new CancelSaleItemHandler(_saleRepository);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        Func<Task> act = () => handler.Handle(new CancelSaleItemCommand(sale.Id, sale.Items.First().Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*cancelled sale*");
    }

    [Fact(DisplayName = "Given update command When handling Then updates header, cancels missing items and adds new items")]
    public async Task UpdateSaleHandler_WithValidCommand_UpdatesSaleGraph()
    {
        var sale = CreateSale();
        var keptItem = sale.Items.First();
        var removedItem = sale.Items.Last();
        var command = new UpdateSaleCommand
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber.Value,
            SaleDate = DateTime.UtcNow.AddDays(-3),
            CustomerId = Guid.NewGuid(),
            CustomerName = "Updated Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Updated Branch",
            Items =
            [
                new UpdateSaleItemCommand
                {
                    Id = keptItem.Id,
                    ProductId = keptItem.ProductId,
                    ProductName = keptItem.ProductName,
                    Quantity = 10,
                    UnitPrice = 20m,
                    Currency = "BRL"
                },
                new UpdateSaleItemCommand
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "New Product",
                    Quantity = 1,
                    UnitPrice = 30m,
                    Currency = "BRL"
                }
            ]
        };
        var handler = new UpdateSaleHandler(_saleRepository, _mapper);

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Sale>());
        _mapper.Map<UpdateSaleResult>(Arg.Any<Sale>())
            .Returns(call => new UpdateSaleResult { Id = call.Arg<Sale>().Id, TotalAmount = call.Arg<Sale>().TotalAmount.Amount });

        var result = await handler.Handle(command, CancellationToken.None);

        sale.CustomerName.Should().Be("Updated Customer");
        keptItem.Quantity.Should().Be(10);
        removedItem.IsCancelled.Should().BeTrue();
        sale.Items.Should().HaveCount(3);
        result.TotalAmount.Should().Be(190m);
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given missing sale When updating Then throws")]
    public async Task UpdateSaleHandler_WhenSaleDoesNotExist_Throws()
    {
        var id = Guid.NewGuid();
        var handler = new UpdateSaleHandler(_saleRepository, _mapper);
        _saleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        Func<Task> act = () => handler.Handle(new UpdateSaleCommand { Id = id }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given cancelled sale When updating Then throws")]
    public async Task UpdateSaleHandler_WhenSaleIsCancelled_Throws()
    {
        var sale = CreateSale();
        sale.Cancel();
        var handler = new UpdateSaleHandler(_saleRepository, _mapper);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        Func<Task> act = () => handler.Handle(new UpdateSaleCommand { Id = sale.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*cancelled sale*");
    }

    private static CreateSaleCommand CreateSaleCommand() => new()
    {
        CustomerId = Guid.NewGuid(),
        CustomerName = "Customer",
        BranchId = Guid.NewGuid(),
        BranchName = "Branch",
        Items =
        [
            new CreateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product A",
                Quantity = 5,
                UnitPrice = 10m,
                Currency = "BRL"
            },
            new CreateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product B",
                Quantity = 10,
                UnitPrice = 25m,
                Currency = "BRL"
            }
        ]
    };

    private static Sale CreateSale()
    {
        var sale = new Sale(DateTime.UtcNow, Guid.NewGuid(), "Customer", Guid.NewGuid(), "Branch");
        sale.AddItem(Guid.NewGuid(), "Product A", 2, new Money(10m));
        sale.AddItem(Guid.NewGuid(), "Product B", 5, new Money(10m));
        return sale;
    }
}
