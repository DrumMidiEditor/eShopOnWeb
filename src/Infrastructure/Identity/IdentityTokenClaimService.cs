using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.ApplicationCore.Constants;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.eShopWeb.Infrastructure.Identity;

/// <summary>
/// アプリケーションユーザートークン取得
/// </summary>
public class IdentityTokenClaimService : ITokenClaimsService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityTokenClaimService( UserManager<ApplicationUser> userManager )
    {
        _userManager = userManager;
    }

    /// <summary>
    /// アプリケーションユーザーのトークン取得
    /// </summary>
    /// <param name="aUserName">ユーザー名</param>
    /// <returns>トークン</returns>
    public async Task<string> GetTokenAsync( string aUserName )
    {
        // クレームリスト作成
        //   ユーザーとユーザーの役割情報追加
        var user   = await _userManager.FindByNameAsync( aUserName );
        var roles  = await _userManager.GetRolesAsync( user );
        var claims = new List<Claim> 
        { 
            new( ClaimTypes.Name, aUserName ) 
        };
        foreach ( var role in roles )
        {
            claims.Add( new( ClaimTypes.Role, role ) );
        }

        // トークン作成
        // https://docs.microsoft.com/ja-jp/dotnet/api/system.identitymodel.tokens.securitytokendescriptor?view=netframework-4.8
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // 発行されるトークンに含める出力クレーム
            // クレームは、発行者によって作成されたエンティティに関するステートメントで
            // そのエンティティのプロパティ、権限、またはその他の品質を記述します。
            // https://docs.microsoft.com/ja-jp/dotnet/api/system.security.claims.claimsidentity?view=net-6.0
            Subject = new( claims.ToArray() ),

            // 有効期限の設定？（この値はUTCである必要があります）
            Expires = DateTime.UtcNow.AddDays( 7 ),

            // トークンに署名するために使用する資格情報
            SigningCredentials = new
                ( 
                    new SymmetricSecurityKey( Encoding.ASCII.GetBytes( AuthorizationConstants.JWT_SECRET_KEY ) ), 
                    SecurityAlgorithms.HmacSha256Signature 
                )
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken( tokenDescriptor );

        return tokenHandler.WriteToken( token );
    }
}
