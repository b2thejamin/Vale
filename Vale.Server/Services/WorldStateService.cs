using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Vale.Server.Data;
using Vale.Server.Data.Entities;
using Vale.Shared.World;

namespace Vale.Server.Services;

public sealed class WorldStateService(IServiceScopeFactory scopeFactory, ILogger<WorldStateService> logger) : IWorldStateService
{
    private readonly ConcurrentDictionary<Guid, PlayerState> _players = new();
    private readonly ConcurrentDictionary<int, StructureState> _structures = new();

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        var persisted = await db.Structures.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var structure in persisted)
        {
            _structures[GetStructureKey(structure.ChunkX, structure.ChunkY, structure.TileX, structure.TileY)] = ToState(structure);
        }

        logger.LogInformation("Loaded {StructureCount} structures from persistence.", _structures.Count);
    }

    public WorldSnapshot BuildSnapshot()
    {
        return new WorldSnapshot(
            WorldConstants.ChunkSize,
            _players.Values.ToArray(),
            _structures.Values.ToArray());
    }

    public PlayerState AddPlayer(Guid connectionPlayerId, string displayName)
    {
        var trimmedName = string.IsNullOrWhiteSpace(displayName)
            ? $"Wanderer-{connectionPlayerId.ToString()[..6]}"
            : displayName.Trim();

        if (trimmedName.Length > 24)
        {
            trimmedName = trimmedName[..24];
        }

        var spawn = new PlayerState(
            connectionPlayerId,
            trimmedName,
            WorldConstants.ChunkSize / 2f,
            WorldConstants.ChunkSize / 2f,
            DateTimeOffset.UtcNow);

        _players[connectionPlayerId] = spawn;
        return spawn;
    }

    public bool RemovePlayer(Guid connectionPlayerId, out PlayerState? removedPlayer)
    {
        return _players.TryRemove(connectionPlayerId, out removedPlayer);
    }

    public bool TryMovePlayer(Guid connectionPlayerId, MovePlayerRequest request, out PlayerState? playerState)
    {
        playerState = null;

        if (!_players.TryGetValue(connectionPlayerId, out var current))
        {
            return false;
        }

        var targetX = Math.Clamp(request.TargetX, WorldConstants.WorldMinTile, WorldConstants.WorldMaxTile + 0.999f);
        var targetY = Math.Clamp(request.TargetY, WorldConstants.WorldMinTile, WorldConstants.WorldMaxTile + 0.999f);

        var deltaX = targetX - current.X;
        var deltaY = targetY - current.Y;
        var distance = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);

        if (distance > WorldConstants.MaxMoveStepPerUpdate)
        {
            var scale = WorldConstants.MaxMoveStepPerUpdate / distance;
            targetX = current.X + (deltaX * scale);
            targetY = current.Y + (deltaY * scale);
        }

        playerState = current with
        {
            X = targetX,
            Y = targetY,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        _players[connectionPlayerId] = playerState;
        return true;
    }

    public async Task<PlacementResult> TryPlaceStructureAsync(Guid connectionPlayerId, PlaceStructureRequest request, CancellationToken cancellationToken)
    {
        if (!_players.ContainsKey(connectionPlayerId))
        {
            return new PlacementResult(false, "Player not found.", null);
        }

        if (!Enum.IsDefined(request.StructureType))
        {
            return new PlacementResult(false, "Invalid structure type.", null);
        }

        if (request.TileX is < WorldConstants.WorldMinTile or > WorldConstants.WorldMaxTile ||
            request.TileY is < WorldConstants.WorldMinTile or > WorldConstants.WorldMaxTile)
        {
            return new PlacementResult(false, "Out of bounds.", null);
        }

        var key = GetStructureKey(0, 0, request.TileX, request.TileY);
        if (_structures.ContainsKey(key))
        {
            return new PlacementResult(false, "Tile already occupied.", null);
        }

        var structureState = new StructureState(
            Guid.NewGuid(),
            0,
            0,
            request.TileX,
            request.TileY,
            request.StructureType,
            connectionPlayerId.ToString("N"),
            DateTimeOffset.UtcNow);

        if (!_structures.TryAdd(key, structureState))
        {
            return new PlacementResult(false, "Tile already occupied.", null);
        }

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

            db.Structures.Add(new StructureEntity
            {
                StructureId = structureState.StructureId,
                ChunkX = structureState.ChunkX,
                ChunkY = structureState.ChunkY,
                TileX = structureState.TileX,
                TileY = structureState.TileY,
                StructureType = structureState.StructureType,
                OwnerPlayerId = structureState.OwnerPlayerId,
                PlacedAtUtc = structureState.PlacedAtUtc
            });

            await db.SaveChangesAsync(cancellationToken);
            return new PlacementResult(true, null, structureState);
        }
        catch (Exception ex)
        {
            _structures.TryRemove(key, out _);
            logger.LogError(ex, "Failed to persist structure placement at {TileX},{TileY}.", request.TileX, request.TileY);
            return new PlacementResult(false, "Failed to persist structure.", null);
        }
    }

    private static int GetStructureKey(int chunkX, int chunkY, int tileX, int tileY)
    {
        return HashCode.Combine(chunkX, chunkY, tileX, tileY);
    }

    private static StructureState ToState(StructureEntity entity)
    {
        return new StructureState(
            entity.StructureId,
            entity.ChunkX,
            entity.ChunkY,
            entity.TileX,
            entity.TileY,
            entity.StructureType,
            entity.OwnerPlayerId,
            entity.PlacedAtUtc);
    }
}
