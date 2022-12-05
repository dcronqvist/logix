// using ImGuiNET;
// using LogiX.Architecture.Serialization;
// using LogiX.Content.Scripting;

// namespace LogiX.Architecture.BuiltinComponents;

// public class InverterData : INodeDescriptionData
// {
//     [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
//     public int DataBits { get; set; }

//     public static INodeDescriptionData GetDefault()
//     {
//         return new InverterData()
//         {
//             DataBits = 1
//         };
//     }
// }

// [ScriptType("INVERTER"), NodeInfo("Inverter", "Arithmetic", "core.markdown.inverter")]
// public class Inverter : Component<InverterData>
// {
//     public override string Name => "INV";
//     public override bool DisplayIOGroupIdentifiers => true;
//     public override bool ShowPropertyWindow => true;

//     private InverterData _data;

//     public override INodeDescriptionData GetDescriptionData()
//     {
//         return _data;
//     }

//     public override void Initialize(InverterData data)
//     {
//         this.ClearIOs();
//         this._data = data;

//         this.RegisterIO("X", data.DataBits, ComponentSide.LEFT);
//         this.RegisterIO("Y", data.DataBits, ComponentSide.RIGHT);
//     }

//     public override void PerformLogic()
//     {
//         var x = this.GetIOFromIdentifier("X");
//         var y = this.GetIOFromIdentifier("Y");

//         var xVal = x.GetValues();

//         if (xVal.AnyUndefined())
//         {
//             return; // Can't do anything if we don't have all the values
//         }

//         var xAsUint = xVal.Reverse().GetAsUInt();

//         // Get inverted x
//         var yAsUint = ~xAsUint;

//         // Convert back to bits
//         var yVal = yAsUint.GetAsLogicValues(this._data.DataBits);

//         y.Push(yVal);
//     }
// }