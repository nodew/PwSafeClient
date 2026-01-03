using System.Collections.Generic;
using System.Text.Json.Serialization;

using PwSafeClient.Cli.Models;

namespace PwSafeClient.Cli.Json;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true)]
[JsonSerializable(typeof(Configuration), TypeInfoPropertyName = nameof(Configuration))]
[JsonSerializable(typeof(ErrorResponse), TypeInfoPropertyName = nameof(ErrorResponse))]
[JsonSerializable(typeof(GetPasswordResponse), TypeInfoPropertyName = nameof(GetPasswordResponse))]
[JsonSerializable(typeof(DatabaseInfoResponse), TypeInfoPropertyName = nameof(DatabaseInfoResponse))]
[JsonSerializable(typeof(EntryDetailsResponse), TypeInfoPropertyName = nameof(EntryDetailsResponse))]
[JsonSerializable(typeof(List<DatabaseListItem>), TypeInfoPropertyName = "DatabaseList")]
[JsonSerializable(typeof(List<EntryListItem>), TypeInfoPropertyName = "EntryList")]
[JsonSerializable(typeof(List<EntrySearchResult>), TypeInfoPropertyName = "EntrySearchResults")]
[JsonSerializable(typeof(List<PolicyListItem>), TypeInfoPropertyName = "PolicyList")]
internal partial class CliJsonContext : JsonSerializerContext
{
}
