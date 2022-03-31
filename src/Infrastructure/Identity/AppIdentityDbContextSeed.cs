using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.ApplicationCore.Constants;

namespace Microsoft.eShopWeb.Infrastructure.Identity;

/// <summary>
/// アプリケーションDBユーザー
/// </summary>
public class AppIdentityDbContextSeed
{
    /// <summary>
    /// アプリケーションDBユーザー作成
    /// </summary>
    /// <param name="aUserManager"></param>
    /// <param name="aRoleManager"></param>
    /// <returns>なし</returns>
    public static async Task SeedAsync( UserManager<ApplicationUser> aUserManager, RoleManager<IdentityRole> aRoleManager )
    {
        // 管理者グループ作成
        await aRoleManager.CreateAsync( new IdentityRole( BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS ) );

        #region デフォルトユーザー作成
        {
            // デフォルトユーザー作成
            string userName = "demouser@microsoft.com";
            var defaultUser = new ApplicationUser 
            { 
                UserName = userName, 
                Email    = userName
            };

            await aUserManager.CreateAsync( defaultUser, AuthorizationConstants.DEFAULT_PASSWORD );
        }
        #endregion

        #region Adminユーザー作成
        {
            // Adminユーザー作成
            string userName = "admin@microsoft.com";
            var adminUser = new ApplicationUser 
            { 
                UserName = userName, 
                Email    = userName
            };

            await aUserManager.CreateAsync( adminUser, AuthorizationConstants.DEFAULT_PASSWORD );

            // Adminユーザーを管理者グループへ追加
            adminUser = await aUserManager.FindByNameAsync( userName );

            await aUserManager.AddToRoleAsync( adminUser, BlazorShared.Authorization.Constants.Roles.ADMINISTRATORS );
        }
        #endregion
    }
}
