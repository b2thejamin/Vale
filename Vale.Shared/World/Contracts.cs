namespace Vale.Shared.World;

public sealed record JoinWorldRequest(string DisplayName);

public sealed record JoinWorldResponse(Guid PlayerId, WorldSnapshot Snapshot);

public sealed record WorldSnapshot(int ChunkSize, IReadOnlyCollection<PlayerState> Players, IReadOnlyCollection<StructureState> Structures);

public sealed record PlayerState(Guid PlayerId, string DisplayName, float X, float Y, DateTimeOffset UpdatedAtUtc);

public sealed record MovePlayerRequest(float TargetX, float TargetY);

public sealed record PlaceStructureRequest(int TileX, int TileY, StructureType StructureType);

public sealed record PlacementResult(bool Success, string? Error, StructureState? Structure);

public sealed record StructureState(
    Guid StructureId,
    int ChunkX,
    int ChunkY,
    int TileX,
    int TileY,
    StructureType StructureType,
    string? OwnerPlayerId,
    DateTimeOffset PlacedAtUtc);
