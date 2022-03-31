using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.Infrastructure.Services;

/// <summary>
/// このクラスは、アカウントの確認とパスワードリセットのための電子メールを
/// 送信するためにアプリケーションによって使用されます。
/// 
/// https://go.microsoft.com/fwlink/?LinkID=532713
/// </summary>
public class EmailSender : IEmailSender
{
    /// <summary>
    /// E-Mail送信（未実装）
    /// </summary>
    /// <param name="aEmail">E-Mailアドレス</param>
    /// <param name="aSubject">主題</param>
    /// <param name="aMessage">本文</param>
    /// <returns>正常に完了したタスク</returns>
    public Task SendEmailAsync( string aEmail, string aSubject, string aMessage )
    {
        // TODO: これを、SendGrid、ローカルSMTPなどを介して実際の電子メール送信ロジックに接続します。

        return Task.CompletedTask;
    }
}
