using LogiX.Logging;
using LogiX.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogiX.Assets
{
    static class AssetManager
    {
        private static Dictionary<string, IAssetLoader> fileExtensionToAssetLoader;
        private static Dictionary<string, Asset> LoadedAssets { get; set; }

        private static void Initialize()
        {
            fileExtensionToAssetLoader = new Dictionary<string, IAssetLoader>()
            {
                { Utility.EXT_IC, new ICDescriptionLoader() }
            };

            LoadedAssets = new Dictionary<string, Asset>();
        }

        public static T GetAsset<T>(string assetName) where T : Asset
        {
            return LoadedAssets[assetName] as T;
        }

        private static IAssetLoader GetLoaderFromFilePath(string filePath)
        {
            string ext = Path.GetExtension(filePath);

            return fileExtensionToAssetLoader[ext];
        }

        public static string[] GetAllAssetFiles()
        {
            Directory.CreateDirectory(Utility.ASSETS_DIR);

            return Directory.GetFiles(Utility.ASSETS_DIR, "*.*", SearchOption.AllDirectories);
        }

        public static bool LoadFile(string filePath)
        {
            try
            {
                IAssetLoader loader = GetLoaderFromFilePath(filePath);
                Asset asset = loader.LoadAsset(filePath);
                asset.Name = Path.GetFileNameWithoutExtension(filePath);
                LoadedAssets.Add(asset.Name, asset);
                LogManager.AddEntry($"Successfully loaded asset '{asset.Name}'!", LogEntryType.INFO);
                return true;
            }
            catch
            {
                LogManager.AddEntry($"Failed to load asset '{Path.GetFileNameWithoutExtension(filePath)}'!", LogEntryType.ERROR);
                return false;
            }
        }

        public static void LoadAllAssets()
        {
            Initialize();

            string[] files = GetAllAssetFiles();

            foreach(string file in files)
            {
                LoadFile(file);
            }
        }

        public static List<T> GetAllAssetsOfType<T>() where T : Asset
        {
            List<T> lst = new List<T>();

            foreach(T t in LoadedAssets.Values.Where(x => x.GetType() == typeof(T)))
            {
                lst.Add(t);
            }
            return lst;
        }

        public static void AddAsset(string name, Asset a)
        {
            a.Name = name;
            LoadedAssets.Add(name, a);
        }

        public static void RemoveAsset(string name)
        {
            if(LoadedAssets.ContainsKey(name))
                LoadedAssets.Remove(name);
        }
    }
}
