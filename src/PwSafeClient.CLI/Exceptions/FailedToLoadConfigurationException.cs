using System;

namespace PwSafeClient.Cli.Exceptions;

internal class FailedToLoadConfigurationException : Exception
{
    public FailedToLoadConfigurationException(string message) : base(message)
    {
    }
}
