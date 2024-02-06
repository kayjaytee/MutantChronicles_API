using MC_Weapon_Calculator.Model;
using System.ComponentModel;

namespace MutantChroniclesAPI.Model.WeaponModel;

public class Weapon
{
    public WeaponCategory Category { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Weight { get; set; }
    public int Length { get; set; }
    public BipodModifier? BipodModifier { get; set; }
    public TelescopicShoulderSupport? TelescopicShoulderSupport { get; set; }
    public SecondaryMode? SecondaryMode { get; set; }
    public ChainBayonet? ChainBayonet { get; set; }
    public SpecialSightModifiers SpecialSightModifiers { get; set; }
    public WeaponFunctionality WeaponFunctionalityEnum { get; set; }
    public Range WeaponRange { get; set; }
    public int StrengthRequirement { get; set; }
    public int ReloadingTime { get; set; }
    public int JammingFactor { get; set; }
    public int TargetMultipleHits { get; set; } //Used for mostly explosive weapons and Shotguns
    public int ShrapnelRange { get; set; }
    public int DamageMin { get; set; }
    public int DamageMax { get; set; }
    public int DamageAdded { get; set; }
    public int MagazineCapacity { get; set; }

    // Enum Properties \\
    public enum WeaponCategory
    {
        [Description("Primitive")]
        Primitive = 0,
        [Description("Handgun")]
        Handgun = 1,
        [Description("SubMachineGun")]
        SubMachineGun = 2,
        [Description("Sniper Rifle")]
        SniperRifle = 3,
        [Description("Shotgun")]
        Shotgun = 4,
        [Description("Assault Rifle")]
        AssaultRifle = 5,
        [Description("Grenade Launcher")]
        GrenadeLauncher = 6,
        [Description("Light Machine Gun")]
        LightMachineGun = 7,
        [Description("Heavy Machine Gun")]
        HeavyMachinegun = 8,
        [Description("Rocket Launcher")]
        RocketLauncher = 9
    }
    public enum WeaponFunctionality
    {
        Manual = 0, //M
        SemiAutomatic = 1, //S
        FullAutomatic = 2, //A
        SemiAutomaticWith3RoundBurst = 3 //3
    }
    public enum Range
    {
        Squares = 0, // Square = 1,5 Meter
        Meters = 1,
    }

    //----------------------------  Temporary Properties  ----------------------------\\
    public Ammo.AmmoType AmmoType { get; set; }
    public int CurrentAmmo { get; set; }
    public bool WeaponIsJammed { get; set; }
    public short SuccessfulUnjamAttempts { get; set; }

}
