namespace LogiX.Components;

public interface ISelectable
{
    void RenderSelected();
    void Move(Vector2 delta);
}