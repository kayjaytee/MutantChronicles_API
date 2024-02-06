using System.ComponentModel;
using static MutantChroniclesAPI.Model.WeaponModel.Weapon;

namespace MutantChroniclesAPI.Model.WeaponModel;

public class SecondaryMode
{
    public bool Mountable { get; set; }
    public bool Equipped { get; set; }
    public string NameDescription { get; set; }
    public decimal SeperateWeight { get; set; }
    public int MagazineCapacity { get; set; }
    public WeaponFunctionality WeaponFunctionalityEnum { get; set; }
    public int WeaponRange { get; set; }
    public int AdditionalStrengthRequirement { get; set; }
    public int JammingFactor { get; set; }
    public int ReloadingTime { get; set; }
    public int TargetMultipleHits { get; set; } //Used for mostly explosive weapons and Shotguns
    public int ShrapnelRange { get; set; }
    public int DamageMin { get; set; }
    public int DamageMax { get; set; }
    public int DamageAdded { get; set; }
    public SecondaryModeWeaponCategory Category { get; set; }

    //----------------------------  Temporary Properties  ----------------------------\\
    public int CurrentAmmo { get; set; }
    public bool WeaponIsJammed { get; set; }
    public short SuccessfulUnjamAttempts { get; set; }

    public enum SecondaryModeWeaponCategory
    {
        [Description("GrenadeLauncher")]
        GrenadeLauncher = 0,
        [Description("Plasma")]
        Plasma = 1,
        [Description("Incinerator")]
        Incinerator = 2,
        [Description("Other")]
        Other = 3
    }

}
