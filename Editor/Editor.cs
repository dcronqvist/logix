using LogiX.Components;
using LogiX.SaveSystem;
using Newtonsoft.Json;

namespace LogiX.Editor;

public class Editor : Application
{
    // Editor states
    public Camera2D editorCamera;
    EditorState editorState;
    public static Simulator simulator;
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
    bool displayDebugWindow;

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
        this.displayDebugWindow = false;
    }

    public void SetProject(Project proj)
    {
        try
        {
            if (this.loadedProject == null)
            {
                this.loadedProject = proj;
                (List<Component> comps, List<Wire> wires) = proj.GetComponentsAndWires();
                simulator.AddComponents(comps);
                simulator.AddWires(wires);
            }
            else
            {
                // There is an already ongoing project, save it

                this.loadedProject = proj;
                simulator.ClearSelection();
                simulator.Components.Clear();
                simulator.Wires.Clear();
                (List<Component> comps, List<Wire> wires) = proj.GetComponentsAndWires();
                simulator.AddComponents(comps);
                simulator.AddWires(wires);
            }

            Raylib.SetWindowTitle("LogiX - " + proj.GetFileName());
        }
        catch (Exception e)
        {
            base.ModalError(e.Message);
        }
        this.LoadComponentButtons();
    }

    public override void LoadContent()
    {
        Vector2 windowSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        this.editorCamera = new Camera2D(windowSize / 2.0f, Vector2.Zero, 0f, 1f);
        this.editorState = EditorState.None;

        simulator = new Simulator();

        Util.OpenSans = Raylib.LoadFontEx($"{Directory.GetCurrentDirectory()}/assets/opensans-bold.ttf", 100, Enumerable.Range(0, 1000).ToArray(), 1000);
        Raylib.SetTextureFilter(Util.OpenSans.texture, TextureFilter.TEXTURE_FILTER_TRILINEAR);

        this.fsm = new EditorFSM();

        // LOAD ALL PLUGINS
        Plugin.TryLoadAllPlugins(out List<Plugin> plugins, out Dictionary<string, string> failedPlugins);
        Util.Plugins = plugins;
        if (failedPlugins.Count > 0)
        {
            string error = "";
            foreach (KeyValuePair<string, string> kvp in failedPlugins)
            {
                error += $"{kvp.Key} failed to load: {kvp.Value}\n";
            }
            base.ModalError(error);
        }

        // ASSIGNING KEYCOMBO ACTIONS
        AddNewMainMenuItem("File", "Save", new EditorAction((editor) => true, (editor) => false, (Editor editor, out string error) =>
        {
            if (this.loadedProject.HasFile())
            {
                this.loadedProject.SaveComponentsInWorkspace(simulator.Components);
                this.loadedProject.SaveToFile(this.loadedProject.LoadedFromFile);
                error = ""; return true;
            }
            else
            {
                this.SaveFile(Util.FileDialogStartDir, (filePath) =>
                {
                    this.loadedProject.SaveToFile(filePath);
                    Settings.SetSetting<string>("latestProject", this.loadedProject.LoadedFromFile);
                    Settings.SaveSettings();
                }, Project.EXTENSION);
                error = "";
                return true;
            }

        }, this.primaryKeyMod, KeyboardKey.KEY_S));

        AddNewMainMenuItem("File", "New Project", new EditorAction((editor) => true, (editor) => false, (Editor editor, out string error) =>
        {
            // Should maybe save current project before?
            SetProject(new Project());
            error = "";
            return true;
        }));

        AddNewMainMenuItem("File", "Open Project", new EditorAction((editor) => true, (editor) => false, (Editor editor, out string error) =>
        {
            this.SelectFile(Util.FileDialogStartDir, (file) =>
            {
                Project p = Project.LoadFromFile(file);
                Settings.SetSetting<string>("latestProject", p.LoadedFromFile);
                Settings.SaveSettings();
                SetProject(p);
            }, Project.EXTENSION);
            error = ""; return true;
        }, this.primaryKeyMod, KeyboardKey.KEY_O));

        AddNewMainMenuItem("File", "Include IC File", new EditorAction((editor) => true, (editor) => false, (Editor editor, out string error) =>
        {
            this.SelectFile(Util.FileDialogStartDir, (file) =>
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

        AddNewMainMenuItem("File", "Include IC Collection", new EditorAction((editor) => true, (editor) => false, (Editor editor, out string error) =>
        {
            this.SelectFile(Util.FileDialogStartDir, (file) =>
            {
                this.loadedProject.IncludeICCollectionFile(file);
                this.LoadComponentButtons();
            }, ICCollection.EXTENSION);
            error = ""; return true;
        }, this.primaryKeyMod, KeyboardKey.KEY_I, KeyboardKey.KEY_C));

        AddNewMainMenuItem("Edit", "Copy", new EditorAction((editor) => simulator.SelectedComponents.Count > 0, (editor) => false, (Editor editor, out string error) => { MMCopy(); error = ""; return true; }, this.primaryKeyMod, KeyboardKey.KEY_C));
        AddNewMainMenuItem("Edit", "Paste", new EditorAction((editor) => this.copiedCircuit != null, (editor) => false, (Editor editor, out string error) => { MMPaste(); error = ""; return true; }, this.primaryKeyMod, KeyboardKey.KEY_V));
        AddNewMainMenuItem("Edit", "Select All", new EditorAction((editor) => true, (editor) => false, (Editor editor, out string error) => { simulator.SelectAllComponents(); error = ""; return true; }, this.primaryKeyMod, KeyboardKey.KEY_A));
        AddNewMainMenuItem("Edit", "Delete Selection", new EditorAction((editor) => simulator.SelectedComponents.Count > 0, (editor) => false, (Editor editor, out string error) => { simulator.DeleteSelection(); error = ""; return true; }, KeyboardKey.KEY_BACKSPACE));
        AddNewMainMenuItem("Edit", "Horizontally Align", new EditorAction((editor) => simulator.SelectedComponents.Count > 0, (editor) => false, (Editor editor, out string error) =>
        {
            List<Component> selected = simulator.SelectedComponents;
            Vector2 middle = Util.GetMiddleOfListOfVectors(selected.Select(c => c.Position).ToList());
            foreach (Component c in selected)
            {
                c.Position = new Vector2(middle.X, c.Position.Y);
            }
            error = "";
            return true;
        }));
        AddNewMainMenuItem("Edit", "Vertically Align", new EditorAction((editor) => simulator.SelectedComponents.Count > 0, (editor) => false, (Editor editor, out string error) =>
        {
            List<Component> selected = simulator.SelectedComponents;
            Vector2 middle = Util.GetMiddleOfListOfVectors(selected.Select(c => c.Position).ToList());
            foreach (Component c in selected)
            {
                c.Position = new Vector2(c.Position.X, middle.Y);
            }
            error = "";
            return true;
        }));
        AddNewMainMenuItem("Edit", "Automatically Name Selected IOs", new EditorAction((editor) => simulator.SelectedComponents.Count > 0 && (simulator.SelectedComponents.All(x => x is Switch) || simulator.SelectedComponents.All(x => x is Lamp)), (editor) => false, (Editor editor, out string error) =>
        {
            List<Component> selected = simulator.SelectedComponents;
            List<Component> orderedByY = selected.OrderBy(c => c.Position.Y).ToList();
            // Lower Y gets index 0, higher Y gets highest index
            for (int i = 0; i < orderedByY.Count; i++)
            {
                if (orderedByY[i] is Switch)
                {
                    ((Switch)orderedByY[i]).ID += i.ToString();
                }
                else if (orderedByY[i] is Lamp)
                {
                    ((Lamp)orderedByY[i]).ID += i.ToString();
                }
            }

            error = "";
            return true;
        }, this.primaryKeyMod, KeyboardKey.KEY_N));
        AddNewMainMenuItem("Integrated Circuits", "Create IC from Selection", new EditorAction((editor) => simulator.SelectedComponents.Count > 0, (editor) => false, (Editor editor, out string error) =>
        {
            CircuitDescription cd = new CircuitDescription(simulator.SelectedComponents);

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
        }, this.primaryKeyMod, KeyboardKey.KEY_I));
        AddNewMainMenuItem("View", "Debug Window", new EditorAction((editor) => true, (editor) => editor.displayDebugWindow, (Editor editor, out string error) =>
        {
            this.displayDebugWindow = !this.displayDebugWindow;
            error = ""; return true;
        }));

        // Initial project based on setting latestProject
        string latestProject = Settings.GetSetting("latestProject").GetValue<string>();
        if (latestProject != null && latestProject != "" && File.Exists(latestProject))
        {
            Project p = Project.LoadFromFile(latestProject);
            SetProject(p);
        }
        else
        {
            SetProject(new Project());
        }
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
        }, "Address Bits", "Address Multibit", "Data Bits", "Data Multibit"));
        this.AddNewComponentCreationContext("I/O", "Constant", () => { return new ConstantComponent(LogicValue.HIGH, UserInput.GetMousePositionInWorld(editorCamera)); }, null);
        this.AddNewComponentCreationContext("I/O", "Clock", () => { return new Clock(500, UserInput.GetMousePositionInWorld(editorCamera)); }, null);
        this.AddNewComponentCreationContext("Common", "Memory", () => { return new MemoryComponent(4, false, 8, false, UserInput.GetMousePositionInWorld(editorCamera)); }, new CCPUSimple(true, true, true, true, (ib, im, ob, om) =>
        {
            return new MemoryComponent(ib, im, ob, om, UserInput.GetMousePositionInWorld(editorCamera));
        }, "Address Bits", "Address Multibit", "Data Bits", "Data Multibit"));
        this.AddNewComponentCreationContext("Common", "Label", () => { return new TextComponent(UserInput.GetMousePositionInWorld(editorCamera)); }, null);
        this.AddNewComponentCreationContext("Common", "Splitter", () => { return new Splitter(2, 2, true, false, UserInput.GetMousePositionInWorld(editorCamera)); }, new CCPUSimple(true, true, true, true, (ib, im, ob, om) =>
        {
            return new Splitter(ib, ob, im, om, UserInput.GetMousePositionInWorld(editorCamera));
        }));
        this.AddNewComponentCreationContext("Common", "Delayer", () => { return new Delayer(100, 1, false, UserInput.GetMousePositionInWorld(editorCamera)); }, new CCPUSimple(true, true, false, false, (ib, im, _, _) =>
        {
            return new Delayer(100, ib, im, UserInput.GetMousePositionInWorld(editorCamera));
        }));
        this.AddNewComponentCreationContext("Common", "Multiplexer", () =>
        {
            return new Multiplexer(2, true, 4, true, UserInput.GetMousePositionInWorld(editorCamera));
        }, new CCPUSimple(true, true, true, true, (ib, im, ob, om) =>
        {
            return new Multiplexer(ib, im, ob, om, UserInput.GetMousePositionInWorld(editorCamera));
        }, "Selector Bits", "Selector Multibit", "Data Bits", "Data Multibit"));
        this.AddNewComponentCreationContext("Common", "Demultiplexer", () =>
        {
            return new Demultiplexer(2, true, 4, true, UserInput.GetMousePositionInWorld(editorCamera));
        }, new CCPUSimple(true, true, true, true, (ib, im, ob, om) =>
        {
            return new Demultiplexer(ib, im, ob, om, UserInput.GetMousePositionInWorld(editorCamera));
        }, "Selector Bits", "Selector Multibit", "Data Bits", "Data Multibit"));
        this.AddNewComponentCreationContext("Common", "Dec to Bin", () =>
        {
            return new DTBC(16, true, UserInput.GetMousePositionInWorld(editorCamera));
        }, new CCPUSimple(true, true, false, false, (ib, im, _, _) =>
        {
            return new DTBC(ib, im, UserInput.GetMousePositionInWorld(editorCamera));
        }, "Decimals", "Multibit"));

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

        // PLUGINS
        foreach (Plugin p in Util.Plugins)
        {
            foreach (KeyValuePair<string, CustomDescription> cds in p.customComponents)
            {
                this.AddNewComponentCreationContext("Plugins", cds.Value.Plugin + ":" + cds.Value.ComponentName, () => { return p.CreateComponent(cds.Key, UserInput.GetMousePositionInWorld(editorCamera)); }, null);
            }
        }

        // ICs
        // Project created ICs
        foreach (ICDescription icd in this.loadedProject.ProjectCreatedICs.ToArray())
        {
            this.AddNewComponentCreationContext("Project ICs", icd.Name, () => { return icd.ToComponent(false).SetPosition(UserInput.GetMousePositionInWorld(editorCamera)); },
            new CCPUIC(icd, (desc) =>
            {
                this.loadedProject.RemoveProjectCreatedIC(icd);
                this.LoadComponentButtons();
            }));
        }
        // Included single IC files
        foreach (ICDescription icd in this.loadedProject.ICsFromFile.ToArray())
        {
            this.AddNewComponentCreationContext("Included ICs", icd.Name, () => { return icd.ToComponent(false).SetPosition(UserInput.GetMousePositionInWorld(editorCamera)); },
            new CCPUIC(icd, (desc) =>
            {
                this.loadedProject.ExcludeICFromFile(icd);
                this.LoadComponentButtons();
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
                    this.LoadComponentButtons();
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

            if (ImGui.TreeNodeEx(cat, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.NoTreePushOnOpen))
            {
                foreach (string component in compsInCategory)
                {
                    Tuple<Func<Component>, IUISubmitter<bool, Editor>?> context = this.componentCreationContexts[component];

                    Vector2 buttonSize = new Vector2(120, 25);
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
        copiedCircuit = new CircuitDescription(simulator.SelectedComponents);
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
        try
        {
            (List<Component> comps, List<Wire> wires) = cd.CreateComponentsAndWires(pos, preserveIDs);
            simulator.AddComponents(comps);
            simulator.AddWires(wires);

            simulator.ClearSelection();

            foreach (Component c in comps)
            {
                simulator.SelectComponent(c);
            }
        }
        catch (Exception e)
        {
            base.ModalError(e.Message, ErrorModalType.OK);
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
                    if (ImGui.MenuItem(inner.Item1, (inner.Item2.HasKeys()) ? inner.Item2.GetShortcutString() : null, inner.Item2.Selected(this), (inner.Item2.Condition(this))))
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

        if (ImGui.BeginMenu("Project"))
        {
            if (ImGui.Button("Reload ICs"))
            {
                this.loadedProject.ReloadProjectICs();
                this.LoadComponentButtons();
            }

            ImGui.Text("Project file: " + this.loadedProject.LoadedFromFile);

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Plugins"))
        {
            // Import new plugin
            if (ImGui.MenuItem("Install plugin from file..."))
            {
                this.SelectFile(Util.FileDialogStartDir, (file) =>
                {
                    if (!Plugin.TryInstall(file, out List<Plugin> newPluginList, out string? error))
                    {
                        base.ModalError(error!);
                    }
                    Util.Plugins = newPluginList;
                    this.SetProject(this.loadedProject);
                }, ".zip");
            }

            if (Util.Plugins.Count > 0 && ImGui.BeginMenu("Uninstall plugin"))
            {
                foreach (Plugin p in Util.Plugins)
                {
                    if (ImGui.MenuItem(p.name + ", " + p.version))
                    {
                        base.ModalError("Are you sure you want to uninstall " + p.name + ", v" + p.version + "?", ErrorModalType.YesNo, (result) =>
                        {
                            if (result == ErrorModalResult.Yes)
                            {
                                File.Delete(p.file);
                                Plugin.TryLoadAllPlugins(out List<Plugin> plugins, out Dictionary<string, string> failedPlugins);
                                Util.Plugins = plugins;
                                if (failedPlugins.Count > 0)
                                {
                                    string error = "";
                                    foreach (KeyValuePair<string, string> kvp in failedPlugins)
                                    {
                                        error += $"{kvp.Key} failed to load: {kvp.Value}\n";
                                    }
                                    base.ModalError(error);
                                }
                                this.SetProject(this.loadedProject);
                            }
                        });
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            // Show list of plugins
            foreach (Plugin plugin in Util.Plugins)
            {
                if (ImGui.BeginMenu(plugin.name + ", " + plugin.version))
                {
                    if (ImGui.MenuItem("About"))
                    {
                        this.ModalError(plugin.GetAboutInfo());
                    }
                    ImGui.Separator();
                    // Here we do all the plugin methods
                    foreach (KeyValuePair<string, PluginMethod> methods in plugin.customMethods)
                    {
                        if (ImGui.MenuItem(methods.Key))
                        {
                            methods.Value.OnRun(this);
                        }
                    }

                    ImGui.EndMenu();
                }
            }

            ImGui.EndMenu();
        }

        ImGui.EndMainMenuBar();

        // COMPONENTS WINDOW

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.SetNextWindowPos(new Vector2(0, 22), ImGuiCond.Always);
        float sidebarWidth = 150;
        ImGui.SetNextWindowSize(new Vector2(sidebarWidth, base.WindowSize.Y - 19), ImGuiCond.Always);
        ImGui.Begin("Components", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings);
        ImGui.PopStyleVar();
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
        if (this.displayDebugWindow && ImGui.Begin("Debug stuff", ref this.displayDebugWindow))
        {
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
            ImGui.Text("IO Any Item Active:");
            ImGui.Text(ImGui.IsAnyItemActive().ToString());
            ImGui.Text("IO Want Mouse:");
            ImGui.Text(ImGui.GetIO().WantCaptureMouse.ToString());
            ImGui.Text($"FSM Current State: {this.fsm.CurrentState?.GetType().Name}");
            ImGui.Text("Selected Components Gate Count:");
            Dictionary<string, int> gateCounts = new Dictionary<string, int>();
            foreach (Component c in simulator.SelectedComponents)
            {
                gateCounts = Util.ConcatGateAmounts(gateCounts, c.GetGateAmount());
            }
            foreach (KeyValuePair<string, int> kvp in gateCounts.OrderByDescending(x => x.Value))
            {
                ImGui.Text($"{kvp.Key}: {kvp.Value}");
            }

            ImGui.Separator();
            ImGui.Text("Settings");

            Dictionary<string, Setting> settings = Settings.GetAllSettings();
            foreach (KeyValuePair<string, Setting> setting in settings)
            {
                ImGui.Text(setting.Key + ": " + setting.Value.Value.ToString());
            }

            ImGui.End();
        }

        // If single selecting a component
        if (simulator.SelectedComponents.Count == 1)
        {
            Component c = simulator.SelectedComponents[0];
            c.OnSingleSelectedSubmitUI();
        }

        if (!ImGui.GetIO().WantCaptureMouse)
        {
            // On right clicking component, open its context menu
            foreach (Component c in simulator.Components)
            {
                if (c.HasContextMenu && ImGui.IsMouseClicked(ImGuiMouseButton.Right) && Raylib.CheckCollisionPointRec(UserInput.GetMousePositionInWorld(this.editorCamera), c.Box))
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
        simulator.AddComponent(comp);
        simulator.ClearSelection();
        simulator.SelectComponent(comp);
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

        this.hoveredInput = simulator.GetInputFromWorldPos(mousePosInWorld);
        this.hoveredOutput = simulator.GetOutputFromWorldPos(mousePosInWorld);
        this.hoveredComponent = simulator.GetComponentFromWorldPos(mousePosInWorld);

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

        simulator.Update(mousePosInWorld);
    }

    public override void Render()
    {
        Raylib.BeginMode2D(this.editorCamera);
        Raylib.ClearBackground(Color.LIGHTGRAY);
        DrawGrid();

        Vector2 mousePosInWorld = UserInput.GetMousePositionInWorld(this.editorCamera);

        simulator.Render(mousePosInWorld);
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

    public void SaveFile(string startDirectory, Action<string> onSelect, params string[] filteredExtensions)
    {
        this.currentModal = new FileDialog(startDirectory, FileDialogType.SaveFile, onSelect, filteredExtensions);
    }

    public override void OnClose()
    {

    }
}