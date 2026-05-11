using Microsoft.AspNetCore.SignalR;
using Vale.Server.Services;
using Vale.Shared.World;

namespace Vale.Server.Hubs;

public sealed class WorldHub(IWorldStateService worldStateService, ILogger<WorldHub> logger) : Hub
{
    private static readonly string PlayerIdKey = "PlayerId";

    public async Task<JoinWorldResponse> JoinWorld(JoinWorldRequest request)
    {
        var playerId = Guid.NewGuid();
        var playerState = worldStateService.AddPlayer(playerId, request.DisplayName);

        Context.Items[PlayerIdKey] = playerId;

        var snapshot = worldStateService.BuildSnapshot();
        var response = new JoinWorldResponse(playerId, snapshot);

        await Clients.Others.SendAsync("PlayerJoined", playerState);

        logger.LogInformation("Player {DisplayName} ({PlayerId}) joined world.", playerState.DisplayName, playerId);

        return response;
    }

    public async Task MovePlayer(MovePlayerRequest request)
    {
        if (!TryGetPlayerId(out var playerId))
        {
            return;
        }

        if (!worldStateService.TryMovePlayer(playerId, request, out var playerState) || playerState is null)
        {
            return;
        }

        await Clients.All.SendAsync("PlayerMoved", playerState);
    }

    public async Task<PlacementResult> PlaceStructure(PlaceStructureRequest request)
    {
        if (!TryGetPlayerId(out var playerId))
        {
            return new PlacementResult(false, "Player not joined.", null);
        }

        var result = await worldStateService.TryPlaceStructureAsync(playerId, request, Context.ConnectionAborted);
        if (result.Success && result.Structure is not null)
        {
            await Clients.All.SendAsync("StructurePlaced", result.Structure);
        }

        return result;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (TryGetPlayerId(out var playerId) && worldStateService.RemovePlayer(playerId, out var removed) && removed is not null)
        {
            await Clients.All.SendAsync("PlayerLeft", removed.PlayerId);
            logger.LogInformation("Player {DisplayName} ({PlayerId}) left world.", removed.DisplayName, removed.PlayerId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private bool TryGetPlayerId(out Guid playerId)
    {
        playerId = Guid.Empty;

        if (!Context.Items.TryGetValue(PlayerIdKey, out var value) || value is not Guid resolved)
        {
            return false;
        }

        playerId = resolved;
        return true;
    }
}
