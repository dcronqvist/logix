namespace LogiX;

public enum ComponentSide
{
    TOP,
    BOTTOM,
    LEFT,
    RIGHT
}

// public class IOGroup
// {
//     // In case of a single bit IO, the list will only contain 1 IO index
//     public int[] IOIndices { get; set; }
//     public string Identifier { get; set; }
//     public ComponentSide Side { get; set; }

//     public IOGroup(string identifier, ComponentSide side, int[] ioIndices)
//     {
//         IOIndices = ioIndices;
//         Identifier = identifier;
//         Side = side;
//     }

//     public static IOGroup FromIndexList(string identifier, ComponentSide side, params int[] ioIndices)
//     {
//         return new IOGroup(identifier, side, ioIndices);
//     }
// }

// public class IOMapping
// {
//     // A mapping of the 5 inputs, to create a 1 2 bit input, and 1 3 bit input:
//     // [{[0, 1], "A1-A0", "LEFT"}, {[2, 3, 4], "B2-B0", "RIGHT"}]
//     // All components will only see ALL their 1 bit inputs, so the mapping should be abstracted away from it.

//     public IOGroup[] Groups { get; set; }

//     public IOMapping(IOGroup[] groups)
//     {
//         Groups = groups;
//     }

//     public IOGroup GetGroup(int index)
//     {
//         return Groups[index];
//     }

//     public int GetAmountOfGroups()
//     {
//         return Groups.Length;
//     }

//     public static IOMapping FromGroups(params IOGroup[] groups)
//     {
//         return new(groups);
//     }
// }