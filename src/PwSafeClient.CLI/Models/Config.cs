using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PwSafeClient.CLI.Models;

/// <summary>
/// Stores the configuration of the CLI.
/// </summary>
public class Config
{
    /// <summary>
    /// Stores the alias of the default database.
    /// </summary>
    [JsonPropertyName("defaultDb")]
    public string? DefaultDatabase { get; set; }

    /// <summary>
    /// Stores the alias and the absolute path to the database file.
    /// </summary>
    [JsonPropertyName("databases")]
    public Dictionary<string, string> Databases { get; set; } = new Dictionary<string, string>();
}
