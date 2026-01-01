using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.Cli.Models;

namespace PwSafeClient.Cli.Contracts.Services;

internal interface IDocumentService
{
    /// <summary>
    /// Try to load a document from given alias or file path.
    /// </summary>
    /// <param name="alias">The alias of file</param>
    /// <param name="filepath">The file info of database</param>
    /// <param name="readOnly">Whether to load the document as readonly</param>
    /// <returns>The document</returns>
    Task<Document?> TryLoadDocumentAsync(string? alias, string? filepath, bool readOnly, PasswordOptions? passwordOptions = null);

    /// <summary>
    /// Save the document to given alias or file path.
    /// </summary>
    /// <param name="alias">The alias of file</param>
    /// <param name="filepath">The file info of database</param>
    /// <returns></returns>
    Task SaveDocumentAsync(string? alias, string? filepath);

    /// <summary>
    /// Save the document to given alias or file path.
    /// </summary>
    /// <param name="document">The document to save</param>
    /// <param name="alias">The alias of file</param>
    /// <param name="filepath">The file info of database</param>
    /// <returns></returns>
    Task SaveDocumentAsync(Document document, string? alias, string? filepath);

    /// <summary>
    /// Get the display name of the document.
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="filepath"></param>
    /// <returns></returns>
    Task<string> GetDocumentDisplayNameAsync(string? alias, string? filepath);

    /// <summary>
    /// Get the file path of the document.
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="filepath"></param>
    /// <returns>The resolvd filepath</returns>
    Task<string> GetDocumentFilePathAsync(string? alias, string? filepath);
}
