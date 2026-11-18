using WpfApp.Common.Models;

namespace WpfApp.Common.Repositories.Interfaces
{
    public interface ILandownerRepository
    {
        Task<int> InsertAsync(LandownerData landowner);

        Task<int> InsertRangeAsync(List<LandownerData> landowners);

        Task UpdateAsync(LandownerData landowner);

        Task DeleteAsync(int landownerId);

        Task<LandownerData?> GetByIdAsync(int landownerId);

        Task<List<LandownerData>> GetAllAsync();
    }
}
