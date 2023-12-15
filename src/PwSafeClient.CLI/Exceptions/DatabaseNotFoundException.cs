using System;

namespace PwSafeClient.CLI.Exceptions;

public class DatabaseNotFoundException : Exception
{
    public DatabaseNotFoundException(string alias) : base($"The database '{alias}' does not exist.")
    {
    }
}
