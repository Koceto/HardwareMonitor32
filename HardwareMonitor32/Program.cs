using HardwareMonitor32.Models;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", false, true);

// Start HWInfo process if not already running
string? executablePath = builder.Configuration.GetSection("Settings")[nameof(Settings.HWInfoExecutablePath)];
if (!string.IsNullOrEmpty(executablePath))
{
    Process[] processes = Process.GetProcessesByName(executablePath.Split("\\").Last().Replace(".exe", ""));
    if (processes.Length <= 0)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = true,
            Verb = "runas"
        };

        Process.Start(startInfo);
    }
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));

builder.Host.UseWindowsService();

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();

app.Run();