using Vale.Shared.World;

namespace Vale.Server.Data.Entities;

public sealed class StructureEntity
{
    public Guid StructureId { get; set; }

    public int ChunkX { get; set; }

    public int ChunkY { get; set; }

    public int TileX { get; set; }

    public int TileY { get; set; }

    public StructureType StructureType { get; set; }

    public string? OwnerPlayerId { get; set; }

    public DateTimeOffset PlacedAtUtc { get; set; }
}
