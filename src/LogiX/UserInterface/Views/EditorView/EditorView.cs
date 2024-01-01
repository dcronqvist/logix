using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DotGL;
using DotGLFW;
using ImGuiNET;
using LogiX.Content;
using LogiX.Graphics;
using LogiX.Graphics.Cameras;
using LogiX.Graphics.Text;
using LogiX.Input;
using LogiX.UserInterfaceContext;
using Symphony;
using LogiX.Model.Circuits;
using LogiX.Model.NodeModel;
using LogiX.Model.Simulation;
using LogiX.UserInterface.Coroutines;
using LogiX.Model.Projects;
using LogiX.Model.Commands;
using LogiX.UserInterface.Actions;

namespace LogiX.UserInterface.Views.EditorView;

public partial class EditorView : IView
{
    private readonly IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> _userInterfaceContext;
    private readonly IMouse<MouseButton> _mouse;
    private readonly IKeyboard<char, Keys, ModifierKeys> _keyboard;
    private readonly IContentManager<ContentMeta> _contentManager;
    private readonly IRenderer _renderer;
    private readonly ICoroutineService _coroutineService;
    private readonly ICoroutineService _guiCoroutineService;
    private readonly IProjectService _projectService;

    private readonly INodeUIHandlerConfigurer _nodeUIHandlerConfigurer;
    private IThreadSafe<ISimulator> _simulator;
    private IThreadSafe<ICircuitDefinition> _currentlySimulatedCircuitDefinition;
    private CircuitDefinitionViewModel _currentlySimulatedCircuitViewModel;

    private readonly INodePresenter _presentation;
    private Invoker _invoker;

    private ICamera2D _camera;
    private Vector2 _cameraFocusPosition;
    private float _cameraZoom;
    private readonly int _gridSize = 20;

    public EditorView(
        IUserInterfaceContext<InputState, Keys, ModifierKeys, MouseButton> userInterfaceContext,
        IMouse<MouseButton> mouse,
        IKeyboard<char, Keys, ModifierKeys> keyboard,
        IContentManager<ContentMeta> contentManager,
        IRenderer renderer,
        IProjectService projectService)
    {
        _userInterfaceContext = userInterfaceContext;
        _mouse = mouse;
        _keyboard = keyboard;
        _contentManager = contentManager;
        _renderer = renderer;
        _coroutineService = new CoroutineService();
        _guiCoroutineService = new CoroutineService();

        _projectService = projectService;

        SetupCamera();
        var font = _contentManager.GetContent<Font>("logix-core:fonts/firacode.font");
        _renderer.Text.PushFont(font);
        _invoker = new Invoker();
        _presentation = new SimplePresentation(renderer, GetMousePositionInWorkspace, mouse, font);
        _nodeUIHandlerConfigurer = new NodeUIHandlerConfigurer(keyboard, mouse, GetMousePositionInWorkspace);

        var loadedProject = _projectService.LoadProjectFromDisk("test.json");
        _projectService.SetProject(loadedProject);
        var mainCircuit = loadedProject.GetProjectCircuitTree().RecursivelyGetFileContents("main");

        // Task.Run(async () =>
        // {
        //     while (true)
        //     {
        //         if (!_running)
        //         {
        //             await Task.Delay(100);
        //             continue;
        //         }
        //         else
        //         {
        //             _simulator.Locked(sim => sim.PerformSimulationStep());
        //         }

        //         await Task.Delay(100);
        //     }
        // });
    }

    // private void OpenCircuit(ICircuitDefinition circuitDefinition)
    // {
    //     _currentlySimulatedCircuitDefinition = new ThreadSafe<ICircuitDefinition>(circuitDefinition);
    //     var simulator = _currentlySimulatedCircuitDefinition.Locked(cd =>
    //     {
    //         var simulator = new Simulator(cd, _nodeUIHandlerConfigurer);
    //         cd.Subscribe(simulator);
    //         return simulator;
    //     });
    //     _simulator = new ThreadSafe<ISimulator>(simulator);
    //     _currentlySimulatedCircuitViewModel = new CircuitDefinitionViewModel(_currentlySimulatedCircuitDefinition, _simulator);

