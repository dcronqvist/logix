using LogiX.Components;

namespace LogiX.SaveSystem;

class DescriptionConverter : JsonConverter<ComponentDescription>
{
    public DescriptionConverter()
    {
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(ComponentDescription).IsAssignableFrom(typeToConvert) && typeToConvert != typeof(DescriptionIntegrated);
    }

    public override ComponentDescription? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument document = JsonDocument.ParseValue(ref reader);
        document.RootElement.TryGetProperty("type", out JsonElement ty);
        int type = ty.GetInt32();
        ComponentType ct = (ComponentType)type;

        JsonSerializerOptions jso = new JsonSerializerOptions(options);
        jso.Converters.Remove(this);

        switch (ct)
        {
            case ComponentType.SWITCH:
                return document.Deserialize<DescriptionSwitch>(jso);
            case ComponentType.LAMP:
                return document.Deserialize<DescriptionLamp>(jso);
            case ComponentType.LOGIC_GATE:
                return document.Deserialize<DescriptionGate>(jso);
            case ComponentType.BUFFER:
                return document.Deserialize<DescriptionBuffer>(jso);
            case ComponentType.TRI_STATE:
                return document.Deserialize<DescriptionTriState>(jso);
            case ComponentType.INTEGRATED:
                return document.Deserialize<DescriptionIntegrated>(options);
        }

        throw new ApplicationException(String.Format("The component type {0} is not supported!", type));
    }

    public override void Write(Utf8JsonWriter writer, ComponentDescription value, JsonSerializerOptions options)
    {
        JsonSerializerOptions jso = new JsonSerializerOptions(options);
        jso.Converters.Remove(this);

        switch (value.Type)
        {
            case ComponentType.SWITCH:
                writer.WriteRawValue(JsonSerializer.Serialize<DescriptionSwitch>((DescriptionSwitch)value, jso));
                break;
            case ComponentType.LAMP:
                writer.WriteRawValue(JsonSerializer.Serialize<DescriptionLamp>((DescriptionLamp)value, jso));
                break;
            case ComponentType.LOGIC_GATE:
                writer.WriteRawValue(JsonSerializer.Serialize<DescriptionGate>((DescriptionGate)value, jso));
                break;
            case ComponentType.BUFFER:
                writer.WriteRawValue(JsonSerializer.Serialize<DescriptionBuffer>((DescriptionBuffer)value, jso));
                break;
            case ComponentType.TRI_STATE:
                writer.WriteRawValue(JsonSerializer.Serialize<DescriptionTriState>((DescriptionTriState)value, jso));
                break;
            case ComponentType.INTEGRATED:
                writer.WriteRawValue(JsonSerializer.Serialize<DescriptionIntegrated>((DescriptionIntegrated)value, options));
                break;
        }
    }
}