using System.Net.Mime;
using Ardalis.ListStartupServices;
using BlazorAdmin;
using BlazorAdmin.Services;
using Blazored.LocalStorage;
using BlazorShared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web;
using Microsoft.eShopWeb.Web.Configuration;
using Microsoft.eShopWeb.Web.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

#region ビルド構成

var builder = WebApplication.CreateBuilder( args );

// ログコンソール出力追加
builder.Logging.AddConsole();

// https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0
// インフラコンフィグ構成（DB接続設定 etc）
// Configの設定は、appsettings.json に入力
Microsoft.eShopWeb.Infrastructure.Dependencies.ConfigureServices( builder.Configuration, builder.Services );

// Cookie構成
builder.Services.AddCookieSettings();

// Cookie認証
builder.Services.AddAuthentication( CookieAuthenticationDefaults.AuthenticationScheme )
    .AddCookie
    (
        options =>
        {
            // クライアント側のスクリプトからCookieへのアクセス可否を設定
            options.Cookie.HttpOnly = true;

            // https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.aspnetcore.http.cookiesecurepolicy?view=aspnetcore-6.0
            // None         : セキュリティ的に非推奨
            // Always       : Secure は常に true とマークされます。 この値はログイン ページおよび認証済み ID を必要とする
            //                後続のすべてのページが HTTPS を使用する場合に使用してください。
            //                HTTPS URL によるローカル開発も必要になります。
            // SameAsRequest: クッキーを提供する URI が HTTPS の場合、Cookie は後続の HTTPS 要求でのみサーバーに返されます。
            //                それ以外の場合、Cookie を提供する URI が HTTP の場合は、すべての HTTP 要求と HTTPS 要求で
            //                クッキーがサーバーに返されます。 この値により、デプロイされたサーバー上のすべての認証済み要求に対して
            //                HTTPS が保証され、localhost 開発および HTTPS サポートを持つサーバーの HTTP もサポートされます。
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

            // https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.aspnetcore.http.samesitemode?view=aspnetcore-6.0#microsoft-aspnetcore-http-samesitemode-unspecified
            // Unspecified: SameSite フィールドは設定されません。クライアントは既定の cookie ポリシーに従う必要があります。
            // None       : クライアントが同じサイトの制限を無効にする必要があることを示します。
            // Lax        : クライアントが "同一サイト" の要求と "クロスサイト" のトップレベルナビゲーションを使用して
            //              クッキーを送信する必要があることを示します。
            // Strict     : クライアントが "同じサイト" 要求で cookie を送信する必要があることを示します。
            options.Cookie.SameSite = SameSiteMode.Lax;
        }
    );

// 役割：ユーザー
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    // https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.aspnetcore.identity.identitybuilderuiextensions.adddefaultui?view=aspnetcore-6.0
    // Identity という名前の領域内の id を使用して、IDENTITY の既定の自己Razor Pages UI をアプリケーションに追加
    .AddDefaultUI()

    // https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.dependencyinjection.identityentityframeworkbuilderextensions.addentityframeworkstores?view=aspnetcore-6.0
    // Id 情報ストアの Entity Framework 実装を追加
    .AddEntityFrameworkStores<AppIdentityDbContext>()

    // https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.aspnetcore.identity.identitybuilder.adddefaulttokenproviders?view=aspnetcore-1.1
    // パスワードのリセット、電子メールの変更、電話番号の変更、および2要素認証トークンの生成のためのトークンを生成するために
    // 使用される既定のトークンプロバイダーを追加
    .AddDefaultTokenProviders();

// 独自サービス
builder.Services.AddScoped<ITokenClaimsService, IdentityTokenClaimService>();

// .NET Core(ConfigureCoreServices)
builder.Services.AddCoreServices( builder.Configuration );

// Webサービス(ConfigureWebServices)
builder.Services.AddWebServices( builder.Configuration );

// 非分散メモリ内実装？共有メモリってこと？
builder.Services.AddMemoryCache();

// https://docs.microsoft.com/ja-jp/aspnet/core/fundamentals/routing?view=aspnetcore-6.0
// ルーティング
builder.Services.AddRouting
    (
        // https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.aspnetcore.routing.routeoptions?view=aspnetcore-6.0
        options =>
        {
            // ???
            // Replace the type and the name used to refer to it with your own
            // IOutboundParameterTransformer implementation
            options.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer);
        }
    );

#region MVC構成

// MVCフレームワーク
builder.Services.AddMvc
    (
        // https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.aspnetcore.mvc.mvcoptions?view=aspnetcore-6.0
        options =>
        {
            // ???
            // アクションを検出するときに IApplicationModelConvention に適用されるインスタンスの ApplicationModel 一覧を取得します。
            options.Conventions.Add( new RouteTokenTransformerConvention( new SlugifyParameterTransformer() ) );
        }
    );

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages
    (
        // https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.aspnetcore.mvc.razorpages.razorpagesoptions?view=aspnetcore-6.0
        options =>
        {
            // 指定されたページの承認が必要です。ログアウト確認が必要ってこと？
            options.Conventions.AuthorizePage( "/Basket/Checkout" );
        }
    );

#endregion


builder.Services.AddHttpContextAccessor();

// 正常性チェック構成
builder.Services
    .AddHealthChecks()
    .AddCheck<ApiHealthCheck>       ( "api_health_check"        , tags: new[] { "apiHealthCheck"        } )
    .AddCheck<HomePageHealthCheck>  ( "home_page_health_check"  , tags: new[] { "homePageHealthCheck"   } );

builder.Services.Configure<ServiceConfig>
    (
        config =>
        {
            config.Services = new List<ServiceDescriptor>( builder.Services );
            config.Path = "/allservices";
        }
    );

