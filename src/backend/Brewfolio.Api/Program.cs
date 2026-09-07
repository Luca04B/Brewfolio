using System.Text.Json;
using System.Text.Json.Serialization;
using Brewfolio.Application.CoffeeBeans;
using Brewfolio.Domain.CoffeeBags;
using Brewfolio.Domain.CoffeeBeans;
using Brewfolio.Domain.Results;
using Brewfolio.Infrastructure;
using Brewfolio.Infrastructure.Images;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<CreateCoffeeBeanHandler>();
builder.Services.AddScoped<ListCoffeeBeansHandler>();
builder.Services.AddScoped<GetCoffeeBeanHandler>();
builder.Services.AddScoped<CreateCoffeeBagHandler>();
builder.Services.AddScoped<ReplaceCoffeeBagHandler>();
builder.Services.AddScoped<OpenCoffeeBagHandler>();
builder.Services.AddScoped<SetCoffeeBagStockHandler>();
builder.Services.AddScoped<DeleteCoffeeBagHandler>();
builder.Services.AddScoped<UpdateCoffeeBeanHandler>();
builder.Services.AddScoped<DuplicateCoffeeBeanHandler>();
builder.Services.AddScoped<DeleteCoffeeBeanHandler>();
builder.Services.AddScoped<ReplaceCoffeeBeanImageHandler>();
builder.Services.AddScoped<RemoveCoffeeBeanImageHandler>();

var connectionString = builder.Configuration.GetConnectionString("Brewfolio")
    ?? throw new InvalidOperationException("Connection string 'Brewfolio' is required.");
var objectStorage = new ObjectStorageOptions(
    builder.Configuration["ObjectStorage:ServiceUrl"] ?? "http://localhost:9000",
    builder.Configuration["ObjectStorage:AccessKey"] ?? "minioadmin",
    builder.Configuration["ObjectStorage:SecretKey"] ?? "minioadmin",
    builder.Configuration["ObjectStorage:BucketName"] ?? "brewfolio-images");
builder.Services.AddBrewfolioInfrastructure(connectionString, objectStorage);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var frontendOrigin = builder.Configuration["Frontend:Origin"] ?? "http://localhost:4200";
        policy.WithOrigins(frontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.MigrateBrewfolioDatabaseAsync();
    if (builder.Configuration.GetValue<bool>("ObjectStorage:Initialize"))
    {
        await app.Services.EnsureBrewfolioObjectStorageAsync();
    }
}

app.UseCors();
app.UseExceptionHandler();

app.MapGet("/api", () => Results.Ok(new
{
    name = "Brewfolio API",
    status = "ready"
}));

app.MapHealthChecks("/health");

var coffeeBeans = app.MapGroup("/api/coffee-beans");

coffeeBeans.MapPost("/", async (
    HttpRequest httpRequest,
    CreateCoffeeBeanHandler handler,
    CancellationToken cancellationToken) =>
{
    var parsedRequest = await ParseCreateRequestAsync(httpRequest, cancellationToken);
    if (parsedRequest is CoffeeBeanRequestError parseError)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Coffee Bean request is invalid",
            detail: parseError.Description,
            extensions: new Dictionary<string, object?> { ["code"] = ErrorContract.Code(parseError.Code) });
    }

    if (parsedRequest is not ValueTuple<CreateCoffeeBeanRequest, CoffeeBeanImageInput?> parsed)
    {
        throw new InvalidOperationException("Coffee Bean request parsing returned an unexpected result.");
    }

    var (request, image) = parsed;
    var result = await handler.HandleAsync(
        new CreateCoffeeBeanCommand(
            request.Name,
            request.Roaster,
            request.Origin,
            request.RoastLevel,
            request.Description,
            request.ProductUrl,
            request.FirstBag,
            image,
            request.AllowDuplicate),
        cancellationToken);

    return result switch
    {
        CoffeeBeanDto coffeeBean => Results.Created($"/api/coffee-beans/{coffeeBean.Id}", coffeeBean),
        CoffeeBeanError error => Results.Problem(
            statusCode: error.Type == CoffeeBeanErrorType.Conflict
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest,
            title: "Coffee Bean validation failed",
            detail: error.Description,
            extensions: new Dictionary<string, object?> { ["code"] = ErrorContract.Code(error.Code) }),
        ValidationFailure failure => ToValidationProblem(failure)
    };
}).DisableAntiforgery();

coffeeBeans.MapGet("/", async (
    string? search,
    bool? isInStock,
    RoastLevel? roastLevel,
    CoffeeBeanSort? sort,
    string? cursor,
    int? limit,
    ListCoffeeBeansHandler handler,
    CancellationToken cancellationToken) =>
{
    if (limit is > 100 or < 1)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid collection limit",
            detail: "Limit must be between 1 and 100.",
            extensions: new Dictionary<string, object?>
            {
                ["code"] = ErrorContract.Code(CoffeeBeanRequestErrorCode.QueryLimitInvalid)
            });
    }

    var result = await handler.HandleAsync(
        new CoffeeBeanQuery(
            search,
            isInStock,
            roastLevel,
            sort ?? CoffeeBeanSort.LatestActivity,
            cursor,
            limit ?? 24),
        cancellationToken);
    return Results.Ok(result);
});

