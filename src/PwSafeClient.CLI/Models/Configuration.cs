using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PwSafeClient.Cli.Models;

internal record Configuration
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

    /// <summary>
    /// The idle time in minutes before the interactive terminal exits.
    /// </summary>
    [JsonPropertyName("idleTime")]
    public int IdleTime { get; set; } = 5;

    /// <summary>
    /// The maximum number of backup files to keep.
    /// </summary>
    [JsonPropertyName("maxBackupCount")]
    public int MaxBackupCount { get; set; } = 5;
}
