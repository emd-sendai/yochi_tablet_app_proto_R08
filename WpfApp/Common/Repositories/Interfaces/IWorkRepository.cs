using WpfApp.Common.Models;

namespace WpfApp.Common.Repositories.Interfaces
{
    public interface IWorkRepository
    {
        Task<int> InsertAsync(WorkData work);

        Task<int> InsertRangeAsync(List<WorkData> works);

        Task UpdateAsync(WorkData work);

        Task DeleteAsync(int workId);

        Task<WorkData?> GetByIdAsync(int workId);

        Task<List<WorkData>> GetAllAsync();
    }
}