coffeeBeans.MapGet("/{id:guid}", async (
    Guid id,
    GetCoffeeBeanHandler handler,
    CancellationToken cancellationToken) =>
{
    var result = await handler.HandleAsync(new CoffeeBeanId(id), cancellationToken);
    return result switch
    {
        CoffeeBeanDto coffeeBean => Results.Ok(coffeeBean),
        CoffeeBeanError error => Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Coffee Bean not found",
            detail: error.Description,
            extensions: new Dictionary<string, object?> { ["code"] = ErrorContract.Code(error.Code) }),
        ValidationFailure failure => ToValidationProblem(failure)
    };
});

coffeeBeans.MapGet("/{id:guid}/image/{variant}", async (
    Guid id,
    string variant,
    ICoffeeBeanRepository repository,
    ICoffeeBeanImageStore imageStore,
    CancellationToken cancellationToken) =>
{
    if (variant is not ("large" or "thumbnail"))
    {
        return Results.NotFound();
    }

    var coffeeBean = await repository.GetAsync(new CoffeeBeanId(id), cancellationToken);
    if (coffeeBean?.ImageKey is null)
    {
        return Results.NotFound();
    }

    var image = await imageStore.OpenAsync(
        coffeeBean.ImageKey,
        variant == "thumbnail",
        cancellationToken);
    return image is null
        ? Results.NotFound()
        : Results.Stream(image.Content, image.ContentType, enableRangeProcessing: true);
});

coffeeBeans.MapPut("/{id:guid}", async (
    Guid id,
    UpdateCoffeeBeanCommand request,
    UpdateCoffeeBeanHandler handler,
    CancellationToken cancellationToken) =>
    ToHttpResult(await handler.HandleAsync(new CoffeeBeanId(id), request, cancellationToken)));

coffeeBeans.MapPost("/{id:guid}/duplicate", async (
    Guid id,
    DuplicateCoffeeBeanRequest request,
    DuplicateCoffeeBeanHandler handler,
    CancellationToken cancellationToken) =>
    ToHttpResult(await handler.HandleAsync(
        new CoffeeBeanId(id),
        request.Acknowledged,
        cancellationToken)));

coffeeBeans.MapDelete("/{id:guid}", async (
    Guid id,
    DeleteCoffeeBeanHandler handler,
    CancellationToken cancellationToken) =>
{
    var result = await handler.HandleAsync(new CoffeeBeanId(id), cancellationToken);
    return result switch
    {
        true => Results.NoContent(),
        false => Results.Problem(statusCode: StatusCodes.Status500InternalServerError),
        CoffeeBeanError error => Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Coffee Bean not found",
            detail: error.Description,
            extensions: new Dictionary<string, object?> { ["code"] = ErrorContract.Code(error.Code) }),
        ValidationFailure failure => ToValidationProblem(failure)
    };
});

coffeeBeans.MapPut("/{id:guid}/image", async (
    Guid id,
    HttpRequest request,
    ReplaceCoffeeBeanImageHandler handler,
    CancellationToken cancellationToken) =>
{
    if (!request.HasFormContentType) return Results.BadRequest();
    var file = (await request.ReadFormAsync(cancellationToken)).Files.GetFile("image");
    if (file is null || file.Length > 5 * 1024 * 1024) return Results.BadRequest();
    using var stream = new MemoryStream();
    await file.CopyToAsync(stream, cancellationToken);
    return ToHttpResult(await handler.HandleAsync(
        new CoffeeBeanId(id),
        stream.ToArray(),
        cancellationToken));
}).DisableAntiforgery();

coffeeBeans.MapDelete("/{id:guid}/image", async (
    Guid id,
    RemoveCoffeeBeanImageHandler handler,
    CancellationToken cancellationToken) =>
    ToHttpResult(await handler.HandleAsync(new CoffeeBeanId(id), cancellationToken)));

coffeeBeans.MapPost("/{coffeeBeanId:guid}/bags", async (
    Guid coffeeBeanId,
    CoffeeBagInput request,
    CreateCoffeeBagHandler handler,
    CancellationToken cancellationToken) =>
{
    var result = await handler.HandleAsync(new CoffeeBeanId(coffeeBeanId), request, cancellationToken);
    return ToHttpResult(result);
});

