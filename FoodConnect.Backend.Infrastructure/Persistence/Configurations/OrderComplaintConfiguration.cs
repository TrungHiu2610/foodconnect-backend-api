using FoodConnect.Backend.Domain.Entities;
using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Infrastructure.Persistence.Configurations
{
    public class OrderComplaintConfiguration : IEntityTypeConfiguration<OrderComplaint>
    {
        public void Configure(EntityTypeBuilder<OrderComplaint> builder)
        {
            builder.HasKey(oc => oc.Id);
            
            builder.Property(oc => oc.BuyerReason)
                .IsRequired()
                .HasMaxLength(2000);
            
            builder.Property(oc => oc.SellerResponse)
                .HasMaxLength(2000);
            
            builder.Property(oc => oc.SellerRespondedAt);
            
            builder.Property(oc => oc.SellerDesiredRefundPercentage)
                .HasColumnType("decimal(5,2)");
            
            builder.Property(oc => oc.SellerDesiredRefundAmount)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(oc => oc.IsSellerRefundFixedAmount)
                .IsRequired();
            
            builder.Property(oc => oc.AdminDecisionReason)
                .HasMaxLength(2000);
            
            builder.Property(oc => oc.AdminDecidedAt);
            
            builder.Property(oc => oc.ApprovedRefundAmount)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(oc => oc.IsApproved)
                .IsRequired();
            
            builder.Property(oc => oc.Status)
                .IsRequired()
                .HasConversion<int>();
            
            builder.Property(oc => oc.CompletedAt);
            
            builder.HasOne(oc => oc.Order)
                .WithOne(o => o.OrderComplaint)
                .HasForeignKey<OrderComplaint>(oc => oc.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(oc => oc.Buyer)
                .WithMany()
                .HasForeignKey(oc => oc.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(oc => oc.Seller)
                .WithMany()
                .HasForeignKey(oc => oc.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(oc => oc.Admin)
                .WithMany()
                .HasForeignKey(oc => oc.AdminId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(oc => oc.Assets)
                .WithOne(a => a.OrderComplaint)
                .HasForeignKey(a => a.OrderComplaintId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
