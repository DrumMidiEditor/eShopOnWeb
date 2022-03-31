using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.eShopWeb.Web.HealthChecks;

/// <summary>
/// ホームページのヘルスチェック
/// </summary>
public class HomePageHealthCheck : IHealthCheck
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HomePageHealthCheck( IHttpContextAccessor aHttpContextAccessor )
    {
        this._httpContextAccessor = aHttpContextAccessor;
    }

    public async Task<HealthCheckResult> CheckHealthAsync( HealthCheckContext aContext, CancellationToken aCancellationToken = default )
    {
        var request = _httpContextAccessor.HttpContext?.Request ?? null ;
        
        if ( request == null )
        { 
            return HealthCheckResult.Unhealthy( "The check indicates an unhealthy result.[HttpContext]" ); 
        }

        // HTTP 要求スキーム＋ホストヘッダー（ポートを含む場合あり）
        string myUrl = request.Scheme + "://" + request.Host.ToString();

        var client = new HttpClient();

        // 指定された URI に GET 要求を非同期操作として送信
        var response = await client.GetAsync( myUrl, aCancellationToken );

        var pageContents = await response.Content.ReadAsStringAsync( aCancellationToken );

        if ( pageContents.Contains( ".NET Bot Black Sweatshirt" ) )
        {
            return HealthCheckResult.Healthy( "The check indicates a healthy result." );
        }

        return HealthCheckResult.Unhealthy( "The check indicates an unhealthy result." );
    }
}