    //     _invoker = new Invoker();
    // }

    private Vector2 GetMousePositionInWorkspace()
    {
        var mousePos = _mouse.GetMousePositionInWindowAsVector2();
        var topLeftOfCamera = new Vector2(_camera.VisibleArea.Left, _camera.VisibleArea.Top);
        var mousePosInCameraView = topLeftOfCamera + (mousePos / _cameraZoom);
        return mousePosInCameraView;
    }

    private Vector2i GetGridAlignedMousePositionInWorkspace()
    {
        var mousePos = GetMousePositionInWorkspace();
        // Round to nearest grid position

        var mousePosGridAligned = new Vector2i((int)MathF.Round(mousePos.X / _gridSize), (int)MathF.Round(mousePos.Y / _gridSize));
        return mousePosGridAligned;
    }

    private void SetupCamera()
    {
        var userInterfaceContextWindowSizeProvider = new ComputedValue<Vector2>(_userInterfaceContext.GetWindowSizeAsVector2);

        _cameraFocusPosition = _userInterfaceContext.GetWindowSizeAsVector2() / 2f;
        _cameraZoom = 1f;
        _camera = new Camera2D(
            userInterfaceContextWindowSizeProvider,
            new ComputedValue<Vector2>(() => _cameraFocusPosition),
            new ComputedValue<float>(() => _cameraZoom));

        _mouse.MouseWheelScrolled += (sender, e) =>
        {
            if (ImGui.GetIO().WantCaptureMouse)
                return;

            _cameraZoom *= e > 0 ? 1.1f : (1f / 1.1f);

            var mousePosInWindow = _mouse.GetMousePositionInWindowAsVector2();
            var windowSize = _userInterfaceContext.GetWindowSizeAsVector2();
            var topLeftOfCamera = new Vector2(_camera.VisibleArea.Left, _camera.VisibleArea.Top);
            var mousePosInCameraView = topLeftOfCamera + (mousePosInWindow / _cameraZoom);
            float moveFactor = 0.1f;
            _cameraFocusPosition += (mousePosInCameraView - _cameraFocusPosition) * (e > 0 ? moveFactor : -moveFactor);
        };
    }

    public void Update(float deltaTime, float totalTime)
    {
        _coroutineService.Update(deltaTime);

        if (_currentlySimulatedCircuitDefinition == null)
            return;

        _simulator.Locked(sim => sim.PerformSimulationStep());

        _userInterfaceContext.SetWindowTitle($"LogiX - {_projectService.GetCurrentProject().GetProjectMetadata().Name}");

        if (ImGui.GetIO().WantCaptureMouse || ImGui.GetIO().WantCaptureKeyboard)
            return;

        bool leftClicked = _mouse.IsMouseButtonPressed(MouseButton.Left);
        bool rightClicked = _mouse.IsMouseButtonPressed(MouseButton.Right);
        var mousePos = GetMousePositionInWorkspace();
        bool isHoveringAnyNode = _currentlySimulatedCircuitViewModel.IsPositionOnAnyNode(mousePos, out var hoveredNode);
        bool isHoveringAnyNodePin = _currentlySimulatedCircuitViewModel.IsPositionOnAnyPin(mousePos, out var hoveredNodePin);
        bool isHoveringAnyWireSegment = _currentlySimulatedCircuitViewModel.IsPositionOnAnyWireSegment(mousePos, out var hoveredWireSegment, out var hoveredSignal);
        bool isHoveringAnyWireSegmentPoint = _currentlySimulatedCircuitViewModel.IsPositionOnAnyWireSegmentPoint(mousePos, out var hoveredSegmentPoint, out var adjacentSegments);

        if (leftClicked && isHoveringAnyNodePin)
        {
            _coroutineService.Run(DrawWire());
        }
        else if (leftClicked && isHoveringAnyNode)
        {
            bool isHoveredNodeSelected = _currentlySimulatedCircuitViewModel.IsNodeSelected(hoveredNode);

            if (!isHoveredNodeSelected)
            {
                _currentlySimulatedCircuitViewModel.ClearSelectedNodes();
                _currentlySimulatedCircuitViewModel.SelectNode(hoveredNode);
            }
            _coroutineService.Run(MoveSelection());
        }
        else if (leftClicked && isHoveringAnyWireSegment)
        {
            _coroutineService.Run(DrawWire());
        }
        else if (leftClicked && !isHoveringAnyNode && !isHoveringAnyWireSegment && !isHoveringAnyWireSegmentPoint)
        {
            _coroutineService.Run(RectangleSelection());
        }

        if (rightClicked && isHoveringAnyWireSegmentPoint)
        {
            _guiCoroutineService.Run(WireSegmentPointContextMenu(hoveredSegmentPoint, adjacentSegments));
        }
        else if (rightClicked && isHoveringAnyWireSegment)
        {
            _guiCoroutineService.Run(WireSegmentContextMenu(hoveredWireSegment, hoveredSignal));
        }

        if (_mouse.IsMouseButtonDown(MouseButton.Middle))
        {
            _cameraFocusPosition -= _mouse.GetMouseDeltaInWindowAsVector2() / _cameraZoom;
        }

        if (_keyboard.IsKeyCombinationPressed(Keys.LeftControl, Keys.Right))
        {
            _coroutineService.Run(RotateSelectedNodes(-1));
        }
        else if (_keyboard.IsKeyCombinationPressed(Keys.LeftControl, Keys.Left))
        {
            _coroutineService.Run(RotateSelectedNodes(1));
        }

        if (_keyboard.IsKeyPressed(Keys.Backspace))
        {
            _coroutineService.Run(DeleteSelection());
        }
    }

