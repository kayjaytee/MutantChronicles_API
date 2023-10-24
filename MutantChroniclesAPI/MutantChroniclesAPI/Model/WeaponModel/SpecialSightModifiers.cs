namespace MutantChroniclesAPI.Model.WeaponModel;

public class SpecialSightModifiers
{
    public bool Mountable { get; set; }
    public bool LaserSight { get; set; }
    public bool NightSight { get; set; }
    public bool TelescopicSight { get; set; }
    public int MinMagnification { get; set; }
    public int MaxMagnification { get; set; }
}
