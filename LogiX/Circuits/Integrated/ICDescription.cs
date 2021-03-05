using LogiX.Circuits.Drawables;
using LogiX.Circuits.Logic;
using LogiX.Circuits.Minimals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Integrated
{
    class ICDescription
    {
        public List<ICComponentDescription> Descriptions { get; set; }
        public int Inputs { get; set; }
        public int Outputs { get; set; }

        public ICDescription()
        {

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

        private List<ICComponentDescription> GenerateDescription(List<DrawableComponent> comps)
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

                List<ICConnectionDescription> to = new List<ICConnectionDescription>();

                foreach (CircuitOutput co in comps[i].Outputs)
                {
                    foreach (DrawableWire cw in co.Signals)
                    {
                        ICConnectionDescription iccd = new ICConnectionDescription(comps.IndexOf(cw.To), cw.FromIndex, cw.ToIndex);
                        to.Add(iccd);
                    }
                }

                ICComponentDescription icccc = new ICComponentDescription(type, to);

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
                    case "Switch":
                        //CircuitSwitch cs = new CircuitSwitch(Vector2.Zero);
                        //cs.SetID(iccd.ID);
                        dc = new MinimalSwitch();
                        break;
                    case "Lamp":
                        //CircuitLamp cl = new CircuitLamp(Vector2.Zero);
                        //cl.SetID(iccd.ID);
                        dc = new MinimalLamp();
                        break;
                    default:
                        // It is not a built in thing - look for it in the IC files folder
                        //ICDescription icd = ResourceManager.GetResource<ICDescription>("ic_" + iccd.Type.Replace(" ", "-"));
                        //dc = new ICComponent(icd, Vector2.Zero);
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
    }
}
