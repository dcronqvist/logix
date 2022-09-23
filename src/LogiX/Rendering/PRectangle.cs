using System.Numerics;

namespace GoodGame.Rendering;

public class PRectangle : Primitive
{
    protected override uint[] GetIndices()
    {
        return Utilities.Arrayify<uint>(
            0, 1, 3,
            1, 2, 3
        );
    }

    protected override Vector2[] GetVertices()
    {
        return Utilities.Arrayify(
            new Vector2(0, 0), // bottom left
            new Vector2(1f, 0), // bottom right
            new Vector2(1f, 1f), // top right
            new Vector2(0f, 1f) // top left
        );
    }
}