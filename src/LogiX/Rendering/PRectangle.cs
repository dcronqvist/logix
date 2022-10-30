using System.Numerics;

namespace LogiX.Rendering;

public class PRectangle : Primitive
{
    public override (Vector2, Vector2, Vector2)[] GetTris()
    {
        return Utilities.Arrayify(
             (new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)),
             (new Vector2(0, 0), new Vector2(1, 1), new Vector2(1, 0))
        );
    }
}