using Newtonsoft.Json.Linq;
using LogiX.Components;

namespace LogiX.SaveSystem;

class ComponentConverter : Newtonsoft.Json.Converters.CustomCreationConverter<ComponentDescription>
{
    public override ComponentDescription Create(Type objectType)
    {
        throw new NotImplementedException();
    }

    public ComponentDescription Create(Type objectType, JObject jObject)
    {
        int type = (int)jObject.Property("type");
        ComponentType ct = (ComponentType)type;

        switch (ct)
        {
            case ComponentType.Button:
                return jObject.ToObject<GenIODescription>();
            case ComponentType.HexViewer:
                return jObject.ToObject<GenIODescription>();
            case ComponentType.Switch:
                return jObject.ToObject<SLDescription>();
            case ComponentType.Lamp:
                return jObject.ToObject<SLDescription>();
            case ComponentType.Gate:
                return jObject.ToObject<GateDescription>();
            case ComponentType.Integrated:
                return jObject.ToObject<ICDescription>();
            case ComponentType.ROM:
                return jObject.ToObject<ROMDescription>();
            case ComponentType.TextLabel:
                return jObject.ToObject<TextComponentDescription>();
            case ComponentType.Memory:
                return jObject.ToObject<MemoryDescription>();
            case ComponentType.Constant:
                return jObject.ToObject<ConstantDescription>();
            case ComponentType.Splitter:
                return jObject.ToObject<SplitterDescription>();
        }

        throw new ApplicationException(String.Format("The component type {0} is not supported!", type));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Load JObject from stream 
        JObject jObject = JObject.Load(reader);

        // Create target object based on JObject 
        var target = Create(objectType, jObject);

        // Populate the object properties 
        serializer.Populate(jObject.CreateReader(), target);

        return target;
    }
}