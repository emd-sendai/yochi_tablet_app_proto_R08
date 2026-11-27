namespace WpfApp.Screens.Admin.Enums
{
    public enum FileOverwriteFailureReason
    {
        DestFileNotFound,   // 差し替え時の手動での存在確認で使用
        FileNotFound,
        DirectoryNotFound,
        UnauthorizedAccess,
        PathTooLong,
        IO,
        Others
    }
}