coffeeBeans.MapPut("/{coffeeBeanId:guid}/bags/{coffeeBagId:guid}", async (
    Guid coffeeBeanId,
    Guid coffeeBagId,
    CoffeeBagInput request,
    ReplaceCoffeeBagHandler handler,
    CancellationToken cancellationToken) =>
{
    var result = await handler.HandleAsync(
        new CoffeeBeanId(coffeeBeanId),
        new CoffeeBagId(coffeeBagId),
        request,
        cancellationToken);
    return ToHttpResult(result);
});

coffeeBeans.MapPost("/{coffeeBeanId:guid}/bags/{coffeeBagId:guid}/open", async (
    Guid coffeeBeanId,
    Guid coffeeBagId,
    OpenCoffeeBagHandler handler,
    CancellationToken cancellationToken) =>
{
    var result = await handler.HandleAsync(
        new CoffeeBeanId(coffeeBeanId),
        new CoffeeBagId(coffeeBagId),
        cancellationToken);
    return ToHttpResult(result);
});

coffeeBeans.MapPut("/{coffeeBeanId:guid}/bags/{coffeeBagId:guid}/stock", async (
    Guid coffeeBeanId,
    Guid coffeeBagId,
    SetCoffeeBagStockRequest request,
    SetCoffeeBagStockHandler handler,
    CancellationToken cancellationToken) =>
{
    var result = await handler.HandleAsync(
        new CoffeeBeanId(coffeeBeanId),
        new CoffeeBagId(coffeeBagId),
        request.IsInStock,
        cancellationToken);
    return ToHttpResult(result);
});

coffeeBeans.MapDelete("/{coffeeBeanId:guid}/bags/{coffeeBagId:guid}", async (
    Guid coffeeBeanId,
    Guid coffeeBagId,
    DeleteCoffeeBagHandler handler,
    CancellationToken cancellationToken) =>
{
    var result = await handler.HandleAsync(
        new CoffeeBeanId(coffeeBeanId),
        new CoffeeBagId(coffeeBagId),
        cancellationToken);
    return ToHttpResult(result);
});

app.Run();

static async Task<CreateCoffeeBeanRequestParseResult>
    ParseCreateRequestAsync(HttpRequest request, CancellationToken cancellationToken)
{
    try
    {
        if (!request.HasFormContentType)
        {
            var jsonRequest = await request.ReadFromJsonAsync<CreateCoffeeBeanRequest>(cancellationToken);
            return jsonRequest is null
                ? new CoffeeBeanRequestError(
                    CoffeeBeanRequestErrorCode.RequestRequired,
                    "Request body is required.")
                : (jsonRequest, null);
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var json = form["data"].ToString();
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        var formRequest = JsonSerializer.Deserialize<CreateCoffeeBeanRequest>(json, options);
        if (formRequest is null)
        {
            return new CoffeeBeanRequestError(
                CoffeeBeanRequestErrorCode.RequestRequired,
                "Form field 'data' is required.");
        }

        var file = form.Files.GetFile("image");
        if (file is null)
        {
            return (formRequest, null);
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            return new CoffeeBeanRequestError(
                CoffeeBeanRequestErrorCode.ImageTooLarge,
                "Image must not exceed 5 MB.");
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        return (formRequest, new CoffeeBeanImageInput(stream.ToArray()));
    }
    catch (JsonException)
    {
        return new CoffeeBeanRequestError(
            CoffeeBeanRequestErrorCode.RequestInvalid,
            "Request JSON is invalid.");
    }
}

static IResult ToHttpResult(CoffeeBeanResult<CoffeeBeanDto> result)
{
    return result switch
    {
        CoffeeBeanDto coffeeBean => Results.Ok(coffeeBean),
        CoffeeBeanError error => Results.Problem(
            statusCode: error.Type switch
            {
                CoffeeBeanErrorType.NotFound => StatusCodes.Status404NotFound,
                CoffeeBeanErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            },
            title: "Coffee Bean operation failed",
            detail: error.Description,
            extensions: new Dictionary<string, object?> { ["code"] = ErrorContract.Code(error.Code) }),
        ValidationFailure failure => ToValidationProblem(failure)
    };
}

static IResult ToValidationProblem(ValidationFailure failure)
{
    var firstError = failure.Errors[0];
    var errors = failure.Errors.Select(error => new
    {
        code = ErrorContract.Code(error.Code),
        field = ErrorContract.Field(error.Field),
        description = error.Description
    });
    return Results.Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: "Validation failed",
        detail: firstError.Description,
        extensions: new Dictionary<string, object?>
        {
            ["code"] = ErrorContract.Code(firstError.Code),
            ["errors"] = errors
        });
}

public sealed record CreateCoffeeBeanRequest(
    string Name,
    string Roaster,
    string? Origin,
    RoastLevel RoastLevel,
    string? Description,
    string? ProductUrl,
    CoffeeBagInput? FirstBag,
    bool AllowDuplicate = false);

public sealed record SetCoffeeBagStockRequest(bool IsInStock);

public sealed record DuplicateCoffeeBeanRequest(bool Acknowledged);

public partial class Program;
