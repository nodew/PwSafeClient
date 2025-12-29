using System.Threading;
using System.Threading.Tasks;

namespace PwSafeClient.Maui.Services;

public interface ISaveFileService
{
    Task<string?> PickSaveFilePathAsync(
        string suggestedFileName,
        CancellationToken cancellationToken = default);
}
