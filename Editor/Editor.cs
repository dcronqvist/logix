using LogiX.Components;
using LogiX.SaveSystem;
using Newtonsoft.Json;

namespace LogiX.Editor;

public class Editor : Application
{
    // Editor states
    public Camera2D editorCamera;
    EditorState editorState;
    public Simulator simulator;
    public Project loadedProject;
    Component contextMenuComponent;
    EditorFSM fsm;
    Dictionary<string, Tuple<Func<Component>, IUISubmitter<bool, Editor>?>> componentCreationContexts;
    Dictionary<string, List<string>> componentCategories;
    Modal currentModal;

    // GATES
    IGateLogic[] availableGateLogics;

    // KEY COMBINATIONS
    KeyboardKey primaryKeyMod;

    // MAIN MENU BAR ACTIONS
    List<Tuple<string, List<Tuple<string, EditorAction>>>> mainMenuButtons;

    // VARIABLES FOR TEMPORARY STUFF
    public ComponentInput? hoveredInput;
    public ComponentOutput? hoveredOutput;
    public Component? hoveredComponent;
    public Vector2 recSelectFirstCorner;

    public ComponentOutput? connectFrom;

    CircuitDescription? copiedCircuit;
    List<SLDescription> icSwitches;
    Dictionary<SLDescription, int> icSwitchGroup;
    Dictionary<SLDescription, int> icLampGroup;
    List<SLDescription> icLamps;
    string icName;

    // UI VARIABLES
    int newComponentBits;
    bool newComponentMultibit;
    int newRomOutputbits;
    bool newRomOutputMultibit;

    public override void Initialize()
    {
        mainMenuButtons = new List<Tuple<string, List<Tuple<string, EditorAction>>>>();
#if OSX
        this.primaryKeyMod = KeyboardKey.KEY_LEFT_SUPER;
#else
        this.primaryKeyMod = KeyboardKey.KEY_LEFT_CONTROL;
#endif

        this.OnWindowResized += (width, height) =>
        {
            Vector2 windowSize = new Vector2(width, height);
            this.editorCamera = new Camera2D(windowSize / 2.0f, Vector2.Zero, 0f, 1f);
            Settings.SetSetting<int>("windowWidth", width);
            Settings.SetSetting<int>("windowHeight", height);
            Settings.SaveSettings();
        };

        availableGateLogics = new IGateLogic[] {
            new ANDLogic(),
            new NANDLogic(),
            new ORLogic(),
            new NORLogic(),
            new XORLogic(),
            new NOTLogic()
        };

        this.componentCreationContexts = new Dictionary<string, Tuple<Func<Component>, IUISubmitter<bool, Editor>?>>();
        this.componentCategories = new Dictionary<string, List<string>>();
    }

    public void SetProject(Project proj)
    {
        if (this.loadedProject == null)
        {
            this.loadedProject = proj;
            (List<Component> comps, List<Wire> wires) = proj.GetComponentsAndWires();
            this.simulator.AddComponents(comps);
            this.simulator.AddWires(wires);
        }
        else
        {
            // There is an already ongoing project, save it
            //this.loadedProject.SaveToFile(Directory.GetCurrentDirectory());

            this.loadedProject = proj;
            (List<Component> comps, List<Wire> wires) = proj.GetComponentsAndWires();
            this.simulator.Components.Clear();
            this.simulator.Wires.Clear();
            this.simulator.AddComponents(comps);
            this.simulator.AddWires(wires);
        }

        this.LoadComponentButtons();
    }

