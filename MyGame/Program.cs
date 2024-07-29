using System;
using Serilog;

namespace SimpleGame
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // Write logs to a file with daily rolling
                .CreateLogger();

            try
            {
                Log.Information("Starting the game...");
                using (var game = new Game())
                    game.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The game encountered a fatal error.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}