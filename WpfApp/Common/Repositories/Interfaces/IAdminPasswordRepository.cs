using WpfApp.Common.Models;

namespace WpfApp.Common.Repositories.Interfaces
{
    public interface IAdminPasswordRepository
    {
        Task<bool> ExistsAsync();

        Task<int> InsertAsync(PasswordData password);

        Task<int> UpdateAsync(PasswordData password);

        Task<int> DeleteAsync();

        Task<PasswordData?> GetAsync();
    }
}
