using System;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console.Cli;

namespace PwSafeClient.Cli.Infrastructure;

public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _builder;
    private IServiceProvider? _provider;
    private readonly object _lock = new();

    public TypeRegistrar(IServiceCollection builder)
    {
        _builder = builder;
    }

    public ITypeResolver Build()
    {
        if (_provider != null)
        {
            return new NonDisposingTypeResolver(_provider);
        }

        lock (_lock)
        {
            _provider ??= _builder.BuildServiceProvider();
        }

        return new NonDisposingTypeResolver(_provider);
    }

    public void Register(Type service, Type implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        _builder.AddSingleton(service, (provider) => func());
    }
}
