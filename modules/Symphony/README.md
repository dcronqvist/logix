# 🎼 Symphony

**A content management library for all your gamedev needs**

Symphony provides a simple API for specifying where to look for content, it will then find all content and allow you to specify how that content must be structure in order to be considered valid, as well as how to load it.

This library was created with the intention to make it easy for game developers to have multiple sources of content, making it easier to load content that was created by e.g. modders.

```csharp
var contentSourceFactory = (path) => {
    if (Path.GetExtension(path) == ".zip")
        return new ZipFileContentSource(path);
    else if (Directory.Exists(path))
        return new DirectoryContentSource(path);
    return null;
};

var validator = new Validator();
var collection = new DirectoryCollectionProvider(@"path/to/content",
                                                 contentSourceFactory);
var loader = new Loader();

var config = new ContentManagerConfig<ContentMeta>(validator,
                                                   collection,
                                                   loader);
var manager = new ContentManager<ContentMeta>(config);

manager.Load();
```
