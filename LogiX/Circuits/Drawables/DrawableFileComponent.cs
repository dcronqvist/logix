using LogiX.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace LogiX.Circuits.Drawables
{
    class DrawableFileComponent : DrawableComponent
    {
        public string FilePath { get; set; }
        public string[] Rows { get; set; }

        public DrawableFileComponent(Vector2 position, string file, bool offsetMiddle) : base(position, Path.GetFileName(file), GetInputAmount(file), GetOutputAmount(file))
        {
            this.FilePath = file;
            Initialize();
            CalculateOffsets(offsetMiddle);
        }

        public static int GetInputAmount(string file)
        {
            int lines = File.ReadAllLines(file).Length;
            return (int)Math.Ceiling(Math.Log2(lines));
        }

        private static int GetOutputAmount(string file)
        {
            string[] lines = File.ReadAllLines(file);
            int charsInFirstRow = lines[0].Length;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Length != charsInFirstRow)
                {
                    // This file cannot be used, there are differing amounts of bits per row.
                    LogManager.AddEntry($"Could not use {file} for File Component. Differing amount of bits on row {i}");
                    throw new Exception($"Could not use {file} for File Component. Differing amount of bits on row {i}");
                }
            }

            return charsInFirstRow;
        }

        private void Initialize()
        {
            List<string> lines = new List<string>();

            using (StreamReader sr = new StreamReader(this.FilePath))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            this.Rows = lines.ToArray();
        }

        protected override void PerformLogic()
        {
            if (AnyInputNAN())
            {
                for (int i = 0; i < base.Outputs.Count; i++)
                {
                    base.Outputs[i].Value = LogicValue.NAN;
                }
                return;
            }

            int address = 0;
            for (int i = 0; i < base.Inputs.Count; i++)
            {
                if(base.Inputs[i].Value == LogicValue.HIGH)
                {
                    address += 0b1 << i;
                }
            }

            string line = this.Rows[address];

            for (int i = 0; i < line.Length; i++)
            {
                base.Outputs[i].Value = line[line.Length - i - 1] == '1' ? LogicValue.HIGH : LogicValue.LOW;
            }
        }
    }
}
