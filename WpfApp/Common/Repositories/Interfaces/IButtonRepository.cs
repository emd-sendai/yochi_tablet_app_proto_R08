using WpfApp.Common.Models;

namespace WpfApp.Common.Repositories.Interfaces
{
    public interface IButtonRepository
    {
        Task<int> CountAsync();

        Task<int> InsertRangeAsync(List<ButtonData> buttons);

        Task<int> UpdateAsync(ButtonData button);

        Task<List<ButtonData>> GetButtonsAsync(int type);
    }
}
