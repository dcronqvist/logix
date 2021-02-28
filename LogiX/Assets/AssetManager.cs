using System;
using System.Collections.Generic;
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

            };

            LoadedAssets = new Dictionary<string, Asset>();
        }

        public static T GetAsset<T>(string assetName) where T : Asset
        {
            return LoadedAssets[assetName] as T;
        }

        public static void LoadAllAssets()
        {

        }
    }
}
