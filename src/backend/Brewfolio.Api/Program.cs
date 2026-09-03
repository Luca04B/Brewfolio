var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
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

app.UseCors();

app.MapGet("/api", () => Results.Ok(new
{
    name = "Brewfolio API",
    status = "ready"
}));

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