    public override void LoadContent()
    {
        Vector2 windowSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        this.editorCamera = new Camera2D(windowSize / 2.0f, Vector2.Zero, 0f, 1f);
        this.editorState = EditorState.None;

        this.simulator = new Simulator();

        Util.OpenSans = Raylib.LoadFontEx($"{Directory.GetCurrentDirectory()}/assets/opensans-bold.ttf", 100, Enumerable.Range(0, 1000).ToArray(), 1000);
        Raylib.SetTextureFilter(Util.OpenSans.texture, TextureFilter.TEXTURE_FILTER_TRILINEAR);

        Settings.LoadSettings();

        this.fsm = new EditorFSM();

        // ASSIGNING KEYCOMBO ACTIONS
        AddNewMainMenuItem("File", "Save", new EditorAction((editor) => true, (Editor editor, out string error) =>
        {
            this.loadedProject.SaveComponentsInWorkspace(this.simulator.Components);
            this.loadedProject.SaveToFile(this.loadedProject.LoadedFromDirectory);
            error = ""; return true;
        }, this.primaryKeyMod, KeyboardKey.KEY_S));

        AddNewMainMenuItem("File", "Open Project", new EditorAction((editor) => true, (Editor editor, out string error) =>
        {
            this.SelectFile(Directory.GetCurrentDirectory(), (file) =>
            {
                Project p = Project.LoadFromFile(file);
                SetProject(p);
            }, Project.EXTENSION);
            error = ""; return true;
        }, this.primaryKeyMod, KeyboardKey.KEY_O));

        AddNewMainMenuItem("File", "Include IC File", new EditorAction((editor) => true, (Editor editor, out string error) =>
        {
            this.SelectFile(Directory.GetCurrentDirectory(), (file) =>
            {
                if (!this.loadedProject.IncludeICFile(file))
                {
                    base.ModalError("Could not include IC file.");
                    return;
                }
                else
                {
                    this.LoadComponentButtons();
                }

            }, ICDescription.EXTENSION);
            error = ""; return true;
        }, this.primaryKeyMod, KeyboardKey.KEY_I, KeyboardKey.KEY_F));

        AddNewMainMenuItem("File", "Include IC Collection", new EditorAction((editor) => true, (Editor editor, out string error) =>
        {
            this.SelectFile(Directory.GetCurrentDirectory(), (file) =>
            {
                this.loadedProject.IncludeICCollectionFile(file);
                this.LoadComponentButtons();
            }, ICCollection.EXTENSION);
            error = ""; return true;
        }, this.primaryKeyMod, KeyboardKey.KEY_I, KeyboardKey.KEY_C));

        AddNewMainMenuItem("Edit", "Copy", new EditorAction((editor) => this.simulator.SelectedComponents.Count > 0, (Editor editor, out string error) => { MMCopy(); error = ""; return true; }, this.primaryKeyMod, KeyboardKey.KEY_C));
        AddNewMainMenuItem("Edit", "Paste", new EditorAction((editor) => this.copiedCircuit != null, (Editor editor, out string error) => { MMPaste(); error = ""; return true; }, this.primaryKeyMod, KeyboardKey.KEY_V));
        AddNewMainMenuItem("Edit", "Select All", new EditorAction((editor) => true, (Editor editor, out string error) => { this.simulator.SelectAllComponents(); error = ""; return true; }, this.primaryKeyMod, KeyboardKey.KEY_A));
        AddNewMainMenuItem("Edit", "Delete Selection", new EditorAction((editor) => this.simulator.SelectedComponents.Count > 0, (Editor editor, out string error) => { this.simulator.DeleteSelection(); error = ""; return true; }, KeyboardKey.KEY_BACKSPACE));
        AddNewMainMenuItem("Integrated Circuits", "Create IC from Selection", new EditorAction((editor) => this.simulator.SelectedComponents.Count > 0, (Editor editor, out string error) =>
        {
            CircuitDescription cd = new CircuitDescription(this.simulator.SelectedComponents);

            if (cd.ValidForIC())
            {
                this.currentModal = new ModalCreateIC(cd);
                error = "";
                return true;
            }
            else
            {
                error = "Could not create IC from selected components since there are switches or lamps without identifiers.";
                return false;
            }
        }));

        /*AddNewMainMenuItem("Integrated Circuits", "Export IC Collection from Selection", () => this.simulator.SelectedComponents.All(x => x is ICComponent), null, null, () =>
        {
            List<ICComponent> components = this.simulator.SelectedComponents.Cast<ICComponent>().ToList();

            List<ICDescription> descriptions = new List<ICDescription>();

            foreach (ICComponent c in components)
            {
                ICDescription icd = c.Description;
                descriptions.Add(icd);
            }

            ICCollection collection = new ICCollection("testing", descriptions);

            base.SelectFolder(Directory.GetCurrentDirectory(), (folder) =>
            {
                collection.SaveToFile(folder);
            }, () =>
            {

            });
        });*/

        SetProject(new Project("unnamed-project"));
    }

