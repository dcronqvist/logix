// using ImGuiNET;
// using LogiX.Architecture.Serialization;
// using LogiX.Content.Scripting;

// namespace LogiX.Architecture.BuiltinComponents;

// public class CounterData : INodeDescriptionData
// {
//     [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
//     public int DataBits { get; set; }

//     public static INodeDescriptionData GetDefault()
//     {
//         return new CounterData()
//         {
//             DataBits = 4,
//         };
//     }
// }

// [ScriptType("COUNTER"), NodeInfo("Counter", "Memory", "core.markdown.counter")]
// public class Counter : Component<CounterData>
// {
//     public override string Name => "CNT";
//     public override bool DisplayIOGroupIdentifiers => true;
//     public override bool ShowPropertyWindow => true;

//     private CounterData _data;

//     public override INodeDescriptionData GetDescriptionData()
//     {
//         return _data;
//     }

//     public override void Initialize(CounterData data)
//     {
//         this.ClearIOs();
//         this._data = data;

//         this.RegisterIO("LD", 1, ComponentSide.LEFT); // Load value
//         this.RegisterIO("C", 1, ComponentSide.LEFT); // increment or decrement current value
//         this.RegisterIO("D", data.DataBits, ComponentSide.LEFT); // Data input

//         this.RegisterIO("EN", 1, ComponentSide.BOTTOM); // only apply if enabled
//         this.RegisterIO("CLK", 1, ComponentSide.BOTTOM); // Update value
//         this.RegisterIO("R", 1, ComponentSide.BOTTOM); // reset value to 0

//         this.RegisterIO("Q", data.DataBits, ComponentSide.RIGHT); // current value

//         this._value = 0;
//     }

//     private bool _previousClock;
//     private uint _value;
//     public override void PerformLogic()
//     {
//         var ld = this.GetIOFromIdentifier("LD").GetValues().First();
//         var c = this.GetIOFromIdentifier("C").GetValues().First();
//         var d = this.GetIOFromIdentifier("D").GetValues();

//         var en = this.GetIOFromIdentifier("EN").GetValues().First();
//         var clk = this.GetIOFromIdentifier("CLK").GetValues().First();
//         var r = this.GetIOFromIdentifier("R").GetValues().First();

//         var q = this.GetIOFromIdentifier("Q");

//         if (ld.IsUndefined() || en.IsUndefined() || clk.IsUndefined())
//         {
//             return; // Undefined values
//         }

//         var enabled = en == LogicValue.HIGH;
//         var clock = clk == LogicValue.HIGH;
//         var reset = r == LogicValue.HIGH;
//         var load = ld == LogicValue.HIGH;

//         if (reset)
//         {
//             // Asynchronous reset
//             this._value = 0;
//         }
//         else if (enabled && clock && !_previousClock)
//         {
//             // Clock rising edge.
//             if (load)
//             {
//                 // Load from data input.
//                 this._value = d.Reverse().GetAsUInt();
//             }
//             else
//             {
//                 if (c == LogicValue.HIGH)
//                 {
//                     // Increment
//                     this._value++;
//                 }
//                 else if (c == LogicValue.LOW)
//                 {
//                     // Decrement
//                     this._value--;
//                 }
//             }
//         }

//         q.Push(this._value.GetAsLogicValues(this._data.DataBits));
//         _previousClock = clock;
//     }
// }