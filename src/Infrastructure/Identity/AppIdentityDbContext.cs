using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.eShopWeb.Infrastructure.Identity;

/// <summary>
/// アプリケーションDBユーザー情報作成
/// </summary>
public class AppIdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public AppIdentityDbContext( DbContextOptions<AppIdentityDbContext> options )
        : base( options )
    {
    }

    protected override void OnModelCreating( ModelBuilder builder )
    {
        base.OnModelCreating( builder );

        // ASP.NET Identityモデルをカスタマイズし、必要に応じてデフォルトをオーバーライドします。
        // たとえば、ASP.NETIdentityテーブル名などの名前を変更できます。
        // base.OnModelCreating（builder）; を呼び出した後、カスタマイズを追加します。
    }
}
