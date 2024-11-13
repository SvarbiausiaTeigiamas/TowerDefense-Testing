using Moq;
using TowerDefense.Api.Contracts.Turn;
using TowerDefense.Api.GameLogic.Attacks;
using TowerDefense.Api.GameLogic.GameState;
using TowerDefense.Api.GameLogic.Grid;
using TowerDefense.Api.GameLogic.Handlers;
using TowerDefense.Api.GameLogic.Player;
using TowerDefense.Api.Hubs;

namespace UnitTests.GameLogic.Handlers;

public class BattleHandlerTests : IDisposable
{
	private readonly Mock<IAttackHandler> _mockAttackHandler;
	private readonly Mock<IGameHandler> _mockGameHandler;
	private readonly Mock<INotificationHub> _mockNotificationHub;
	private readonly Mock<IPlayer> _mockPlayer1;
	private readonly Mock<IPlayer> _mockPlayer2;
	private readonly Mock<IArenaGrid> _mockArenaGrid1;
	private readonly Mock<IArenaGrid> _mockArenaGrid2;
	private readonly State _testGameState;
	private readonly BattleHandler _battleHandler;

	public BattleHandlerTests()
	{
		_mockAttackHandler = new Mock<IAttackHandler>();
		_mockGameHandler = new Mock<IGameHandler>();
		_mockNotificationHub = new Mock<INotificationHub>();
		_mockPlayer1 = new Mock<IPlayer>();
		_mockPlayer2 = new Mock<IPlayer>();
		_mockArenaGrid1 = new Mock<IArenaGrid>();
		_mockArenaGrid2 = new Mock<IArenaGrid>();

		_testGameState = new State
		{
			Players = new IPlayer[] { _mockPlayer1.Object, _mockPlayer2.Object },
		};

		_mockPlayer1.Setup(p => p.Name).Returns("Player1");
		_mockPlayer2.Setup(p => p.Name).Returns("Player2");
		_mockPlayer1.Setup(p => p.ArenaGrid).Returns(_mockArenaGrid1.Object);
		_mockPlayer2.Setup(p => p.ArenaGrid).Returns(_mockArenaGrid2.Object);

		GameOriginator.GameState = _testGameState;

		_battleHandler = new BattleHandler(
			_mockAttackHandler.Object,
			_mockGameHandler.Object,
			_mockNotificationHub.Object
		);
	}

	public void Dispose()
	{
		GameOriginator.GameState = new State();
	}

	[Fact]
	public async Task HandleEndTurn_NormalBattle_CalculatesAttacksAndUpdatesPlayers()
	{
		var player1Attack = new Attack
		{
			ItemAttackDeclarations = new List<AttackDeclaration>
			{
				new AttackDeclaration { EarnedMoney = 100, GridItemId = 0 },
			},
			DirectAttackDeclarations = new List<AttackDeclaration>
			{
				new AttackDeclaration { Damage = 10 },
			},
		};

		var player2Attack = new Attack
		{
			ItemAttackDeclarations = new List<AttackDeclaration>
			{
				new AttackDeclaration { EarnedMoney = 50, GridItemId = 0 },
			},
			DirectAttackDeclarations = new List<AttackDeclaration>
			{
				new AttackDeclaration { Damage = 5 },
			},
		};

		_mockPlayer1.Setup(p => p.Money).Returns(0);
		_mockPlayer2.Setup(p => p.Money).Returns(0);
		_mockPlayer1.Setup(p => p.Health).Returns(100);
		_mockPlayer2.Setup(p => p.Health).Returns(100);

		_mockAttackHandler
			.Setup(h => h.HandlePlayerAttacks(_mockArenaGrid1.Object, _mockArenaGrid2.Object))
			.Returns(player1Attack);

		_mockAttackHandler
			.Setup(h => h.HandlePlayerAttacks(_mockArenaGrid2.Object, _mockArenaGrid1.Object))
			.Returns(player2Attack);

		await _battleHandler.HandleEndTurn();

		_mockPlayer1.VerifySet(p => p.Money = 100);
		_mockPlayer2.VerifySet(p => p.Money = 50);

		_mockNotificationHub.Verify(
			h => h.SendPlayersTurnResult(It.IsAny<Dictionary<string, EndTurnResponse>>()),
			Times.Once
		);
	}

