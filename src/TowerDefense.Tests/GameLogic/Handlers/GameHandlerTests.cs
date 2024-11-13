using Moq;
using TowerDefense.Api.GameLogic.GameState;
using TowerDefense.Api.GameLogic.Handlers;
using TowerDefense.Api.GameLogic.Player;
using TowerDefense.Api.Hubs;

namespace UnitTests.GameLogic.Handlers;

public class GameHandlerTests : IDisposable
{
    private readonly Mock<INotificationHub> _notificationHubMock;
    private readonly GameHandler _gameHandler;
    private readonly FirstLevelPlayer _player;

    public GameHandlerTests()
    {
        _notificationHubMock = new Mock<INotificationHub>();
        _player = new FirstLevelPlayer();

        _gameHandler = new GameHandler(_notificationHubMock.Object);

        GameOriginator.GameState = new State();
    }

    public void Dispose()
    {
        GameOriginator.GameState = new State();
    }

    [Fact]
    public async Task ResetGame_ShouldCallNotificationHubAndResetGameState()
    {
        _notificationHubMock.Setup(x => x.ResetGame()).Returns(Task.CompletedTask);

        await _gameHandler.ResetGame();

        _notificationHubMock.Verify(x => x.ResetGame(), Times.Once);
        Assert.NotNull(GameOriginator.GameState);
        Assert.IsType<State>(GameOriginator.GameState);
    }

    [Fact]
    public async Task FinishGame_ShouldNotifyWinnerAndResetGameState()
    {
        _notificationHubMock
            .Setup(x => x.NotifyGameFinished(It.IsAny<IPlayer>()))
            .Returns(Task.CompletedTask);

        await _gameHandler.FinishGame(_player);

        _notificationHubMock.Verify(x => x.NotifyGameFinished(_player), Times.Once);
        Assert.NotNull(GameOriginator.GameState);
        Assert.IsType<State>(GameOriginator.GameState);
    }

    [Fact]
    public async Task ResetGame_WhenNotificationHubThrows_ShouldPropagateException()
    {
        var expectedException = new Exception("Hub error");
        _notificationHubMock.Setup(x => x.ResetGame()).ThrowsAsync(expectedException);

        var exception = await Assert.ThrowsAsync<Exception>(() => _gameHandler.ResetGame());
        Assert.Same(exception, expectedException);
    }

    [Fact]
    public async Task FinishGame_WhenNotificationHubThrows_ShouldPropagateException()
    {
        var expectedException = new Exception("Hub error");
        _notificationHubMock
            .Setup(x => x.NotifyGameFinished(It.IsAny<IPlayer>()))
            .ThrowsAsync(expectedException);

        var exception = await Assert.ThrowsAsync<Exception>(() => _gameHandler.FinishGame(_player));
        Assert.Same(exception, expectedException);
    }
}
