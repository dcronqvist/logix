using System.Diagnostics.CodeAnalysis;
using StbImageSharp;
using Symphony;

namespace LogiX.Content;

public interface IContentItemLoader
{
    IAsyncEnumerable<LoadEntryResult> TryLoadAsync(IContentSource source, IContentStructure structure, string pathToItem);
}