    private void IssueCommand(ICommand command) => _invoker.ExecuteCommand(command);

    public void Render(float deltaTime, float totalTime)
    {
        var backgroundColor = _currentlySimulatedCircuitDefinition is null ? ColorF.Darken(ColorF.LightGray, 0.2f) : ColorF.Darken(ColorF.LightGray, 0.7f);
        GL.glClearColor(backgroundColor.R, backgroundColor.G, backgroundColor.B, backgroundColor.A);
        GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);

        GL.glEnable(GL.GL_BLEND);
        GL.glBlendFunc(GL.GL_ONE, GL.GL_ONE_MINUS_SRC_ALPHA);
        var pShader = _contentManager.GetContent<ShaderProgram>("logix-core:shaders/primitives.shader");
        var font = _contentManager.GetContent<Font>("logix-core:fonts/firacode.font");

        if (_currentlySimulatedCircuitDefinition != null)
        {
            DrawGrid(_camera);

            var mousePos = GetMousePositionInWorkspace();
            bool isHoveringAnyWireSegment = _currentlySimulatedCircuitViewModel.IsPositionOnAnyWireSegment(mousePos, out var hoveredWireSegment, out var hoveredSignal);

            _currentlySimulatedCircuitViewModel.Render(
                _presentation,
                pShader,
                font,
                _renderer,
                _gridSize,
                _camera,
                hoveredWireSegment
            );
        }
        _coroutineService.Render(_renderer, deltaTime, totalTime);
        _renderer.Primitives.FinalizeRender(pShader, _camera);
        _renderer.Text.FinalizeRender(font, _camera);
    }

    private void DrawGrid(ICamera2D camera)
    {
        // Get camera's position and the size of the current view
        // This depends on the zoom of the camera. Dividing by the
        // zoom gives a correct representation of the actual visible view.
        var viewSize = camera.VisibleArea.Size.ToVector2();
        var camPos = camera.FocusPosition;

        int pixelsInBetweenLines = _gridSize;

        var color = ColorF.Darken(ColorF.LightGray, 0.63f);

        // Draw vertical lines
        for (int i = (int)((camPos.X - (viewSize.X / 2.0F)) / pixelsInBetweenLines); i < ((camPos.X + (viewSize.X / 2.0F)) / pixelsInBetweenLines); i++)
        {
            int lineX = i * pixelsInBetweenLines;
            int lineYstart = (int)(camPos.Y - (viewSize.Y / 2.0F));
            int lineYend = (int)(camPos.Y + (viewSize.Y / 2.0F));

            _renderer.Primitives.RenderLine(new Vector2(lineX, lineYstart), new Vector2(lineX, lineYend), 1, color);
        }

        // Draw horizontal lines
        for (int i = (int)((camPos.Y - (viewSize.Y / 2.0F)) / pixelsInBetweenLines); i < ((camPos.Y + (viewSize.Y / 2.0F)) / pixelsInBetweenLines); i++)
        {
            int lineY = i * pixelsInBetweenLines;
            int lineXstart = (int)(camPos.X - (viewSize.X / 2.0F));
            int lineXend = (int)(camPos.X + (viewSize.X / 2.0F));

            _renderer.Primitives.RenderLine(new Vector2(lineXstart, lineY - 0.5f), new Vector2(lineXend, lineY - 0.5f), 1, color);
        }
    }

    public void SubmitGUI(float deltaTime, float totalTime)
    {
        SubmitTopMenuBar(out var topBarSize);
        SubmitBottomMenuBar(out var bottomBarSize);
        SubmitSideBar(new Vector2(0, topBarSize.Y), _userInterfaceContext.GetWindowHeight() - topBarSize.Y - bottomBarSize.Y, out float sidebarWidth);

        _currentlySimulatedCircuitViewModel?.SubmitGUI(_invoker);

        _guiCoroutineService.Update(deltaTime);
    }

    private bool _running = true;
    private void SubmitTopMenuBar(out Vector2 menuBarSize)
    {
        ImGui.BeginMainMenuBar();
        menuBarSize = ImGui.GetWindowSize();

        var availableEditorActionsTree = _projectService.GetCurrentProject().GetAvailableEditorActionsTree();

        void submitTreeNode(IVirtualFileTree<string, IEditorAction> node)
        {
            if (ImGui.BeginMenu($"{node.Directory}"))
            {
                foreach (var child in node.GetDirectories())
                {
                    submitTreeNode(child);
                }

                foreach (var (menuItem, menuAction) in node.GetFiles())
                {
                    bool canExecute = menuAction.CanExecute(_invoker, _currentlySimulatedCircuitViewModel);
                    bool isSelected = menuAction.IsSelected(_invoker, _currentlySimulatedCircuitViewModel);
                    string shortCutString = GetShortcutString(menuAction.GetShortcut());
                    if (ImGui.MenuItem(menuItem, shortCutString, isSelected, canExecute))
                    {
                        menuAction.Execute(_invoker, _currentlySimulatedCircuitViewModel);
                    }
                }

                ImGui.EndMenu();
            }
        }

        foreach (var child in availableEditorActionsTree.GetDirectories())
        {
            submitTreeNode(child);
        }

        ImGui.EndMainMenuBar();
    }

    private static string GetShortcutString(IEnumerable<Keys> shortcut) => string.Join(" + ", shortcut.Select(k => k.ToString()));

    private void SubmitBottomMenuBar(out Vector2 menuBarSize)
    {
        float menuBarHeight = ImGui.GetFontSize() + (ImGui.GetStyle().FramePadding.Y * 2.0f);

        ImGui.SetNextWindowPos(new Vector2(0, ImGui.GetIO().DisplaySize.Y - menuBarHeight));
        ImGui.SetNextWindowSize(new Vector2(ImGui.GetIO().DisplaySize.X, menuBarHeight), ImGuiCond.Always);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.Zero);
        ImGui.Begin("##BOTTOMMENUBAR", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.MenuBar);
        ImGui.BeginMenuBar();
        menuBarSize = ImGui.GetWindowSize();

        // ImGui.Text($"Selected nodes: {_currentlySimulatedCircuitViewModel.GetSelectedNodes().Count}");

        ImGui.Text($"Coroutines: {_coroutineService.RunningCount}");
        ImGui.Text($"GUI Coroutines: {_guiCoroutineService.RunningCount}");

        var mousePos = GetMousePositionInWorkspace();
        // bool isHoveringAnyNodePin = _currentlySimulatedCircuitViewModel.IsPositionOnAnyPin(mousePos, out var hoveredNodePin);

        // ImGui.Text($"Hovering node pin: {hoveredNodePin?.PinID}");

        ImGui.EndMenuBar();
    }

    private void SubmitSideBar(Vector2 topLeft, float height, out float width)
    {
        float sidebarMinWidth = 200;
        float sidebarMaxWidth = 800;

        ImGui.SetNextWindowPos(topLeft);
        ImGui.SetNextWindowSizeConstraints(new Vector2(sidebarMinWidth, height), new Vector2(sidebarMaxWidth, height));
        ImGui.Begin("##SIDEBAR", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav);

        ImGui.SeparatorText("Project circuits");
        var projectCircuitTree = _projectService.GetCurrentProject().GetProjectCircuitTree();

        static void submitTreeNode<TValue>(IVirtualFileTree<string, TValue> node, Action<string, TValue> onClickNode, Action<string, TValue> contextMenu, bool skipRoot = false)
        {
            if (!skipRoot)
            {
                if (ImGui.TreeNodeEx($"{ImGuiIcons.ICON_FOLDER} {node.Directory}"))
                {
                    foreach (var child in node.GetDirectories())
                    {
                        submitTreeNode<TValue>(child, onClickNode, contextMenu);
                    }

                    foreach (var childItem in node.GetFiles())
                    {
                        ImGui.TreeNodeEx($"{ImGuiIcons.ICON_CODE} {childItem.Key}", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                        if (ImGui.IsItemClicked())
                        {
                            onClickNode.Invoke(childItem.Key, childItem.Value);
                        }
                        if (ImGui.BeginPopupContextItem())
                        {
                            contextMenu.Invoke(childItem.Key, childItem.Value);
                            ImGui.EndPopup();
                        }
                    }

                    ImGui.TreePop();
                }
            }
            else
            {
                foreach (var child in node.GetDirectories())
                {
                    submitTreeNode<TValue>(child, onClickNode, contextMenu);
                }

                foreach (var childItem in node.GetFiles())
                {
                    ImGui.TreeNodeEx($"{ImGuiIcons.ICON_CODE} {childItem.Key}", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                    if (ImGui.IsItemClicked())
                    {
                        onClickNode.Invoke(childItem.Key, childItem.Value);
                    }
                    if (ImGui.BeginPopupContextItem())
                    {
                        contextMenu.Invoke(childItem.Key, childItem.Value);
                        ImGui.EndPopup();
                    }
                }
            }
        }

        submitTreeNode(projectCircuitTree, (_, _) => { }, (circName, circ) =>
        {
            if (ImGui.MenuItem("Open"))
            {
                _guiCoroutineService.Run(OpenCircuit(circ));
            }
        }, true);

        ImGui.SeparatorText("Available nodes");

        var availableNodes = _projectService.GetCurrentProject().GetAvailableNodesTree();
        submitTreeNode(availableNodes, (name, node) => _coroutineService.Run(PlaceNode(node)), (_, _) => { }, true);

        width = ImGui.GetWindowSize().X;
        ImGui.End();

        ImGui.Begin("Timeline");

        ImGui.Text($"Undo / Redo: {_invoker.UndoStack.Count} / {_invoker.RedoStack.Count}");

        ImGui.Separator();

        var redoStack = _invoker.RedoStack.ToArray();
        var undoStack = _invoker.UndoStack.ToArray();

        ImGui.PushStyleColor(ImGuiCol.Text, ColorF.Darken(ColorF.LightGray, 0.63f).ToVector4());
        for (int i = 0; i < redoStack.Length; i++)
        {
            ImGui.PushID($"REDO{i}");
            if (ImGui.Selectable(redoStack.ElementAt(redoStack.Length - i - 1).GetTitle()))
                _invoker.Redo(redoStack.Length - i);
        }
        ImGui.PopStyleColor();

        for (int i = 0; i < undoStack.Length; i++)
        {
            ImGui.PushID($"UNDO{i}");
            if (ImGui.Selectable(undoStack.ElementAt(i).GetTitle()))
                _invoker.Undo(i);
        }

        if (ImGui.Selectable("(initial state)"))
            _invoker.Undo(undoStack.Length);

        ImGui.End();
    }
}