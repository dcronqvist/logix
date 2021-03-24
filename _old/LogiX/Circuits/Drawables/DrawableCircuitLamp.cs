using LogiX.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Drawables
{
    class DrawableCircuitLamp : DrawableComponent
    {
        public string ID = "";

        public DrawableCircuitLamp(Vector2 position, bool offsetMiddle) : base(position, "", 1, 0)
        {
            Size = new Vector2(25, 30);
            CalculateOffsets(offsetMiddle);
        }

        public override void Draw(Vector2 mousePosInWorld)
        {
            if (Inputs[0].Value == LogicValue.HIGH)
            {
                BlockColor = Utility.COLOR_ON;
            }
            else
            {
                BlockColor = Utility.COLOR_BLOCK_DEFAULT;
            }

            base.Draw(mousePosInWorld);
        }

        protected override void PerformLogic()
        {
            
        }
    }
}
