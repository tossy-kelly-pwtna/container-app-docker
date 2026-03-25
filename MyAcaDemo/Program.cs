using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);

// Add services here
// builder.Services.AddControllers();
// builder.Services.AddDbContext<StoreContext>();

builder.WebHost.UseUrls("http://*:8080");

var app = builder.Build();

// Configure middleware here
// app.UseRouting();
// app.MapControllers();

app.Run();
