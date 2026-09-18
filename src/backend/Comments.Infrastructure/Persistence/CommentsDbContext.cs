using Comments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Comments.Infrastructure.Persistence;

/// <summary>EF Core database session for comments and attachments.</summary>
public class CommentsDbContext(DbContextOptions<CommentsDbContext> options) : DbContext(options)
{
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up all IEntityTypeConfiguration<> in this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommentsDbContext).Assembly);
    }
}