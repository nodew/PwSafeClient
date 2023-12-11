using System.IO;
using System.Threading.Tasks;
using Medo.Security.Cryptography.PasswordSafe;

namespace PwSafeClient.CLI.Contracts.Helpers;

public interface IDocumentHelper
{
    /// <summary>
    /// Try to load a document from given alias or file path.
    /// </summary>
    /// <param name="alias">The alias of file</param>
    /// <param name="fileInfo">The file info of database</param>
    /// <param name="readOnly">Whether to load the document as readonly</param>
    /// <returns>The document</returns>
    Task<Document?> TryLoadDocumentAsync(string? alias, FileInfo? fileInfo, bool readOnly);
}