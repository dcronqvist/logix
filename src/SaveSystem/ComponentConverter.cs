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
                document.RootElement.TryGetProperty("componentIdentifier", out JsonElement ci);
                document.RootElement.TryGetProperty("componentData", out JsonElement cd);
                CustomComponentData ccd = (CustomComponentData)cd.Deserialize(Util.GetCustomDataTypeOfCustomComponent(ci.Deserialize<string>()));
                CustomDescription customDescription = document.Deserialize<CustomDescription>(jso);
                customDescription.Data = ccd;
                return customDescription;
        }

        throw new ApplicationException(String.Format("The component type {0} is not supported!", type));
    }

    public override void Write(Utf8JsonWriter writer, ComponentDescription value, JsonSerializerOptions options)
    {
        switch (value.Type)
        {
            case ComponentType.Button:
                writer.WriteRawValue(JsonSerializer.Serialize<GenIODescription>((GenIODescription)value, jso));
                break;
            case ComponentType.HexViewer:
                writer.WriteRawValue(JsonSerializer.Serialize<GenIODescription>((GenIODescription)value, jso));
                break;
            case ComponentType.Switch:
                writer.WriteRawValue(JsonSerializer.Serialize<SLDescription>((SLDescription)value, jso));
                break;
            case ComponentType.Lamp:
                writer.WriteRawValue(JsonSerializer.Serialize<SLDescription>((SLDescription)value, jso));
                break;
            case ComponentType.Gate:
                writer.WriteRawValue(JsonSerializer.Serialize<GateDescription>((GateDescription)value, jso));
                break;
            case ComponentType.Integrated:
                writer.WriteRawValue(JsonSerializer.Serialize<ICDescription>((ICDescription)value, options));
                break;
            case ComponentType.ROM:
                writer.WriteRawValue(JsonSerializer.Serialize<ROMDescription>((ROMDescription)value, jso));
                break;
            case ComponentType.TextLabel:
                writer.WriteRawValue(JsonSerializer.Serialize<TextComponentDescription>((TextComponentDescription)value, jso));
                break;
            case ComponentType.Memory:
                writer.WriteRawValue(JsonSerializer.Serialize<MemoryDescription>((MemoryDescription)value, jso));
                break;
            case ComponentType.Constant:
                writer.WriteRawValue(JsonSerializer.Serialize<ConstantDescription>((ConstantDescription)value, jso));
                break;
            case ComponentType.Splitter:
                writer.WriteRawValue(JsonSerializer.Serialize<SplitterDescription>((SplitterDescription)value, jso));
                break;
            case ComponentType.Clock:
                writer.WriteRawValue(JsonSerializer.Serialize<ClockDescription>((ClockDescription)value, jso));
                break;
            case ComponentType.Delayer:
                writer.WriteRawValue(JsonSerializer.Serialize<DelayerDescription>((DelayerDescription)value, jso));
                break;
            case ComponentType.Mux:
                writer.WriteRawValue(JsonSerializer.Serialize<MUXDescription>((MUXDescription)value, jso));
                break;
            case ComponentType.Demux:
                writer.WriteRawValue(JsonSerializer.Serialize<MUXDescription>((MUXDescription)value, jso));
                break;
            case ComponentType.DTBC:
                writer.WriteRawValue(JsonSerializer.Serialize<DTBCDescription>((DTBCDescription)value, jso));
                break;
            case ComponentType.Custom:
                writer.WriteRawValue(JsonSerializer.Serialize<CustomDescription>((CustomDescription)value, jso));
                break;
        }
    }
}