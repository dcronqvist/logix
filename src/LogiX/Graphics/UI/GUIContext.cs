// using System.Numerics;

// namespace LogiX.Graphics.UI;

// public class GUIContext
// {
//     public Font Font { get; set; }
//     public List<GUIWindow> Windows { get; private set; } = new();
//     public int CurrentWindowIndex { get; set; } = -1;
//     public GUIWindow CurrentWindow => this.Windows[this.CurrentWindowIndex];
//     public List<int> WindowOrder { get; private set; } = new();

//     public GUIWindow HoveredWindow { get; set; }
//     public GUIWindow FocusedWindow { get; set; }
//     public GUIWindow DraggingWindow { get; set; }
//     public Vector2 DragOffset { get; set; }

//     public Stack<string> PushedIDs { get; private set; } = new();
//     public Stack<Vector2> PushedSizes { get; private set; } = new();

//     public static GUIContext GetDefault()
//     {
//         var context = new GUIContext();
//         context.Font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
//         return context;
//     }

//     public Vector2 MeasureString(string text)
//     {
//         return this.Font.MeasureString(text, 1f);
//     }

//     public void PushNextID(string id)
//     {
//         this.PushedIDs.Push(id);
//     }

//     public string GetNextID(string label)
//     {
//         if (this.PushedIDs.Count > 0)
//         {
//             return this.PushedIDs.Pop();
//         }
//         else
//         {
//             return Utilities.GetHash(this.CurrentWindow.Name + label);
//         }
//     }

//     public void PushNextSize(Vector2 size)
//     {
//         this.PushedSizes.Push(size);
//     }

//     public bool TryGetPushedNextSize(out Vector2 size)
//     {
//         return this.PushedSizes.TryPop(out size);
//     }

//     public GUIWindow GetWindow(string label, Vector2 initialPosition)
//     {
//         foreach (var window in this.Windows)
//         {
//             if (window.Name == label)
//             {
//                 this.CurrentWindowIndex = this.Windows.IndexOf(window);
//                 return window;
//             }
//         }

//         // Create a new window
//         var w = new GUIWindow(label, initialPosition);
//         this.Windows.Add(w);
//         this.CurrentWindowIndex = this.Windows.Count - 1;
//         this.WindowOrder.Insert(0, this.CurrentWindowIndex);
//         this.FocusWindow(w);
//         return w;
//     }

//     public void SetWindowAtFront(GUIWindow window)
//     {
//         // Use WindowOrder
//         var index = this.Windows.IndexOf(window);

//         if (this.WindowOrder.Contains(index))
//         {
//             this.WindowOrder.Remove(index);
//         }

//         this.WindowOrder.Insert(0, index);
//     }

//     public void FocusWindow(GUIWindow window)
//     {
//         this.FocusedWindow = window;
//     }

//     public void ResetFocusedWindow()
//     {
//         this.FocusedWindow = null;
//     }
// }