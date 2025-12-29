using System.Threading;
using System.Threading.Tasks;

namespace PwSafeClient.Maui.Services;

public sealed class NoopSaveFileService : ISaveFileService
{
    public Task<string?> PickSaveFilePathAsync(string suggestedFileName, CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(null);
}
