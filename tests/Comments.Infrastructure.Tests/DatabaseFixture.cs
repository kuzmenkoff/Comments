using Comments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Xunit;

namespace Comments.Infrastructure.Tests;

/// <summary>Spins up a real MS SQL container, applies migrations, and hands out contexts.</summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var ctx = CreateContext();
        await ctx.Database.MigrateAsync();   // build the schema from our migrations
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    /// <summary>A fresh DbContext pointing at the container's database.</summary>
    public CommentsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CommentsDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;
        return new CommentsDbContext(options);
    }

    /// <summary>Clears data between tests so each test starts clean.</summary>
    public async Task ResetAsync()
    {
        await using var ctx = CreateContext();
        await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Attachments; DELETE FROM Comments;");
    }
}
