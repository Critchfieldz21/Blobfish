using BackendLibrary;
using BlazorApp1.Components;
using SQL3cs;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ShopTicketService>();
// Use existing SQLite helper in Services/database.cs
builder.Services.AddSingleton<SQL3cs.CustomerData>();

// Load detection models
builder.Services.AddSingleton<DetectModelService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DetectModelService>>();
    var formViewModelPath = "Services/Models/form_best.onnx";
    var sectionViewModelPath = "Services/Models/section_best.onnx";
    return new DetectModelService(logger, formViewModelPath, sectionViewModelPath);
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorBootstrap();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    // Display timestamps in hh:mm:ss format for each log message
    options.TimestampFormat = "hh:mm:ss.fff ";
    options.SingleLine = true;
});

var app = builder.Build();

// Ensure SQLite tables exist at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SQL3cs.CustomerData>();
    db.CreateTables();
    // Preload history from DB so it persists across restarts
    var stService = scope.ServiceProvider.GetRequiredService<ShopTicketService>();
    var loggerFactory2 = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var existing = db.LoadTickets(loggerFactory2);
    if (existing.Count > 0)
    {
        stService.History = existing.Cast<ShopTicket?>().ToList();
    }
}

app.MapGet("/export/{val}/{index}", (String val, int index, ShopTicketService sTService) =>
{
    var ticket = sTService.GetHistoryTicket(index+1);
    if (ticket == null)
    {
        return Results.NotFound();
    }

    if(val.Equals("true", StringComparison.OrdinalIgnoreCase))
    {
        var json = ticket.ToJson();
        return Results.File(
            System.Text.Encoding.UTF8.GetBytes(json),
            "application/json",
            $"{Path.GetFileNameWithoutExtension(ticket.FileName)}_info.json"
        );
    }
    else
    {
        var csv = ticket.ToCsv();
        return Results.File(
            System.Text.Encoding.UTF8.GetBytes(csv),
            "text/csv",
            $"{Path.GetFileNameWithoutExtension(ticket.FileName)}_info.csv"
        );
    }
});

app.MapGet("/export/batch/{val}", (bool val, string? ids, ShopTicketService sTService) =>
{
    if (string.IsNullOrWhiteSpace(ids)) return Results.BadRequest("ids query missing");
    var parts = ids.Split(',', StringSplitOptions.RemoveEmptyEntries);
    var tickets = parts.Select(p => {
        if (int.TryParse(p, out var idx))
        {
            // incoming ids are zero-based (history list indices), but GetHistoryTicket expects 1-based index
            try { return sTService.GetHistoryTicket(idx + 1); } catch { return null; }
        }
        return null;
    }).Where(t => t != null).ToList();

    if (!tickets.Any()) return Results.NotFound();

    if (val)
    {
        // JSON array of ticket objects
        var jsonParts = tickets.Select(t => t!.ToJson());
        var combined = "[" + string.Join(",", jsonParts) + "]";
        return Results.File(
            System.Text.Encoding.UTF8.GetBytes(combined),
            "application/json",
            $"batch_export_{DateTime.UtcNow:yyyyMMddHHmmss}.json"
        );
    }
    else
    {
        // CSV: use header from first ticket, then append rows from each ticket
        var csvLines = new List<string>();
        var firstCsv = tickets[0]!.ToCsv().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var header = firstCsv[0];
        csvLines.Add(header);
        foreach (var t in tickets)
        {
            var lines = t!.ToCsv().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 1) csvLines.Add(lines[1]);
        }
        var combined = string.Join("\n", csvLines);
        return Results.File(
            System.Text.Encoding.UTF8.GetBytes(combined),
            "text/csv",
            $"batch_export_{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
        );
    }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Preload packages to reduce first-use latency of PDF extraction
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var preloadLogger = loggerFactory.CreateLogger<PreloadService>();
PreloadService.PreloadPackages(preloadLogger);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();