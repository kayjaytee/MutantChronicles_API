using MongoDB.Bson;
using MutantChroniclesAPI.Converter;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MutantChroniclesAPI.Model.CharacterModel;
public class Armor
{
    public string Name { get; private set; }
    public int Absorb { get; private set; }

    [JsonConverter(typeof(StringEnumConverterWithDescription))]
    public ArmorType Type { get; private set; }

    [JsonConverter(typeof(StringEnumConverterWithDescription))]
    public ArmorMaterial Material { get; private set; }

    public Armor(string name, int absorb, ArmorType type, ArmorMaterial material)
    {
        Name = name;
        Absorb = absorb;
        Type = type;
        Material = material;
    }

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
        Trenchcoat = 4,

        [Description("Bodysuit")]
        Bodysuit = 5,

        [Description("Arms")]
        Arms = 6,

        [Description("Gloves")]
        Gloves = 7,

        [Description("Legs")]
        Legs = 8,

        [Description("Knee")]
        Knee = 9,

        [Description("Shoulders")]
        Shoulders = 10
    }

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

}

/// <summary>
/// ArmorValues are applied to body parts; their values are copied from a characters existing armor and adds a block chance on top of it.
/// </summary>
public class ArmorValues //better test coverage?
{
    public int Absorb { get; private set; }
    public double BlockChance { get; private set; }
    public bool MeleeProtection { get; private set; }
    public bool RangeProtection { get; private set; }
    public bool FireProof { get; private set; }
    public ArmorValues(int absorb, double blockChance, bool meleeProtection, bool rangeProtection, bool fireProof)
    {
        Absorb = absorb;
        BlockChance = blockChance;
        MeleeProtection = meleeProtection;
        RangeProtection = rangeProtection;
        FireProof = fireProof;
    }
}


