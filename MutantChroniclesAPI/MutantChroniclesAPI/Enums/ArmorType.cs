using MutantChroniclesAPI.Converter;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MutantChroniclesAPI.Enums;

[JsonConverter(typeof(StringEnumConverterWithDescription))]
public enum ArmorType
{
    [Description("Head")]
    Head = 1,

    [Description("Harness")]
    Harness = 2,

    [Description("Jacket")]
    Jacket = 3,

    [Description("Trenchcoat")]
    Bodysuit = 4,

    [Description("Bodysuit")]
    Trenchcoat = 5,

    [Description("Arms")]
    Arms = 6,

    [Description("Gloves")]
    Gloves = 7,

    [Description("Legs")]
    Legs = 8,

    [Description("Knee")]
    Knee = 9
}
