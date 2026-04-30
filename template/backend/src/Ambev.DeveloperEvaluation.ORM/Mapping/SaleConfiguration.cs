using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping
{
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.ToTable("Sales");

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id)
                   .HasColumnType("uuid")
                   .HasDefaultValueSql("gen_random_uuid()");

            builder.OwnsOne(s => s.SaleNumber, sn =>
            {
                sn.Property(x => x.Value)
                  .HasColumnName("SaleNumber")
                  .HasMaxLength(50)
                  .IsRequired();
            });

            builder.Property(s => s.SaleDate).IsRequired();

            builder.Property(s => s.CustomerId).IsRequired();
            builder.Property(s => s.CustomerName).IsRequired().HasMaxLength(100);

            builder.Property(s => s.BranchId).IsRequired();
            builder.Property(s => s.BranchName).IsRequired().HasMaxLength(100);

            builder.OwnsOne(s => s.TotalAmount, m =>
            {
                m.Property(x => x.Amount)
                 .HasColumnName("TotalAmount")
                 .HasColumnType("numeric(18,2)");
                m.Property(x => x.Currency)
                 .HasColumnName("TotalAmountCurrency")
                 .HasMaxLength(3);
            });

            builder.Property(s => s.IsCancelled).IsRequired().HasDefaultValue(false);
            builder.Property(s => s.CreatedAt).IsRequired();
            builder.Property(s => s.UpdatedAt);

            builder.HasMany(s => s.Items)
                   .WithOne()
                   .HasForeignKey("SaleId")
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(s => s.Items)
                   .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Metadata
                   .FindNavigation(nameof(Sale.Items))!
                   .SetField("_items");
        }
    }
}
