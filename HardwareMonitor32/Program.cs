using HardwareMonitor32.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", false, true);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));

builder.Host.UseWindowsService();

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();

app.Run();