using LogiX.Utils;
using Newtonsoft.Json;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Drawables
{
    [JsonObject(MemberSerialization.OptOut)]
    abstract class DrawableComponent : CircuitComponent
    {
        public Vector2 Position { get; set; }
        [JsonIgnore]
        public Vector2 MiddlePosition { get; set; }
        public Vector2 Size { get; set; }
        [JsonIgnore]
        public Rectangle Box { get; set; }
        [JsonIgnore]
        public Color BlockColor { get; set; }
        [JsonIgnore]
        public Color BorderColor { get; set; }
        [JsonIgnore]
        public Color IOHoverColor { get; set; }

        public string Text { get; set; }
        public Vector2 TextSize { get; set; }

        public DrawableComponent(Vector2 position, string text, int inputs, int outputs) : base(inputs, outputs)
        {
            this.BlockColor = Utility.COLOR_BLOCK_DEFAULT;
            this.BorderColor = Utility.COLOR_BLOCK_BORDER_DEFAULT;
            this.IOHoverColor = Utility.COLOR_IO_HOVER_DEFAULT;

            // Measure text size.
            this.Text = text;
            this.Position = position;
        }

        public void CalculateOffsets(bool offsetMiddle)
        {
            CalculateSizes();

            if(offsetMiddle)
                this.Position = Position - Size / 2;
            this.MiddlePosition = Position + Size / 2;
        }

        public virtual void CalculateSizes()
        {
            // Measure text size.
            this.TextSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), Text, Utility.TEXT_SIZE, 1f);

            // Get longest input id & longest output id, add it.
            int maxInp = 0;
            int maxOutp = 0;
            for (int i = 0; i < Inputs.Count; i++)
            {
                string inp = GetInputID(i);
                int x = Raylib.MeasureText(inp, Utility.TEXT_SIZE);
                maxInp = Math.Max(maxInp, x);
            }
            for (int i = 0; i < Outputs.Count; i++)
            {
                string outp = GetOutputID(i);
                int y = Raylib.MeasureText(outp, Utility.TEXT_SIZE);
                maxOutp = Math.Max(maxOutp, y);
            }

            this.Size = new Vector2(TextSize.X + (15 * (Math.Sign(Math.Max(maxInp, maxOutp)) + 1)) + Math.Max(maxInp, maxOutp) * 2, (Math.Max(this.Inputs.Count, this.Outputs.Count)) * Utility.IO_V_DIST);
        }

        public virtual string GetInputID(int index)
        {
            return null;
        }
        public virtual string GetOutputID(int index)
        {
            return null;
        }

        public float GetIOYPosition(int ios, int index)
        {
            // height/2 - ((max(in, out) - 1) * dist_between)/2
            float start = Size.Y / 2 - ((ios - 1) * Utility.IO_V_DIST) / 2;

            float pos = start + index * Utility.IO_V_DIST;

            return pos;
        }

        public Vector2 GetInputPosition(int index)
        {
            Vector2 basePos = Position;
            int inputs = Inputs.Count;

            return basePos + new Vector2(-Utility.IO_H_DIST, GetIOYPosition(inputs, index));
        }

        public Vector2 GetOutputPosition(int index)
        {
            Vector2 basePos = Position;
            int outputs = Outputs.Count;

            return basePos + new Vector2(Size.X + Utility.IO_H_DIST, GetIOYPosition(outputs, index));
        }

        public int GetInputIndexFromPosition(Vector2 pos)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                Vector2 inputPos = GetInputPosition(i);
                if((pos - inputPos).Length() < Utility.IO_SIZE)
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetOutputIndexFromPosition(Vector2 pos)
        {
            for (int i = 0; i < Outputs.Count; i++)
            {
                Vector2 outputPos = GetOutputPosition(i);
                if ((pos - outputPos).Length() < Utility.IO_SIZE)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool PositionIsOnIOs(Vector2 pos)
        {
            if(GetInputIndexFromPosition(pos) != -1 || GetOutputIndexFromPosition(pos) != -1)
            {
                return true;
            }
            return false;
        }

        public void DrawInputs(Vector2 mousePosInWorld)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                Vector2 pos = GetInputPosition(i);

                Color col = Inputs[i].Value == LogicValue.NAN ? Utility.COLOR_NAN : (Inputs[i].Value == LogicValue.HIGH ? Utility.COLOR_ON : Utility.COLOR_OFF);

                if ((mousePosInWorld - pos).Length() < Utility.IO_SIZE)
                {
                    col = IOHoverColor;
                }

                // Line to IO ball
                Vector2 startPos = new Vector2(Position.X, pos.Y);

                Vector2[] points = new Vector2[] { startPos, pos };
                Raylib.DrawLineStrip(points, points.Length, BorderColor);

                Raylib.DrawCircleV(pos, Utility.IO_SIZE, col);

                string s = GetInputID(i);
                if(s != null)
                {
                    Vector2 inputIdSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), s, Utility.TEXT_SIZE, 1f);

                    Raylib.DrawTextEx(Raylib.GetFontDefault(), s, new Vector2(pos.X + Utility.IO_H_DIST + 3, pos.Y - inputIdSize.Y / 2f), Utility.TEXT_SIZE, 1f, BorderColor);
                }
            }
        }

        public void DrawOutputs(Vector2 mousePosInWorld)
        {
            for (int i = 0; i < Outputs.Count; i++)
            {
                Vector2 pos = GetOutputPosition(i);

                Color col = Outputs[i].Value == LogicValue.NAN ? Utility.COLOR_NAN : (Outputs[i].Value == LogicValue.HIGH ? Utility.COLOR_ON : Utility.COLOR_OFF);

                if ((mousePosInWorld - pos).Length() < Utility.IO_SIZE)
                {
                    col = IOHoverColor;
                }

                // Line to IO ball
                Vector2 startPos = new Vector2(Position.X + Box.width, pos.Y);

                Vector2[] points = new Vector2[] { startPos, pos };
                Raylib.DrawLineStrip(points, points.Length, BorderColor);

                Raylib.DrawCircleV(pos, Utility.IO_SIZE, col);

                string s = GetOutputID(i);
                if (s != null)
                {
                    Vector2 inputIdSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), s, Utility.TEXT_SIZE, 1f);

                    Raylib.DrawTextEx(Raylib.GetFontDefault(), s, new Vector2(pos.X - inputIdSize.X - 3 - Utility.IO_H_DIST, pos.Y - inputIdSize.Y / 2f), Utility.TEXT_SIZE, 1f, BorderColor);
                }
            }
        }

        public virtual void Draw(Vector2 mousePosInWorld)
        {
            CalculateSizes();
            Box = new Rectangle(Position.X, Position.Y, Size.X, Size.Y);
            Raylib.DrawRectanglePro(Box, Vector2.Zero, 0f, BlockColor);
            DrawInputs(mousePosInWorld);
            DrawOutputs(mousePosInWorld);

            if (Text != "")
            {
                Vector2 middle = Position + Size / 2f;
                Vector2 textPos = middle - TextSize / 2f;
                Raylib.DrawTextEx(Raylib.GetFontDefault(), Text, new Vector2(textPos.X, textPos.Y), Utility.TEXT_SIZE, 1f, BorderColor);
            }

            Vector2 topLeft = new Vector2(Box.x, Box.y);
            Vector2 topRight = new Vector2(Box.x + Box.width, Box.y);
            Vector2 bottomLeft = new Vector2(Box.x, Box.y + Box.height);
            Vector2 bottomRight = new Vector2(Box.x + Box.width, Box.y + Box.height);
            Vector2[] points = new Vector2[] { topLeft, topRight, bottomRight, bottomLeft, topLeft };

            Raylib.DrawLineStrip(points, points.Length, BorderColor);
        }

        public void DrawSelected()
        {
            float offset = 3f;
            float thick = 2f;

            Vector2 topLeft = new Vector2(Box.x - offset, Box.y - offset);
            Vector2 topRight = new Vector2(Box.x + Box.width + offset, Box.y - offset);
            Vector2 bottomLeft = new Vector2(Box.x - offset, Box.y + Box.height + offset);
            Vector2 bottomRight = new Vector2(Box.x + Box.width + offset, Box.y + Box.height + offset);

            Utility.DrawRectangleLines(topLeft, topRight, bottomRight, bottomLeft, thick, Utility.COLOR_SELECTED_DEFAULT);
        }
    }
}
