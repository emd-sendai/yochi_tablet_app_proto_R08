namespace WpfApp.Screens.Admin.Enums
{
    public enum FileAddFailureReason
    {
        FileNotFound,
        FileAlreadyExists, // 追加時の手動での存在確認で使用
        DirectoryNotFound,
        UnauthorizedAccess,
        PathTooLong,
        IO,
        Others
    }
}
