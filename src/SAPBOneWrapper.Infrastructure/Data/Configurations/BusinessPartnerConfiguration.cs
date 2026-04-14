using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAPBOneWrapper.Domain.Entities;

namespace SAPBOneWrapper.Infrastructure.Data.Configurations;

public class BusinessPartnerConfiguration : IEntityTypeConfiguration<BusinessPartner>
{
    public void Configure(EntityTypeBuilder<BusinessPartner> builder)
    {
        builder.HasKey(bp => bp.CardCode);

        builder.Property(bp => bp.CardCode)
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(bp => bp.CardName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(bp => bp.Phone).HasMaxLength(20);
        builder.Property(bp => bp.Email).HasMaxLength(100);
        builder.Property(bp => bp.Address).HasMaxLength(200);
        builder.Property(bp => bp.City).HasMaxLength(100);
        builder.Property(bp => bp.Country).HasMaxLength(3);
        builder.Property(bp => bp.PostCode).HasMaxLength(20);
        builder.Property(bp => bp.Currency).HasMaxLength(3);
        builder.Property(bp => bp.CreditLimit).HasColumnType("decimal(18,2)");
        builder.Property(bp => bp.TaxId).HasMaxLength(50);
        builder.Property(bp => bp.Remarks).HasMaxLength(500);

        builder.HasIndex(bp => bp.CardName);
        builder.HasIndex(bp => bp.CardType);
        builder.HasIndex(bp => bp.SyncStatus);
    }
}
