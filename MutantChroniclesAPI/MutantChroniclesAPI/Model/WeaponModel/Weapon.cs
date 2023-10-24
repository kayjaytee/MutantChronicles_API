using MC_Weapon_Calculator.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MutantChroniclesAPI.Model.WeaponModel;

public class Weapon
{
    public WeaponCategory Category {  get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Weight { get; set; }
    public int Length { get; set; }
    public BipodModifier BipodModifier { get; set; }
    public TelescopicShoulderSupport TelescopicShoulderSupport { get; set; }
    public SecondaryMode? SecondaryMode { get; set; }
    public GrenadeLauncher GrenadeLauncherEnum { get; set; }
    public ChainBayonet ChainBayonetEnum { get; set; }
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

    //----------------------------  Temporary Properties  ----------------------------\\
    public Ammo.AmmoType AmmoType { get; set; }
    public int CurrentAmmo { get; set; }
    public bool WeaponIsJammed { get; set; }
    public short SuccessfulUnjamAttempts { get; set; }


    public enum WeaponCategory
    {
        [Description("Primitive")]
        Primitive = 0,
        [Description("Handgun")]
        Handgun = 1,
        [Description("SubMachineGun")]
        SubMachineGun = 2,
        [Description("Grenade Launcher")]
        GrenadeLauncher = 3,
        [Description("Assault Rifle")]
        AssaultRifle = 4,
        [Description("Sniper Rifle")]
        SniperRifle = 5,
        [Description("Shotgun")]
        Shotgun = 6,
        [Description("Light Machine Gun")]
        LightMachineGun = 7,
        [Description("Heavy Machine Gun")]
        HeavyMachinegun = 8,
        [Description("Rocket Launcher")]
        RocketLauncher = 9
    }
    public enum GrenadeLauncher
    {
        No = 0,
        Yes = 1,
        Optional = 2,
        Detachable = 3
    }
    public enum ChainBayonet
    {
        No = 0,
        Yes = 1,
        Optional = 2,
        Detachable = 3
    }

    public enum WeaponFunctionality
    {
        Manual = 0,
        SemiAutomatic = 1,
        FullAutomatic = 2,
        SemiAutomaticWith3RoundBurst = 3
    }
    public enum Range
    {
        Squares = 0, // Square = 1,5 Meter
        Meters = 1,
    }
}
