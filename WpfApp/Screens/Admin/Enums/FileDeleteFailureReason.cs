namespace WpfApp.Screens.Admin.Enums
{
    public enum FileDeleteFailureReason
    {
        IsUsedInLink,  // 削除時の手動での存在確認で使用
        FileNotFound,  // 削除時の手動での存在確認で使用
        DirectoryNotFound,
        UnauthorizedAccess,
        PathTooLong,
        IO,
        Others
    }
}
