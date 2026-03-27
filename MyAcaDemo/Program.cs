

var builder = WebApplication.CreateBuilder(args);

// Bind to port 8080
builder.WebHost.UseUrls("http://*:8080");

var app = builder.Build();

// Example endpoint so you don’t get 404
app.MapGet("/", async context =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Hello Logger!");
    await context.Response.WriteAsync("Hello Logger!");
});

app.Run();