using Moq;
using TowerDefense.Api.GameLogic.GameState;
using TowerDefense.Api.GameLogic.Handlers;

namespace UnitTests.GameLogic.Handlers
{
    public class TurnHandlerTests
    {
        private readonly Mock<IBattleHandler> _battleHandlerMock;
        private readonly TurnHandler _turnHandler;
        private readonly State _gameState;

        public TurnHandlerTests()
        {
            _battleHandlerMock = new Mock<IBattleHandler>();
            _gameState = new State();
            GameOriginator.GameState = _gameState;
            _turnHandler = new TurnHandler(_battleHandlerMock.Object);
        }

        [Fact]
        public async Task TryEndTurn_PlayerAlreadyFinished_DoesNothing()
        {
            const string playerName = "Player1";
            _gameState.PlayersFinishedTurn.Add(playerName, true);

            await _turnHandler.TryEndTurn(playerName);

            _battleHandlerMock.Verify(x => x.HandleEndTurn(), Times.Never);
            Assert.Single(_gameState.PlayersFinishedTurn);
        }

        [Fact]
        public async Task TryEndTurn_NotAllPlayersFinished_AddsPlayerToFinishedList()
        {
            const string playerName = "Player1";

            await _turnHandler.TryEndTurn(playerName);

            Assert.True(_gameState.PlayersFinishedTurn.ContainsKey(playerName));
            _battleHandlerMock.Verify(x => x.HandleEndTurn(), Times.Never);
        }

        [Fact]
        public async Task TryEndTurn_AllPlayersFinished_CallsHandleEndTurnAndClearsFinishedList()
        {
            _battleHandlerMock.Setup(x => x.HandleEndTurn()).Returns(Task.CompletedTask);

            await _turnHandler.TryEndTurn("Player1");
            await _turnHandler.TryEndTurn("Player2");

            _battleHandlerMock.Verify(x => x.HandleEndTurn(), Times.Once);
            Assert.Empty(_gameState.PlayersFinishedTurn);
        }
    }
}