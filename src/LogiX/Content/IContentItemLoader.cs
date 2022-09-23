using System.Diagnostics.CodeAnalysis;
using StbImageSharp;
using Symphony;

namespace GoodGame.Content;

public interface IContentItemLoader
{
    Task<LoadEntryResult> TryLoad(IContentSource source, IContentStructure structure, string pathToItem);
}