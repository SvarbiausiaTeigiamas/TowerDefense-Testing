﻿using Moq;
using Xunit;
using TowerDefense.Api.GameLogic.Player;
using TowerDefense.Api.Hubs;
using TowerDefense.Api.GameLogic.GameState;
using TowerDefense.Api.GameLogic.Handlers;

namespace UnitTests.GameLogic.Handlers
{
	public class InitialGameSetupHandlerTests
	{
		private readonly Mock<INotificationHub> _notificationHubMock;
		private readonly InitialGameSetupHandler _handler;

		public InitialGameSetupHandlerTests()
		{
			GameOriginator.GameState = new State();
			_notificationHubMock = new Mock<INotificationHub>();
			_handler = new InitialGameSetupHandler(_notificationHubMock.Object);
		}

		[Fact]
		public void AddNewPlayer_ShouldCreatePlayerWithAllComponents()
		{
			// Act
			var player = _handler.AddNewPlayer("TestPlayer");

			// Assert
			Assert.NotNull(player);
			Assert.Equal("TestPlayer", player.Name);
			Assert.NotNull(player.ArenaGrid);
			Assert.NotNull(player.Shop);
			Assert.NotNull(player.PerkStorage);
		}

		[Fact]
		public void SetConnectionIdForPlayer_ShouldUpdatePlayerConnectionId()
		{
			// Arrange
			var player = _handler.AddNewPlayer("TestPlayer");

			// Act
			_handler.SetConnectionIdForPlayer("TestPlayer", "connection123");

			// Assert
			Assert.Equal("connection123", player.ConnectionId);
		}

		[Fact]
		public void AddPlayerToGame_WhenMaxPlayersReached_ShouldThrowArgumentException()
		{
			// Arrange
			_handler.AddNewPlayer("Player1");
			_handler.AddNewPlayer("Player2");

			// Act & Assert
			Assert.Throws<ArgumentException>(() => _handler.AddPlayerToGame("Player3"));
		}

		[Fact]
		public async Task TryStartGame_WhenNotEnoughPlayers_ShouldNotNotifyGameStart()
		{
			// Arrange
			_handler.AddNewPlayer("Player1");

			// Act
			await _handler.TryStartGame();

			// Assert
			_notificationHubMock.Verify(
				x => x.NotifyGameStart(It.IsAny<IPlayer>(), It.IsAny<IPlayer>()),
				Times.Never);
		}

		[Fact]
		public async Task TryStartGame_WhenEnoughPlayers_ShouldNotifyGameStart()
		{
			// Arrange
			var player1 = _handler.AddNewPlayer("Player1");
			var player2 = _handler.AddNewPlayer("Player2");

			// Act
			await _handler.TryStartGame();

			// Assert
			_notificationHubMock.Verify(
				x => x.NotifyGameStart(player1, player2),
				Times.Once);
		}
	}
}