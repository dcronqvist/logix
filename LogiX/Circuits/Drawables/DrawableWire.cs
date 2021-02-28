using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Drawables
{
    class DrawableWire : CircuitWire
    {
        public DrawableComponent From { get; set; }
        public DrawableComponent To { get; set; }
        public int FromIndex { get; set; }
        public int ToIndex { get; set; }

        public DrawableWire(DrawableComponent from, DrawableComponent to, int fromIndex, int toIndex)
        {
            this.From = from;
            this.To = to;
            this.FromIndex = fromIndex;
            this.ToIndex = toIndex;
        }

        public void Draw()
        {
            float thickness = 3f;

            Color col = base.Value == LogicValue.NAN ? Color.RED : (base.Value == LogicValue.HIGH ? Color.SKYBLUE : Color.WHITE);

            Raylib.DrawLineBezier(From.GetOutputPosition(FromIndex), To.GetInputPosition(ToIndex), thickness, col);
        }
    }
}
