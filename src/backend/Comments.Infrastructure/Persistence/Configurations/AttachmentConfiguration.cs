using Comments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Comments.Infrastructure.Persistence.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> b)
    {
        b.HasKey(a => a.Id);

        b.Property(a => a.FileName).IsRequired().HasMaxLength(260);
        b.Property(a => a.ContentType).IsRequired().HasMaxLength(100);
        b.Property(a => a.Content).IsRequired();  // varbinary(max)

        // One-to-one: Attachment holds the FK CommentId.
        b.HasOne(a => a.Comment)
            .WithOne(c => c.Attachment)
            .HasForeignKey<Attachment>(a => a.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(a => a.CommentId).IsUnique();
    }
}
