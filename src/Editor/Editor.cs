using System.Diagnostics.CodeAnalysis;
using System.Text;
using LogiX.Components;
using LogiX.Editor.Commands;
using LogiX.Editor.StateMachine;
using LogiX.SaveSystem;
using QuikGraph;

namespace LogiX.Editor;

public class Editor : Application
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

    // EDITOR ACTIONS
    public List<EditorAction> EditorActions { get; set; }
    public List<(string, List<EditorAction>)> EditorActionCategories { get; set; }

    public Dictionary<string, EditorTab> EditorTabs { get; set; }

    private string? _currentEditorTab;
    public string? CurrentEditorTab
    {
        get => _currentEditorTab;
        set
        {
            if (value != _currentEditorTab)
            {
                _currentEditorTab = value;

                if (value != null)
                    this.EditorTabs[value].OnEnter(this);
            }
        }
    }
    public EditorTab? CurrentTab => this.CurrentEditorTab != null ? this.EditorTabs[this.CurrentEditorTab] : null;
    public Simulator? Simulator => this.CurrentTab?.Simulator;
    public EditorFSM? FSM => this.CurrentTab?.FSM;
    public Project CurrentProject { get; set; }

    // TEMPORARY VARIABLES FOR CONNECTIONS
    public IO FirstClickedIO { get; set; }
    public WireNode FirstClickedWireNode { get; set; }
    public Wire FirstClickedWire { get; set; }

    // WINDOW AND UI
    public List<EditorWindow> EditorWindows { get; set; }
    public Func<bool> CurrentContextMenu { get; set; }
    public string? ContextMenuID { get; set; }
    public Vector2 MouseStartPos { get; set; }

    // TEMPORARY VARIABLES FOR MENU BAR UI
    public string newCircuitName;

    public Editor()
    {
        this.ComponentCreationContexts = new List<ComponentCreationContext>();
        this.ComponentCreationContextCategories = new List<(string, List<ComponentCreationContext>)>();
        this.EditorWindows = new List<EditorWindow>();
        this.EditorTabs = new Dictionary<string, EditorTab>();
        this.EditorActions = new List<EditorAction>();
        this.EditorActionCategories = new List<(string, List<EditorAction>)>();
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

        this.newCircuitName = "";
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

    public void Execute(Command<Editor> command, bool doExecute = true)
    {
        this.Execute(command, this, doExecute);
    }

    public void Execute(Command<Editor> command, Editor editor, bool doExecute = true)
    {
        this.CurrentTab.Execute(command, editor, doExecute);
    }

    public void Undo()
    {
        this.CurrentTab.Undo(this);
    }

    public void Redo()
    {
        this.CurrentTab.Redo(this);
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

        if (!File.Exists("./project.json"))
        {
            this.CurrentProject = new Project();
            this.OpenEditorTab(new EditorTab(this.CurrentProject.NewCircuit("Main")));
        }
        else
        {
            this.CurrentProject = Project.LoadFromFile("./project.json");
            this.OpenEditorTab(new EditorTab(this.CurrentProject.Circuits[0]));
        }

        // THEY ARE HERE FOR NOW
        this.ComponentCreationContexts.Clear();
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "AND Gate", () => new LogicGate(this.GetWorldMousePos(), 2, new ANDLogic()), new CCPUGate(new ANDLogic())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "NAND Gate", () => new LogicGate(this.GetWorldMousePos(), 2, new NANDLogic()), new CCPUGate(new NANDLogic())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "OR Gate", () => new LogicGate(this.GetWorldMousePos(), 2, new ORLogic()), new CCPUGate(new ORLogic())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "NOR Gate ", () => new LogicGate(this.GetWorldMousePos(), 2, new NORLogic()), new CCPUGate(new NORLogic())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "XOR Gate", () => new LogicGate(this.GetWorldMousePos(), 2, new XORLogic()), new CCPUGate(new XORLogic())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "XNOR Gate", () => new LogicGate(this.GetWorldMousePos(), 2, new XNORLogic()), new CCPUGate(new XNORLogic())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "NOT Gate", () => new BufferComponent(this.GetWorldMousePos(), 1, new InverterLogic())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "Buffer", () => new BufferComponent(this.GetWorldMousePos(), 1, new BufferLogic())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("Logic Gates", "Tri-State Buffer", () => new TriStateComponent(this.GetWorldMousePos(), 1)));

        this.AddNewComponentCreationContext(new ComponentCreationContext("I/O", "Switch", () => new Switch(1, this.GetWorldMousePos())));
        this.AddNewComponentCreationContext(new ComponentCreationContext("I/O", "Lamp", () => new Lamp(1, this.GetWorldMousePos())));

        // MAIN MENU ITEMS
        this.EditorActions.Clear();
        this.AddNewEditorAction(new EditorAction("File", "New Circuit...", (editor =>
        {
            editor.Modal("Create New Circuit", () =>
            {
                ImGui.InputText("Name", ref editor.newCircuitName, 24, ImGuiInputTextFlags.None);
                ImGui.Separator();

                if (ImGui.Button("Create") || Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
                {
                    Circuit newCirc = this.CurrentProject.NewCircuit(this.newCircuitName);
                    this.newCircuitName = "";
                    this.OpenEditorTab(new EditorTab(newCirc));
                    return true;
                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    return true;
                }

                return false;
            }, ImGuiWindowFlags.AlwaysAutoResize);

        }), null, this.GetPrimaryMod(), KeyboardKey.KEY_N));
        this.AddNewEditorAction(new EditorAction("File", "Quicksave", (editor =>
        {
            editor.CurrentTab.Save();
            this.CurrentProject.SaveToFile("./project.json");
        }), null, this.GetPrimaryMod(), KeyboardKey.KEY_S));
        this.AddNewEditorAction(new EditorAction("Edit", "Undo", (editor =>
        {
            editor.Undo();
        }), null, this.GetPrimaryMod(), KeyboardKey.KEY_Z));
        this.AddNewEditorAction(new EditorAction("Edit", "Redo", (editor =>
        {
            editor.Redo();
        }), null, this.GetPrimaryMod(), KeyboardKey.KEY_Y));
        this.AddNewEditorAction(new EditorAction("Edit", "Rotate CW", (editor =>
        {
            List<Command<Editor>> commands = new List<Command<Editor>>();

            foreach (Component c in editor.Simulator.Selection.Where(x => x is Component))
            {
                commands.Add(new CommandRotateComponent(c, 1));
            }

            editor.Execute(new MultiCommand<Editor>("Rotated Components Clockwise", commands));

        }), null, this.GetPrimaryMod(), KeyboardKey.KEY_RIGHT));
        this.AddNewEditorAction(new EditorAction("Edit", "Rotate CCW", (editor =>
        {
            List<Command<Editor>> commands = new List<Command<Editor>>();

            foreach (Component c in editor.Simulator.Selection.Where(x => x is Component))
            {
                commands.Add(new CommandRotateComponent(c, -1));
            }

            editor.Execute(new MultiCommand<Editor>("Rotated Components Counter Clockwise", commands));

        }), null, this.GetPrimaryMod(), KeyboardKey.KEY_LEFT));
        this.AddNewEditorAction(new EditorAction("Edit", "Delete Selection", (editor =>
        {
            List<Component> comps = editor.Simulator.Selection.Where(x => x is Component).Cast<Component>().ToList();

            List<Command<Editor>> commands = new List<Command<Editor>>();
            foreach (Component c in comps)
            {
                commands.Add(new CommandDeleteComponent(c));
            }

            List<JunctionWireNode> nodes = editor.Simulator.Selection.Where(x => x is JunctionWireNode).Cast<JunctionWireNode>().ToList();
            foreach (JunctionWireNode wn in nodes)
            {
                editor.Simulator.TryGetJunctionFromPosition(wn.GetPosition(), out JunctionWireNode? jwn, out Wire? nodeOnWire);

                List<Edge<WireNode>> edgesAdjacent = nodeOnWire!.Graph.AdjacentEdges(wn).ToList();

                foreach (Edge<WireNode> edge in edgesAdjacent)
                {
                    commands.Add(new CommandDeleteWireSegment(edge.MiddleOfEdge()));
                }
            }

            MultiCommand<Editor> multiCommand = new MultiCommand<Editor>($"Deleted {comps.Count} Components & {nodes.Count} junctions", commands.ToArray());
            editor.Execute(multiCommand);
        }), (editor => editor?.Simulator?.Selection.Where(x => x is Component).Cast<Component>().Count() > 0 && !ImGui.IsAnyItemActive()), KeyboardKey.KEY_BACKSPACE));
    }

    public void AddNewComponentCreationContext(ComponentCreationContext ccc)
    {
        this.ComponentCreationContexts.Add(ccc);

        // GROUP BY CATEGORIES
        this.ComponentCreationContextCategories = this.ComponentCreationContexts.GroupBy(ccc => ccc.Category).Select(group => (group.Key, group.ToList())).ToList();
    }

    public void AddNewEditorAction(EditorAction action)
    {
        this.EditorActions.Add(action);

        this.EditorActionCategories = this.EditorActions.GroupBy(ea => ea.Category).Select(group => (group.Key, group.ToList())).ToList();
    }

    public void OpenEditorTab(EditorTab tab)
    {
        this.EditorTabs.Add(tab.Name, tab);
        this.CurrentEditorTab = tab.Name;
    }

    public void SubmitMainMenuBar()
    {
        foreach ((string category, List<EditorAction> actions) in this.EditorActionCategories)
        {
            if (ImGui.BeginMenu(category))
            {
                foreach (EditorAction ea in actions)
                {
                    if (ImGui.MenuItem(ea.Name, Util.KeyComboString(ea.Shortcut), false, ea.CanExecute(this)))
                    {
                        ea.Execute(this);
                    }
                }

                ImGui.EndMenu();
            }
        }
    }

    public void SubmitComponentCreations()
    {
        foreach ((string category, List<ComponentCreationContext> contexts) in this.ComponentCreationContextCategories)
        {
            if (ImGui.TreeNodeEx(category, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.NoTreePushOnOpen))
            {
                for (int i = 0; i < contexts.Count; i++)
                {
                    ComponentCreationContext ccc = contexts[i];

                    Vector2 buttonSize = new Vector2(140, 25);
                    ImGui.Button(ccc.Name, buttonSize);
                    if (ImGui.IsItemClicked())
                    {
                        CommandNewComponent cnc = new CommandNewComponent(ccc.DefaultComponent());
                        this.Execute(cnc, this);
                        this.FSM.SetState<ESMovingSelection>(this, 0);
                    }

                    if (ccc.PopupCreator != null)
                    {
                        if (ImGui.BeginPopupContextItem(ccc.Category + ccc.Name))
                        {
                            if (ccc.PopupCreator.Create(this, out Component? component))
                            {
                                CommandNewComponent cnc = new CommandNewComponent(component);
                                this.Execute(cnc, this);
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
        // MAIN MENU BAR
        ImGui.BeginMainMenuBar();
        this.SubmitMainMenuBar();
        ImGui.EndMainMenuBar();

        // SIDEBAR WINDOW
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.SetNextWindowPos(new Vector2(0, 22), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(this.SidebarWidth, base.WindowSize.Y - (this.MainMenuBarHeight - 2)), ImGuiCond.Always);
        ImGui.Begin("Components", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings);
        ImGui.PopStyleVar();

        // Here we should submit all custom circuits to the sidebar and
        this.SubmitCircuitCreations();
        this.SubmitComponentCreations();
        ImGui.End();

        Vector2 windowSize = this.WindowSize;
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoDocking;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.SetNextWindowPos(new Vector2(this.SidebarWidth, this.MainMenuBarHeight), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(windowSize.X - this.SidebarWidth, this.CircuitTabBarHeight), ImGuiCond.Always);

        ImGui.Begin("Main Dockspace", flags);
        ImGui.PopStyleVar();

        ImGui.BeginTabBar("Tabs", ImGuiTabBarFlags.None | ImGuiTabBarFlags.Reorderable | ImGuiTabBarFlags.AutoSelectNewTabs);

        foreach (KeyValuePair<string, EditorTab> kvp in this.EditorTabs)
        {
            bool open = true;

            if (this.FSM.CurrentState.ForcesSameTab && kvp.Key != this.CurrentEditorTab)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
            }

            if (ImGui.BeginTabItem(kvp.Key, ref open, kvp.Value.HasChanges() ? ImGuiTabItemFlags.UnsavedDocument : ImGuiTabItemFlags.None))
            {
                this.CurrentEditorTab = kvp.Key;
                ImGui.EndTabItem();
            }

            if (this.FSM.CurrentState.ForcesSameTab && kvp.Key != this.CurrentEditorTab)
            {
                ImGui.PopStyleVar();
            }

            if (!open)
            {
                if (kvp.Value.TryClose())
                {
                    // TODO: Remove the tab
                    this.EditorTabs.Remove(kvp.Key);
                    if (this.CurrentEditorTab == kvp.Key)
                    {
                        this.CurrentEditorTab = this.EditorTabs.Count > 0 ? this.EditorTabs.First().Key : null;
                    }
                }
                else
                {
                    this.ModalError($"You have unsaved changes in '{kvp.Key}'. Closing the tab before\nsaving will discard all changes. Are you sure you want to close it?", ModalButtonsType.YesNo, yes: () =>
                    {
                        // TODO: Remove the tab with same code as above ^
                        this.EditorTabs.Remove(kvp.Key);
                        if (this.CurrentEditorTab == kvp.Key)
                        {
                            this.CurrentEditorTab = this.EditorTabs.Count > 0 ? this.EditorTabs.First().Key : null;
                        }
                    });
                }
            }

        }

        ImGui.EndTabBar();

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

        this.CurrentTab?.SubmitUI(this);
        this.IsMouseInWorld = Raylib.CheckCollisionPointRec(UserInput.GetMousePositionInWindow(), Util.CreateRecFromTwoCorners(new Vector2(this.SidebarWidth, this.MainMenuBarHeight + this.CircuitTabBarHeight), this.WindowSize)) && !ImGui.GetIO().WantCaptureKeyboard;
        this.IsMouseInWorld = this.IsMouseInWorld && !ImGui.GetIO().WantCaptureMouse;
    }

    private void SubmitCircuitCreations()
    {
        if (ImGui.TreeNodeEx("Circuits", ImGuiTreeNodeFlags.NoTreePushOnOpen))
        {
            foreach (Circuit circ in this.CurrentProject.Circuits)
            {
                Vector2 buttonSize = new Vector2(140, 25);

                if (circ.Name == this.CurrentEditorTab)
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);

                ImGui.Button(circ.Name, buttonSize);

                if (circ.Name == this.CurrentEditorTab)
                    ImGui.PopStyleVar();

                if (circ.Name != this.CurrentEditorTab)
                {
                    if (ImGui.IsItemClicked())
                    {
                        Component c = new IntegratedComponent(this.GetWorldMousePos().SnapToGrid(), circ);
                        CommandNewComponent cnc = new CommandNewComponent(c);
                        this.Execute(cnc, this);
                        this.FSM?.SetState<ESMovingSelection>(this, 0);
                    }

                    if (ImGui.BeginPopupContextItem(circ.Name))
                    {
                        if (ImGui.MenuItem("Edit"))
                        {
                            if (this.EditorTabs.ContainsKey(circ.Name))
                            {
                                this.CurrentEditorTab = circ.Name;
                            }
                            else
                            {
                                this.OpenEditorTab(new EditorTab(circ));
                            }
                        }
                        ImGui.EndPopup();
                    }
                }
            }
        }
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
        this.CurrentTab?.Update(this);

        // CHECK IF EDITOR ACTIONS SHORTCUTS ARE PRESSED
        foreach (EditorAction ea in this.EditorActions)
        {
            if (ea.CanExecute(this) && UserInput.KeyComboPressed(ea.Shortcut))
            {
                ea.Execute(this);
            }
        }
    }

    public unsafe override void Render()
    {
        this.camera.target = this.CurrentTab == null ? Vector2.Zero : this.CurrentTab.CameraTarget;
        this.camera.zoom = this.CurrentTab == null ? 1f : this.CurrentTab.CameraZoom;

        Raylib.BeginMode2D(this.camera);

        if (this.CurrentTab != null)
        {
            Raylib.ClearBackground(Settings.GetSettingValue<Color>("editorBackgroundColor"));
            DrawGrid();
        }
        else
        {
            Raylib.ClearBackground(Settings.GetSettingValue<Color>("editorBackgroundColor").Multiply(0.3f));
        }

        // RENDER SIMULATION HERE
        this.CurrentTab?.Render(this);
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