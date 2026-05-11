using Microsoft.EntityFrameworkCore;
using Vale.Server.Data;
using Vale.Server.Hubs;
using Vale.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("GameDatabase")));
builder.Services.AddSingleton<IWorldStateService, WorldStateService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    await dbContext.Database.MigrateAsync();
}

var worldService = app.Services.GetRequiredService<IWorldStateService>();
await worldService.InitializeAsync(app.Lifetime.ApplicationStopping);

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapHub<WorldHub>("/hubs/world");
app.MapFallbackToFile("index.html");

app.Run();
