using System;

namespace PwSafeClient.Cli.Json;

internal sealed record ErrorResponse(string Error);

internal sealed record GetPasswordResponse(Guid Id, bool CopiedToClipboard);

internal sealed record DatabaseListItem(string Alias, string Path, bool IsDefault);

internal sealed record DatabaseInfoResponse(
    Guid Uuid,
    string Name,
    string Description,
    string Version,
    string? LastSavedBy,
    DateTime LastSavedOn,
    string? LastSavedApplication,
    string? LastSavedMachine,
    int ItemCount);

internal sealed record EntryListItem(
    Guid Id,
    string Group,
    string Title,
    string Username,
    string Url,
    DateTime Created,
    DateTime Modified);

internal sealed record EntryDetailsResponse(
    Guid Id,
    string Group,
    string Title,
    string Username,
    string Url,
    string Notes,
    string PasswordPolicy,
    DateTime Created,
    DateTime Modified,
    bool HasPassword);

internal sealed record EntrySearchResult(
    Guid Id,
    string Group,
    string Title,
    string Username,
    string Url,
    DateTime Created,
    DateTime Modified);

internal sealed record PolicyListItem(
    string Name,
    int Length,
    bool Pronounceable,
    bool UseLowercase,
    int MinimumLowercaseCount,
    bool UseUppercase,
    int MinimumUppercaseCount,
    bool UseDigits,
    int MinimumDigitCount,
    bool UseSymbols,
    int MinimumSymbolCount,
    string SymbolSet,
    bool UseEasyVision,
    bool UseHexDigits);
