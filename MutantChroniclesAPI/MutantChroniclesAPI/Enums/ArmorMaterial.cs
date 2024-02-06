using MutantChroniclesAPI.Converter;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MutantChroniclesAPI.Enums;

[JsonConverter(typeof(StringEnumConverterWithDescription))]
public enum ArmorMaterial
{
    [Description("Cloth")]
    Cloth = 1,

    [Description("Plastic")]
    Plastic = 2,

    [Description("Ballistic")]
    Ballistic = 3,

    [Description("Bulletproof")]
    BulletProof = 4,

    [Description("LightCombat")]
    LightCombat = 5,

    [Description("HeavyCombat")]
    HeavyCombat = 6,

    [Description("ExtraHeavyCombat")]
    ExtraHeavyCombat = 7,
}
