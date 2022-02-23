using LogiX.SaveSystem;

namespace LogiX.Components;

public class Switch : Component
{
    public LogicValue Value { get; set; }

    public Switch(Vector2 position) : base(position, ComponentType.SWITCH)
    {
        this.Value = LogicValue.LOW;

        this.AddIO(1, new IOConfig(ComponentSide.RIGHT));
    }

    public override void PerformLogic()
    {
        this.GetIO(0).PushValues(this.Value);
    }

    public override void Interact(Editor.Editor editor)
    {
        if (Raylib.CheckCollisionPointRec(editor.GetWorldMousePos(), this.GetRectangle()))
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
            {
                this.Value = this.Value == LogicValue.LOW ? LogicValue.HIGH : LogicValue.LOW;
            }
        }
    }
}