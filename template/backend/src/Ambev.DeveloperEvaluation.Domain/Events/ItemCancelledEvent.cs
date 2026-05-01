using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events
{
    public class ItemCancelledEvent
    {
        public Sale Sale { get; }
        public SaleItem CancelledItem { get; }

        public ItemCancelledEvent(Sale sale, SaleItem cancelledItem)
        {
            Sale = sale;
            CancelledItem = cancelledItem;
        }
    }
}
