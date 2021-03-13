using LogiX.Assets;
using LogiX.Circuits.Drawables;
using LogiX.Circuits.Logic;
using LogiX.Circuits.Minimals;
using LogiX.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Integrated
{
    class ICDescription : Asset
    {
        public List<ICComponentDescription> Descriptions { get; set; }
        public int Inputs { get; set; }
        public int Outputs { get; set; }

        public ICDescription()
        {

        }

        public static bool ValidateComponents(List<DrawableComponent> components)
        {
            if (components.Count < 1)
                return false;

            return GenerateDescription(components) != null;
        }

        public ICDescription(List<DrawableComponent> components)
        {
            this.Descriptions = GenerateDescription(components);
            this.Inputs = CountComponentsOfType<DrawableCircuitSwitch>(components);
            this.Outputs = CountComponentsOfType<DrawableCircuitLamp>(components);
        }

        public int CountComponentsOfType<T>(IEnumerable<CircuitComponent> components) where T : CircuitComponent
        {
            int count = 0;
            foreach (CircuitComponent cc in components)
            {
                if (cc.GetType() == typeof(T))
                    count++;
            }
            return count;
        }

        private static List<ICComponentDescription> GenerateDescription(List<DrawableComponent> comps)
        {
            List<ICComponentDescription> llicc = new List<ICComponentDescription>();

            for (int i = 0; i < comps.Count; i++)
            {
                //-----------------------------------------
                string type = "";

                if (comps[i] is DrawableLogicGate)
                {
                    type = ((DrawableLogicGate)comps[i]).GetLogicName();
                }
                else if(comps[i] is DrawableCircuitLamp)
                {
                    type = "Lamp";
                }
                else if (comps[i] is DrawableCircuitSwitch)
                {
                    type = "Switch";
                }
                else if (comps[i] is DrawableIC)
                {
                    type = ((DrawableIC)comps[i]).Description.Name;
                }

                List<ICConnectionDescription> to = new List<ICConnectionDescription>();

                foreach (CircuitOutput co in comps[i].Outputs)
                {
                    foreach (DrawableWire cw in co.Signals)
                    {
                        int indexOf = comps.IndexOf(cw.To);
                        if(indexOf == -1)
                        {
                            return null;
                        }
                        ICConnectionDescription iccd = new ICConnectionDescription(indexOf, cw.FromIndex, cw.ToIndex);
                        to.Add(iccd);
                    }
                }

                ICComponentDescription icccc = new ICComponentDescription(type, to);
                if(comps[i] is DrawableCircuitLamp)
                {
                    if (((DrawableCircuitLamp)comps[i]).ID == "")
                        return null;

                    icccc.SetID(((DrawableCircuitLamp)comps[i]).ID);
                }
                if (comps[i] is DrawableCircuitSwitch)
                {
                    if (((DrawableCircuitSwitch)comps[i]).ID == "")
                        return null;

                    icccc.SetID(((DrawableCircuitSwitch)comps[i]).ID);
                }
                if (comps[i] is DrawableIC)
                {
                    DrawableIC dic = (DrawableIC)comps[i];
                    icccc.SetDescription(dic.Description);
                }

                llicc.Add(icccc);
            }

            return llicc;
        }

        public List<CircuitComponent> GenerateComponents()
        {
            CircuitComponent[] components = new CircuitComponent[Descriptions.Count];

            for (int i = 0; i < Descriptions.Count; i++)
            {
                ICComponentDescription iccd = Descriptions[i];
                CircuitComponent dc = null;
                switch (iccd.Type)
                {
                    case "ANDGateLogic":
                        dc = new MinimalLogicGate(new ANDGateLogic());
                        break;
                    case "ORGateLogic":
                        dc = new MinimalLogicGate(new ORGateLogic());
                        break;
                    case "XORGateLogic":
                        dc = new MinimalLogicGate(new XORGateLogic());
                        break;
                    case "NORGateLogic":
                        dc = new MinimalLogicGate(new NORGateLogic());
                        break;
                    case "Switch":
                        //CircuitSwitch cs = new CircuitSwitch(Vector2.Zero);
                        //cs.SetID(iccd.ID);
                        dc = new MinimalSwitch() { ID = iccd.ID };
                        break;
                    case "Lamp":
                        //CircuitLamp cl = new CircuitLamp(Vector2.Zero);
                        //cl.SetID(iccd.ID);
                        dc = new MinimalLamp() { ID = iccd.ID };
                        break;
                    default:
                        // It is not a built in thing - look for it in the IC files folder
                        ICDescription icd = iccd.Description;
                        dc = new DrawableIC(Vector2.Zero, icd.Name, icd);
                        break;
                }
                components[i] = dc;
            }

            for (int i = 0; i < Descriptions.Count; i++)
            {
                ICComponentDescription iccd = Descriptions[i];

                foreach (ICConnectionDescription connection in iccd.To)
                {
                    CircuitWire cw = new CircuitWire();

                    components[i].AddOutputWire(connection.OutIndex, cw);
                    components[connection.To].SetInputWire(connection.InIndex, cw);
                }
            }

            return components.ToList();
        }

        public void SaveToFile(string name)
        {
            base.Name = name;
            string filePath = Utility.CreateICFilePath(name);
            using(StreamWriter sw = new StreamWriter(filePath))
            {
                sw.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            //AssetManager.AddAsset(name, this);
        }

        public bool DeleteFile()
        {
            string file = Utility.CreateICFilePath(base.Name);

            if (File.Exists(file))
            {
                File.Delete(file);
                
                return true;
            }
            return false;
        }
    }
}
