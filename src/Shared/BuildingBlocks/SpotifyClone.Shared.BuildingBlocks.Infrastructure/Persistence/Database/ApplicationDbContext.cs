using Microsoft.EntityFrameworkCore;
using SpotifyClone.Shared.BuildingBlocks.Application.Outbox;

namespace SpotifyClone.Shared.BuildingBlocks.Infrastructure.Persistence.Database;

public abstract class ApplicationDbContext<TDbContext>(
    string schema, DbContextOptions<TDbContext> options)
    : DbContext(options)
    where TDbContext : DbContext
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(schema);
    }
}