// blazor configuration
var configSection = builder.Configuration.GetRequiredSection(BaseUrlConfiguration.CONFIG_NAME);
builder.Services.Configure<BaseUrlConfiguration>(configSection);

var baseUrlConfig = configSection.Get<BaseUrlConfiguration>();

// Blazor Admin Required Services for Prerendering
builder.Services.AddScoped<HttpClient>(s => new HttpClient
{
    BaseAddress = new Uri(baseUrlConfig.WebBase)
});

// add blazor services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<HttpService>();
builder.Services.AddBlazorServices();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

#endregion

#region アプリ構成

var app = builder.Build();

app.Logger.LogInformation( "App created..." );

#region URLベース設定

// appsettings.json の設定を使用
var catalogBaseUrl = builder.Configuration.GetValue( typeof( string ), "CatalogBaseUrl" ) as string;
if ( !string.IsNullOrEmpty( catalogBaseUrl ) )
{
    app.Use
        (
            ( context, next ) =>
            {
                // 要求のベースパスを取得または設定します。 パスのベースの末尾にスラッシュを使用することはできません。
                context.Request.PathBase = new PathString( catalogBaseUrl );
                return next();
            }
        );
}

#endregion

#region 正常性チェック：有効化

// https://docs.microsoft.com/ja-jp/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-6.0
// 正常性チェックは通常、アプリの状態を確認する目的で、外部の監視サービスまたはコンテナー オーケストレーターと共に使用されます。
// 正常性チェックをアプリに追加する前に、使用する監視システムを決定します。
// 監視システムからは、作成する正常性チェックの種類とそのエンドポイントの設定方法が指示されます。
app.UseHealthChecks
    (
        "/health",
        new HealthCheckOptions
        {
            ResponseWriter = async ( context, report ) =>
            {
                // https://docs.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.diagnostics.healthchecks.healthreport?view=dotnet-plat-ext-6.0
                var result = new
                {
                    // すべての正常性チェックの集計状態を表す HealthStatus を取得します。
                    // Status の値は、正常性チェックによって報告される最も重大な状態になります。
                    // チェックが実行されなかった場合、値は常に Healthy です。
                    status = report.Status.ToString(),

                    // 各正常性チェックの結果
                    // Degraded	 1: 正常性チェックによって、コンポーネントがデグレード状態であると判断されたことを示します。
                    // Healthy   2: 正常性チェックによって、コンポーネントが正常であると判断されたことを示します。
                    // Unhealthy 0: 正常性チェックによって、コンポーネントが異常な状態であると判断されたか
                    //              正常性チェックの実行中にハンドルされない例外がスローされたことを示します。
                    errors = report.Entries.Select
                    (
                        e => new
                        {
                            key = e.Key,
                            value = Enum.GetName( typeof( HealthStatus ), e.Value.Status )
                        }
                    )
                }.ToJson();
                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync( result );
            }
        }
    );

#endregion

if ( app.Environment.IsDevelopment() )
{
    // テスト構成
    app.Logger.LogInformation( "Adding Development middleware..." );

    app.UseDeveloperExceptionPage();
    app.UseShowAllServicesMiddleware();
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    // 本番構成
    app.Logger.LogInformation( "Adding non-Development middleware..." );

    app.UseExceptionHandler( "/Error" );
    app.UseHsts();
}

// HTTP 要求を HTTPS にリダイレクトするためのHTTPS リダイレクト ミドルウェア
app.UseHttpsRedirection();

// ルートパス "/" から Blazor Webasframework ファイルを処理するようにアプリケーションを構成
app.UseBlazorFrameworkFiles();

// Web ルート内のファイルの提供
app.UseStaticFiles();

// ルーティング
app.UseRouting();

// Cookie ポリシー
app.UseCookiePolicy();

// 認証
app.UseAuthentication();

// 認可
app.UseAuthorization();

#region エンドポイント設定

app.UseEndpoints
    (
        endpoints =>
        {
            // ルート テンプレート
            endpoints.MapControllerRoute( "default", "{controller:slugify=Home}/{action:slugify=Index}/{id?}" );

            // Razor Pages のエンドポイント
            endpoints.MapRazorPages();

            // 正常性チェック
            endpoints.MapHealthChecks( "home_page_health_check", new() { Predicate = check => check.Tags.Contains( "homePageHealthCheck" ) } );
            endpoints.MapHealthChecks( "api_health_check"      , new() { Predicate = check => check.Tags.Contains( "apiHealthCheck" ) } );

            // https://docs.microsoft.com/ja-jp/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-6.0
            // 対話型コンポーネントの着信接続を受け入れるように構成？よくわからない
            //endpoints.MapBlazorHub("/admin");

            // 要求の URL パスにファイル名が含まれておらず、他のエンドポイントが一致していないケースを処理するためのもの
            endpoints.MapFallbackToFile( "index.html" );
        }
    );

#endregion

app.Logger.LogInformation( "Seeding Database..." );

using ( var scope = app.Services.CreateScope() )
{
    var scopedProvider = scope.ServiceProvider;

    try
    {
        var catalogContext = scopedProvider.GetRequiredService<CatalogContext>();

        await CatalogContextSeed.SeedAsync( catalogContext, app.Logger );

        var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scopedProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await AppIdentityDbContextSeed.SeedAsync( userManager, roleManager );
    }
    catch ( Exception ex )
    {
        app.Logger.LogError( ex, "An error occurred seeding the DB." );
    }
}

#endregion

app.Logger.LogInformation( "LAUNCHING" );
app.Run();
