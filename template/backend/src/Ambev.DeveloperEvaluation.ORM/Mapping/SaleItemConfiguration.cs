using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
               .HasColumnType("uuid")
               .HasDefaultValueSql("gen_random_uuid()");

        builder.Property<Guid>("SaleId").IsRequired();

        builder.Property(i => i.ProductId).IsRequired();
        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.IsCancelled).IsRequired().HasDefaultValue(false);

        builder.OwnsOne(i => i.UnitPrice, m =>
        {
            m.Property(x => x.Amount)
             .HasColumnName("UnitPrice")
             .HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency)
             .HasColumnName("UnitPriceCurrency")
             .HasMaxLength(3);
        });

        builder.OwnsOne(i => i.Discount, m =>
        {
            m.Property(x => x.Amount)
             .HasColumnName("Discount")
             .HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency)
             .HasColumnName("DiscountCurrency")
             .HasMaxLength(3);
        });

        builder.OwnsOne(i => i.TotalAmount, m =>
        {
            m.Property(x => x.Amount)
             .HasColumnName("TotalAmount")
             .HasColumnType("numeric(18,2)");
            m.Property(x => x.Currency)
             .HasColumnName("TotalAmountCurrency")
             .HasMaxLength(3);
        });
    }
}
