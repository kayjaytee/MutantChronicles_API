using MutantChroniclesAPI.Converter;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MutantChroniclesAPI.Enums;

[JsonConverter(typeof(StringEnumConverterWithDescription))]
public enum FiringMode
{

    [Description("SingleRound")]
    SingleRound = 0,

    [Description("Burst")]
    Burst = 1,

    [Description("FullAuto")]
    FullAuto = 2,

    [Description("RapidVolley")]
    RapidVolley = 3,

    [Description("AreaSpray")]
    AreaSpray = 4 //Always considered Uncontrolled Action
}
