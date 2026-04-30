using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using System.Security.Cryptography;

namespace Ambev.DeveloperEvaluation.Domain.Entities
{
    public class Sale : BaseEntity
    {
        public SaleNumber SaleNumber { get; private set; } = null!;
        public DateTime SaleDate { get; private set; }

        public Guid CustomerId { get; private set; }
        public string CustomerName { get; private set; } = string.Empty;

        public Guid BranchId { get; private set; }
        public string BranchName { get; private set; } = string.Empty;

        public Money TotalAmount { get; private set; } = null!;
        public bool IsCancelled { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private readonly List<SaleItem> _items = [];
        public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

        private Sale() { }

        public Sale(
            DateTime saleDate,
            Guid customerId,
            string customerName,
            Guid branchId,
            string branchName)
        {
            Id = Guid.NewGuid();
            SaleNumber = new SaleNumber(GenerateSaleNumber());
            SaleDate = saleDate;
            CustomerId = customerId;
            CustomerName = customerName;
            BranchId = branchId;
            BranchName = branchName;
            TotalAmount = new Money(0);
            IsCancelled = false;
            CreatedAt = DateTime.UtcNow;
        }

        public SaleItem AddItem(Guid productId, string productName, int quantity, Money unitPrice)
        {
            if (quantity > 20)
                throw new DomainException("Cannot sell more than 20 identical items.");

            var discount = CalculateDiscount(quantity, unitPrice);
            var item = new SaleItem(productId, productName, quantity, unitPrice, discount);
            _items.Add(item);
            RecalculateTotal();
            return item;
        }

        public void UpdateItem(Guid itemId, int newQuantity, Money newUnitPrice)
        {
            if (newQuantity > 20)
                throw new DomainException("Cannot sell more than 20 identical items.");

            var item = GetItemOrThrow(itemId);
            var newDiscount = CalculateDiscount(newQuantity, newUnitPrice);
            item.UpdateQuantity(newQuantity);
            item.UpdateDiscount(newDiscount);
            RecalculateTotal();
            UpdatedAt = DateTime.UtcNow;
        }

        public void CancelItem(Guid itemId)
        {
            var item = GetItemOrThrow(itemId);
            item.Cancel();
            RecalculateTotal();
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            IsCancelled = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateHeader(
            DateTime saleDate,
            Guid customerId,
            string customerName,
            Guid branchId,
            string branchName)
        {
            SaleDate = saleDate;
            CustomerId = customerId;
            CustomerName = customerName;
            BranchId = branchId;
            BranchName = branchName;
            UpdatedAt = DateTime.UtcNow;
        }

        private string GenerateSaleNumber()
        {
            var guidBytes = Id.ToByteArray();
            var hashBytes = SHA256.HashData(guidBytes);

            var hashValue = BitConverter.ToUInt64(hashBytes, 0);

            var suffix = hashValue % 10_000_000_000;

            return $"AMB-{DateTime.UtcNow:yyyyMMdd}-{suffix:D10}";
        }

        private static Money CalculateDiscount(int quantity, Money unitPrice)
        {
            var rate = quantity switch
            {
                >= 10 and <= 20 => 0.20m,
                >= 4 => 0.10m,
                _ => 0m
            };

            return unitPrice
                .Multiply(quantity)
                .Multiply(rate);
        }

        private void RecalculateTotal()
        {
            var activeItems = _items.Where(i => !i.IsCancelled).ToList();
            if (activeItems.Count == 0)
            {
                TotalAmount = new Money(0, TotalAmount?.Currency ?? "BRL");
                return;
            }

            var currency = activeItems.First().TotalAmount.Currency;
            TotalAmount = activeItems.Aggregate(
                new Money(0, currency),
                (acc, item) => acc.Add(item.TotalAmount));
        }

        private SaleItem GetItemOrThrow(Guid itemId)
        {
            return _items.FirstOrDefault(i => i.Id == itemId)
                ?? throw new KeyNotFoundException($"SaleItem {itemId} not found.");
        }
    }
}
