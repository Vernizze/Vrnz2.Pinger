using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Vrnz2.Pinger.Crosscutting.StartupHandlers
{
    public static class LogConfig
    {
        public static ILogger Config()
            => Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(Serilog.Events.LogEventLevel.Verbose, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();
    }
}
