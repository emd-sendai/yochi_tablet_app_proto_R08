using WpfApp.Common.Enums;

namespace WpfApp.Common.Extensions
{
    public static class FileGetFailureReasonExtensions
    {
        public static string GetErrorMessage(this FileGetFailureReason reason)
        {
            return reason switch
            {
                FileGetFailureReason.DirectryNotExists => "表示できるファイルがありません。",
                FileGetFailureReason.FileNotExists => "表示できるファイルがありません。",
                FileGetFailureReason.DirectoryNotFound => "ファイルの取得中に対象のフォルダが削除されました。",
                FileGetFailureReason.UnauthorizedAccess => "ファイルやフォルダへのアクセス権限がないため表示できません。",
                FileGetFailureReason.IO => "ファイルを表示できません。\n使用状況等を確認してください。",
                FileGetFailureReason.Others => "エラーが発生したため表示できませんでした。\n管理者へお問い合わせください。",
                _ => "不正なエラーが発生したため表示できませんでした。\n管理者へお問い合わせください。"
            };
        }
    }
}
