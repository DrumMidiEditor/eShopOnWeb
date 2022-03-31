using BlazorShared;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopWeb.Web.HealthChecks;

/// <summary>
/// APIのヘルスチェック
/// </summary>
public class ApiHealthCheck : IHealthCheck
{
    private readonly BaseUrlConfiguration _baseUrlConfiguration;

    public ApiHealthCheck( IOptions<BaseUrlConfiguration> aBaseUrlConfiguration )
    {
        this._baseUrlConfiguration = aBaseUrlConfiguration.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync( HealthCheckContext aContext, CancellationToken aCancellationToken = default )
    {
        string myUrl = this._baseUrlConfiguration.ApiBase + "catalog-items";

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
