using MutantChroniclesAPI.Model.CharacterModel;

namespace MutantChroniclesAPI.Model;

public class BattleGrid
{

    public int Row { get; }
    public int Column { get; }
    public Character Character { get; set; }

}
