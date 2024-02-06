namespace MutantChroniclesAPI.Model.WeaponModel;


public class ChainBayonet //nullable values? maybe
{
    public bool Mountable { get; set; }
    public bool Equipped { get; set; }
    public string NameDescription { get; set; }
    public int Length { get; set; }
    public int DamageMin { get; set; }
    public int DamageMax { get; set;}
    public int DamageAdded { get; set; }

}
