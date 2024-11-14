using Xunit;
using TowerDefense.Api.GameLogic.GameState;
using TowerDefense.Api.GameLogic.Handlers;
using TowerDefense.Api.GameLogic.Items;
using TowerDefense.Api.GameLogic.Items.Models;
using TowerDefense.Api.GameLogic.Player;
using System.Linq;

namespace UnitTests.GameLogic.Handlers;

[Collection("Handlers")]
public class InventoryHandlerTests : IDisposable
{
    private readonly InventoryHandler _sut;
    private readonly State _gameState;

    public InventoryHandlerTests()
    {
        GameOriginator.GameState = new State();
        _gameState = GameOriginator.GameState;
        _sut = new InventoryHandler();

        // Add sample players with inventories for testing
        _gameState.Players[0] = new FirstLevelPlayer
        {
            Name = "PlayerOne",
            Inventory = new Inventory { Items = new List<IItem> { new Rockets() } }
        };
        _gameState.Players[1] = new FirstLevelPlayer
        {
            Name = "PlayerTwo",
            Inventory = new Inventory { Items = new List<IItem> { new Shield() } }
        };
    }

    public void Dispose()
    {
        GameOriginator.GameState = new State();
    }

    [Fact]
    public void GetPlayerInventory_WithValidPlayerName_ReturnsCorrectInventory()
    {
        // Act
        var inventory = _sut.GetPlayerInventory("PlayerOne");

        // Assert
        Assert.NotNull(inventory);
        Assert.Single(inventory.Items);
        Assert.Equal("Rockets", inventory.Items[0].Id);
    }

    [Fact]
    public void GetPlayerInventory_WithInvalidPlayerName_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => _sut.GetPlayerInventory("NonExistentPlayer")
        );
    }

    [Fact]
    public void GetPlayerInventory_WithPlayerHavingMultipleItems_ReturnsCompleteInventory()
    {
        // Arrange
        _gameState.Players[0].Inventory.Items.Add(new Shield());

        // Act
        var inventory = _sut.GetPlayerInventory("PlayerOne");

        // Assert
        Assert.NotNull(inventory);
        Assert.Equal(2, inventory.Items.Count);
        Assert.Contains(inventory.Items, item => item.Id == "Rockets");
        Assert.Contains(inventory.Items, item => item.Id == "Shield");
    }

    [Fact]
    public void GetPlayerInventory_WithEmptyInventory_ReturnsEmptyInventory()
    {
        // Arrange
        _gameState.Players[1].Inventory.Items.Clear();

        // Act
        var inventory = _sut.GetPlayerInventory("PlayerTwo");

        // Assert
        Assert.NotNull(inventory);
        Assert.Empty(inventory.Items);
    }

    [Fact]
    public void GetPlayerInventory_PlayerWithoutInventory_ReturnsEmptyInventory()
    {
        // Arrange
        _gameState.Players[0].Inventory.Items.Clear();

        // Act
        var inventory = _sut.GetPlayerInventory("PlayerOne");

        // Assert
        Assert.NotNull(inventory);
        Assert.Empty(inventory.Items);
    }
}
