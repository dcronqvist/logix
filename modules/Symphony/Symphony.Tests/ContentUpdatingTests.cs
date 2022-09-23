// using System;
// using System.Collections.Generic;
// using System.Diagnostics.CodeAnalysis;
// using System.IO;
// using System.Text;
// using System.Text.Json;
// using Xunit;

// namespace Symphony.Tests;

// public class ContentUpdatingTests
// {
//     class TestValueUpdatesMetadata : ContentMetadata
//     {
//         public string Name { get; set; }
//         public string Author { get; set; }
//     }

//     class TestValueUpdatesValidator : IContentStructureValidator<TestValueUpdatesMetadata>
//     {
//         public bool TryValidateMod(IContentStructure structure, [NotNullWhen(true)] out TestValueUpdatesMetadata? metadata, [NotNullWhen(false)] out string? error)
//         {
//             if (structure.TryGetFileStream("metadata.json", out var stream))
//             {
//                 try
//                 {
//                     using (var reader = new StreamReader(stream))
//                     {
//                         var options = new JsonSerializerOptions()
//                         {
//                             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//                             IncludeFields = true
//                         };
//                         metadata = JsonSerializer.Deserialize<TestValueUpdatesMetadata>(reader.ReadToEnd(), options)!;

//                         error = null;
//                         return true;
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     error = $"Failed to deserialize metadata.json: {ex.Message}";
//                     metadata = null;
//                     return false;
//                 }
//             }
//             else
//             {
//                 error = "Missing metadata.json";
//                 metadata = null;
//                 return false;
//             }
//         }
//     }

//     class TestValueUpdatesContentItem : ContentItem<string>
//     {
//         public TestValueUpdatesContentItem(string identifier, IContentSource source, string content) : base(identifier, source, content)
//         {
//         }

//         protected override void OnContentUpdated(object newContent)
//         {
//             // Nothing
//         }
//     }

//     class TestValueUpdatesLoader : IContentLoader<TestValueUpdatesMetadata>
//     {
//         public IEnumerable<ContentItem> LoadContent(TestValueUpdatesMetadata metadata, IContentSource source)
//         {
//             using (var structure = source.GetStructure())
//             {
//                 using (var reader = new StreamReader(structure.GetFileStream("metadata.json")))
//                 {
//                     yield return new TestValueUpdatesContentItem($"{source.GetIdentifier()}.metadata", source, reader.ReadToEnd());
//                 }
//             }
//         }
//     }

//     [Fact]
//     public void TestValueUpdates()
//     {
//         // Setup
//         var entryMetadata = new TestContentEntry("metadata.json", Encoding.UTF8.GetBytes("{\"name\": \"Content 1, the BEST content\", \"author\": \"Author McAuthorson\"}"));
//         var content1 = new TestContentSource("content1", entryMetadata);

//         // Create manager with config
//         var collection = IContentCollectionProvider.FromListOfSources(content1);
//         var config = new ContentManagerConfiguration<TestValueUpdatesMetadata>(new TestValueUpdatesValidator(), collection, new TestValueUpdatesLoader());
//         var manager = new ContentManager<TestValueUpdatesMetadata>(config);

//         // Load content
//         manager.Load();

//         // Get value of a content item
//         var content1ItemMetadata = manager.GetContentItem<TestValueUpdatesContentItem>("content1.metadata");
//         Assert.NotNull(content1ItemMetadata);
//         var valueBefore = content1ItemMetadata!.Content;

//         // Only change the underlying data inside the content source
//         entryMetadata.Data = Encoding.UTF8.GetBytes("{\"name\": \"Content 1, the BEST content, NOW UPDATED\", \"author\": \"Author McAuthorson\"}");

//         // Reload the content
//         manager.Load();

//         // Get value of the content item after reload
//         var valueAfter = content1ItemMetadata!.Content;

//         // Value should have changed
//         Assert.NotEqual(valueBefore, valueAfter);
//     }
// }