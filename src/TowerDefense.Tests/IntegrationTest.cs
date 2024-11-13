using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TowerDefense.Api.Bootstrap;
using TowerDefense.Api.Bootstrap.AutoMapper;
using TowerDefense.Api.Constants;
using TowerDefense.Api.Contracts.Grid;
using TowerDefense.Api.Contracts.Player;
using TowerDefense.Api.Contracts.Shop;
using TowerDefense.Api.Contracts.Turn;
using Xunit.Abstractions;

namespace UnitTests;

public class GameIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _client;

    public GameIntegrationTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
    {
        _output = output;

        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost:7042"),
            }
        );
    }

    [Fact]
    public async Task TestGameWorkflow()
    {
        await AddPlayers_OnGameStart_PlayersAddedSuccessfully();
        await BuyAndPlaceItems_RocketAndShield_ItemsBoughtAndPlacedSuccessfully();
        await EndTurns_UntilGameFinished_PlayerReachedZeroHealth();
    }

    private async Task AddPlayers_OnGameStart_PlayersAddedSuccessfully()
    {
        // Register both players
        var player1Response = await _client.PostAsJsonAsync(
            "/api/players",
            new AddNewPlayerRequest { PlayerName = "PlayerOne" }
        );
        var player2Response = await _client.PostAsJsonAsync(
            "/api/players",
            new AddNewPlayerRequest { PlayerName = "PlayerTwo" }
        );

        // Ensure both were added successfully
        Assert.Equal(HttpStatusCode.OK, player1Response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, player2Response.StatusCode);

        var player1Info = await _client.GetAsync("/api/players/PlayerOne");
        new GetPlayerInfoResponse
        {
            PlayerName = "PlayerOne",
            Armor = 100,
            Health = 100,
            Money = 1000,
        }
            .Should()
            .BeEquivalentTo(await player1Info.Content.ReadFromJsonAsync<GetPlayerInfoResponse>());

        var player2Info = await _client.GetAsync("/api/players/PlayerTwo");
        new GetPlayerInfoResponse
        {
            PlayerName = "PlayerTwo",
            Armor = 100,
            Health = 100,
            Money = 1000,
        }
            .Should()
            .BeEquivalentTo(await player2Info.Content.ReadFromJsonAsync<GetPlayerInfoResponse>());
    }

    private async Task BuyAndPlaceItems_RocketAndShield_ItemsBoughtAndPlacedSuccessfully()
    {
        var player1BuyItemResponse = await _client.PostAsJsonAsync(
            "/api/shop",
            new BuyShopItemRequest { ItemId = "Rockets", PlayerName = "PlayerOne" }
        );
        var player2BuyItemResponse = await _client.PostAsJsonAsync(
            "/api/shop",
            new BuyShopItemRequest { ItemId = "Shield", PlayerName = "PlayerTwo" }
        );

        Assert.Equal(HttpStatusCode.OK, player1BuyItemResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, player2BuyItemResponse.StatusCode);

        var player1Items = await _client.GetAsync("/api/inventory/PlayerOne");
        var player1ItemsBody = await player1Items.Content.ReadAsStringAsync();
        JsonDocument
            .Parse(player1ItemsBody)
            .RootElement.GetProperty("items")
            .GetArrayLength()
            .Should()
            .Be(1);

        var player2Items = await _client.GetAsync("/api/inventory/PlayerTwo");
        var player2ItemsBody = await player2Items.Content.ReadAsStringAsync();
        JsonDocument
            .Parse(player2ItemsBody)
            .RootElement.GetProperty("items")
            .GetArrayLength()
            .Should()
            .Be(1);

        var player1ItemId = JsonDocument
            .Parse(player1ItemsBody)
            .RootElement.GetProperty("items")[0]
            .GetProperty("id")
            .GetString();
        var player2ItemId = JsonDocument
            .Parse(player2ItemsBody)
            .RootElement.GetProperty("items")[0]
            .GetProperty("id")
            .GetString();

        var player1PlaceItemResponse = await _client.PostAsJsonAsync(
            "/api/players/place-item",
            new AddGridItemRequest
            {
                GridItemId = 36,
                PlayerName = "PlayerOne",
                InventoryItemId = player1ItemId,
            }
        );
        var player2PlaceItemResponse = await _client.PostAsJsonAsync(
            "/api/players/place-item",
            new AddGridItemRequest
            {
                GridItemId = 36,
                PlayerName = "PlayerOne",
                InventoryItemId = player2ItemId,
            }
        );

        Assert.Equal(HttpStatusCode.OK, player1PlaceItemResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, player2PlaceItemResponse.StatusCode);
    }

    private async Task EndTurns_UntilGameFinished_PlayerReachedZeroHealth()
    {
        async Task EndTurns()
        {
            var first = await _client.PostAsJsonAsync(
                "/api/players/endturn",
                new EndTurnRequest { PlayerName = "PlayerOne" }
            );
            var second = await _client.PostAsJsonAsync(
                "/api/players/endturn",
                new EndTurnRequest { PlayerName = "PlayerTwo" }
            );

            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
            Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        }

        await EndTurns();
        await EndTurns();
        await EndTurns();

        var player2InfoResp = await _client.GetAsync("/api/players/PlayerTwo");
        Assert.Equal(HttpStatusCode.OK, player2InfoResp.StatusCode);

        var player2Info = await player2InfoResp.Content.ReadFromJsonAsync<GetPlayerInfoResponse>();
        Assert.Equal(20, player2Info.Health);

        await EndTurns();

        player2InfoResp = await _client.GetAsync("/api/players/PlayerTwo");
        Assert.Equal(HttpStatusCode.OK, player2InfoResp.StatusCode);

        player2Info = await player2InfoResp.Content.ReadFromJsonAsync<GetPlayerInfoResponse>();
        Assert.Equal(0, player2Info.Health);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls("http://localhost:5042", "https://localhost:7042");

        builder.ConfigureServices(services =>
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSignalR();
            services.SetupGameEngine();
            services.SetupAutoMapper();

            services.AddCors(options =>
            {
                options.AddPolicy(
                    Policy.DevelopmentCors,
                    b =>
                    {
                        b.WithOrigins("https://localhost:3000")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .SetIsOriginAllowed((_) => true)
                            .AllowCredentials();
                    }
                );
            });
        });
    }
}
