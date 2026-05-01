using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale
{
    public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateSaleHandler> _logger;

        public CreateSaleHandler(ISaleRepository saleRepository,
            IMapper mapper,
            ILogger<CreateSaleHandler> logger)
        {
            _saleRepository = saleRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
        {
            var sale = new Sale(DateTime.UtcNow,
                command.CustomerId,
                command.CustomerName,
                command.BranchId,
                command.BranchName);

            foreach (var item in command.Items)
            {
                var unitPrice = new Money(item.UnitPrice, item.Currency);
                sale.AddItem(item.ProductId, item.ProductName, item.Quantity, unitPrice);
            }

            var created = await _saleRepository.CreateAsync(sale, cancellationToken);

            _logger.LogInformation("SaleCreated: {@Event}", new SaleCreatedEvent(created));

            return _mapper.Map<CreateSaleResult>(created);
        }
    }
}
