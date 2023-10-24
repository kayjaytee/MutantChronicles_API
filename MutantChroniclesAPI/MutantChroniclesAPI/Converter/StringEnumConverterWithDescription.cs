using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.Json;
using MutantChroniclesAPI.Enums;

namespace MutantChroniclesAPI.Converter;


/// <summary>
/// The Purpose of the StringEnumConverterWithDescription is to improve the readability for Enums for different query options.
/// Instead of listing options as 1,2,3,4,5,6 etc, the options have instead proper descriptions that determine what the uses are.
/// </summary>
/// 

public class StringEnumConverterWithDescription : JsonConverter<Enum>
{
    //Needs to check if Enum is convertable
    public override bool CanConvert(Type typeToConvert)
    {
        var fields = typeToConvert.GetFields();
        foreach (var field in fields)
        {
            if (field.GetCustomAttributes(typeof(DescriptionAttribute), false).Length > 0)
            {
                return typeToConvert.IsEnum;
            }
        }
        return false;
    }


    //Converts JSON to Enum
    public override Enum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (Enum)Enum.Parse(typeToConvert, reader.GetString(), ignoreCase: true);
    }

    //Converts Enum to JSON
    public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(GetEnumDescription(value));
    }

    // Checks for Custom description >
    // if found >
    // returns Enum With Description Attribute.
    private string GetEnumDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var descriptionAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
        return descriptionAttribute?.Description ?? value.ToString();
    }

}
