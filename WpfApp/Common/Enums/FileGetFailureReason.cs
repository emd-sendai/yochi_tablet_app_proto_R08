namespace WpfApp.Common.Enums
{
    public enum FileGetFailureReason
    {
        DirectryNotExists, // 表示時の手動での存在確認時に使用
        FileNotExists, // 表示時の手動での存在確認時に使用
        DirectoryNotFound,
        UnauthorizedAccess,
        IO,
        Others
    }
}
