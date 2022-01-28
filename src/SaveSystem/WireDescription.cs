namespace LogiX.SaveSystem;

public class WireDescription
{
    [JsonPropertyName("bits")]
    public int Bits { get; set; }
    [JsonPropertyName("from")]
    public string From { get; set; }
    [JsonPropertyName("fromOutputIndex")]
    public int FromOutputIndex { get; set; }
    [JsonPropertyName("to")]
    public string To { get; set; }
    [JsonPropertyName("toInputIndex")]
    public int ToInputIndex { get; set; }

    [JsonPropertyName("intermediatePoints")]
    public List<Vector2> IntermediatePoints { get; set; }

    public WireDescription(int bits, string from, int fromOutputIndex, string to, int toInputIndex, List<Vector2> intermediatePoints)
    {
        Bits = bits;
        From = from;
        FromOutputIndex = fromOutputIndex;
        To = to;
        ToInputIndex = toInputIndex;
        this.IntermediatePoints = intermediatePoints;
    }
}