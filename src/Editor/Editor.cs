using System.Diagnostics.CodeAnalysis;
using System.Text;
using LogiX.Components;
using LogiX.Editor.Commands;
using LogiX.Editor.StateMachine;

namespace LogiX.Editor;

public class Editor : Application<Editor>
{
    // EDITOR LAYOUT VARIABLES
    public int SidebarWidth { get; set; } = 170;
    public int MainMenuBarHeight { get; set; } = 22;
    public int CircuitTabBarHeight { get; set; } = 38;

    // EDITOR STUFF
    public Camera2D camera;
    public List<ComponentCreationContext> ComponentCreationContexts { get; set; }
    public List<(string, List<ComponentCreationContext>)> ComponentCreationContextCategories { get; set; }
    public bool IsMouseInWorld { get; set; }

    public Dictionary<string, EditorTab> EditorTabs { get; set; }
    public string CurrentEditorTab { get; set; }
    public EditorTab CurrentTab => this.EditorTabs[this.CurrentEditorTab];
    public Simulator Simulator => this.CurrentTab.Simulator;
    public EditorFSM FSM => this.CurrentTab.FSM;

    // TEMPORARY VARIABLES FOR CONNECTIONS
    public IO FirstClickedIO { get; set; }
    public WireNode FirstClickedWireNode { get; set; }

    // WINDOW AND UI
    public List<EditorWindow> EditorWindows { get; set; }
    public Func<bool> CurrentContextMenu { get; set; }
    public string? ContextMenuID { get; set; }
    public Vector2 MouseStartPos { get; set; }

    public Editor()
    {
        this.ComponentCreationContexts = new List<ComponentCreationContext>();
        this.ComponentCreationContextCategories = new List<(string, List<ComponentCreationContext>)>();
        this.EditorWindows = new List<EditorWindow>();
        this.EditorTabs = new Dictionary<string, EditorTab>();
    }

    public override void Initialize()
    {
        this.OnWindowResized += (width, height) =>
        {
            Vector2 windowSize = new Vector2(width, height);
            this.camera = new Camera2D(windowSize / 2.0f, Vector2.Zero, 0f, 1f);
            Settings.SetSetting<int>("windowWidth", width);
            Settings.SetSetting<int>("windowHeight", height);
            Settings.SaveSettings();
        };
    }

    public bool EditorWindowOfTypeOpen<T>() where T : EditorWindow
    {
        foreach (EditorWindow window in this.EditorWindows)
        {
            if (window is T)
            {
                return true;
            }
        }
        return false;
    }

    public bool EditorWindowOfTypeOpen(Type t)
    {
        foreach (EditorWindow window in this.EditorWindows)
        {
            if (window.GetType() == t)
            {
                return true;
            }
        }
        return false;
    }

    public void OpenEditorWindow(EditorWindow window)
    {
        if (EditorWindowOfTypeOpen(window.GetType()))
        {
            return;
        }
        this.EditorWindows.Add(window);
    }

    public Vector2 GetWorldMousePos()
    {
        this.camera.target = this.CurrentTab.CameraTarget;
        this.camera.zoom = this.CurrentTab.CameraZoom;
        return UserInput.GetMousePositionInWorld(this.camera);
    }

    public override void LoadContent()
    {
        this.camera = new Camera2D(base.WindowSize / 2f, Vector2.Zero, 0f, 1f);
        Util.Editor = this;

        this.OpenEditorTab(new EditorTab("Circuit"));
    }

    public void AddNewComponentCreationContext(ComponentCreationContext ccc)
    {
        this.ComponentCreationContexts.Add(ccc);

        // GROUP BY CATEGORIES
        this.ComponentCreationContextCategories = this.ComponentCreationContexts.GroupBy(ccc => ccc.Category).Select(group => (group.Key, group.ToList())).ToList();
    }

    public void OpenEditorTab(EditorTab tab)
    {
        this.EditorTabs.Add(tab.Name, tab);
        this.CurrentEditorTab = tab.Name;
    }

