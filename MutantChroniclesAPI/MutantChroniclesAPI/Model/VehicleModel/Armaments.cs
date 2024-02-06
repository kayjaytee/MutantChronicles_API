namespace MutantChroniclesAPI.Model.VehicleModel;

public class Armament
{
    public required string Name { get; set; }
    //public WeaponFunctionality WeaponFunctionalityEnum { get; set; }
    public int AmmoCapacity { get; set; }
    public int DamageMin { get; set; }
    public int DamageMax { get; set; }
    public int DamageAdded { get; set; }
    public int Range { get; set; }

    //----------------------------  Temporary Effects  ----------------------------\\
    public int CurrentAmmo { get; set; }
    public bool Broken { get; set; } = false;
    public bool WeaponIsJammed { get; set; } = false;

}