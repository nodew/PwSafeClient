using System;

namespace PwSafeClient.Cli.Exceptions;

internal class FailedToSaveConfigurationException : Exception
{
    public FailedToSaveConfigurationException(string message) : base(message)
    {
    }
}