    public void SubmitComponentCreations()
    {
        foreach ((string category, List<ComponentCreationContext> contexts) in this.ComponentCreationContextCategories)
        {
            if (ImGui.TreeNodeEx(category, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.NoTreePushOnOpen))
            {
                foreach (ComponentCreationContext ccc in contexts)
                {
                    Vector2 buttonSize = new Vector2(140, 25);
                    ImGui.Button(ccc.Name, buttonSize);
                    if (ImGui.IsItemClicked())
                    {
                        CommandNewComponent cnc = new CommandNewComponent(ccc.DefaultComponent());
                        base.Execute(cnc, this);
                        this.FSM.SetState<ESMovingSelection>(this, 0);
                    }

                    if (ImGui.BeginPopupContextItem(ccc.Category + ccc.Name))
                    {
                        if (ccc.PopupCreator != null)
                        {
                            if (ccc.PopupCreator.Create(this, out Component? component))
                            {
                                CommandNewComponent cnc = new CommandNewComponent(component);
                                base.Execute(cnc, this);
                                this.FSM.SetState<ESMovingSelection>(this, 0);
                            }
                            ImGui.EndPopup();
                        }
                    }
                }
            }
        }
    }

    public override void SubmitUI()
    {
        // THEY ARE HERE FOR NOW
        this.ComponentCreationContexts.Clear();
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "AND", () => new LogicGate(this.GetWorldMousePos(), 2, new ANDLogic()), new CCPUGate(new ANDLogic())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "OR", () => new LogicGate(this.GetWorldMousePos(), 2, new ORLogic()), new CCPUGate(new ORLogic())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "XOR", () => new LogicGate(this.GetWorldMousePos(), 2, new XORLogic()), new CCPUGate(new XORLogic())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "NOR", () => new LogicGate(this.GetWorldMousePos(), 2, new NORLogic()), new CCPUGate(new NORLogic())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("I/O", "Switch", () => new Switch(this.GetWorldMousePos())));

        // MAIN MENU BAR
        ImGui.BeginMainMenuBar();

        ImGui.MenuItem("File");
        ImGui.MenuItem("Edit");

        ImGui.EndMainMenuBar();

        // SIDEBAR WINDOW
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.SetNextWindowPos(new Vector2(0, 22), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(this.SidebarWidth, base.WindowSize.Y - (this.MainMenuBarHeight - 2)), ImGuiCond.Always);
        ImGui.Begin("Components", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings);
        ImGui.PopStyleVar();
        this.SubmitComponentCreations();
        ImGui.End();

        Vector2 windowSize = this.WindowSize;
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoDocking;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.SetNextWindowPos(new Vector2(this.SidebarWidth, this.MainMenuBarHeight), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(windowSize.X - this.SidebarWidth, this.CircuitTabBarHeight), ImGuiCond.Always);

        ImGui.Begin("Main Dockspace", flags);
        ImGui.PopStyleVar();

        ImGui.BeginTabBar("Tabs", ImGuiTabBarFlags.None);

        foreach (KeyValuePair<string, EditorTab> kvp in this.EditorTabs)
        {
            bool open = true;

            if (this.FSM.CurrentState.ForcesSameTab && kvp.Key != this.CurrentEditorTab)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
            }

            if (ImGui.BeginTabItem(kvp.Key.PadRight(15), ref open, ImGuiTabItemFlags.None))
            {
                this.CurrentEditorTab = kvp.Key;
                ImGui.EndTabItem();
            }

