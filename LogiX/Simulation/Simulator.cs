using LogiX.Circuits;
using LogiX.Circuits.Drawables;
using LogiX.Utils;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace LogiX.Simulation
{
    class Simulator
    {

        public List<DrawableWire> AllWires { get; set; }

        public List<DrawableComponent> AllComponents { get; set; }
        public List<DrawableComponent> SelectedComponents { get; set; }
        public List<DrawableComponent> Interactables
        {
            get
            {
                return AllComponents.Where(x => x.GetType().GetInterfaces().Contains(typeof(IUpdateable))).ToList();
            }
        }

        public Simulator()
        {
            AllComponents = new List<DrawableComponent>();
            SelectedComponents = new List<DrawableComponent>();
            AllWires = new List<DrawableWire>();
        }

        public void AddWire(DrawableWire wire)
        {
            AllWires.Add(wire);
        }

        public void RemoveWire(DrawableWire wire)
        {
            if(AllWires.Contains(wire))
                AllWires.Remove(wire);
        }

        public void AddSelectedComponent(DrawableComponent dc)
        {
            if(!SelectedComponents.Contains(dc))
                SelectedComponents.Add(dc);
        }

        public void ClearSelectedComponents()
        {
            SelectedComponents.Clear();
        }

        public void DeleteSelectedComponents()
        {
            foreach(DrawableComponent dc in SelectedComponents)
            {
                foreach(CircuitInput ci in dc.Inputs)
                {
                    if (ci.Signal != null)
                    {
                        DrawableWire dw = (DrawableWire)ci.Signal;
                        dw.From.RemoveOutputWire(dw.FromIndex, dw);
                        RemoveWire((DrawableWire)ci.Signal);
                    }
                }
                foreach (CircuitOutput co in dc.Outputs)
                {
                    co.Signals.ForEach(x =>
                    {
                        DrawableWire dw = (DrawableWire)x;
                        dw.To.RemoveInputWire(dw.ToIndex);
                        RemoveWire((DrawableWire)x);
                    });
                }

                AllComponents.Remove(dc);
            }
            SelectedComponents.Clear();
        }

        public void AddComponent(DrawableComponent dc)
        {
            AllComponents.Add(dc);
        }

        public void RemoveComponent(DrawableComponent dc)
        {
            AllComponents.Remove(dc);
        }

        public DrawableComponent GetComponentFromPosition(Vector2 position)
        {
            foreach(DrawableComponent dc in AllComponents)
            {
                if (dc.Box.Contains(position.ToPoint()))
                {
                    return dc;
                }
            }
            return null;
        }

        public Tuple<int, DrawableComponent> GetComponentAndInputFromPos(Vector2 position)
        {
            foreach(DrawableComponent dc in AllComponents)
            {
                int index = dc.GetInputIndexFromPosition(position);

                if (index != -1)
                {
                    return new Tuple<int, DrawableComponent>(index, dc);
                }
            }
            return null;
        }

        public Tuple<int, DrawableComponent> GetComponentAndOutputFromPos(Vector2 position)
        {
            foreach (DrawableComponent dc in AllComponents)
            {
                int index = dc.GetOutputIndexFromPosition(position);

                if (index != -1)
                {
                    return new Tuple<int, DrawableComponent>(index, dc);
                }
            }
            return null;
        }

        public void Update(Vector2 mousePosInWorld)
        {
            foreach(DrawableComponent dc in AllComponents)
            {
                dc.UpdateInputsAndLogic();
            }

            foreach (DrawableComponent dc in AllComponents)
            {
                dc.UpdateOutputs();
            }

            foreach(DrawableComponent dc in Interactables)
            {
                ((IUpdateable)dc).Update(this, mousePosInWorld);
            }
        }

        public void Render(Vector2 mousePosInWorld)
        {
            foreach (DrawableComponent dc in AllComponents)
            {
                dc.Draw(mousePosInWorld);
            }

            foreach(DrawableWire wire in AllWires)
            {
                wire.Draw();
            }
            
            foreach(DrawableComponent dc in SelectedComponents)
            {
                dc.DrawSelected();
            }
        }
    }
}
