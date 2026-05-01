using Ambev.DeveloperEvaluation.Domain.Entities;
using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class ItemCancelledEvent : INotification
{
    public Sale Sale { get; }
    public SaleItem CancelledItem { get; }

    public ItemCancelledEvent(Sale sale, SaleItem cancelledItem)
    {
        Sale = sale;
        CancelledItem = cancelledItem;
    }
}
