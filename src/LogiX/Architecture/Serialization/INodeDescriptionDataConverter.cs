using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogiX.Architecture.Serialization;

public class INodeDescriptionDataConverter : JsonConverter<NodeDescription>
{
    public const string NODE_TYPE_ID = "nodeTypeID";
    public const string NODE_DATA = "data";
    public const string NODE_POSITION = "position";
    public const string NODE_ROTATION = "rotation";
    public const string NODE_ID = "id";

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsAssignableTo(typeof(NodeDescription));
    }

    public override NodeDescription Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument document = JsonDocument.ParseValue(ref reader);
        document.RootElement.TryGetProperty(NODE_TYPE_ID, out JsonElement ty);
        string type = ty.GetString();

        var scriptType = NodeDescription.GetNodeScriptTypeFromIdentifier(type);

        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Remove(this);

        INodeDescriptionData dataInstance = null;

        var dataType = NodeDescription.GetNodeTypesDataParameterType(type);
        if (document.RootElement.TryGetProperty(NODE_DATA, out JsonElement data))
        {
            dataInstance = (INodeDescriptionData)data.Deserialize(dataType, options);
        }
        else
        {
            // If there is a missing data property, we just give it a default one.
            dataInstance = NodeDescription.CreateDefaultNodeDescriptionData(type);
        }


        document.RootElement.TryGetProperty(NODE_POSITION, out JsonElement posEle);
        var position = (Vector2i)posEle.Deserialize(typeof(Vector2i), newOptions);

        document.RootElement.TryGetProperty(NODE_ROTATION, out JsonElement rotEle);
        var rotation = rotEle.GetInt32();

        if (document.RootElement.TryGetProperty(NODE_ID, out JsonElement idEle))
        {
            var id = (Guid)idEle.Deserialize(typeof(Guid), newOptions);
            return new NodeDescription(type, position, rotation, id, dataInstance);
        }
        else
        {
            return new NodeDescription(type, position, rotation, Guid.NewGuid(), dataInstance);
        }
    }

    public override void Write(Utf8JsonWriter writer, NodeDescription value, JsonSerializerOptions options)
    {
        var newOptions = new JsonSerializerOptions(options);
        //newOptions.Converters.Remove(this);

        writer.WriteStartObject();
        writer.WriteString(NODE_TYPE_ID, value.NodeTypeID);
        writer.WritePropertyName(NODE_DATA);
        JsonSerializer.Serialize(writer, value.Data, value.Data.GetType(), newOptions);
        writer.WritePropertyName(NODE_POSITION);
        JsonSerializer.Serialize(writer, value.Position, newOptions);
        writer.WriteNumber(NODE_ROTATION, value.Rotation);
        writer.WriteString(NODE_ID, value.ID.ToString());
        writer.WriteEndObject();
    }
}