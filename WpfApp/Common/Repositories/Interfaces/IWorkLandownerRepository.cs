using WpfApp.Common.Models;

namespace WpfApp.Common.Repositories.Interfaces
{
    public interface IWorkLandownerRepository
    {
        Task<int> InsertAsync(WorkLandownerData workLandowner);

        Task<int> InsertRangeAsync(List<WorkLandownerData> workLandowners);

        Task<int> DeleteAsync(int workId, int landownerId);

        Task<List<WorkLandownerData>> GetWorkLandownersByLandownerId(int landownerId);

        Task<List<LandownerData>> GetLandownersByWorkIdAsync(int workId);

        Task<List<LandownerData>> GetNotRelatedLandownersByWorkIdAsync(int workId);
    }
}
