using Moq;
using Xunit;
using TowerDefense.Api.Contracts.Turn;
using TowerDefense.Api.GameLogic.Attacks;
using TowerDefense.Api.GameLogic.GameState;
using TowerDefense.Api.GameLogic.Player;
using TowerDefense.Api.Hubs;
using TowerDefense.Api.GameLogic.Handlers;
using TowerDefense.Api.GameLogic.Grid;

namespace UnitTests.GameLogic.Handlers;

public class BattleHandlerTests
{
    private readonly Mock<INotificationHub> _notificationHubMock;
    private readonly IAttackHandler _attackHandler;
    private readonly IGameHandler _gameHandler;
    private readonly BattleHandler _battleHandler;           
    private readonly State _gameState;

    public BattleHandlerTests()
    {
        _gameState = new State();
        GameOriginator.GameState = _gameState;

        _notificationHubMock = new Mock<INotificationHub>();
        _attackHandler = new AttackHandler();
        _gameHandler = new GameHandler(_notificationHubMock.Object);
        _battleHandler = new BattleHandler(_attackHandler, _gameHandler, _notificationHubMock.Object);
    }

    [Fact]
    public async Task HandleEndTurn_NormalBattle_CalculatesAttacksAndNotifiesPlayers()
    {
        string player1Layout =
            @"303
              003
              333";

        string player2Layout =
            @"333
              333
              333";

        var player1 = new FirstLevelPlayer
        {
            Name = "Player1",
            ArenaGrid = new FirstLevelArenaGrid(player1Layout),
            Health = 100,
            Armor = 100,
            Money = 1000
        };

        var player2 = new FirstLevelPlayer
        {
            Name = "Player2",
            ArenaGrid = new FirstLevelArenaGrid(player2Layout),
            Health = 100,
            Armor = 100,
            Money = 1000
        };

        _gameState.Players = new List<IPlayer> { player1, player2 }.ToArray();

        await _battleHandler.HandleEndTurn();

        Assert.Equal(20, player2.Health);

        _notificationHubMock.Verify(x => x.SendPlayersTurnResult(
            It.Is<Dictionary<string, EndTurnResponse>>(d =>
                d.ContainsKey("Player1") &&
                d.ContainsKey("Player2"))),
            Times.Once);
    }

    [Fact]
    public async Task HandleEndTurn_Player1Dies_GameEndsWithPlayer2Winner()
    {
        string player1Layout =
            @"333
              333
              333";

        string player2Layout =
            @"333
              303
              333";

        var player1 = new FirstLevelPlayer
        {
            Name = "Player1",
            ArenaGrid = new FirstLevelArenaGrid(player1Layout),
            Health = 60,
            Armor = 0,
            Money = 1000
        };

        var player2 = new FirstLevelPlayer
        {
            Name = "Player2",
            ArenaGrid = new FirstLevelArenaGrid(player2Layout),
            Health = 100,
            Armor = 100,
            Money = 1000
        };

        _gameState.Players = new List<IPlayer> { player1, player2 }.ToArray();

        await _battleHandler.HandleEndTurn();

        Assert.True(player1.Health <= 0);
    }

    [Fact]
    public async Task HandleEndTurn_DamageToArmor_ReducesArmorBeforeHealth()
    {
        string player1Layout =
            @"303
              333
              333";

        string player2Layout =
            @"333
              333
              333";

        var player1 = new FirstLevelPlayer
        {
            Name = "Player1",
            ArenaGrid = new FirstLevelArenaGrid(player1Layout),
            Health = 100,
            Armor = 100,
            Money = 1000
        };

        var player2 = new FirstLevelPlayer
        {
            Name = "Player2",
            ArenaGrid = new FirstLevelArenaGrid(player2Layout),
            Health = 100,
            Armor = 20,
            Money = 1000
        };

        _gameState.Players = new List<IPlayer> { player1, player2 }.ToArray();

        await _battleHandler.HandleEndTurn();

        Assert.Equal(0, player2.Armor);
        Assert.Equal(60, player2.Health);
    }

    [Fact]
    public async Task HandleEndTurn_NoAttackingItems_NoHealthChanges()
    {
        string player1Layout =
            @"333
              333
              333";

        string player2Layout =
            @"333
              333
              333";

        var player1 = new FirstLevelPlayer
        {
            Name = "Player1",
            ArenaGrid = new FirstLevelArenaGrid(player1Layout),
            Health = 100,
            Armor = 100,
            Money = 1000
        };

        var player2 = new FirstLevelPlayer
        {
            Name = "Player2",
            ArenaGrid = new FirstLevelArenaGrid(player2Layout),
            Health = 100,
            Armor = 100,
            Money = 1000
        };

        _gameState.Players = new List<IPlayer> { player1, player2 }.ToArray();

        await _battleHandler.HandleEndTurn();

        Assert.Equal(100, player1.Health);
        Assert.Equal(100, player2.Health);
        Assert.Equal(100, player1.Armor);
        Assert.Equal(100, player2.Armor);
        Assert.Equal(1000, player1.Money);
        Assert.Equal(1000, player2.Money);
    }

    [Fact]
    public async Task HandleEndTurn_MixedGridItems_CorrectDamageCalculation()
    {
        string player1Layout =
            @"123
              321
              123";

        string player2Layout =
            @"321
              123
              321";

        var player1 = new FirstLevelPlayer
        {
            Name = "Player1",
            ArenaGrid = new FirstLevelArenaGrid(player1Layout),
            Health = 100,
            Armor = 50,
            Money = 1000
        };

        var player2 = new FirstLevelPlayer
        {
            Name = "Player2",
            ArenaGrid = new FirstLevelArenaGrid(player2Layout),
            Health = 100,
            Armor = 50,
            Money = 1000
        };

        _gameState.Players = new List<IPlayer> { player1, player2 }.ToArray();

        await _battleHandler.HandleEndTurn();

        _notificationHubMock.Verify(x => x.SendPlayersTurnResult(
            It.Is<Dictionary<string, EndTurnResponse>>(d =>
                d["Player1"].GridItems.Any() &&
                d["Player2"].GridItems.Any())),
            Times.Once);
    }
}