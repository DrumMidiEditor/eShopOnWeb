using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.eShopWeb.Infrastructure;

/// <summary>
/// 依存関係
/// </summary>
public static class Dependencies
{
    /// <summary>
    /// サービス構成
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="services"></param>
    public static void ConfigureServices( IConfiguration configuration, IServiceCollection services )
    {
        var useOnlyInMemoryDatabase = bool.Parse( configuration[ "UseOnlyInMemoryDatabase" ] ?? "false" );

        if ( useOnlyInMemoryDatabase )
        {
            services.AddDbContext<CatalogContext>
                ( 
                    c => c.UseInMemoryDatabase( "Catalog" ) 
                );
         
            services.AddDbContext<AppIdentityDbContext>
                ( 
                    options => options.UseInMemoryDatabase( "Identity" )
                );
        }
        else
        {
            // https://www.microsoft.com/en-us/download/details.aspx?id=54284
            // use real database
            // Requires LocalDB which can be installed with SQL Server Express 2016
            services.AddDbContext<CatalogContext>
                ( 
                    c => c.UseSqlServer( configuration.GetConnectionString( "CatalogConnection" ) ) 
                );

            // Add Identity DbContext
            services.AddDbContext<AppIdentityDbContext>
                ( 
                    options => options.UseSqlServer( configuration.GetConnectionString( "IdentityConnection" ) )
                );
        }
    }
}
