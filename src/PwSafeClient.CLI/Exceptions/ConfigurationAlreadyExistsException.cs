using System;

namespace PwSafeClient.Cli.Exceptions;

internal class ConfigurationAlreadyExistsException : Exception
{
    public ConfigurationAlreadyExistsException(string message) : base(message)
    {
    }
}
