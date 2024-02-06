using Microsoft.AspNetCore.Mvc;
using MutantChroniclesAPI.Enums;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Model.EnviromentModel;
using MutantChroniclesAPI.Model.WeaponModel;
using static MC_Weapon_Calculator.Model.Ammo;
using static MutantChroniclesAPI.Model.CharacterModel.Target;

namespace MutantChroniclesAPI.Interface;

public interface ICombatService
{
    public void AmmoHandler(Weapon weapon, FiringMode firingMode,RapidVolleyBulletsCount? rapidVolleyBulletsCount,ref int baseChance, ref int shotsToCalculate,bool secondaryModeActivated);
    public void ApplyDamageToBodyPart(BodyPart bodyPart, int damage, AmmoType? ammoType);
    public void ApplyStressToTarget(Character targetCharacter, Character characterCurrentTurn);
    public bool AvoidAttempt(Character target, FiringMode firingMode, Enviroment.Light light, Enviroment.Weather weather);
    public int CalculateHitChance(Character characterCurrentTurn, int baseChance, Enviroment.Light? light, Enviroment.Weather? weather);
    public void CheckForDefeatedCombatants(List<(Character character, int initiative)> initiativeOrder, List<Character> combatants, List<Character> defeated);
    public void CheckForWounds(List<Character> combatants);
    public void InitializeTemporaryBodyPoints(Target target);
    public bool IsFiringModeAllowed(Weapon weapon, FiringMode firingMode, bool secondaryMode);
    public bool IsMagazineEmptyCheck(Weapon weapon, bool? secondaryModeActivated);
    public List<(Character character, int combined)> InitiativeRoll(List<Character> combatants);
    public void RemoveWeaponJam(Character characterCurrentTurn, bool? secondaryModeActivated);
    public short ResetBurningCondition(Character character);
    public string StringBuilderFormatInitiative(List<(Character character, int initiative)> sortedCharacters, int round, Character? currentCharacterTurn);
    public void UpdateTurn(List<(Character character, int initiative)> initiativeOrder, Character characterCurrentTurn, int currentRound, string combatStatus);


}
