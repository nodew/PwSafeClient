using System;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Infrastructure;

public sealed class NonDisposingTypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider;

    public NonDisposingTypeResolver(IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public object? Resolve(Type? type)
    {
        if (type == null)
        {
            return null;
        }

        var service = _provider.GetService(type);
        if (service != null)
        {
            return service;
        }

        if (type.IsAbstract || type.IsInterface)
        {
            return null;
        }

        return ActivatorUtilities.CreateInstance(_provider, type);
    }

    public void Dispose()
    {
        // Intentionally do not dispose the underlying service provider.
        // This enables running multiple commands in-process (interactive mode).
    }
}
