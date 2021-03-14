using LogiX.Circuits;
using LogiX.Circuits.Drawables;
using LogiX.Circuits.Integrated;
using LogiX.Circuits.Logic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace LogiX.Projects
{
    class WorkspaceContainer
    {
        public List<WorkspaceComponent> Components { get; set; }

        public WorkspaceContainer()
        {

        }

        public WorkspaceContainer(List<DrawableComponent> components)
        {
            this.Components = GenerateComponents(components);
        }

        public List<WorkspaceComponent> GenerateComponents(List<DrawableComponent> components)
        {
            List<WorkspaceComponent> comps = new List<WorkspaceComponent>();

            for (int i = 0; i < components.Count; i++)
            {
                DrawableComponent dc = components[i];

                string type = "";

                if (dc is DrawableLogicGate)
                {
                    type = ((DrawableLogicGate)dc).GetLogicName();
                }
                else if (dc is DrawableCircuitLamp)
                {
                    type = "Lamp";
                }
                else if (dc is DrawableCircuitSwitch)
                {
                    type = "Switch";
                }
                else if (dc is DrawableIC)
                {
                    type = ((DrawableIC)dc).Description.Name;
                }
                else if (dc is DrawableHexViewer)
                {
                    type = "HexViewer";
                }

                List<WorkspaceComponentConnection> connections = new List<WorkspaceComponentConnection>();

                foreach(CircuitOutput co in dc.Outputs)
                {
                    foreach(DrawableWire dw in co.Signals)
                    {
                        WorkspaceComponentConnection wcc = new WorkspaceComponentConnection(components.IndexOf(dw.To), dw.FromIndex, dw.ToIndex);
                        connections.Add(wcc);
                    }
                }

                WorkspaceComponent wc = new WorkspaceComponent(type, dc.Position, connections);

                if(dc is DrawableCircuitLamp)
                {
                    wc.SetID(((DrawableCircuitLamp)dc).ID);
                }
                if (dc is DrawableCircuitSwitch)
                {
                    wc.SetID(((DrawableCircuitSwitch)dc).ID);
                }

                comps.Add(wc);
            }

            return comps;
        }
    
        private ICDescription GetDescription(string name, List<ICDescription> all)
        {
            foreach (ICDescription description in all)
            {
                if (description.Name == name)
                    return description;
            }
            return null;
        }

        public Tuple<List<DrawableComponent>, List<DrawableWire>> GenerateDrawables(List<ICDescription> availableDescriptions, Vector2 offset)
        {
            DrawableComponent[] components = new DrawableComponent[Components.Count];

            for (int i = 0; i < Components.Count; i++)
            {
                WorkspaceComponent wc = Components[i];
                DrawableComponent dc = null;
                switch (wc.Type)
                {
                    case "ANDGateLogic":
                        dc = new DrawableLogicGate(wc.Position + offset, "AND", new ANDGateLogic(), false);
                        break;
                    case "NANDGateLogic":
                        dc = new DrawableLogicGate(wc.Position + offset, "NAND", new NANDGateLogic(), false);
                        break;
                    case "ORGateLogic":
                        dc = new DrawableLogicGate(wc.Position + offset, "OR", new ORGateLogic(), false);
                        break;
                    case "NORGateLogic":
                        dc = new DrawableLogicGate(wc.Position + offset, "NOR", new NORGateLogic(), false);
                        break;
                    case "XORGateLogic":
                        dc = new DrawableLogicGate(wc.Position + offset, "XOR", new XORGateLogic(), false);
                        break;
                    case "XNORGateLogic":
                        dc = new DrawableLogicGate(wc.Position + offset, "XNOR", new XNORGateLogic(), false);
                        break;
                    case "NOTGateLogic":
                        dc = new DrawableLogicGate(wc.Position + offset, "NOT", new NOTGateLogic(), false);
                        break;
                    case "Switch":
                        dc = new DrawableCircuitSwitch(wc.Position + offset, false) { ID = wc.ID };
                        break;
                    case "Lamp":
                        dc = new DrawableCircuitLamp(wc.Position + offset, false) { ID = wc.ID };
                        break;
                    case "HexViewer":
                        dc = new DrawableHexViewer(wc.Position + offset, false);
                        break;
                    default:
                        ICDescription icd = GetDescription(wc.Type, availableDescriptions);
                        if (icd != null)
                            dc = new DrawableIC(wc.Position + offset, icd.Name, icd, false);
                        else
                            break;
                        break;
                }
                components[i] = dc;
            }

            List<DrawableWire> wires = new List<DrawableWire>();

            for (int i = 0; i < Components.Count; i++)
            {
                WorkspaceComponent iccd = Components[i];

                foreach (WorkspaceComponentConnection connection in iccd.ConnectedTo)
                {
                    try
                    {
                        DrawableWire cw = new DrawableWire(components[i], components[connection.To], connection.OutIndex, connection.InIndex);

                        components[i].AddOutputWire(connection.OutIndex, cw);
                        components[connection.To].SetInputWire(connection.InIndex, cw);

                        wires.Add(cw);
                    }
                    catch
                    {

                    }
                }
            }

            return new Tuple<List<DrawableComponent>, List<DrawableWire>>(components.ToList(), wires);
        }
    }
}
