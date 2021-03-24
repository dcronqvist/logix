using LogiX.Projects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogiX.Assets
{
    class LogixProjectLoader : IAssetLoader
    {
        public Asset LoadAsset(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                return JsonConvert.DeserializeObject<LogiXProject>(sr.ReadToEnd());
            }
        }
    }
}
