using System.Threading;
using System.Threading.Tasks;

namespace PwSafeClient.Maui.Services;

public interface IFilePickerService
{
    Task<string?> PickDatabaseFileAsync(CancellationToken cancellationToken = default);
}
