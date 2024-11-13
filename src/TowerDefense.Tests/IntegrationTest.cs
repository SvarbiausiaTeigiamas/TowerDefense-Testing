using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TowerDefense.Api.Bootstrap;
using TowerDefense.Api.Bootstrap.AutoMapper;
using TowerDefense.Api.Constants;
using TowerDefense.Api.Contracts.Inventory;
using TowerDefense.Api.Contracts.Player;
using TowerDefense.Api.Contracts.Shop;
using TowerDefense.Api.GameLogic.Items;
using TowerDefense.Api.GameLogic.Items.Models;
using Xunit.Abstractions;

namespace UnitTests;

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
    public async Task AddPlayers_OnGameStart_PlayersAddedSuccessfully()
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

    [Fact]
    public async Task BuyItems_RocketAndShield_ItemsBoughtSuccessfully()
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
        JsonDocument
            .Parse(await player1Items.Content.ReadAsStringAsync())
            .RootElement.GetProperty("items")
            .GetArrayLength()
            .Should()
            .Be(1);

        var player2Items = await _client.GetAsync("/api/inventory/PlayerTwo");
        JsonDocument
            .Parse(await player2Items.Content.ReadAsStringAsync())
            .RootElement.GetProperty("items")
            .GetArrayLength()
            .Should()
            .Be(1);
    }
}
