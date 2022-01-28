using LogiX.Components;

namespace LogiX.SaveSystem;

class ComponentConverter : JsonConverter<ComponentDescription>
{
    private JsonSerializerOptions jso;

    public ComponentConverter()
    {
        jso = new JsonSerializerOptions();
        jso.IncludeFields = true;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(ComponentDescription).IsAssignableFrom(typeToConvert) && typeToConvert != typeof(ICDescription);
    }

    public override ComponentDescription? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument document = JsonDocument.ParseValue(ref reader);
        document.RootElement.TryGetProperty("type", out JsonElement ty);
        int type = ty.GetInt32();
        ComponentType ct = (ComponentType)type;

        switch (ct)
        {
            case ComponentType.Button:
                return document.Deserialize<GenIODescription>(jso);
            case ComponentType.HexViewer:
                return document.Deserialize<GenIODescription>(jso);
            case ComponentType.Switch:
                return document.Deserialize<SLDescription>(jso);
            case ComponentType.Lamp:
                return document.Deserialize<SLDescription>(jso);
            case ComponentType.Gate:
                return document.Deserialize<GateDescription>(jso);
            case ComponentType.Integrated:
                return document.Deserialize<ICDescription>(options);
            case ComponentType.ROM:
                return document.Deserialize<ROMDescription>(jso);
            case ComponentType.TextLabel:
                return document.Deserialize<TextComponentDescription>(jso);
            case ComponentType.Memory:
                return document.Deserialize<MemoryDescription>(jso);
            case ComponentType.Constant:
                return document.Deserialize<ConstantDescription>(jso);
            case ComponentType.Splitter:
                return document.Deserialize<SplitterDescription>(jso);
            case ComponentType.Clock:
                return document.Deserialize<ClockDescription>(jso);
            case ComponentType.Delayer:
                return document.Deserialize<DelayerDescription>(jso);
            case ComponentType.Mux:
                return document.Deserialize<MUXDescription>(jso);
            case ComponentType.Demux:
                return document.Deserialize<MUXDescription>(jso);
            case ComponentType.DTBC:
                return document.Deserialize<DTBCDescription>(jso);
            case ComponentType.Custom:
                return document.Deserialize<CustomDescription>(jso);
        }

        throw new ApplicationException(String.Format("The component type {0} is not supported!", type));
    }

    public override void Write(Utf8JsonWriter writer, ComponentDescription value, JsonSerializerOptions jso)
    {
        throw new NotImplementedException();
    }
}