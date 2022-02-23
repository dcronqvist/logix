using System.Diagnostics.CodeAnalysis;
using System.Text;
using LogiX.Components;

namespace LogiX.Editor;

public class Editor : Application<Editor>
{
    // EDITOR STUFF
    public Camera2D camera;

    public Simulator Simulator { get; set; }
    public List<ComponentCreationContext> ComponentCreationContexts { get; set; }
    public List<(string, List<ComponentCreationContext>)> ComponentCreationContextCategories { get; set; }
    public EditorFSM FSM { get; set; }

    // TEMPORARY VARIABLES FOR CONNECTIONS
    public IO FirstClickedIO { get; set; }

    // KEY COMBINATIONS
    public List<EditorWindow> EditorWindows { get; set; }

    public Editor()
    {
        this.Simulator = new Simulator();
        this.ComponentCreationContexts = new List<ComponentCreationContext>();
        this.ComponentCreationContextCategories = new List<(string, List<ComponentCreationContext>)>();
        this.FSM = new EditorFSM();
        this.EditorWindows = new List<EditorWindow>();
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
        return UserInput.GetMousePositionInWorld(this.camera);
    }

    public override void LoadContent()
    {
        this.camera = new Camera2D(base.WindowSize / 2f, Vector2.Zero, 0f, 1f);
        Util.Editor = this;

        // COMPONENT CREATION CONTEXTS
    }

    public void AddNewComponentCreationContext(ComponentCreationContext ccc)
    {
        this.ComponentCreationContexts.Add(ccc);

        // GROUP BY CATEGORIES
        this.ComponentCreationContextCategories = this.ComponentCreationContexts.GroupBy(ccc => ccc.Category).Select(group => (group.Key, group.ToList())).ToList();
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
        float sidebarWidth = 170;
        ImGui.SetNextWindowSize(new Vector2(sidebarWidth, base.WindowSize.Y - 19), ImGuiCond.Always);
        ImGui.Begin("Components", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings);
        ImGui.PopStyleVar();
        this.SubmitComponentCreations();
        ImGui.End();

        ImGui.Begin("COMMANDS", ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoNav);
        if (ImGui.Button("Undo"))
        {
            base.Undo(this);
        }
        ImGui.SameLine();
        if (ImGui.Button("Redo"))
        {
            base.Redo(this);
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

        this.FSM.SubmitUI(this);
        ImGui.Text(this.FSM.CurrentState.GetType().Name);
    }

    public void NewComponent(Component c, bool absolutePosition = false)
    {
        if (!absolutePosition)
        {
            c.Position -= c.Size / 2f;
        }
        this.Simulator.Selection.Clear();
        this.Simulator.AddComponent(c);
        this.Simulator.SelectComponent(c);
    }

    public void DrawGrid()
    {
        // Get camera's position and the size of the current view
        // This depends on the zoom of the camera. Dividing by the
        // zoom gives a correct representation of the actual visible view.
        Vector2 camPos = this.camera.target;
        Vector2 viewSize = UserInput.GetViewSize(this.camera);

        int pixelsInBetweenLines = 250;

        // Draw vertical lines
        for (int i = (int)((camPos.X - viewSize.X / 2.0F) / pixelsInBetweenLines); i < ((camPos.X + viewSize.X / 2.0F) / pixelsInBetweenLines); i++)
        {
            int lineX = i * pixelsInBetweenLines;
            int lineYstart = (int)(camPos.Y - viewSize.Y / 2.0F);
            int lineYend = (int)(camPos.Y + viewSize.Y / 2.0F);

            Raylib.DrawLine(lineX, lineYstart, lineX, lineYend, Color.DARKGRAY);
        }

        // Draw horizontal lines
        for (int i = (int)((camPos.Y - viewSize.Y / 2.0F) / pixelsInBetweenLines); i < ((camPos.Y + viewSize.Y / 2.0F) / pixelsInBetweenLines); i++)
        {
            int lineY = i * pixelsInBetweenLines;
            int lineXstart = (int)(camPos.X - viewSize.X / 2.0F);
            int lineXend = (int)(camPos.X + viewSize.X / 2.0F);
            Raylib.DrawLine(lineXstart, lineY, lineXend, lineY, Color.DARKGRAY);
        }
    }

    public override void Update()
    {
        // UPDATE SIMULATION HERE
        this.Simulator.Interact(this);
        this.Simulator.PerformLogic();
        this.FSM.Update(this);
    }

    public override void Render()
    {
        Raylib.BeginMode2D(this.camera);
        Raylib.ClearBackground(Settings.GetSettingValue<Color>("editorBackgroundColor"));
        DrawGrid();

        // RENDER SIMULATION HERE
        this.Simulator.Render();
        this.FSM.Render(this);

        Raylib.EndMode2D();
    }

    public override void OnClose()
    {

    }
}