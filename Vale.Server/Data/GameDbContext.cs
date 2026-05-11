using Microsoft.EntityFrameworkCore;
using Vale.Server.Data.Entities;

namespace Vale.Server.Data;

public sealed class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    public DbSet<StructureEntity> Structures => Set<StructureEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StructureEntity>(builder =>
        {
            builder.HasKey(s => s.StructureId);
            builder.Property(s => s.OwnerPlayerId).HasMaxLength(64);
            builder.HasIndex(s => new { s.ChunkX, s.ChunkY, s.TileX, s.TileY }).IsUnique();
        });
    }
}