    public void LoadComponentButtons()
    {
        this.componentCategories = new Dictionary<string, List<string>>();
        this.componentCreationContexts = new Dictionary<string, Tuple<Func<Component>, IUISubmitter<bool, Editor>?>>();

        // I/O
        this.AddNewComponentCreationContext("I/O", "Switch", () => { return new Switch(1, UserInput.GetMousePositionInWorld(editorCamera)); }, new CCPUSimple(false, false, true, false, (_, _, ob, _) =>
        {
            return new Switch(ob, UserInput.GetMousePositionInWorld(editorCamera));
        }));
        this.AddNewComponentCreationContext("I/O", "Button", () => { return new Button(1, UserInput.GetMousePositionInWorld(editorCamera)); }, null);
        this.AddNewComponentCreationContext("I/O", "Lamp", () => { return new Lamp(1, UserInput.GetMousePositionInWorld(editorCamera)); }, new CCPUSimple(true, true, false, false, (ib, _, _, _) =>
        {
            return new Lamp(ib, UserInput.GetMousePositionInWorld(editorCamera));
        }));
        this.AddNewComponentCreationContext("I/O", "Hex Viewer", () => { return new HexViewer(4, false, UserInput.GetMousePositionInWorld(editorCamera)); }, new CCPUSimple(true, true, false, false, (ib, im, _, _) =>
        {
            return new HexViewer(ib, im, UserInput.GetMousePositionInWorld(editorCamera));
        }));
        this.AddNewComponentCreationContext("I/O", "ROM", () => { return new ROM(false, 4, false, 4, UserInput.GetMousePositionInWorld(editorCamera)); }, new CCPUSimple(true, true, true, true, (ib, im, ob, om) =>
        {
            return new ROM(im, ib, om, ob, UserInput.GetMousePositionInWorld(editorCamera));
        }));
        this.AddNewComponentCreationContext("I/O", "Memory", () => { return new MemoryComponent(4, false, 8, false, UserInput.GetMousePositionInWorld(editorCamera)); }, new CCPUSimple(true, true, true, true, (ib, im, ob, om) =>
        {
            return new MemoryComponent(ib, im, ob, om, UserInput.GetMousePositionInWorld(editorCamera));
        }));
        this.AddNewComponentCreationContext("I/O", "Label", () => { return new TextComponent(UserInput.GetMousePositionInWorld(editorCamera)); }, null);
        this.AddNewComponentCreationContext("I/O", "Constant", () => { return new ConstantComponent(LogicValue.HIGH, UserInput.GetMousePositionInWorld(editorCamera)); }, null);
        this.AddNewComponentCreationContext("I/O", "Splitter", () => { return new Splitter(2, 2, true, false, UserInput.GetMousePositionInWorld(editorCamera)); }, new CCPUSimple(true, true, true, true, (ib, im, ob, om) =>
        {
            return new Splitter(ib, ob, im, om, UserInput.GetMousePositionInWorld(editorCamera));
        }));
        this.AddNewComponentCreationContext("I/O", "Clock", () => { return new Clock(500, UserInput.GetMousePositionInWorld(editorCamera)); }, null);

        // GATES
        foreach (IGateLogic logic in this.availableGateLogics)
        {
            this.AddNewComponentCreationContext("Gates", logic.GetLogicText(), () =>
            {
                return new LogicGate(logic.DefaultBits(), false, logic, UserInput.GetMousePositionInWorld(editorCamera));
            }, new CCPUSimple(true, true, false, false, (ib, im, _, _) =>
            {
                return new LogicGate(ib, im, logic, UserInput.GetMousePositionInWorld(editorCamera));
            }));
        }

        // ICs
        // Project created ICs
        foreach (ICDescription icd in this.loadedProject.ProjectCreatedICs.ToArray())
        {
            this.AddNewComponentCreationContext("Project ICs", icd.Name, () => { return icd.ToComponent(false).SetPosition(UserInput.GetMousePositionInWorld(editorCamera)); },
            new CCPUIC(icd, (desc) =>
            {
                this.loadedProject.RemoveProjectCreatedIC(icd);
            }));
        }
        // Included single IC files
        foreach (ICDescription icd in this.loadedProject.ICsFromFile.ToArray())
        {
            this.AddNewComponentCreationContext("Included ICs", icd.Name, () => { return icd.ToComponent(false).SetPosition(UserInput.GetMousePositionInWorld(editorCamera)); },
            new CCPUIC(icd, (desc) =>
            {
                this.loadedProject.ExcludeICFromFile(icd);
            }));
        }
        // Included collections
        foreach (KeyValuePair<string, ICCollection> collection in this.loadedProject.ICCollections.ToArray())
        {
            foreach (ICDescription icd in collection.Value.ICs.ToArray())
            {
                this.AddNewComponentCreationContext(collection.Key, icd.Name, () => { return icd.ToComponent(false).SetPosition(UserInput.GetMousePositionInWorld(editorCamera)); },
                new CCPUIC(icd, (desc) =>
                {
                    this.loadedProject.ExcludeICCollection(collection.Value);
                }));
            }
        }
    }

