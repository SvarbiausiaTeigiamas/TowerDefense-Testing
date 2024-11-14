using Xunit;
using TowerDefense.Api.GameLogic.GameState;
using TowerDefense.Api.GameLogic.Handlers;
using TowerDefense.Api.GameLogic.Player;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests.GameLogic.Handlers;

public class PlayerHandlerTests : IDisposable
{
    private readonly PlayerHandler _sut;
    private readonly State _gameState;

    public PlayerHandlerTests()
    {
        GameOriginator.GameState = new State();
        _gameState = GameOriginator.GameState;
        _sut = new PlayerHandler();

        // Add sample players to the game state for testing
        _gameState.Players[0] = new FirstLevelPlayer { Name = "PlayerOne" };
        _gameState.Players[1] = new FirstLevelPlayer { Name = "PlayerTwo" };
    }

    public void Dispose()
    {
        GameOriginator.GameState = new State();
    }

    [Fact]
    public void GetPlayer_WithValidPlayerName_ReturnsCorrectPlayer()
    {
        // Act
        var player = _sut.GetPlayer("PlayerOne");

        // Assert
        Assert.NotNull(player);
        Assert.Equal("PlayerOne", player.Name);
    }

    [Fact]
    public void GetPlayer_WithInvalidPlayerName_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.GetPlayer("NonExistentPlayer"));
    }

    [Fact]
    public void GetPlayers_ReturnsAllPlayersInGameState()
    {
        // Act
        var players = _sut.GetPlayers();

        // Assert
        Assert.NotNull(players);
        Assert.Equal(2, players.Count());
        Assert.Contains(players, p => p.Name == "PlayerOne");
        Assert.Contains(players, p => p.Name == "PlayerTwo");
    }

    [Fact]
    public void GetPlayer_WithVariousNames_ReturnsCorrectPlayers()
    {
        var playerOne = _sut.GetPlayer("PlayerOne");
        var playerTwo = _sut.GetPlayer("PlayerTwo");

        Assert.Equal("PlayerOne", playerOne.Name);
        Assert.Equal("PlayerTwo", playerTwo.Name);
    }

    [Fact]
    public void GetPlayers_WithoutAnyModification_ReturnsInitialPlayers()
    {
        var players = _sut.GetPlayers();

        Assert.Equal(2, players.Count());
        Assert.Contains(players, p => p.Name == "PlayerOne");
        Assert.Contains(players, p => p.Name == "PlayerTwo");
    }

    [Fact]
    public void GetPlayers_DoesNotContainUnaddedPlayer()
    {
        var players = _sut.GetPlayers();

        Assert.DoesNotContain(players, p => p.Name == "NonExistentPlayer");
    }
}
