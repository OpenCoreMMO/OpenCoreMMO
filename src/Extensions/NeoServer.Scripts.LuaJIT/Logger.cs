using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class Logger
{
    private static Logger _instance;
    public static Logger GetInstance() => _instance == null ? _instance = new Logger() : _instance;

    private ILogger _logger;

    public Logger()
    {
        _logger = new LoggerConfiguration().CreateLogger();
    }

    public void Debug(string message, string args = "")
    {
        _logger.Debug(message, args);
    }

    public void Info(string message, string args = "")
    {
        _logger.Information(message, args);
    }

    public void Warn(string message, string args = "")
    {
        _logger.Warning(message, args);
    }

    public void Error(string message, string args = "")
    {
        _logger.Error(message, args);
    }
}
