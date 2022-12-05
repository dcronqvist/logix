// using ImGuiNET;
// using LogiX.Architecture.Serialization;
// using LogiX.Content.Scripting;

// namespace LogiX.Architecture.BuiltinComponents;

// public class SRLatchData : IComponentDescriptionData
// {
//     public static IComponentDescriptionData GetDefault()
//     {
//         return new SRLatchData();
//     }
// }

// [ScriptType("SRLATCH"), ComponentInfo("SR Latch", "Memory", "core.markdown.srlatch")]
// public class SRLatch : Component<SRLatchData>
// {
//     public override string Name => "SR Latch";
//     public override bool DisplayIOGroupIdentifiers => true;
//     public override bool ShowPropertyWindow => false;

//     private SRLatchData _data;
//     private LogicValue _currentState;

//     public override IComponentDescriptionData GetDescriptionData()
//     {
//         return _data;
//     }

//     public override void Initialize(SRLatchData data)
//     {
//         this.ClearIOs();
//         this._data = data;

//         this.RegisterIO("S", 1, ComponentSide.LEFT, "set");
//         this.RegisterIO("R", 1, ComponentSide.LEFT, "reset");

//         this.RegisterIO("Q", 1, ComponentSide.RIGHT);
//         this.RegisterIO("!Q", 1, ComponentSide.RIGHT);

//         this.TriggerSizeRecalculation();
//         this._currentState = LogicValue.UNDEFINED;
//     }

//     public override void PerformLogic()
//     {
//         var set = this.GetIOFromIdentifier("S").GetValues().First();
//         var reset = this.GetIOFromIdentifier("R").GetValues().First();

//         var q = this.GetIOFromIdentifier("Q");
//         var qNot = this.GetIOFromIdentifier("!Q");

//         if (set == LogicValue.HIGH && reset == LogicValue.HIGH)
//         {
//             this._currentState = LogicValue.UNDEFINED;
//         }
//         else if (set == LogicValue.HIGH)
//         {
//             this._currentState = LogicValue.HIGH;
//         }
//         else if (reset == LogicValue.HIGH)
//         {
//             this._currentState = LogicValue.LOW;
//         }

//         q.Push(this._currentState);
//         qNot.Push(this._currentState == LogicValue.UNDEFINED ? LogicValue.UNDEFINED : this._currentState == LogicValue.HIGH ? LogicValue.LOW : LogicValue.HIGH);
//     }
// }