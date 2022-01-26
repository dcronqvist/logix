namespace LogiX.SaveSystem;

public class WireDescription
{
    [JsonProperty(PropertyName = "bits")]
    public int Bits { get; set; }
    [JsonProperty(PropertyName = "from")]
    public string From { get; set; }
    [JsonProperty(PropertyName = "fromOutputIndex")]
    public int FromOutputIndex { get; set; }
    [JsonProperty(PropertyName = "to")]
    public string To { get; set; }
    [JsonProperty(PropertyName = "toInputIndex")]
    public int ToInputIndex { get; set; }

    public WireDescription(int bits, string from, int fromOutputIndex, string to, int toInputIndex)
    {
        Bits = bits;
        From = from;
        FromOutputIndex = fromOutputIndex;
        To = to;
        ToInputIndex = toInputIndex;
    }
}