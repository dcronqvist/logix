using System.IO;

namespace LogiX.UserInterfaceContext;

public interface IFileSystemProvider
{
    bool FileExists(string path);
    bool DirectoryExists(string path);

    Stream ReadFile(string path);
    void WriteFile(string path, Stream contents);

    void DeleteFile(string path);
    void DeleteDirectory(string path);

    void CreateDirectory(string path);
}

public class FileSystemProvider : IFileSystemProvider
{
    public bool FileExists(string path) => File.Exists(path);
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public Stream ReadFile(string path) => File.OpenRead(path);
    public void WriteFile(string path, Stream contents)
    {
        using var fileStream = File.OpenWrite(path);
        contents.CopyTo(fileStream);
    }

    public void DeleteFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    public void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
}
