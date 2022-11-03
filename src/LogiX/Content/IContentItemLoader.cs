using System.Diagnostics.CodeAnalysis;
using StbImageSharp;
using Symphony;

namespace LogiX.Content;

public interface IContentItemLoader
{
    Task<LoadEntryResult> TryLoad(IContentSource source, IContentStructure structure, string pathToItem);
}