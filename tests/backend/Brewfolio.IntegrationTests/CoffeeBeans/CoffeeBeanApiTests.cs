using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Brewfolio.Application.CoffeeBeans;
using Brewfolio.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Brewfolio.IntegrationTests.CoffeeBeans;

public sealed class CoffeeBeanApiTests : IClassFixture<SqlServerFixture>
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly SqlServerFixture _fixture;

    public CoffeeBeanApiTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreatedCoffeeBeanIsReturnedByTheCollectionEndpoint()
    {
        await using var factory = CreateFactory();
        await MigrateAsync(factory);
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync(
            "/api/coffee-beans",
            new
            {
                name = "  Ethiopia Bombe  ",
                roaster = "  Example Roasters  ",
                origin = "Ethiopia",
                roastLevel = "Light",
                description = "Floral",
                productUrl = "https://example.com/coffee"
            },
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CoffeeBeanDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("Ethiopia Bombe", created.Name);
        Assert.Equal("Example Roasters", created.Roaster);
        Assert.Equal($"/api/coffee-beans/{created.Id}", createResponse.Headers.Location?.OriginalString);

        var detail = await client.GetFromJsonAsync<CoffeeBeanDto>(
            $"/api/coffee-beans/{created.Id}", JsonOptions);
        Assert.Equal("Ethiopia", detail?.Origin);

        var listed = await client.GetFromJsonAsync<CoffeeBeanPageDto>("/api/coffee-beans", JsonOptions);
        Assert.Contains(listed!.Items, coffeeBean => coffeeBean.Id == created.Id);
    }

    [Fact]
    public async Task UnknownCoffeeBeanReturnsNotFoundProblemDetails()
    {
        await using var factory = CreateFactory();
        await MigrateAsync(factory);
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/coffee-beans/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        Assert.Equal("coffeeBean.notFound", problem?.Code);
    }

    [Fact]
    public async Task CoffeeBeanCanBeEditedDuplicatedAndDeleted()
    {
        await using var factory = CreateFactory();
        await MigrateAsync(factory);
        using var client = factory.CreateClient();
        var create = await client.PostAsJsonAsync("/api/coffee-beans", new
        {
            name = "Lifecycle product",
            roaster = "Original roaster",
            roastLevel = "Light"
        });
        var original = await create.Content.ReadFromJsonAsync<CoffeeBeanDto>(JsonOptions);

        var update = await client.PutAsJsonAsync($"/api/coffee-beans/{original!.Id}", new
        {
            name = "Lifecycle product edited",
            roaster = "New roaster",
            origin = "Kenya",
            roastLevel = "Medium",
            description = "Updated",
            productUrl = "https://example.com/updated"
        });
        var updated = await update.Content.ReadFromJsonAsync<CoffeeBeanDto>(JsonOptions);
        Assert.Equal("New roaster", updated?.Roaster);

        var unacknowledged = await client.PostAsJsonAsync(
            $"/api/coffee-beans/{original.Id}/duplicate",
            new { acknowledged = false });
        Assert.Equal(HttpStatusCode.Conflict, unacknowledged.StatusCode);

        var duplicate = await client.PostAsJsonAsync(
            $"/api/coffee-beans/{original.Id}/duplicate",
            new { acknowledged = true });
        var copy = await duplicate.Content.ReadFromJsonAsync<CoffeeBeanDto>(JsonOptions);
        Assert.EndsWith("(Copy)", copy?.Name);
        Assert.Empty(copy!.Bags);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/coffee-beans/{original.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/coffee-beans/{original.Id}")).StatusCode);
    }

    [Fact]
    public async Task ExactDuplicateRequiresExplicitAcknowledgement()
    {
        await using var factory = CreateFactory();
        await MigrateAsync(factory);
        using var client = factory.CreateClient();
        var name = $"Duplicate {Guid.NewGuid():N}";
        var request = new { name, roaster = "Same Roaster", roastLevel = "Unknown" };
        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/coffee-beans", request)).StatusCode);

        var conflict = await client.PostAsJsonAsync("/api/coffee-beans", new
        {
            name = $"  {name.ToUpperInvariant()}  ",
            roaster = " same roaster ",
            roastLevel = "Unknown"
        });
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);

        var acknowledged = await client.PostAsJsonAsync("/api/coffee-beans", new
        {
            name,
            roaster = "Same Roaster",
            roastLevel = "Unknown",
            allowDuplicate = true
        });
        Assert.Equal(HttpStatusCode.Created, acknowledged.StatusCode);
    }

    [Fact]
    public async Task CollectionRejectsLimitsAboveMaximum()
    {
        await using var factory = CreateFactory();
        await MigrateAsync(factory);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/coffee-beans?limit=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("coffeeBean.query.limit", (await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>())?.Code);
    }

    [Fact]
    public async Task CoffeeBagsCanBeAddedUpdatedOpenedToggledAndDeleted()
    {
        await using var factory = CreateFactory();
        await MigrateAsync(factory);
        using var client = factory.CreateClient();
        var createProduct = await client.PostAsJsonAsync("/api/coffee-beans", new
        {
            name = "Bag lifecycle",
            roaster = "Example",
            roastLevel = "Unknown"
        });
        var product = await createProduct.Content.ReadFromJsonAsync<CoffeeBeanDto>(JsonOptions);

        var add = await client.PostAsJsonAsync($"/api/coffee-beans/{product!.Id}/bags", new
        {
            purchasedOn = "2026-09-01",
            roastedOn = "2026-08-28",
            openedOn = (string?)null,
            initialWeightGrams = 250,
            pricePaid = 18.50m,
            isInStock = true
        });
        Assert.True(add.IsSuccessStatusCode, await add.Content.ReadAsStringAsync());
        var withBag = await add.Content.ReadFromJsonAsync<CoffeeBeanDto>(JsonOptions);
        var bagId = Assert.Single(withBag!.Bags).Id;

        var stock = await client.PutAsJsonAsync(
            $"/api/coffee-beans/{product.Id}/bags/{bagId}/stock",
            new { isInStock = false });
        Assert.False((await stock.Content.ReadFromJsonAsync<CoffeeBeanDto>(JsonOptions))!.IsInStock);

        var opened = await client.PostAsync(
            $"/api/coffee-beans/{product.Id}/bags/{bagId}/open",
            null);
        Assert.NotNull((await opened.Content.ReadFromJsonAsync<CoffeeBeanDto>(JsonOptions))!.Bags[0].OpenedOn);

        var deleted = await client.DeleteAsync($"/api/coffee-beans/{product.Id}/bags/{bagId}");
        Assert.Empty((await deleted.Content.ReadFromJsonAsync<CoffeeBeanDto>(JsonOptions))!.Bags);
    }

    [Fact]
    public async Task InvalidCoffeeBeanReturnsProblemDetailsWithStableCode()
    {
        await using var factory = CreateFactory();
        await MigrateAsync(factory);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/coffee-beans",
            new { name = "", roaster = "Example Roasters" },
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("coffeeBean.name.required", problem.Code);
    }

    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Brewfolio", _fixture.ConnectionString);
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<BrewfolioDbContext>>();
                services.AddDbContext<BrewfolioDbContext>(options =>
                    options.UseSqlServer(_fixture.ConnectionString));
            });
        });
    }

    private static async Task MigrateAsync(WebApplicationFactory<Program> factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BrewfolioDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private sealed record ProblemDetailsResponse(int Status, string Code);
}
