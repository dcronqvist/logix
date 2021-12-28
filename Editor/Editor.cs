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
    Project loadedProject;
    Component contextMenuComponent;
    EditorFSM fsm;

    // GATES
    IGateLogic[] availableGateLogics;

    // KEY COMBINATIONS
    KeyboardKey primaryKeyMod;

    // MAIN MENU BAR ACTIONS
    List<Tuple<string, List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>>> mainMenuButtons;

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
        mainMenuButtons = new List<Tuple<string, List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>>>();
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
        AddNewMainMenuItem("File", "Save", () => true, this.primaryKeyMod, KeyboardKey.KEY_S, () =>
        {
            this.loadedProject.SaveComponentsInWorkspace(this.simulator.Components);
            this.loadedProject.SaveToFile(Directory.GetCurrentDirectory());
        });

        AddNewMainMenuItem("File", "Open Project...", () => true, null, null, () =>
        {
            base.SelectFile(Directory.GetCurrentDirectory(), (file) =>
            {
                Project p = Project.LoadFromFile(file);
                SetProject(p);
            }, () =>
            {

            }, Project.EXTENSION);
        });

        AddNewMainMenuItem("File", "Include IC from file...", () => true, null, null, () =>
        {
            // TODO: Add file dialog to pick file and include
            base.SelectFile(Directory.GetCurrentDirectory(), (file) =>
            {
                if (!this.loadedProject.IncludeICFile(file))
                {
                    base.ModalError("Could not include IC file.");
                }
            }, () =>
            {

            }, ICDescription.EXTENSION);
        });

        AddNewMainMenuItem("File", "Include IC Collection from file...", () => true, null, null, () =>
        {
            base.SelectFile(Directory.GetCurrentDirectory(), (file) =>
            {
                this.loadedProject.IncludeICCollectionFile(file);
            }, () => { }, ICCollection.EXTENSION);
        });

        AddNewMainMenuItem("Edit", "Copy", () => this.simulator.SelectedComponents.Count > 0, this.primaryKeyMod, KeyboardKey.KEY_C, MMCopy);
        AddNewMainMenuItem("Edit", "Paste", () => this.copiedCircuit != null, this.primaryKeyMod, KeyboardKey.KEY_V, MMPaste);
        AddNewMainMenuItem("Edit", "Select All", () => true, this.primaryKeyMod, KeyboardKey.KEY_A, this.simulator.SelectAllComponents);
        AddNewMainMenuItem("Integrated Circuits", "Create IC from Selection", () => this.simulator.SelectedComponents.Count > 0, this.primaryKeyMod, KeyboardKey.KEY_I, MMCreateIC);
        AddNewMainMenuItem("Integrated Circuits", "Export IC Collection from Selection", () => this.simulator.SelectedComponents.All(x => x is ICComponent), null, null, () =>
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
        });

        SetProject(new Project("unnamed-project"));
    }

    public void AddNewMainMenuItem(string mainButton, string actionButtonName, Func<bool> enabled, KeyboardKey? hold, KeyboardKey? press, Action action)
    {
        if (!this.mainMenuButtons.Exists(x => x.Item1 == mainButton))
        {
            this.mainMenuButtons.Add(new Tuple<string, List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>>(mainButton, new List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>()));
        }

        Tuple<string, List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>> mainMenuButton = this.mainMenuButtons.Find(x => x.Item1 == mainButton);

        mainMenuButton.Item2.Add(new Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>(actionButtonName, enabled, hold, press, action));
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

    public void MMCreateIC()
    {
        CircuitDescription cd = new CircuitDescription(this.simulator.SelectedComponents);

        if (cd.ValidForIC())
        {
            this.editorState = EditorState.MakingIC;
            this.copiedCircuit = cd;
            icSwitchGroup = new Dictionary<SLDescription, int>();
            icSwitches = copiedCircuit.GetSwitches();
            icSwitches = icSwitches.OrderBy(x => x.Position.Y).ToList();
            for (int i = 0; i < icSwitches.Count; i++)
            {
                icSwitchGroup.Add(icSwitches[i], i);
            }
            icLampGroup = new Dictionary<SLDescription, int>();
            icLamps = copiedCircuit.GetLamps();
            icLamps = icLamps.OrderBy(x => x.Position.Y).ToList();
            for (int i = 0; i < icLamps.Count; i++)
            {
                icLampGroup.Add(icLamps[i], i);
            }
            icName = "";
        }
        else
        {
            base.ModalError("Cannot create integrated circuit from selected components, \nsince some switches or lamps have no identifier.");
        }
    }

    public override void SubmitUI()
    {
        // MAIN MENU BAR
        ImGui.BeginMainMenuBar();

        foreach (Tuple<string, List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>> tup in this.mainMenuButtons)
        {
            if (ImGui.BeginMenu(tup.Item1))
            {
                foreach (Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action> inner in tup.Item2)
                {
                    if (ImGui.MenuItem(inner.Item1, (inner.Item3 != null && inner.Item4 != null) ? UserInput.KeyComboString(inner.Item3.Value, inner.Item4.Value) : null, false, inner.Item2()))
                    {
                        if (inner.Item5 != null)
                        {
                            inner.Item5();
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
        ImGui.Text("Gates");
        for (int i = 0; i < this.availableGateLogics.Length; i++)
        {
            CreateNewGateButton(this.availableGateLogics[i]);
        }
        ImGui.Separator();
        ImGui.Text("I/O");

        CreateNewComponentButton("Switch", true, (bits, multibit, worldPos) =>
        {
            return new Switch(bits, worldPos);
        }, 1, false);
        CreateNewComponentButton("Button", false, (bits, multibit, worldPos) =>
        {
            return new Button(bits, worldPos);
        }, 1, false);
        CreateNewComponentButton("Lamp", true, (bits, multibit, worldPos) =>
        {
            return new Lamp(bits, worldPos);
        }, 1, false);
        CreateNewComponentButton("Hex Viewer", true, (bits, multibit, worldPos) =>
        {
            return new HexViewer(bits, multibit, worldPos);
        }, 4, false);
        CreateNewROMButton();

        ImGui.Separator();
        ImGui.Text("Project ICs");

        foreach (ICDescription icd in this.loadedProject.ProjectCreatedICs.ToArray())
        {
            CreateNewICButton(icd, () =>
            {
                this.loadedProject.RemoveProjectCreatedIC(icd);
            });
        }

        ImGui.Text("Included ICs");

        foreach (ICDescription icd in this.loadedProject.ICsFromFile.ToArray())
        {
            CreateNewICButton(icd, () =>
            {
                this.loadedProject.ExcludeICFromFile(icd);
            });
        }

        ImGui.Text("Collections");

        foreach (KeyValuePair<string, ICCollection> kvp in this.loadedProject.ICCollections.ToArray())
        {
            ImGui.Text(kvp.Key);
            foreach (ICDescription icd in kvp.Value.ICs)
            {
                CreateNewICButton(icd, () =>
                {
                    this.loadedProject.ExcludeICCollection(kvp.Value);
                });
            }
        }

        ImGui.End();

        if (this.editorState == EditorState.MakingIC)
        {
            ImGui.OpenPopup("Create Integrated Circuit");
        }

        // MAKING IC WINDOW
        Vector2 windowSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        ImGui.SetNextWindowPos(windowSize / 2, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        Vector2 popupSize = new Vector2(120) * 4f;
        ImGui.SetNextWindowSizeConstraints(popupSize, popupSize);
        ImGui.SetNextWindowSize(popupSize);
        if (ImGui.BeginPopupModal("Create Integrated Circuit"))
        {
            ImGui.InputText("Circuit name", ref this.icName, 25);
            ImGui.Separator();

            ImGui.Columns(2);
            ImGui.Text("Inputs");
            ImGui.NextColumn();
            ImGui.Text("Outputs");
            ImGui.Separator();
            ImGui.NextColumn();

            for (int i = 0; i < icSwitches.Count; i++)
            {
                SLDescription sw = icSwitches[i];
                ImGui.PushID(sw.ID);
                ImGui.SetNextItemWidth(80);
                int gr = this.icSwitchGroup[sw];
                ImGui.InputInt("", ref gr, 1, 1);
                this.icSwitchGroup[sw] = gr;
                ImGui.PopID();
                ImGui.SameLine();
                int group = this.icSwitchGroup[sw];
                ImGui.Text(sw.Name);
                ImGui.SameLine();
                ImGui.PushID(sw.ID + "up");
                if (ImGui.Button("^"))
                {
                    int nNext = i - 1;
                    if (nNext >= 0)
                    {
                        icSwitches[i] = icSwitches[nNext];
                        icSwitches[nNext] = sw;
                    }
                }
                ImGui.PopID();
                ImGui.PushID(sw.ID + "down");
                ImGui.SameLine();
                if (ImGui.Button("v"))
                {
                    int nNext = i + 1;
                    if (nNext < icSwitches.Count)
                    {
                        icSwitches[i] = icSwitches[nNext];
                        icSwitches[nNext] = sw;
                    }
                }
                ImGui.PopID();
                /*
                if (ImGui.IsItemActive() && !ImGui.IsItemHovered())
                {
                    int nNext = i + (ImGui.GetMouseDragDelta(ImGuiMouseButton.Left).Y < 0f ? -1 : 1);
                    if (nNext >= 0 && nNext < icSwitches.Count)
                    {
                        icSwitches[i] = icSwitches[nNext];
                        icSwitches[nNext] = sw;
                    }
                }*/
            }

            ImGui.NextColumn();

            for (int i = 0; i < icLamps.Count; i++)
            {
                SLDescription sw = icLamps[i];
                ImGui.PushID(sw.ID);
                ImGui.SetNextItemWidth(80);
                int gr = this.icLampGroup[sw];
                ImGui.InputInt("", ref gr, 1, 1);
                this.icLampGroup[sw] = gr;
                ImGui.PopID();
                ImGui.SameLine();
                int group = this.icLampGroup[sw];
                ImGui.Text(sw.Name);
                ImGui.SameLine();
                ImGui.PushID(sw.ID + "up");
                if (ImGui.Button("^"))
                {
                    int nNext = i - 1;
                    if (nNext >= 0)
                    {
                        icLamps[i] = icLamps[nNext];
                        icLamps[nNext] = sw;
                    }
                }
                ImGui.PopID();
                ImGui.PushID(sw.ID + "down");
                ImGui.SameLine();
                if (ImGui.Button("v"))
                {
                    int nNext = i + 1;
                    if (nNext < icLamps.Count)
                    {
                        icLamps[i] = icLamps[nNext];
                        icLamps[nNext] = sw;
                    }
                }
                ImGui.PopID();
            }

            ImGui.Columns(1);
            ImGui.Separator();

            if (ImGui.Button("Create"))
            {
                if (copiedCircuit != null)
                {
                    if (copiedCircuit.ValidForIC())
                    {
                        List<List<string>> inputOrder = new List<List<string>>();
                        List<List<string>> outputOrder = new List<List<string>>();

                        int max = 0;
                        foreach (KeyValuePair<SLDescription, int> kvp in this.icSwitchGroup)
                        {
                            max = Math.Max(max, kvp.Value);
                        }

                        for (int i = 0; i <= max; i++)
                        {
                            if (this.icSwitchGroup.ContainsValue(i))
                            {
                                List<SLDescription> inGroup = this.icSwitchGroup.Where(x => x.Value == i).Select(x => x.Key).ToList();
                                inputOrder.Add(inGroup.Select(x => x.ID).ToList());
                            }
                        }

                        foreach (KeyValuePair<SLDescription, int> kvp in this.icLampGroup)
                        {
                            max = Math.Max(max, kvp.Value);
                        }

                        for (int i = 0; i <= max; i++)
                        {
                            if (this.icLampGroup.ContainsValue(i))
                            {
                                List<SLDescription> inGroup = this.icLampGroup.Where(x => x.Value == i).Select(x => x.Key).ToList();
                                outputOrder.Add(inGroup.Select(x => x.ID).ToList());
                            }
                        }

                        ICDescription icd = new ICDescription(this.icName, Vector2.Zero, copiedCircuit, inputOrder, outputOrder);

                        //this.simulator.AddComponent(icd.ToComponent());
                        this.loadedProject.AddProjectCreatedIC(icd);
                    }
                    else
                    {
                        Console.WriteLine("circuit contains unnamed switches or lamps.");
                    }
                }

                ImGui.CloseCurrentPopup();
                this.editorState = EditorState.None;
            }
            ImGui.EndPopup();
        }

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
    }

    public void CreateNewICButton(ICDescription icd, Action onExclude)
    {
        Vector2 buttonSize = new Vector2(94, 25);
        ImGui.Button(icd.Name, buttonSize);

        if (ImGui.IsItemClicked())
        {
            Component comp = icd.ToComponent(false);
            comp.Position = UserInput.GetMousePositionInWorld(this.editorCamera);
            this.NewComponent(comp);
        }

        if (ImGui.BeginPopupContextItem(icd.Name))
        {
            if (ImGui.Button("Export to file"))
            {
                base.SelectFolder(Directory.GetCurrentDirectory(), (folder) =>
                {
                    icd.SaveToFile(folder);
                }, () =>
                {

                });
            }
            if (ImGui.Button("Exclude from project"))
            {
                onExclude();
            }

            ImGui.EndPopup();
        }
    }

    public void CreateNewComponentButton(string text, bool multibitPop, Func<int, bool, Vector2, Component> createComponent, int defaultBits, bool defaultMultibit)
    {
        Vector2 buttonSize = new Vector2(94, 25);
        ImGui.Button(text, buttonSize);
        if (ImGui.IsItemClicked())
        {
            // Create new component
            Component comp = createComponent(defaultBits, defaultMultibit, UserInput.GetMousePositionInWorld(this.editorCamera));
            this.NewComponent(comp);
        }

        if (multibitPop)
        {
            if (ImGui.BeginPopupContextItem(text))
            {
                ImGui.SetNextItemWidth(80);
                ImGui.InputInt("Bits", ref newComponentBits);
                ImGui.Checkbox("Multibit", ref newComponentMultibit);
                ImGui.Separator();
                ImGui.Button("Create");

                if (ImGui.IsItemClicked())
                {
                    // Create new component
                    Component comp = createComponent(newComponentBits, newComponentMultibit, UserInput.GetMousePositionInWorld(this.editorCamera));
                    this.NewComponent(comp);
                }

                ImGui.EndPopup();
            }
        }
    }

    public void CreateNewROMButton()
    {
        Vector2 buttonSize = new Vector2(94, 25);
        ImGui.Button("ROM", buttonSize);
        if (ImGui.IsItemClicked())
        {
            // Create new component
            ROM r = new ROM(false, 4, false, 4, UserInput.GetMousePositionInWorld(this.editorCamera));
            this.NewComponent(r);
        }

        if (ImGui.BeginPopupContextItem("ROM"))
        {
            ImGui.SetNextItemWidth(80);
            ImGui.InputInt("Addressable Bits", ref newComponentBits);
            ImGui.Checkbox("Address Multibit", ref newComponentMultibit);
            ImGui.SetNextItemWidth(80);
            ImGui.InputInt("Output Bits", ref newRomOutputbits);
            ImGui.Checkbox("Output Multibit", ref newRomOutputMultibit);
            ImGui.Separator();
            ImGui.Button("Create");

            if (ImGui.IsItemClicked())
            {
                // Create new component
                ROM comp = new ROM(newComponentMultibit, newComponentBits, newRomOutputMultibit, newRomOutputbits, UserInput.GetMousePositionInWorld(this.editorCamera));
                this.NewComponent(comp);
            }

            ImGui.EndPopup();
        }
    }

    public void CreateNewGateButton(IGateLogic logic)
    {
        CreateNewComponentButton(logic.GetLogicText(), logic.AllowMultibit(), (bits, multibit, worldPos) =>
        {
            return new LogicGate(bits, multibit, logic, worldPos);
        }, logic.DefaultBits(), false);
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
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE))
            {
                this.simulator.DeleteSelection();
            }

            foreach (Tuple<string, List<Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action>>> tup in this.mainMenuButtons)
            {
                foreach (Tuple<string, Func<bool>, KeyboardKey?, KeyboardKey?, Action> inner in tup.Item2)
                {
                    if (inner.Item3.HasValue && inner.Item4.HasValue)
                    {
                        if (UserInput.KeyComboPressed(inner.Item3.Value, inner.Item4.Value) && inner.Item2())
                        {
                            if (inner.Item5 != null)
                            {
                                inner.Item5();
                            }
                        }
                    }
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
}