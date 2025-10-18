using FoodConnect.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoodConnect.Backend.Domain.Enums;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id)
                .HasConversion<int>() 
                .ValueGeneratedNever();
            builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
            builder.HasIndex(r => r.Name).IsUnique();

            builder.HasData(
                new Role { Id = RoleEnum.Admin, Name = RoleEnum.Admin.ToString() },
                new Role { Id = RoleEnum.Seller, Name = RoleEnum.Seller.ToString() },
                new Role { Id = RoleEnum.Buyer, Name = RoleEnum.Buyer.ToString() }
            );
        }
    }
}
