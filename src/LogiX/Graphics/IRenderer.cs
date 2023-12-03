using LogiX.Graphics.Rendering;
using LogiX.Rendering;

namespace LogiX.Graphics;

public interface IRenderer
{
    PrimitiveRenderer Primitives { get; }
    TextRenderer Text { get; }
}

public class Renderer : IRenderer
{
    public PrimitiveRenderer Primitives { get; }
    public TextRenderer Text { get; }

    public Renderer(PrimitiveRenderer primitives, TextRenderer text)
    {
        Primitives = primitives;
        Text = text;
    }
}
