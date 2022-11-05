using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogiX.Architecture.Serialization;

public class IComponentDescriptionDataConverter : JsonConverter<ComponentDescription>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsAssignableTo(typeof(ComponentDescription));
    }

    public override ComponentDescription Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument document = JsonDocument.ParseValue(ref reader);
        document.RootElement.TryGetProperty("componentTypeID", out JsonElement ty);
        string type = ty.GetString();

        var scriptType = ComponentDescription.GetComponentScriptTypeFromIdentifier(type);

        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Remove(this);

        document.RootElement.TryGetProperty("data", out JsonElement data);
        var dataType = ComponentDescription.GetComponentTypesDataParameterType(type);

        var dataInstance = (IComponentDescriptionData)data.Deserialize(dataType, options);

        document.RootElement.TryGetProperty("position", out JsonElement posEle);
        var position = (Vector2i)posEle.Deserialize(typeof(Vector2i), newOptions);

        document.RootElement.TryGetProperty("rotation", out JsonElement rotEle);
        var rotation = rotEle.GetInt32();

        return new ComponentDescription(type, position, rotation, dataInstance);
    }

    public override void Write(Utf8JsonWriter writer, ComponentDescription value, JsonSerializerOptions options)
    {
        var newOptions = new JsonSerializerOptions(options);
        //newOptions.Converters.Remove(this);

        writer.WriteStartObject();
        writer.WriteString("componentTypeID", value.ComponentTypeID);
        writer.WritePropertyName("data");
        JsonSerializer.Serialize(writer, value.Data, value.Data.GetType(), newOptions);
        writer.WritePropertyName("position");
        JsonSerializer.Serialize(writer, value.Position, newOptions);
        writer.WriteNumber("rotation", value.Rotation);
        writer.WriteEndObject();
    }
}