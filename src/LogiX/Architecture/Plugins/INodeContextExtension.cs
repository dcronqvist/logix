namespace LogiX.Architecture.Plugins;

public interface INodeContextExtension
{
    public string NodeType { get; }
    public string MenuItemName { get; }

    public void Execute(Editor editor, Node node);
}