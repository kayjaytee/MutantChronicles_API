using MutantChroniclesAPI.Converter;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MutantChroniclesAPI.Model.CharacterModel;

public class Armor
{
    public string Name { get; set; }
    public int ArmorValue { get; set; }
    
    [JsonConverter(typeof(StringEnumConverterWithDescription))]
    public ArmorType Type { get; set; }

    public Armor(string name, int armorValue, ArmorType type)
    {
        Name = name;
        ArmorValue = armorValue;
        Type = type;
        ApplyArmorValueToBodyPart();
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
        Knee = 9
    }

    public int Head { get; private set; }
    public int Chest { get; private set; }
    public int Stomach { get; private set; }
    public int RightArm { get; private set; }
    public int LeftArm { get; private set; }
    public int RightLeg { get; private set; }
    public int LeftLeg { get; private set; }

    private void ApplyArmorValueToBodyPart()
    {
        switch (Type)
        {
            case ArmorType.Head:
                Head = ArmorValue;
                break;
            case ArmorType.Harness:
                Chest = ArmorValue;
                Stomach = ArmorValue;
                break;
            case ArmorType.Jacket:
                Chest = ArmorValue;
                Stomach = ArmorValue;
                RightArm = ArmorValue;
                LeftArm = ArmorValue;
                break;
            case ArmorType.Trenchcoat: //WIP
                Chest = ArmorValue;
                Stomach = ArmorValue;
                RightArm = ArmorValue;
                LeftArm = ArmorValue;
                RightLeg = ArmorValue;
                LeftLeg = ArmorValue;
                break;
            case ArmorType.Bodysuit:
                Chest = ArmorValue;
                Stomach = ArmorValue;
                RightArm = ArmorValue;
                LeftArm = ArmorValue;
                RightLeg = ArmorValue;
                LeftLeg = ArmorValue;
                break;
            case ArmorType.Arms:
                RightArm = ArmorValue;
                LeftArm = ArmorValue;
                break;
            case ArmorType.Gloves: //WIP
                RightArm = ArmorValue;
                LeftArm = ArmorValue;
                break;
            case ArmorType.Legs:
                RightLeg = ArmorValue;
                LeftLeg = ArmorValue;
                break;
            case ArmorType.Knee: //WIP
                RightLeg = ArmorValue;
                LeftLeg = ArmorValue;
                break;
            default:
                break;
        }
    }
}