    public void AddNewComponentCreationContext(string category, string componentName, Func<Component> defaultCreator, IUISubmitter<bool, Editor>? contextMenu)
    {
        if (!this.componentCategories.ContainsKey(category))
        {
            this.componentCategories.Add(category, new List<string>());
        }

        this.componentCategories[category].Add(componentName);

        this.componentCreationContexts.Add(componentName, new Tuple<Func<Component>, IUISubmitter<bool, Editor>?>(defaultCreator, contextMenu));
    }

    public void HandleComponentCreationContexts()
    {
        foreach (KeyValuePair<string, List<string>> category in this.componentCategories)
        {
            List<string> compsInCategory = category.Value;
            string cat = category.Key;

            ImGui.Text(cat);

            foreach (string component in compsInCategory)
            {
                Tuple<Func<Component>, IUISubmitter<bool, Editor>?> context = this.componentCreationContexts[component];

                Vector2 buttonSize = new Vector2(94, 25);
                ImGui.Button(component, buttonSize);
                if (ImGui.IsItemClicked())
                {
                    // Use default creator to create new component
                    this.NewComponent(context.Item1());
                }

                // If a IUISubmitter was supplied, then we want to show
                // it as a context menu
                if (context.Item2 != null)
                {
                    if (ImGui.BeginPopupContextItem(component))
                    {
                        if (context.Item2.SubmitUI(this))
                        {
                            ImGui.CloseCurrentPopup();
                        }
                        ImGui.EndPopup();
                    }
                }
            }
        }
    }

    public void AddNewMainMenuItem(string mainButton, string actionButtonName, EditorAction action)
    {
        if (!this.mainMenuButtons.Exists(x => x.Item1 == mainButton))
        {
            this.mainMenuButtons.Add(new Tuple<string, List<Tuple<string, EditorAction>>>(mainButton, new List<Tuple<string, EditorAction>>()));
        }

        Tuple<string, List<Tuple<string, EditorAction>>> mainMenuButton = this.mainMenuButtons.Find(x => x.Item1 == mainButton);

        mainMenuButton.Item2.Add(new Tuple<string, EditorAction>(actionButtonName, action));
    }

    public void MMCopy()
    {
        copiedCircuit = new CircuitDescription(this.simulator.SelectedComponents);
    }

    public void MMPaste()
    {
        if (copiedCircuit != null)
        {
            this.PasteComponentsAndWires(copiedCircuit, UserInput.GetMousePositionInWorld(this.editorCamera), false);
        }
    }

