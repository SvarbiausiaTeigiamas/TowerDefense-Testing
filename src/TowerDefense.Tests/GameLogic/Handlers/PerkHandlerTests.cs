using Microsoft.VisualBasic;
using Moq;
using TowerDefense.Api.GameLogic.GameState;
using TowerDefense.Api.GameLogic.Handlers;
using TowerDefense.Api.GameLogic.Perks;
using TowerDefense.Api.GameLogic.PerkStorage;
using TowerDefense.Api.GameLogic.Player;

namespace UnitTests.GameLogic.Handlers;

[Collection("Handlers")]
public class PerkHandlerTests : IDisposable
{
    private readonly FirstLevelPlayer _firstPlayer;
    private readonly FirstLevelPlayer _secondPlayer;
    private readonly State _gameState;

    public PerkHandlerTests()
    {
        _firstPlayer = new FirstLevelPlayer
        {
            Name = "Player1",
            PerkStorage = new FirstLevelPerkStorage(),
        };
        _secondPlayer = new FirstLevelPlayer
        {
            Name = "Player2",
            PerkStorage = new FirstLevelPerkStorage(),
        };

        _gameState = new State
        {
            Players = new IPlayer[] { _firstPlayer, _secondPlayer },
        };
    }

    [Fact]
    public void GetPerks_ShouldReturnPlayerPerkStorage()
    {
        // Arrange
        var playerName = "Player1";
        var FirstLevelPerkStorage = new FirstLevelPerkStorage();

        var firstLevelPlayer = new FirstLevelPlayer
        {
            Name = playerName,
            PerkStorage = FirstLevelPerkStorage,
        };

        State gameState = new State
        {
            Players = new IPlayer[] { firstLevelPlayer },
        };

        GameOriginator.GameState = gameState;

        var perkHandler = new PerkHandler();

        // Act
        var result = perkHandler.GetPerks(playerName);

        // Assert
        Assert.Equal(firstLevelPlayer.PerkStorage, result);
    }

    [Fact]
    public void UsePerk_ShouldHalveEnemyMoney_WhenPerkTypeIsCutInHalf()
    {
        // Arrange
        var perk = new CutInHalfPerk { Id = 1 };

        var player = new FirstLevelPlayer
        {
            Name = "player",
            Money = 1000,
            PerkStorage = new FirstLevelPerkStorage(),
        };
        var enemyPlayer = new FirstLevelPlayer
        {
            Name = "enemyPlayer",
            Money = 1000,
            PerkStorage = new FirstLevelPerkStorage(),
        };

        State gameState = new State
        {
            Players = new IPlayer[] { player, enemyPlayer },
        };

        GameOriginator.GameState = gameState;

        var perkHandler = new PerkHandler();

        // Act
        perkHandler.UsePerk("player", 1);

        // Assert
        Assert.Equal(500, enemyPlayer.Money);
        Assert.DoesNotContain(perk, player.PerkStorage.Perks);
    }

    [Fact]
    public void UsePerk_ShouldDoNothing_WhenPerkIsNotFound()
    {
        // Arrange
        var perk = new CutInHalfPerk { Id = 1 };

        var player = new FirstLevelPlayer
        {
            Name = "player",
            Money = 1000,
            PerkStorage = new FirstLevelPerkStorage(),
        };
        var enemyPlayer = new FirstLevelPlayer
        {
            Name = "enemyPlayer",
            Money = 1000,
            PerkStorage = new FirstLevelPerkStorage(),
        };

        State gameState = new State
        {
            Players = new IPlayer[] { player, enemyPlayer },
        };

        GameOriginator.GameState = gameState;

        var perkHandler = new PerkHandler();

        // Act
        perkHandler.UsePerk("player", -1);

        // Assert
        Assert.Equal(1000, enemyPlayer.Money);
    }

    public void Dispose()
    {
        GameOriginator.GameState = new State();
    }
}
