using WpfApp.Common.Models;

namespace WpfApp.Common.Repositories.Interfaces
{
    public interface IWorkRepository
    {
        Task<int> InsertAndGetWorkIdAsync(WorkData work);

        Task<int> InsertRangeAsync(List<WorkData> works);

        Task<int> UpdateAsync(WorkData work);

        Task<int> DeleteAsync(int workId);

        Task<WorkData?> GetByIdAsync(int workId);

        Task<List<WorkData>> GetAllAsync();
    }
}
