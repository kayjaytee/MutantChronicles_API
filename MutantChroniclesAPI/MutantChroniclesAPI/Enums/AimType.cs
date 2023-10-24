using MutantChroniclesAPI.Converter;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MutantChroniclesAPI.Enums;

[JsonConverter(typeof(StringEnumConverterWithDescription))]

public enum AimType
{
    [Description("Uncontrolled")]
    Uncontrolled = 0,
    [Description("Aimed")]
    Aimed = 1,
    [Description("AccurateAimed")]
    AccurateAimed = 2
}
