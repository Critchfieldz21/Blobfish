using BackendLibrary;
using BlazorApp1.Components;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ShopTicketService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorBootstrap();

var app = builder.Build();

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

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();