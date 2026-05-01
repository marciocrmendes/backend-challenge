using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public Money Discount { get; private set; } = null!;
    public Money TotalAmount { get; private set; } = null!;
    public bool IsCancelled { get; private set; }

    private SaleItem() { }

    public SaleItem(Guid productId, string productName, int quantity, Money unitPrice, Money discount)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (unitPrice is null)
            throw new ArgumentNullException(nameof(unitPrice));

        if (discount is null)
            throw new ArgumentNullException(nameof(discount));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));

        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = discount;

        CalculateTotalAmount();
    }

    private void CalculateTotalAmount()
    {
        var subtotal = UnitPrice.Multiply(Quantity);
        TotalAmount = Discount.Amount > 0 ? subtotal.Subtract(Discount) : subtotal;
    }

    public void Cancel()
    {
        IsCancelled = true;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));

        Quantity = newQuantity;
        CalculateTotalAmount();
    }

    public void UpdateDiscount(Money newDiscount)
    {
        ArgumentNullException.ThrowIfNull(newDiscount);

        Discount = newDiscount;
        CalculateTotalAmount();
    }
}
