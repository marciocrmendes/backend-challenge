using Ambev.DeveloperEvaluation.Application.Sales.Events.Consumers;
using Ambev.DeveloperEvaluation.Domain.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class MessagingModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("RabbitMQ")
            ?? "amqp://guest:guest@localhost:5672";

        builder.Services.AddRebus(
            configure => configure
                .Transport(t => t.UseRabbitMq(connectionString, "app.main"))
                .Routing(r => r.TypeBased()
                    .Map<SaleCreatedEvent>("queue.sale.created")
                    .Map<SaleModifiedEvent>("queue.sale.modified")
                    .Map<SaleCancelledEvent>("queue.sale.cancelled")
                    .Map<ItemCancelledEvent>("queue.item.cancelled")),
            onCreated: async bus =>
            {
                await bus.Subscribe<SaleCreatedEvent>();
                await bus.Subscribe<SaleModifiedEvent>();
                await bus.Subscribe<SaleCancelledEvent>();
                await bus.Subscribe<ItemCancelledEvent>();
            }
        );

        builder.Services.AutoRegisterHandlersFromAssemblyOf<SaleCreatedConsumer>();
    }
}
