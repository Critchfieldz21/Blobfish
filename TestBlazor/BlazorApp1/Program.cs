using BackendLibrary;
using BlazorApp1.Components;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ShopTicketService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorBootstrap();

var app = builder.Build();

app.MapGet("/export/{val}/{index}", (bool val, int index, ShopTicketService sTService) =>
{
    var ticket = sTService.GetTicket(index+1);
    if (ticket == null)
    {
        return Results.NotFound();
    }

    if(val == true)
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