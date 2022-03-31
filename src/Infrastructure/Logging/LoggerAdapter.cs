using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.Infrastructure.Logging;

/// <summary>
/// ログ出力補助
/// </summary>
/// <typeparam name="T"></typeparam>
public class LoggerAdapter<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public LoggerAdapter( ILoggerFactory loggerFactory )
    {
        _logger = loggerFactory.CreateLogger<T>();
    }

    public void LogWarning( string aMessage, params object[] aArgs )
    {
        _logger.LogWarning( aMessage ?? "", aArgs );
    }

    public void LogInformation( string aMessage, params object[] aArgs )
    {
        _logger.LogInformation( aMessage ?? "", aArgs );
    }
}
