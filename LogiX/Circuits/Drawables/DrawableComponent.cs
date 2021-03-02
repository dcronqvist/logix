using LogiX.Utils;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Drawables
{
    abstract class DrawableComponent : CircuitComponent
    {
        const float DIST_BETWEEN_INPUTS = 16;
        const float DIST_BLOCK_IO = 10;
        const float IO_RADIUS = 6;
        const int TEXT_SIZE = 10;
        const int UNDERTEXT_SIZE = 10;

        public Vector2 Position { get; set; }
        public Vector2 MiddlePosition { get; set; }
        public Vector2 Size { get; set; }
        public System.Drawing.RectangleF Box { get; set; }
        public Color BlockColor { get; set; }
        public Color BorderColor { get; set; }
        public Color IOHoverColor { get; set; }

        public string Text { get; set; }
        public Vector2 TextSize { get; set; }

        public DrawableComponent(Vector2 position, string text, int inputs, int outputs) : base(inputs, outputs)
        {
            this.BlockColor = Utility.COLOR_BLOCK_DEFAULT;
            this.BorderColor = Color.BLACK;
            this.IOHoverColor = Color.SKYBLUE;

            // Measure text size.
            this.TextSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), text, TEXT_SIZE, 1f);
            this.Size = new Vector2(TextSize.X + 15, (Math.Max(inputs, outputs)) * DIST_BETWEEN_INPUTS);

            this.Text = text;
            this.Position = position - Size / 2;
            this.MiddlePosition = Position + Size / 2;
        }

        public float GetIOYPosition(int ios, int index)
        {
            // height/2 - ((max(in, out) - 1) * dist_between)/2
            float start = Size.Y / 2 - ((ios - 1) * DIST_BETWEEN_INPUTS) / 2;

            float pos = start + index * DIST_BETWEEN_INPUTS;

            return pos;
        }

        public Vector2 GetInputPosition(int index)
        {
            Vector2 basePos = Position;
            int inputs = Inputs.Count;

            return basePos + new Vector2(-DIST_BLOCK_IO, GetIOYPosition(inputs, index));
        }

        public Vector2 GetOutputPosition(int index)
        {
            Vector2 basePos = Position;
            int outputs = Outputs.Count;

            return basePos + new Vector2(Size.X + DIST_BLOCK_IO, GetIOYPosition(outputs, index));
        }

        public int GetInputIndexFromPosition(Vector2 pos)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                Vector2 inputPos = GetInputPosition(i);
                if((pos - inputPos).Length() < IO_RADIUS)
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
                if ((pos - outputPos).Length() < IO_RADIUS)
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

                if ((mousePosInWorld - pos).Length() < IO_RADIUS)
                {
                    col = IOHoverColor;
                }

                // Line to IO ball
                Raylib.DrawLine((int)Position.X, (int)pos.Y, (int)(Position.X - DIST_BLOCK_IO), (int)pos.Y, BorderColor);

                Raylib.DrawCircle((int)pos.X, (int)pos.Y, IO_RADIUS, col);
                Raylib.DrawCircleLines((int)pos.X, (int)pos.Y, IO_RADIUS, BorderColor);
            }
        }

        public void DrawOutputs(Vector2 mousePosInWorld)
        {
            for (int i = 0; i < Outputs.Count; i++)
            {
                Vector2 pos = GetOutputPosition(i);

                Color col = Outputs[i].Value == LogicValue.NAN ? Utility.COLOR_NAN : (Outputs[i].Value == LogicValue.HIGH ? Utility.COLOR_ON : Utility.COLOR_OFF);

                if ((mousePosInWorld - pos).Length() < IO_RADIUS)
                {
                    col = IOHoverColor;
                }

                // Line to IO ball
                Raylib.DrawLine((int)(Position.X + Size.X), (int)pos.Y, (int)(Position.X + Size.X + DIST_BLOCK_IO), (int)pos.Y, BorderColor);

                Raylib.DrawCircle((int)pos.X, (int)pos.Y, IO_RADIUS, col);
                Raylib.DrawCircleLines((int)pos.X, (int)pos.Y, IO_RADIUS, BorderColor);
            }
        }

        public virtual void Draw(Vector2 mousePosInWorld)
        {
            Box = new System.Drawing.RectangleF(Position.X, Position.Y, Size.X, Size.Y);
            Rectangle rec = new Rectangle(Position.X, Position.Y, Size.X, Size.Y);
            Raylib.DrawRectanglePro(rec, Vector2.Zero, 0f, BlockColor);
            DrawInputs(mousePosInWorld);
            DrawOutputs(mousePosInWorld);

            Vector2 middle = Position + Size / 2f;
            Vector2 textPos = middle - TextSize / 2f;
            Raylib.DrawTextEx(Raylib.GetFontDefault(), Text, new Vector2((int)textPos.X, (int)textPos.Y), TEXT_SIZE, 1f, BorderColor);
            Raylib.DrawRectangleLinesEx(rec, 1, BorderColor);
        }

        public void DrawSelected()
        {
            int offset = 3;
            Rectangle selectionRec = new Rectangle((int)Position.X - offset, (int)Position.Y - offset, (int)(Size.X + offset * 2), (int)(Size.Y + offset * 2));
            Raylib.DrawRectangleLinesEx(selectionRec, 2, Color.BLUE);
        }
    }
}
