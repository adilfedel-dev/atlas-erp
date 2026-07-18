using AtlasERP.Core.Domain.Expenses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasERP.Infrastructure.PerCompany.Configurations;

public class TravelExpenseLineItemConfiguration : IEntityTypeConfiguration<TravelExpenseLineItem>
{
    public void Configure(EntityTypeBuilder<TravelExpenseLineItem> builder)
    {
        builder.ToTable("TravelExpenseLineItems");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Description).IsRequired().HasMaxLength(300);
        builder.Property(l => l.Amount).HasColumnType("decimal(18,2)");
    }
}