	[Fact]
	public async Task HandleEndTurn_PlayerDies_GameEnds()
	{
		_mockPlayer1.Setup(p => p.Health).Returns(-5);
		_mockPlayer2.Setup(p => p.Health).Returns(100);
		_mockPlayer2.Setup(p => p.Armor).Returns(0);

		_mockAttackHandler
			.Setup(h => h.HandlePlayerAttacks(_mockArenaGrid1.Object, _mockArenaGrid2.Object))
			.Returns(new Attack());

		_mockAttackHandler
			.Setup(h => h.HandlePlayerAttacks(_mockArenaGrid2.Object, _mockArenaGrid1.Object))
			.Returns(new Attack());

		await _battleHandler.HandleEndTurn();

		_mockGameHandler.Verify(h => h.FinishGame(_mockPlayer1.Object), Times.Never);
		_mockGameHandler.Verify(h => h.FinishGame(_mockPlayer2.Object), Times.Once);
		_mockNotificationHub.Verify(
			h => h.SendPlayersTurnResult(It.IsAny<Dictionary<string, EndTurnResponse>>()),
			Times.Never
		);
	}

	[Fact]
	public async Task HandleEndTurn_WithArmor_DamageReducesArmorFirst()
	{
		var player1Attack = new Attack
		{
			DirectAttackDeclarations = new List<AttackDeclaration>
			{
				new AttackDeclaration { Damage = 50 },
			},
		};

		var initialArmor = 30;
		var initialHealth = 100;

		_mockPlayer2.Setup(p => p.Armor).Returns(initialArmor);
		_mockPlayer2.Setup(p => p.Health).Returns(initialHealth);

		_mockAttackHandler
			.Setup(h => h.HandlePlayerAttacks(_mockArenaGrid1.Object, _mockArenaGrid2.Object))
			.Returns(player1Attack);

		_mockAttackHandler
			.Setup(h => h.HandlePlayerAttacks(_mockArenaGrid2.Object, _mockArenaGrid1.Object))
			.Returns(new Attack());

		await _battleHandler.HandleEndTurn();

		_mockPlayer2.VerifySet(p => p.Armor = -20, Times.Once);
		_mockPlayer2.VerifyGet(p => p.Armor);
		_mockPlayer2.VerifyGet(p => p.Health);
	}

	[Fact]
	public async Task HandleEndTurn_GridItemsReceiveAttacks_AttackResultsAreCollected()
	{
		var mockGridItem1 = new Mock<GridItem>();
		var mockGridItem2 = new Mock<GridItem>();

		mockGridItem1.Setup(g => g.Id).Returns(0);
		mockGridItem2.Setup(g => g.Id).Returns(0);

		var attackDeclaration = new AttackDeclaration { GridItemId = 0, Damage = 10 };
		var expectedAttackResult = new AttackResult { GridId = 0, Damage = new FireDamage() };

		mockGridItem1
			.Setup(g => g.HandleAttack(It.IsAny<AttackDeclaration>()))
			.Returns(expectedAttackResult);

		_mockArenaGrid1
			.Setup(g => g.GridItems)
			.Returns(new[] { mockGridItem1.Object, mockGridItem2.Object });

		var player2Attack = new Attack
		{
			ItemAttackDeclarations = new List<AttackDeclaration> { attackDeclaration },
		};

		_mockAttackHandler
			.Setup(h => h.HandlePlayerAttacks(_mockArenaGrid1.Object, _mockArenaGrid2.Object))
			.Returns(new Attack());
		_mockAttackHandler
			.Setup(h => h.HandlePlayerAttacks(_mockArenaGrid2.Object, _mockArenaGrid1.Object))
			.Returns(player2Attack);

		await _battleHandler.HandleEndTurn();

		mockGridItem1.Verify(g => g.HandleAttack(attackDeclaration), Times.Once);
		mockGridItem2.Verify(g => g.HandleAttack(It.IsAny<AttackDeclaration>()), Times.Once);
	}
}