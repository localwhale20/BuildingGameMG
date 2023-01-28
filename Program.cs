using System;
using System.IO;
using System.Reflection;
using Serilog;

string logPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Logs/Latest.log");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Debug()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information(logPath);

AppDomain.CurrentDomain.UnhandledException += (sender, ev) => {
    Log.Fatal(((Exception) ev.ExceptionObject).ToString());
};

using var game = new BuildingGame.Game();
game.Run();

Log.CloseAndFlush();
