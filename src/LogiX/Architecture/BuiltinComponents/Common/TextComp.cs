// using System.Drawing;
// using System.Numerics;
// using ImGuiNET;
// using LogiX.Architecture.Serialization;
// using LogiX.Content.Scripting;
// using LogiX.Graphics;
// using LogiX.Rendering;

// namespace LogiX.Architecture.BuiltinComponents;

// public class TextCompData : INodeDescriptionData
// {
//     [NodeDescriptionProperty("Text", StringMultiline = true)]
//     public string Text { get; set; }

//     public static INodeDescriptionData GetDefault()
//     {
//         return new TextCompData()
//         {
//             Text = ""
//         };
//     }
// }

// [ScriptType("TEXTCOMP"), NodeInfo("Text", "Common", "core.markdown.text")]
// public class TextComp : Component<TextCompData>
// {
//     public override string Name => "";
//     public override bool DisplayIOGroupIdentifiers => true;
//     public override bool ShowPropertyWindow => true;

//     private TextCompData _data;

//     public override INodeDescriptionData GetDescriptionData()
//     {
//         return _data;
//     }

//     public override void Initialize(TextCompData data)
//     {
//         this.ClearIOs();
//         this._data = data;

//         this.TriggerSizeRecalculation();
//     }

//     public override void PerformLogic()
//     {

//     }

//     public override RectangleF GetBoundingBox(out Vector2 textSize)
//     {
//         if (this._bounds != RectangleF.Empty)
//         {
//             textSize = _textSize;
//             return this._bounds;
//         }

//         // Otherwise, calculate the size of the component.
//         // In order to trigger a recalculation of the component's size, set _size to Vector2i.Zero.
//         var font = Utilities.GetFont("core.font.default", 8); //LogiX.ContentManager.GetContentItem<Font>("core.font.default-regular-8");

//         var textScale = 1f;
//         var gridSize = Constants.GRIDSIZE;
//         var lines = this._data.Text.Split('\n');
//         var lineMeasures = lines.Select(l => font.MeasureString(l, textScale));

//         var maxTextWidth = lineMeasures.Max(l => l.X);
//         var sumHeight = lineMeasures.Sum(l => l.Y);

//         var textWidth = Utilities.CeilToMultipleOf(maxTextWidth, gridSize);
//         var textHeight = Utilities.CeilToMultipleOf(sumHeight, gridSize);
//         this._textSize = new Vector2(maxTextWidth, sumHeight);

//         var size = new Vector2(Math.Max(textWidth, Constants.GRIDSIZE), Math.Max(textHeight, Constants.GRIDSIZE));
//         this._bounds = this.Position.ToVector2(Constants.GRIDSIZE).CreateRect(size);
//         textSize = _textSize;
//         return this._bounds;
//     }

//     public override void Render(Camera2D camera)
//     {
//         //this.TriggerSizeRecalculation();
//         // Position of component

//         var font = Utilities.GetFont("core.font.default", 8); //LogiX.ContentManager.GetContentItem<Font>("core.font.default-regular-8");
//         var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
//         var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.text");

//         var pos = this.Position.ToVector2(Constants.GRIDSIZE);
//         var rect = this.GetBoundingBox(out var textSize);
//         var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
//         var realSize = size.ToVector2(Constants.GRIDSIZE);

//         PrimitiveRenderer.RenderRectangle(rect, Vector2.Zero, 0f, ColorF.White);

//         var textLines = this._data.Text.Split('\n');

//         var middle = new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
//         var lineSize = textSize.Y / textLines.Length;

//         for (int i = 0; i < textLines.Length; i++)
//         {
//             var line = textLines[i];
//             var linePos = new Vector2(middle.X - textSize.X / 2, middle.Y - textSize.Y / 2 + i * lineSize);
//             TextRenderer.RenderText(font, line, linePos, 1f, 0f, ColorF.Black, camera);
//         }
//         //TextRenderer.RenderText(tShader, font, this.Name, textPos, 1, this.Rotation == 0 || this.Rotation == 2 ? 0f : MathF.PI / 2f, ColorF.Black, camera);
//     }
// }