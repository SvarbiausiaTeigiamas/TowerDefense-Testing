using Moq;
using TowerDefense.Api.Contracts.Turn;
using TowerDefense.Api.GameLogic.Attacks;
using TowerDefense.Api.GameLogic.GameState;
using TowerDefense.Api.GameLogic.Grid;
using TowerDefense.Api.GameLogic.Player;
using TowerDefense.Api.Hubs;
using TowerDefense.Api.GameLogic.Handlers;

namespace UnitTests.GameLogic.Handlers;

public class BattleHandlerTests
{
    private (
        IAttackHandler attackHandler,
        IGameHandler gameHandler,
        Mock<INotificationHub> notificationHub,
        IPlayer player1,
        IPlayer player2,
        IArenaGrid player1Grid,
        IArenaGrid player2Grid,
        BattleHandler battleHandler
    ) MakeBattleHandler()
    {
        GameOriginator.GameState = new State();  
        
        var notificationHubMock = new Mock<INotificationHub>();
        var attackHandler= new AttackHandler();
        var gameHandler= new GameHandler(notificationHubMock.Object);
        var player1= new FirstLevelPlayer();
        var player2= new FirstLevelPlayer();
        var player1Grid= new FirstLevelArenaGrid();
        var player2Grid=  new FirstLevelArenaGrid();
        player1.ArenaGrid = player1Grid;
        player2.ArenaGrid = player2Grid;
        player1.Name = "name1";
        player2.Name = "name2";

        GameOriginator.GameState.Players = new List<IPlayer>
        {
            player1,
            player2
        }.ToArray();

        var sut = new BattleHandler(
            attackHandler,
            gameHandler,
            notificationHubMock.Object
        );
        return (
            attackHandler,
            gameHandler,
            notificationHubMock,
            player1,
            player2,
            player1Grid,
            player2Grid,
            sut
        );
    }

    [Fact]
    public async Task HandleEndTurn_NormalBattle_ProcessesAttacksAndNotifiesPlayers()
    {
        var (
            attackHandler,
            gameHandler,
            notificationHubMock,
            player1,
            player2,
            player1Grid,
            player2Grid,
            sut
        ) = MakeBattleHandler();

        var player1Attack = new Attack
        {
            DirectAttackDeclarations = new List<AttackDeclaration> { new() { Damage = 10 } },
            ItemAttackDeclarations = new List<AttackDeclaration> { new() { EarnedMoney = 50 } },
        };

        var player2Attack = new Attack
        {
            DirectAttackDeclarations = new List<AttackDeclaration> { new() { Damage = 20 } },
            ItemAttackDeclarations = new List<AttackDeclaration> { new() { EarnedMoney = 30 } },
        };

        await sut.HandleEndTurn();

        Assert.Equal(100, player1.Armor);
        Assert.Equal(100, player2.Armor);

        notificationHubMock.Verify(
            x => x.SendPlayersTurnResult(It.IsAny<Dictionary<string, EndTurnResponse>>()),
            Times.Once
        );
    }

    /*
    [Fact]
    public async Task HandleEndTurn_DamageExceedsArmor_ReducesHealth()
    {
        var (
            attackHandlerMock,
            gameHandlerMock,
            notificationHubMock,
            player1Mock,
            player2Mock,
            player1GridMock,
            player2GridMock,
            sut
        ) = MakeBattleHandler();
        
        var player1Attack = new Attack
        {
            DirectAttackDeclarations = new List<AttackDeclaration> { new() { Damage = 70 } },
            ItemAttackDeclarations = new List<AttackDeclaration>(),
        };

        attackHandlerMock
            .Setup(x => x.HandlePlayerAttacks(player1GridMock.Object, player2GridMock.Object))
            .Returns(player1Attack);
        attackHandlerMock
            .Setup(x => x.HandlePlayerAttacks(player2GridMock.Object, player1GridMock.Object))
            .Returns(player1Attack);

        await sut.HandleEndTurn();

        Assert.Equal(0, player2Mock.Object.Armor);
        Assert.Equal(80, player2Mock.Object.Health);
    }

    [Fact]
    public async Task HandleEndTurn_Player1Dies_FinishesGameWithPlayer2AsWinner()
    {
        var (
            attackHandlerMock,
            gameHandlerMock,
            notificationHubMock,
            player1Mock,
            player2Mock,
            player1GridMock,
            player2GridMock,
            sut
        ) = MakeBattleHandler();
        
        var player2Attack = new Attack
        {
            DirectAttackDeclarations = new List<AttackDeclaration> { new() { Damage = 150 } },
            ItemAttackDeclarations = new List<AttackDeclaration>(),
        };

        attackHandlerMock
            .Setup(x => x.HandlePlayerAttacks(player2GridMock.Object, player1GridMock.Object))
            .Returns(player2Attack);

        await sut.HandleEndTurn();

        Assert.Equal(0, player1Mock.Object.Health);
        gameHandlerMock.Verify(x => x.FinishGame(player2Mock.Object), Times.Once);
        notificationHubMock.Verify(
            x => x.SendPlayersTurnResult(It.IsAny<Dictionary<string, EndTurnResponse>>()),
            Times.Never
        );
    }
    */

    /*[Fact]
    public async Task NotifyPlayerGridItems_ProcessesAttacksCorrectly()
    {
        // Arrange
        var gridItem1 = new Mock<IGridItem>();
        var gridItem2 = new Mock<IGridItem>();
        gridItem1.Setup(x => x.Id).Returns("item1");
        gridItem2.Setup(x => x.Id).Returns("item2");

        var attackResult1 = new AttackResult();
        var attackResult2 = new AttackResult();
        gridItem1.Setup(x => x.HandleAttack(It.IsAny<AttackDeclaration>())).Returns(attackResult1);
        gridItem2.Setup(x => x.HandleAttack(It.IsAny<AttackDeclaration>())).Returns(attackResult2);

        player1GridMock.Setup(x => x.GridItems).Returns(new List<IGridItem> { gridItem1.Object, gridItem2.Object });

        var attacks = new List<AttackDeclaration>
        {
            new() { GridItemId = "item1" },
            new() { GridItemId = "item2" }
        };

        // Act & Assert
        var attack = new Attack
        {
            ItemAttackDeclarations = attacks
        };
        attackHandlerMock.Setup(x => x.HandlePlayerAttacks(player1GridMock.Object, player2GridMock.Object))
                         .Returns(attack);

        await sut.HandleEndTurn();

        gridItem1.Verify(x => x.HandleAttack(It.Is<AttackDeclaration>(a => a.GridItemId == "item1")), Times.Once);
        gridItem2.Verify(x => x.HandleAttack(It.Is<AttackDeclaration>(a => a.GridItemId == "item2")), Times.Once);
    }

    [Fact]
    public async Task HandleEndTurn_NegativeHealthClampsToZero()
    {
        // Arrange
        var attack = new Attack
        {
            DirectAttackDeclarations = new List<AttackDeclaration>
            {
                new() { Damage = 1000 } // Way more than health + armor
            }
        };

        attackHandlerMock.Setup(x => x.HandlePlayerAttacks(player2GridMock.Object, player1GridMock.Object))
                         .Returns(attack);

        // Act
        await sut.HandleEndTurn();

        // Assert
        Assert.Equal(0, player1Mock.Object.Health);
        Assert.Equal(0, player1Mock.Object.Armor);
    }*/
}
