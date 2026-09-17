using Comments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Comments.Infrastructure.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> b)
    {
        b.HasKey(c => c.Id);

        b.Property(c => c.UserName).IsRequired().HasMaxLength(64);
        b.Property(c => c.Email).IsRequired().HasMaxLength(254);
        b.Property(c => c.HomePage).HasMaxLength(2048);
        b.Property(c => c.Text).IsRequired().HasMaxLength(10_000);
        b.Property(c => c.CreatedAt).IsRequired();

        // Self-reference: a comment has many replies via ParentId.
        // Restrict delete: SQL Server forbids cascade on self-reference.
        b.HasMany(c => c.Replies)
            .WithOne()
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes that back the main-page queries:
        b.HasIndex(c => c.ParentId);                       // filter top-level / join children
        b.HasIndex(c => new { c.ParentId, c.CreatedAt });  // top-level LIFO paging
        b.HasIndex(c => c.UserName);                        // sort by user name
        b.HasIndex(c => c.Email);                           // sort by email
    }
}