            if (this.FSM.CurrentState.ForcesSameTab && kvp.Key != this.CurrentEditorTab)
            {
                ImGui.PopStyleVar();
            }

        }

        ImGui.EndTabBar();

        ImGui.End();

        ImGui.Begin("COMMANDS", ImGuiWindowFlags.NoNav);
        ImGui.SameLine();
        ImGui.Text(this.IsMouseInWorld ? "Mouse in world" : "Mouse not in world");
        ImGui.Text(ImGui.GetIO().WantCaptureKeyboard ? "Keyboard wanted" : "Keyboard not wanted");
        ImGui.Text("Zoom: " + this.camera.zoom.ToString());
        ImGui.Text("STATE: " + this.FSM.CurrentState.ToString());
        ImGui.Text("MOUSEINWORLD: " + this.GetWorldMousePos().ToString());
        ImGui.Text("MOUSEINWINDOW: " + UserInput.GetMousePositionInWindow().ToString());
        ImGui.Text("WINDOW: " + this.WindowSize.ToString());

        if (ImGui.Button("Undo"))
        {
            base.Undo(this);
        }
        ImGui.SameLine();
        if (ImGui.Button("Redo"))
        {
            base.Redo(this);
        }
        ImGui.SameLine();
        if (ImGui.Button("NEW"))
        {
            this.OpenEditorTab(new EditorTab(this.EditorTabs.Count.ToString()));
        }
        for (int i = 0; i < base.Commands.Count; i++)
        {
            Command<Editor> c = base.Commands[i];

            if (i > base.CurrentCommandIndex)
            {
                ImGui.TextDisabled(c.ToString());
            }
            else
            {
                ImGui.Text(c.ToString());
            }
        }
        ImGui.End();

        if (this.ContextMenuID != null)
        {
            ImGui.OpenPopup("###" + this.ContextMenuID);

            if (ImGui.BeginPopup("###" + this.ContextMenuID, ImGuiWindowFlags.NoMove))
            {
                if (!this.CurrentContextMenu())
                {
                    this.ContextMenuID = null;
                    ImGui.CloseCurrentPopup();
                }

                Vector2 popupSize = ImGui.GetWindowSize();
                Rectangle rec = new Rectangle(this.MouseStartPos.X, this.MouseStartPos.Y, popupSize.X, popupSize.Y).Inflate(25);
                if (!rec.ContainsVector2(UserInput.GetMousePositionInWindow()))
                {
                    this.ContextMenuID = null;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }

        this.EditorTabs[this.CurrentEditorTab].SubmitUI(this);
        this.IsMouseInWorld = Raylib.CheckCollisionPointRec(UserInput.GetMousePositionInWindow(), Util.CreateRecFromTwoCorners(new Vector2(this.SidebarWidth, this.MainMenuBarHeight + this.CircuitTabBarHeight), this.WindowSize)) && !ImGui.GetIO().WantCaptureKeyboard;
    }

    public void NewComponent(Component c, bool absolutePosition = false)
    {
        if (!absolutePosition)
        {
            c.Position -= c.Size / 2f;
        }
        c.Position = c.Position.SnapToGrid();
        this.Simulator.Selection.Clear();
        this.Simulator.AddComponent(c);
        this.Simulator.Select(c);
    }

    public void DrawGrid()
    {
        // Get camera's position and the size of the current view
        // This depends on the zoom of the camera. Dividing by the
        // zoom gives a correct representation of the actual visible view.
        Vector2 viewSize = UserInput.GetViewSize(this.camera);
        Vector2 camPos = this.camera.target;

        int pixelsInBetweenLines = Util.GridSizeX;

        // Draw vertical lines
        for (int i = (int)((camPos.X - viewSize.X / 2.0F) / pixelsInBetweenLines); i < ((camPos.X + viewSize.X / 2.0F) / pixelsInBetweenLines); i++)
        {
            int lineX = i * pixelsInBetweenLines;
            int lineYstart = (int)(camPos.Y - viewSize.Y / 2.0F);
            int lineYend = (int)(camPos.Y + viewSize.Y / 2.0F);

            Raylib.DrawLine(lineX, lineYstart, lineX, lineYend, Color.DARKGRAY.Opacity(0.1f));
        }

        // Draw horizontal lines
        for (int i = (int)((camPos.Y - viewSize.Y / 2.0F) / pixelsInBetweenLines); i < ((camPos.Y + viewSize.Y / 2.0F) / pixelsInBetweenLines); i++)
        {
            int lineY = i * pixelsInBetweenLines;
            int lineXstart = (int)(camPos.X - viewSize.X / 2.0F);
            int lineXend = (int)(camPos.X + viewSize.X / 2.0F);
            Raylib.DrawLine(lineXstart, lineY, lineXend, lineY, Color.DARKGRAY.Opacity(0.1f));
        }
    }

    public override void Update()
    {
        // UPDATE SIMULATION HERE
        this.EditorTabs[this.CurrentEditorTab].Update(this);
    }

    public unsafe override void Render()
    {
        this.camera.target = this.CurrentTab.CameraTarget;
        this.camera.zoom = this.CurrentTab.CameraZoom;

        Raylib.BeginMode2D(this.camera);
        Raylib.ClearBackground(Settings.GetSettingValue<Color>("editorBackgroundColor"));
        DrawGrid();

        // RENDER SIMULATION HERE
        this.EditorTabs[this.CurrentEditorTab].Render(this);
        Raylib.EndMode2D();

        Rectangle rTop = Util.CreateRecFromTwoCorners(Vector2.Zero, new Vector2(this.WindowSize.X, this.MainMenuBarHeight + this.CircuitTabBarHeight));
        Rectangle rLeft = Util.CreateRecFromTwoCorners(Vector2.Zero, new Vector2(this.SidebarWidth, this.WindowSize.Y));
        Raylib.DrawRectangleRec(rTop, Color.GRAY);
        Raylib.DrawRectangleRec(rLeft, Color.GRAY);

    }

    public override void OnClose()
    {

    }

    public void OpenContextMenu(string id, Func<bool> submit)
    {
        this.CurrentContextMenu = submit;
        this.ContextMenuID = id;
        this.MouseStartPos = UserInput.GetMousePositionInWindow();
    }
}