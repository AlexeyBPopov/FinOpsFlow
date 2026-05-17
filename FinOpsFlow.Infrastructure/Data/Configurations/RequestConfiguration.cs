using FinOpsFlow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinOpsFlow.Infrastructure.Data.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(4000);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.Priority).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(r => r.CreatedBy)
            .WithMany(u => u.CreatedRequests)
            .HasForeignKey(r => r.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AssignedTo)
            .WithMany(u => u.AssignedRequests)
            .HasForeignKey(r => r.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Category)
            .WithMany(c => c.Requests)
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Priority);
        builder.HasIndex(r => r.CreatedAt);
        builder.HasIndex(r => r.AssignedToId);
    }
}