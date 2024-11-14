using Moq;
using TowerDefense.Api.GameLogic.GameState;
using TowerDefense.Api.GameLogic.Grid;
using TowerDefense.Api.GameLogic.Handlers;
using TowerDefense.Api.GameLogic.Player;

namespace UnitTests.GameLogic.Handlers;

[Collection("Handlers")]
public class GridHandlerTests : IDisposable
{
    [Fact]
    public void GetGridItems_PlayerExists_ReturnsArenaGrid()
    {
        // Arrange
        var mockArenaGrid = new Mock<IArenaGrid>();
        var firstLevelPlayer = new FirstLevelPlayer
        {
            Name = "Player1",
            ArenaGrid = mockArenaGrid.Object,
        };

        State _gameState = new State { Players = new IPlayer[] { firstLevelPlayer } };

        GameOriginator.GameState = _gameState;

        var gridHandler = new GridHandler();

        // Act
        var result = gridHandler.GetGridItems("Player1");

        // Assert
        Assert.Equal(mockArenaGrid.Object, result);
    }

    [Fact]
    public void GetGridItems_PlayerDoesNotExist_ThrowsException()
    {
        // Arrange
        var mockArenaGrid = new Mock<IArenaGrid>();
        var firstLevelPlayer = new FirstLevelPlayer
        {
            Name = "Player1",
            ArenaGrid = mockArenaGrid.Object,
        };

        State _gameState = new State { Players = new IPlayer[] { firstLevelPlayer } };

        GameOriginator.GameState = _gameState;

        var gridHandler = new GridHandler();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => gridHandler.GetGridItems("Player2"));
    }

    public void Dispose()
    {
        GameOriginator.GameState = new State();
    }
}
