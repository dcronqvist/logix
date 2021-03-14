using LogiX.Utils;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Drawables
{
    class DrawableHexViewer : DrawableComponent
    {
        private string InternalHex { get; set; }

        public DrawableHexViewer(Vector2 position, bool offsetMiddle) : base(position, "", 4, 0)
        {
            InternalHex = "-";
            CalculateOffsets(offsetMiddle);
        }

        public override void CalculateSizes()
        {
            this.Size = new Vector2(40, 50);
        }

        public override void Draw(Vector2 mousePosInWorld)
        {
            base.Draw(mousePosInWorld);
            float fontSize = 40;
            Vector2 textSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), InternalHex, fontSize, 1f);
            Color col = Utility.COLOR_ON;

            if(InternalHex == "-")
            {
                col = Utility.COLOR_NAN;
            }
            Raylib.DrawTextEx(Raylib.GetFontDefault(), InternalHex, Position + Size / 2f - textSize / 2f, fontSize, 1f, col);
        }

        // Found at 
        // https://stackoverflow.com/questions/5612306/converting-long-string-of-binary-to-hex-c-sharp
        private string BinaryStringToHexString(string binary)
        {
            if (string.IsNullOrEmpty(binary))
                return binary;

            StringBuilder result = new StringBuilder(binary.Length / 8 + 1);

            // TODO: check all 1's or 0's... throw otherwise

            int mod4Len = binary.Length % 8;
            if (mod4Len != 0)
            {
                // pad to length multiple of 8
                binary = binary.PadLeft(((binary.Length / 8) + 1) * 8, '0');
            }

            for (int i = 0; i < binary.Length; i += 8)
            {
                string eightBits = binary.Substring(i, 8);
                result.AppendFormat("{0:X}", Convert.ToByte(eightBits, 2));
            }

            return result.ToString();
        }

        protected override void PerformLogic()
        {
            if (AnyInputNAN())
            {
                this.InternalHex = "-";
                return;
            }

            int a = this.Inputs[0].Value == LogicValue.HIGH ? 1 : 0;
            int b = this.Inputs[1].Value == LogicValue.HIGH ? 1 : 0;
            int c = this.Inputs[2].Value == LogicValue.HIGH ? 1 : 0;
            int d = this.Inputs[3].Value == LogicValue.HIGH ? 1 : 0;
            string binary = $"{d}{c}{b}{a}";

            this.InternalHex = BinaryStringToHexString(binary);
        }
    }
}
