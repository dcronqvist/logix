// using ImGuiNET;
// using LogiX.Architecture.Serialization;
// using LogiX.Content.Scripting;

// namespace LogiX.Architecture.BuiltinComponents;

// public class MultiplierData : IComponentDescriptionData
// {
//     [ComponentDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
//     public int DataBits { get; set; }

//     public static IComponentDescriptionData GetDefault()
//     {
//         return new MultiplierData()
//         {
//             DataBits = 1
//         };
//     }
// }

// [ScriptType("MULTIPLIER"), ComponentInfo("Multiplier", "Arithmetic", "core.markdown.multiplier")]
// public class Multiplier : Component<MultiplierData>
// {
//     public override string Name => "MUL";
//     public override bool DisplayIOGroupIdentifiers => true;
//     public override bool ShowPropertyWindow => true;

//     private MultiplierData _data;

//     public override IComponentDescriptionData GetDescriptionData()
//     {
//         return _data;
//     }

//     public override void Initialize(MultiplierData data)
//     {
//         this.ClearIOs();
//         this._data = data;

//         this.RegisterIO("A", data.DataBits, ComponentSide.LEFT, "input");
//         this.RegisterIO("B", data.DataBits, ComponentSide.LEFT, "input");
//         this.RegisterIO("CIN", 1, ComponentSide.TOP, "borrowin");
//         this.RegisterIO("COUT", data.DataBits, ComponentSide.BOTTOM, "borrowout");
//         this.RegisterIO("S", data.DataBits, ComponentSide.RIGHT, "output");
//     }

//     public override void PerformLogic()
//     {
//         var a = this.GetIOFromIdentifier("A");
//         var b = this.GetIOFromIdentifier("B");
//         var bin = this.GetIOFromIdentifier("CIN");
//         var bout = this.GetIOFromIdentifier("COUT");
//         var s = this.GetIOFromIdentifier("S");

//         var aValues = a.GetValues();
//         var bValues = b.GetValues();
//         var binValues = bin.GetValues();

//         if (aValues.AnyUndefined() || bValues.AnyUndefined() || binValues.AnyUndefined())
//         {
//             return; // Can't do anything if we don't have all the values
//         }

//         var aAsuint = aValues.Reverse().GetAsUInt();
//         var bAsuint = bValues.Reverse().GetAsUInt();
//         var binAsuint = binValues.Reverse().GetAsUInt();

//         var sum = aAsuint * bAsuint + binAsuint;

//         var sumAsBits = sum.GetAsLogicValues(this._data.DataBits);
//         var boutAsBits = (sum >> this._data.DataBits).GetAsLogicValues(this._data.DataBits);

//         s.Push(sumAsBits);
//         bout.Push(boutAsBits);
//     }
// }