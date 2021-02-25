using System;
using System.Collections.Generic;
using System.Text;

namespace LogiX.Circuits
{
    class CircuitOutput : CircuitIO
    {
        public List<CircuitWire> Signals { get; set; }

        public CircuitOutput()
        {
            this.Signals = new List<CircuitWire>();
        }

        public void AddOutputSignal(CircuitWire wire)
        {
            this.Signals.Add(wire);
        }

        public void RemoveOutputSignal(CircuitWire wire)
        {
            this.Signals.Remove(wire);
        }

        public void RemoveOutputSignal(int index)
        {
            this.Signals.RemoveAt(index);
        }

        public void SetSignals()
        {
            foreach(CircuitWire cw in Signals)
            {
                cw.Value = Value;
            }
        }
    }
}
