using LogiX.Simulation;
using LogiX.Utils;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Drawables
{
    class DrawableCircuitSwitch : DrawableComponent, IUpdateable
    {
        public LogicValue Value { get; set; }

        public DrawableCircuitSwitch(Vector2 position) : base(position, "", 0, 1)
        {
            Value = LogicValue.LOW;
            Size = new Vector2(25, 30);
            Text = "0";
            CalculateOffsets();
        }

        public override void Draw(Vector2 mousePosInWorld)
        {
            if(Value == LogicValue.HIGH)
            {
                BlockColor = Utility.COLOR_ON;
                Text = "1";
            }
            else
            {
                BlockColor = Utility.COLOR_BLOCK_DEFAULT;
                Text = "0";
            }

            base.Draw(mousePosInWorld);
        }

        public void Update(Simulator sim, Vector2 mousePosInWorld)
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
            {
                if (Box.Contains(mousePosInWorld.ToPoint()))
                {
                    Value = Value == LogicValue.HIGH ? LogicValue.LOW : LogicValue.HIGH;
                }
            }
        }

        protected override void PerformLogic()
        {
            Outputs[0].Value = Value;
        }
    }
}
