using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Assets
{
    interface IAssetLoader
    {
        public Asset LoadAsset(string filePath);
    }
}
