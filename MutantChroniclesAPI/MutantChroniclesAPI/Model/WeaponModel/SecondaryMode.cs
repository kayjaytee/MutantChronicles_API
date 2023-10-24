using MC_Weapon_Calculator.Model;
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
    public int Jammingfactor { get; set; }
    public int ReloadingTime { get; set; }
    public int TargetMultipleHits { get; set; } //Used for mostly explosive weapons and Shotguns
    public int ShrapnelRange { get; set; }
    public int DamageMin { get; set; }
    public int DamageMax { get; set; }
    public int DamageAdded { get; set; }

    //----------------------------  Temporary Properties  ----------------------------\\
    public int CurrentAmmo { get; set; }
    public bool WeaponIsJammed { get; set; }
    public short SuccessfulUnjamAttempts { get; set; }

    public enum WeaponCategory
    {
        GrenadeLauncher = 0,
        Plasma = 1,
        Incinerator = 2,
        Other = 3
    }

}
