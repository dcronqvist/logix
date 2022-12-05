// using ImGuiNET;
// using LogiX.Architecture.Serialization;
// using LogiX.Content.Scripting;

// namespace LogiX.Architecture.BuiltinComponents;

// public class DemultiplexerData : IComponentDescriptionData
// {
//     [ComponentDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
//     public int DataBits { get; set; }

//     [ComponentDescriptionProperty("Select Bits", IntMinValue = 1, IntMaxValue = 32)]
//     public int SelectBits { get; set; }

//     public static IComponentDescriptionData GetDefault()
//     {
//         return new DemultiplexerData()
//         {
//             DataBits = 1,
//             SelectBits = 1
//         };
//     }
// }

// [ScriptType("DEMULTIPLEXER"), ComponentInfo("Demultiplexer", "Plexers", "core.markdown.demultiplexer")]
// public class Demultiplexer : Component<DemultiplexerData>
// {
//     public override string Name => "DEMUX";
//     public override bool DisplayIOGroupIdentifiers => true;
//     public override bool ShowPropertyWindow => true;

//     private DemultiplexerData _data;

//     public override IComponentDescriptionData GetDescriptionData()
//     {
//         return _data;
//     }

//     public override void Initialize(DemultiplexerData data)
//     {
//         this.ClearIOs();
//         this._data = data;

//         var outputs = Math.Pow(2, data.SelectBits);

//         for (int i = 0; i < outputs; i++)
//         {
//             this.RegisterIO($"O{i}", data.DataBits, ComponentSide.RIGHT, "output");
//         }

//         for (int i = 0; i < this._data.SelectBits; i++)
//         {
//             this.RegisterIO($"S{i}", 1, ComponentSide.TOP, "select");
//         }

//         this.RegisterIO("input", data.DataBits, ComponentSide.LEFT, "input");

//         this.TriggerSizeRecalculation();
//     }

//     public override void PerformLogic()
//     {
//         var selects = this.GetIOsWithTag("select");

//         if (selects.Select(v => v.GetValues().First()).Any(v => v == LogicValue.UNDEFINED))
//         {
//             return;
//         }

//         var select = selects.Select(v => v.GetValues().First()).GetAsInt();
//         var input = this.GetIOFromIdentifier("input").GetValues();

//         var outputs = this.GetIOsWithTag("output");
//         outputs[select].Push(input);
//     }
// }