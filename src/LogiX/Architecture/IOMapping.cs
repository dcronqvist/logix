namespace LogiX;

public enum ComponentSide
{
    TOP,
    BOTTOM,
    LEFT,
    RIGHT
}

public class IOGroup
{
    // In case of a single bit IO, the list will only contain 1 IO index
    public List<int> IOIndices { get; set; }
    public string Identifier { get; set; }
    public ComponentSide Side { get; set; }

    public IOGroup(string identifier, ComponentSide side, params int[] indices)
    {
        IOIndices = indices.ToList();
        Identifier = identifier;
        Side = side;
    }
}

public class IOMapping
{
    // A mapping of the 5 inputs, to create a 1 2 bit input, and 1 3 bit input:
    // [{[0, 1], "A1-A0", "LEFT"}, {[2, 3, 4], "B2-B0", "RIGHT"}]
    // All components will only see ALL their 1 bit inputs, so the mapping should be abstracted away from it.

    public List<IOGroup> Mapping { get; set; }

    public IOMapping(List<IOGroup> mapping)
    {
        Mapping = mapping;
    }

    public IOMapping(params IOGroup[] mapping)
    {
        Mapping = mapping.ToList();
    }

    public IOGroup GetGroup(int index)
    {
        return Mapping[index];
    }

    public int GetAmountOfGroups()
    {
        return Mapping.Count;
    }
}