using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LogiX.Circuits.Integrated;
using Newtonsoft.Json;

namespace LogiX.Assets
{
    class ICDescriptionLoader : IAssetLoader
    {
        public Asset LoadAsset(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                return JsonConvert.DeserializeObject<ICDescription>(sr.ReadToEnd());
            }
        }
    }
}
