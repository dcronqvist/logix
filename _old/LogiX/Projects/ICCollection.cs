using LogiX.Assets;
using LogiX.Circuits.Integrated;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Projects
{
    class ICCollection : Asset
    {
        public List<ICDescription> Descriptions { get; set; }

        public ICCollection()
        {
            
        }

        public ICCollection(string name, List<ICDescription> descriptions)
        {
            base.Name = name;
            this.Descriptions = descriptions;
        }
    }
}
