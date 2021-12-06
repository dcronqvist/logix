namespace LogiX.Editor;

public enum EditorState
{
    None = 0,
    MovingCamera = 1,
    MovingSelection = 2,
    RectangleSelecting = 3,
    HoveringInput = 4,
    HoveringOutput = 5,
    OutputToInput = 6,
    HoveringWire = 7,
    MakingIC = 8,
}