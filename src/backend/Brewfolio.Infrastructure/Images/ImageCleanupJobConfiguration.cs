using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brewfolio.Infrastructure.Images;

internal sealed class ImageCleanupJobConfiguration : IEntityTypeConfiguration<ImageCleanupJob>
{
    public void Configure(EntityTypeBuilder<ImageCleanupJob> builder)
    {
        builder.ToTable("ImageCleanupJobs");
        builder.HasKey(job => job.Id);
        builder.Property(job => job.ImageKey).HasMaxLength(320).IsRequired();
        builder.HasIndex(job => job.NextAttemptAt);
    }
}
