using FoodConnect.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Addresses");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.RecipientName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.Ward)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.District)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.Latitude)
                .HasPrecision(10, 7);

            builder.Property(a => a.Longitude)
                .HasPrecision(10, 7);

            builder.Property(a => a.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(a => a.Note)
                .HasMaxLength(500);

            builder.Property(a => a.AddressType)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(Domain.Enums.AddressTypeEnum.Home);

            builder.HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => new { a.UserId, a.IsDefault });
        }
    }
}