    public void PasteComponentsAndWires(CircuitDescription cd, Vector2 pos, bool preserveIDs)
    {
        (List<Component> comps, List<Wire> wires) = cd.CreateComponentsAndWires(pos, preserveIDs);
        this.simulator.AddComponents(comps);
        this.simulator.AddWires(wires);

        this.simulator.ClearSelection();

        foreach (Component c in comps)
        {
            this.simulator.SelectComponent(c);
        }
    }

    public override void SubmitUI()
    {
        // MAIN MENU BAR
        ImGui.BeginMainMenuBar();

        foreach (Tuple<string, List<Tuple<string, EditorAction>>> tup in this.mainMenuButtons)
        {
            if (ImGui.BeginMenu(tup.Item1))
            {
                foreach (Tuple<string, EditorAction> inner in tup.Item2)
                {
                    if (ImGui.MenuItem(inner.Item1, (inner.Item2.HasKeys()) ? inner.Item2.GetShortcutString() : null, false, (inner.Item2.Condition(this))))
                    {
                        if (!inner.Item2.Execute(this, out string error))
                        {
                            this.ModalError(error);
                        }
                    }
                }

                ImGui.EndMenu();
            }
        }

        ImGui.Separator();

        if (ImGui.BeginMenu(this.loadedProject.Name))
        {
            if (ImGui.Button("Reload ICs"))
            {
                this.loadedProject.ReloadProjectICs();
            }

            ImGui.EndMenu();
        }

        ImGui.EndMainMenuBar();

        // SIDEBAR

        ImGui.SetNextWindowPos(new Vector2(0, 22), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(120, Raylib.GetScreenHeight() - 19), ImGuiCond.Always);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.3f, 0.3f, 0.3f, 0.8f));
        ImGui.Begin("Sidebar", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoNavInputs);
        ImGui.End();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        // COMPONENTS WINDOW

        ImGui.Begin("Components", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoNavInputs);
        this.HandleComponentCreationContexts();
        ImGui.End();

        if (this.editorState == EditorState.MakingIC)
        {
            ImGui.OpenPopup("Create Integrated Circuit");
        }

        // MAKING IC WINDOW
        /*
        Vector2 windowSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        ImGui.SetNextWindowPos(windowSize / 2, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        Vector2 popupSize = new Vector2(120) * 4f;
        ImGui.SetNextWindowSizeConstraints(popupSize, popupSize);
        ImGui.SetNextWindowSize(popupSize);
        if (ImGui.BeginPopupModal("Create Integrated Circuit"))
        {
            
            ImGui.EndPopup();
        }*/

        // DEBUG WINDOW

        bool x = true;
        ImGui.Begin("Debug stuff", ref x, ImGuiWindowFlags.NoNav);
        ImGui.Text("Mouse Position in Window:");
        ImGui.Text(UserInput.GetMousePositionInWindow().ToString());
        ImGui.Text("Camera Position:");
        ImGui.Text(this.editorCamera.target.ToString());
        ImGui.Text("Mouse Position in World:");
        ImGui.Text(UserInput.GetMousePositionInWorld(this.editorCamera).ToString());
        ImGui.Text("Camera View Size:");
        ImGui.Text(UserInput.GetViewSize(this.editorCamera).ToString());
        ImGui.Text("Current editor state:");
        ImGui.Text(this.editorState.ToString());
        ImGui.Text("IO Want Keyboard:");
        ImGui.Text(ImGui.GetIO().WantCaptureKeyboard.ToString());
        ImGui.Text("IO Want Mouse:");
        ImGui.Text(ImGui.GetIO().WantCaptureMouse.ToString());
        ImGui.Text($"FSM Current State: {this.fsm.CurrentState?.GetType().Name}");

        ImGui.Separator();
        ImGui.Text("Settings");

        Dictionary<string, Setting> settings = Settings.GetAllSettings();
        foreach (KeyValuePair<string, Setting> setting in settings)
        {
            ImGui.Text(setting.Key + ": " + setting.Value.Value.ToString());
        }

        ImGui.End();

        // If single selecting a component
        if (this.simulator.SelectedComponents.Count == 1)
        {
            Component c = this.simulator.SelectedComponents[0];
            c.OnSingleSelectedSubmitUI();
        }

        if (!ImGui.GetIO().WantCaptureMouse)
        {
            // On right clicking component, open its context menu
            foreach (Component c in this.simulator.Components)
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && Raylib.CheckCollisionPointRec(UserInput.GetMousePositionInWorld(this.editorCamera), c.Box))
                {
                    this.contextMenuComponent = c;
                    ImGui.OpenPopup($"###ContextMenuComp{this.contextMenuComponent.uniqueID}");
                }
            }
        }

        ImGui.SetNextWindowPos(UserInput.GetMousePositionInWindow() - new Vector2(5, 5), ImGuiCond.Appearing);
        if (ImGui.BeginPopupContextWindow($"###ContextMenuComp{(this.contextMenuComponent != null ? this.contextMenuComponent.uniqueID : 5)}"))
        {
            this.contextMenuComponent.SubmitContextPopup(this);
            ImGui.EndPopup();
        }

        if (this.currentModal != null)
        {
            if (this.currentModal.SubmitUI(this))
            {
                this.currentModal = null;
            }
        }
    }

    public void DrawGrid()
    {
        // Get camera's position and the size of the current view
        // This depends on the zoom of the camera. Dividing by the
        // zoom gives a correct representation of the actual visible view.
        Vector2 camPos = this.editorCamera.target;
        Vector2 viewSize = UserInput.GetViewSize(this.editorCamera);

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

    public void NewComponent(Component comp)
    {
        this.simulator.AddComponent(comp);
        this.simulator.ClearSelection();
        this.simulator.SelectComponent(comp);
        this.fsm.SetState<StateMovingSelection>();
    }

    public override void Update()
    {
        Vector2 mousePosInWorld = UserInput.GetMousePositionInWorld(this.editorCamera);

        if (!ImGui.GetIO().WantCaptureMouse)
        {
            if (Raylib.GetMouseWheelMove() > 0)
            {
                this.editorCamera.zoom = 1.05F * this.editorCamera.zoom;
            }
            if (Raylib.GetMouseWheelMove() < 0)
            {
                this.editorCamera.zoom = (1f / 1.05f) * this.editorCamera.zoom;
            }
        }

        this.hoveredInput = this.simulator.GetInputFromWorldPos(mousePosInWorld);
        this.hoveredOutput = this.simulator.GetOutputFromWorldPos(mousePosInWorld);
        this.hoveredComponent = this.simulator.GetComponentFromWorldPos(mousePosInWorld);

        this.fsm.Update(this);

        if (!ImGui.GetIO().WantCaptureKeyboard)
        {
            List<List<EditorAction>> actions = this.mainMenuButtons.Select(x => x.Item2.Select(y => y.Item2).ToList()).ToList();
            foreach (List<EditorAction> lea in actions)
            {
                foreach (EditorAction ea in lea)
                {
                    ea.Update(this);
                }
            }
        }

        this.simulator.Update(mousePosInWorld);
    }

    public override void Render()
    {
        Raylib.BeginMode2D(this.editorCamera);
        Raylib.ClearBackground(Color.LIGHTGRAY);
        DrawGrid();

        Vector2 mousePosInWorld = UserInput.GetMousePositionInWorld(this.editorCamera);

        this.simulator.Render(mousePosInWorld);
        this.fsm.Render(this);

        Raylib.EndMode2D();
    }

    public void SelectFile(string startDirectory, Action<string> onSelect, params string[] filteredExtensions)
    {
        this.currentModal = new FileDialog(startDirectory, FileDialogType.SelectFile, onSelect, filteredExtensions);
    }

    public void SelectFolder(string startDirectory, Action<string> onSelect)
    {
        this.currentModal = new FileDialog(startDirectory, FileDialogType.SelectFolder, onSelect);
    }

    public override void OnClose()
    {

    }
}