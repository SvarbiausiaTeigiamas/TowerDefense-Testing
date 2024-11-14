using TowerDefense.Api.GameLogic.GameState;
using TowerDefense.Api.GameLogic.Handlers;
using TowerDefense.Api.GameLogic.Player;
using TowerDefense.Api.GameLogic.Shop;

namespace UnitTests.GameLogic.Handlers;

[Collection("Handlers")]
public class ShopHandlerTests : IDisposable
{
    [Fact]
    public void GetPlayerShop_ShouldReturnPlayerShop()
    {
        var shop = new FirstLevelShop();

        // Arrange
        var player = new FirstLevelPlayer
        {
            Name = "Player1",
            Shop = shop
        };

        var gameState = new State
        {
            Players = new IPlayer[] { player },
        };

        GameOriginator.GameState = gameState;

        var shopHandler = new ShopHandler();

        // Act
        var result = shopHandler.GetPlayerShop("Player1");

        // Assert
        Assert.Equal(player.Shop, result);
    }

    [Fact]
    public void TryBuyItem_ShouldReturnFalse_WhenItemNotFound()
    {
        var shop = new FirstLevelShop();

        // Arrange
        var player = new FirstLevelPlayer
        {
            Name = "Player1",
            Shop = shop
        };

        var gameState = new State
        {
            Players = new IPlayer[] { player },
        };

        GameOriginator.GameState = gameState;

        var shopHandler = new ShopHandler();

        // Act
        var result = shopHandler.TryBuyItem("Player1", "InvalidItem");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryBuyItem_ShouldReturnFalse_WhenPlayerCannotAffordItem()
    {
        var shop = new FirstLevelShop();

        // Arrange
        var player = new FirstLevelPlayer
        {
            Name = "Player1",
            Money = 0,
            Shop = shop
        };

        var gameState = new State
        {
            Players = new IPlayer[] { player },
        };

        GameOriginator.GameState = gameState;

        var shopHandler = new ShopHandler();

        // Act
        var result = shopHandler.TryBuyItem("Player1", "Rockets");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryBuyItem_ShouldReturnTrue_WhenPlayerCanAffordItem()
    {
        var shop = new FirstLevelShop();

        // Arrange
        var player = new FirstLevelPlayer
        {
            Name = "Player1",
            Money = 1000,
            Shop = shop
        };

        var gameState = new State
        {
            Players = new IPlayer[] { player },
        };

        GameOriginator.GameState = gameState;

        var shopHandler = new ShopHandler();

        // Act
        var result = shopHandler.TryBuyItem("Player1", "Rockets");

        // Assert
        Assert.True(result);
    }

    public void Dispose()
    {
        GameOriginator.GameState = new State();
    }
}
