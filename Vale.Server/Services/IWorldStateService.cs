using Vale.Shared.World;

namespace Vale.Server.Services;

public interface IWorldStateService
{
    Task InitializeAsync(CancellationToken cancellationToken);
    WorldSnapshot BuildSnapshot();
    PlayerState AddPlayer(Guid connectionPlayerId, string displayName);
    bool RemovePlayer(Guid connectionPlayerId, out PlayerState? removedPlayer);
    bool TryMovePlayer(Guid connectionPlayerId, MovePlayerRequest request, out PlayerState? playerState);
    Task<PlacementResult> TryPlaceStructureAsync(Guid connectionPlayerId, PlaceStructureRequest request, CancellationToken cancellationToken);
}
