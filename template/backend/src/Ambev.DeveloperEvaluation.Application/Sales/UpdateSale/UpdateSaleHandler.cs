using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale
{
    public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateSaleHandler> _logger;

        public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, ILogger<UpdateSaleHandler> logger)
        {
            _saleRepository = saleRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
        {
            var validator = new UpdateSaleCommandValidator();
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
            if (sale is null)
                throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");
            if (sale.IsCancelled)
                throw new DomainException("Cannot update a cancelled sale.");

            sale.UpdateHeader(
                command.SaleDate,
                command.CustomerId,
                command.CustomerName,
                command.BranchId,
                command.BranchName);

            var incomingIds = command.Items
                .Where(i => i.Id.HasValue)
                .Select(i => i.Id!.Value)
                .ToHashSet();

            foreach (var existingItem in sale.Items.Where(i => !i.IsCancelled))
            {
                if (!incomingIds.Contains(existingItem.Id))
                    sale.CancelItem(existingItem.Id);
            }

            foreach (var itemCmd in command.Items)
            {
                var price = new Money(itemCmd.UnitPrice, itemCmd.Currency);
                if (itemCmd.Id.HasValue)
                    sale.UpdateItem(itemCmd.Id.Value, itemCmd.Quantity, price);
                else
                    sale.AddItem(itemCmd.ProductId, itemCmd.ProductName, itemCmd.Quantity, price);
            }

            var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

            _logger.LogInformation("SaleModified: {@Event}", new SaleModifiedEvent(updated));

            return _mapper.Map<UpdateSaleResult>(updated);
        }
    }
}
