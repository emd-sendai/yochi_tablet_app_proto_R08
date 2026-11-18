using WpfApp.Common.Models;

namespace WpfApp.Common.Repositories.Interfaces
{
    public interface IWorkLandownerRepository
    {
        Task<int> InsertAsync(WorkLandownerData workLandowner);

        Task<int> InsertRangeAsync(List<WorkLandownerData> workLandowners);

        Task RemoveLandownerAsync(int workId, int landownerId);

        Task<List<int>> GetLandownerIdsByWorkIdAsync(int workId);
    }
}
