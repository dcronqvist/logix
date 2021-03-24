using LogiX.Projects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogiX.Assets
{
    class ICCollectionLoader : IAssetLoader
    {
        public Asset LoadAsset(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                return JsonConvert.DeserializeObject<ICCollection>(sr.ReadToEnd());
            }
        }
    }
}
