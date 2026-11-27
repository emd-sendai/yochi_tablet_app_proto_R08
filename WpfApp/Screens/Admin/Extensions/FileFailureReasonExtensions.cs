using WpfApp.Screens.Admin.Enums;

namespace WpfApp.Screens.Admin.Extensions
{
    public static class FileFailureReasonExtensions
    {
        public static string GetErrorMessage(this FileAddFailureReason reason)
        {
            return reason switch
            {
                FileAddFailureReason.FileNotFound => "選択されたファイルが存在しません。",
                FileAddFailureReason.FileAlreadyExists => "既に同名のファイルが存在します。",
                FileAddFailureReason.DirectoryNotFound => "フォルダが存在しません。",
                FileAddFailureReason.UnauthorizedAccess => "ファイルやフォルダへのアクセス権限がありません。",
                FileAddFailureReason.PathTooLong => "ファイル名が対応可能文字数を超過しています。",
                FileAddFailureReason.IO => "ファイルの使用状況やフォルダの空き容量等を確認してください。",
                FileAddFailureReason.Others => "その他のエラー（管理者へお問い合わせください。）",
                _ => "不正なエラー（管理者へお問い合わせください。）"
            };
        }

        public static string GetErrorMessage(this FileOverwriteFailureReason reason)
        {
            return reason switch
            {
                FileOverwriteFailureReason.DestFileNotFound => "ファイルが存在しないため差し替えできません。",
                FileOverwriteFailureReason.FileNotFound => "ファイルが存在しないため差し替えできません。",
                FileOverwriteFailureReason.DirectoryNotFound => "フォルダが存在しないため差し替えできません。",
                FileOverwriteFailureReason.UnauthorizedAccess => "ファイルやフォルダへのアクセス権限がないため差し替えできません。",
                FileOverwriteFailureReason.PathTooLong => "ファイル名が対応可能文字数を超過しているため差し替えできません。",
                FileOverwriteFailureReason.IO => "ファイルを差し替えできません。\n使用状況やフォルダの空き容量等を確認してください。",
                FileOverwriteFailureReason.Others => "エラーが発生したため差し替えできませんでした。\n管理者へお問い合わせください。",
                _ => "不正なエラーが発生したため差し替えできませんでした。\n管理者へお問い合わせください。"
            };
        }

        public static string GetErrorMessage(this FileDeleteFailureReason reason)
        {
            return reason switch
            {
                FileDeleteFailureReason.IsUsedInLink=> "リンクテキストから参照されているため削除できません。",
                FileDeleteFailureReason.FileNotFound => "ファイルが存在しないため削除できません。",
                FileDeleteFailureReason.DirectoryNotFound => "フォルダが存在しないため削除できません。",
                FileDeleteFailureReason.UnauthorizedAccess => "ファイルやフォルダへのアクセス権限がないため削除できません。",
                FileDeleteFailureReason.PathTooLong => "削除対象ファイル名が対応可能文字数を超過しているため削除できません。",
                FileDeleteFailureReason.IO => "ファイルを削除できません。\n使用状況や読み取り制限等を確認してください。",
                FileDeleteFailureReason.Others => "エラーが発生したため削除できませんでした。\n管理者へお問い合わせください。",
                _ => "不正なエラーが発生したため削除できませんでした。\n管理者へお問い合わせください。"
            };
        }
    }
}
