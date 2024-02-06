using Microsoft.AspNetCore.Mvc;
using MutantChroniclesAPI.Enums;
using MutantChroniclesAPI.Interface;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Model.EnviromentModel;
using MutantChroniclesAPI.Model.WeaponModel;
using System.Text;
using static MC_Weapon_Calculator.Model.Ammo;
using static MutantChroniclesAPI.Model.CharacterModel.Target;

namespace MutantChroniclesAPI.Services.Combat;

public class CombatService : ICombatService
{
    public void InitializeTemporaryBodyPoints(Target target)
    {
        target.Head.TemporaryBodyPoints = target.Head.MaximumBodyPoints;
        target.Chest.TemporaryBodyPoints = target.Chest.MaximumBodyPoints;
        target.Stomach.TemporaryBodyPoints = target.Stomach.MaximumBodyPoints;
        target.RightArm.TemporaryBodyPoints = target.RightArm.MaximumBodyPoints;
        target.LeftArm.TemporaryBodyPoints = target.LeftArm.MaximumBodyPoints;
        target.RightLeg.TemporaryBodyPoints = target.RightLeg.MaximumBodyPoints;
        target.LeftLeg.TemporaryBodyPoints = target.LeftLeg.MaximumBodyPoints;
    }
    public List<(Character character, int combined)> InitiativeRoll(List<Character> combatants)
    {
        if (combatants is null)
        {
            throw new ArgumentNullException(nameof(combatants), "Combatants list cannot be null.");
        }

        var initiativeResults = new List<(Character character, int combined)>();
        foreach (var character in combatants)
        {
            int diceResult = Dice.Roll1D10();
            int combined = character.InitiativeBonus + diceResult;
            initiativeResults.Add((character, combined));
        }

        var initiativeOrder = initiativeResults
                             .OrderByDescending(x => x.combined)
                             .ThenByDescending(x => x.character.InitiativeBonus)
                             .ThenBy(x => Dice.Roll1D10()) //Tie-breaker; WIP
                             .ToList();

        return initiativeOrder;
    }
    public string StringBuilderFormatInitiative(List<(Character character, int initiative)> sortedCharacters, int round, Character? currentCharacterTurn)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Round {round}");

        foreach (var (character, initiative) in sortedCharacters)
        {
            stringBuilder.AppendLine($"{character.Name.ToUpperInvariant()} | Initiative: {initiative}");
            stringBuilder.AppendLine($"Actions: {character.ActionsRemaining}/{character.ActionsPerRound}");
            stringBuilder.AppendLine();
        }

        if (currentCharacterTurn is not null)
        {
            stringBuilder.AppendLine("-------------------------------");
            stringBuilder.AppendLine($"Current Turn: {currentCharacterTurn.Name.ToUpperInvariant()}");
            stringBuilder.AppendLine("-------------------------------");
        }

        var formatString = stringBuilder.ToString().Trim(); // Trim to remove leading/trailing whitespaces

        return formatString;
    }

    public void UpdateTurn(List<(Character character, int initiative)> initiativeOrder,
                            Character characterCurrentTurn,
                            int currentRound, string combatStatus)
    {

        if (initiativeOrder.Count <= 0)
        {
            //InitiativeOrder is empty, do nothing
            return;
        }

        // Check if the current character's actions are out
        if (characterCurrentTurn.ActionsRemaining <= 0)
        {
            // Find the index of the current character in the initiative order
            int currentIndex = initiativeOrder.FindIndex(entry => entry.character == characterCurrentTurn);

            // Move to the next character in the initiative order
            int nextIndex = (currentIndex + 1) % initiativeOrder.Count;
            characterCurrentTurn = initiativeOrder[nextIndex].character;
        }
        else
        {
            // Current character still has actions, do nothing
        }

        combatStatus = StringBuilderFormatInitiative(initiativeOrder, currentRound, characterCurrentTurn);
    }

    public void RemoveWeaponJam(Character characterCurrentTurn, bool? secondaryModeActivated)
    {
        characterCurrentTurn.MainHandEquipment.WeaponIsJammed = false;
        characterCurrentTurn.MainHandEquipment.SuccessfulUnjamAttempts = 0;
        switch (secondaryModeActivated)
        {
            case true:
                characterCurrentTurn.MainHandEquipment.SecondaryMode.WeaponIsJammed = false;
                characterCurrentTurn.MainHandEquipment.SecondaryMode.SuccessfulUnjamAttempts = 0;
                break;

            case false:
            case null:
                characterCurrentTurn.MainHandEquipment.WeaponIsJammed = false;
                characterCurrentTurn.MainHandEquipment.SuccessfulUnjamAttempts = 0;
                break;
        }
    } //MISSING TEST

    public bool IsFiringModeAllowed(Weapon weapon, FiringMode firingMode, bool secondaryMode)
    {
        var function = weapon.WeaponFunctionalityEnum;
        if (secondaryMode is true)
        {
            function = weapon.SecondaryMode.WeaponFunctionalityEnum;
        }

        switch (function)
        {
            case Weapon.WeaponFunctionality.Manual:
                return firingMode is FiringMode.SingleRound;

            case Weapon.WeaponFunctionality.SemiAutomatic:
                return firingMode is FiringMode.SingleRound ||
                       firingMode is FiringMode.RapidVolley;

            case Weapon.WeaponFunctionality.FullAutomatic:
                return true; // All firing modes are allowed

            case Weapon.WeaponFunctionality.SemiAutomaticWith3RoundBurst:
                return firingMode is FiringMode.SingleRound ||
                       firingMode is FiringMode.RapidVolley ||
                       firingMode is FiringMode.Burst;

            default:
                return false;
        }
    }

    public void CheckForWounds(List<Character> combatants)
    {
        foreach (var character in combatants)
        {
            //Temporary List to keep track of body parts wounded
            List<BodyPart> WoundedBodyParts = new List<BodyPart>();
            List<BodyPart> ZeroBodyPointsLeftBodyPart = new List<BodyPart>();

            foreach (var bodyPart in new[]
            { character.Target.Head,
              character.Target.Chest,
              character.Target.Stomach,
              character.Target.RightArm,
              character.Target.LeftArm,
              character.Target.RightLeg,
              character.Target.LeftLeg })
            {
                if (bodyPart.TemporaryBodyPoints < bodyPart.MaximumBodyPoints)
                {
                    WoundedBodyParts.Add(bodyPart);
                }

                if (bodyPart.TemporaryBodyPoints <= 0)
                {
                    ZeroBodyPointsLeftBodyPart.Add(bodyPart);
                }

                if (bodyPart.TemporaryBodyPoints <= bodyPart.MaximumBodyPoints - 2)
                {
                    character.Wounds = Character.EnviromentWounds.OneOrTwoHitsInOneBodyPart;
                }
                if (bodyPart.TemporaryBodyPoints <= bodyPart.MaximumBodyPoints - 3)
                {
                    character.Wounds = Character.EnviromentWounds.ThreeOrFourHitsInOneBodyPart;
                }
            }

            //Checks for the list for severe 
            switch (WoundedBodyParts.Count)
            {
                case >= 2:
                    character.Wounds = Character.EnviromentWounds.YouAreWoundedInMoreThanOneBodyPart;
                    break;

                default:
                    break;
            }
            switch (ZeroBodyPointsLeftBodyPart.Count)
            {
                case 1:
                    character.Wounds = Character.EnviromentWounds.OneBodyPartHasZeroBodyPointsLeft;
                    break;
                case >= 2:
                    character.Wounds = Character.EnviromentWounds.TwoOrMoreBodyPartsHaveNoBodyPointsLeft;
                    break;

                default:
                    break;
            }

            switch (character.Target.Head.TemporaryBodyPoints)
            {
                case 1:
                    character.HeadHasOneBodyPointRemaining.Equals(Character.HeadOneBodyPointRemaining.True);
                    break;
                default:
                    character.HeadHasOneBodyPointRemaining.Equals(Character.HeadOneBodyPointRemaining.False);
                    break;
            }

            switch (character.Target.Chest.TemporaryBodyPoints)
            {
                case 1:
                    character.ChestHasOneBodyPointRemaining.Equals(Character.ChestOneBodyPointRemaining.True);
                    break;
                default:
                    character.ChestHasOneBodyPointRemaining.Equals(Character.ChestOneBodyPointRemaining.False);
                    break;
            }

            switch (character.Target.Stomach.TemporaryBodyPoints)
            {
                case 1:
                    character.StomachHasOneBodyPointRemaining.Equals(Character.StomachOneBodyPointRemaining.True);
                    break;
                default:
                    character.ChestHasOneBodyPointRemaining.Equals(Character.StomachOneBodyPointRemaining.False);
                    break;
            }

            character.CalculateActionsPerRound();
            character.ActionsPerRound = character.ActionsPerRound -
                      (int)character.HeadHasOneBodyPointRemaining -
                      (int)character.ChestHasOneBodyPointRemaining -
                      (int)character.StomachHasOneBodyPointRemaining;

            switch (character.Target.RightArm.TemporaryBodyPoints)
            {
                case 1:
                    character.RightArmHasOneBodyPointRemaining.Equals(Character.RightArmOneBodyPointRemaining.True);
                    break;
                default:
                    character.HeadHasOneBodyPointRemaining.Equals(Character.RightArmOneBodyPointRemaining.False);
                    break;
            }

            switch (character.Target.LeftArm.TemporaryBodyPoints)
            {
                case 1:
                    character.RightArmHasOneBodyPointRemaining.Equals(Character.RightArmOneBodyPointRemaining.True);
                    break;
                default:
                    character.HeadHasOneBodyPointRemaining.Equals(Character.RightArmOneBodyPointRemaining.False);
                    break;
            }

            //Arm debuffs are applied during action

            switch (character.Target.RightLeg.TemporaryBodyPoints)
            {
                case 1:
                    character.RightLegHasOneBodyPointRemaining.Equals(Character.RightLegOneBodyPointRemaining.True);
                    break;
                default:
                    character.RightLegHasOneBodyPointRemaining.Equals(Character.RightLegOneBodyPointRemaining.False);
                    break;
            }

            switch (character.Target.LeftLeg.TemporaryBodyPoints)
            {
                case 1:
                    character.LeftLegHasOneBodyPointRemaining.Equals(Character.LeftLegOneBodyPointRemaining.True);
                    break;
                default:
                    character.LeftLegHasOneBodyPointRemaining.Equals(Character.LeftLegOneBodyPointRemaining.False);
                    break;
            }

            character.CalculateMovementAllowance();
            //character.MovementAllowance.Meters = character.MovementAllowance.Meters -
            //    (int)character.RightLegHasOneBodyPointRemaining -
            //    (int)character.LeftLegHasOneBodyPointRemaining; //FIGURING OUT SYNTAX HERE
        }
    }

    public void CheckForDefeatedCombatants(List<(Character character, int initiative)> initiativeOrder,
                                            List<Character> combatants,
                                            List<Character> defeated)
    {
        for (int i = combatants.Count - 1; i >= 0; i--)
        {
            var character = combatants[i];
            bool isUnconciousOrDefeated =
                    character.Target.Head.TemporaryBodyPoints <= 0 ||
                    character.Target.Chest.TemporaryBodyPoints <= 0 ||
                    character.Target.Stomach.TemporaryBodyPoints <= 0 ||
                    character.Target.TemporaryBodyPoints <= 0;

            if (isUnconciousOrDefeated)
            {
                defeated.Add(character);
                combatants.RemoveAt(i);
                initiativeOrder.RemoveAt(i);
            }
        }
    }

    public bool IsMagazineEmptyCheck(Weapon weapon, bool? secondaryModeActivated)
    {
        //Check for empty magazine-clip
        if (weapon.SecondaryMode is not null)
        {
            if (weapon.SecondaryMode.CurrentAmmo <= 0 && secondaryModeActivated is true)
            {
                return true;
            }
        }
        if (weapon.CurrentAmmo <= 0)
        {
            return true;
        }

        return false;
    }
    public void ApplyStressToTarget(Character targetCharacter, Character characterCurrentTurn)
    {
        //Stress Mechanic
        //Stronger Stress Effects will override the default case
        switch (targetCharacter.Stress)
        {
            case Character.EnviromentStress.WARNINGThreeSecondsToAutoDestruct:
            case Character.EnviromentStress.YourClothesAreOnFire:
            case Character.EnviromentStress.YouAreMidairFallingTowardCertainDeath:
                break;

            default:
                if (!targetCharacter.CurrentlyUnderFire.Contains(characterCurrentTurn))
                {
                    targetCharacter.CurrentlyUnderFire.Add(characterCurrentTurn);
                }
                switch (targetCharacter.CurrentlyUnderFire.Count)
                {

                    case 0:
                        targetCharacter.Stress = Character.EnviromentStress.None;
                        break;

                    case 1:
                        targetCharacter.Stress = Character.EnviromentStress.SomeoneFiresAtYou;
                        break;

                    case >= 2:
                        targetCharacter.Stress = Character.EnviromentStress.PeopleFireAtYouFromSeveralDirections;
                        break;

                    default:
                        break;
                }
                if (targetCharacter.BurningCondition >= 1)
                {
                    targetCharacter.Stress = Character.EnviromentStress.YourClothesAreOnFire;
                }
                break;
        }

    }
    public short ResetBurningCondition(Character character)
    {
        return character.BurningCondition = 0;
    }
    public void BurningConditionTick(List<Character> combatants)
    {
        foreach (var character in combatants)
        {
            Predicate<Armor> armorDestroyed = equipment => equipment.Absorb == 0;
            if (character.BurningCondition >= 1)
            {
                switch (character.BurningCondition)
                {
                    case 0:
                    default:
                        break;

                    case <= 10:
                        foreach (var bodyPart in new[]
                        {   character.Target.Head,
                            character.Target.Chest,
                            character.Target.Stomach,
                            character.Target.RightArm,
                            character.Target.LeftArm,
                            character.Target.RightLeg,
                            character.Target.LeftLeg })
                        {
                            foreach (var equipment in character.Armor)
                            {
                                switch (equipment.Material)
                                {
                                    case Armor.ArmorMaterial.BulletProof:
                                    case Armor.ArmorMaterial.LightCombat:
                                    case Armor.ArmorMaterial.HeavyCombat:
                                    case Armor.ArmorMaterial.ExtraHeavyCombat:

                                        //IF ANY FIREPROOF ARMOR IS EQUIPPED,
                                        //IT WILL PROTECT OVERLAP THE SENSITIVE EQUIPMENT

                                        break;
                                    default:
                                        switch (equipment.Material)
                                        {
                                            case Armor.ArmorMaterial.Cloth:
                                            case Armor.ArmorMaterial.Plastic:
                                            case Armor.ArmorMaterial.Ballistic:
                                                //equipment.Absorb--;
                                                if (equipment.Absorb == 0)
                                                {
                                                    character.Armor.RemoveAll(armorDestroyed);
                                                    character.UpdateArmorValuesForBodyParts();
                                                }
                                                else
                                                {
                                                    bodyPart.TemporaryBodyPoints--;
                                                }

                                                break;
                                        }
                                        break;
                                }
                            }

                        }
                        break;

                    case > 10:
                        foreach (var bodyPart in new[]
                        {   character.Target.Head,
                            character.Target.Chest,
                            character.Target.Stomach,
                            character.Target.RightArm,
                            character.Target.LeftArm,
                            character.Target.RightLeg,
                            character.Target.LeftLeg })
                        {
                            //Regardless of Fireproof Armor; if the burning tick passes 10+, it will start to damage regardless
                            bodyPart.TemporaryBodyPoints--;

                        }

                        character.BurningCondition++;
                        break;
                }
                character.BurningCondition++;
            }
        }

    }

    public void ApplyDamageToBodyPart(BodyPart bodyPart, int damage, AmmoType? ammoType)
    {
        int armorValue = bodyPart.ArmorValues.Sum(x => x.Absorb);
        int guaranteedAbsorb = 0;
        foreach (var equipment in bodyPart.ArmorValues)
        {
            var roll = Dice.RollBlockChance(equipment.BlockChance);
            if (roll is false || equipment.RangeProtection is false)
            {
                armorValue = armorValue - equipment.Absorb;
            }
        }

        if (armorValue > 10)
        {
            for (int i = 10; i < armorValue; i++)
            {
                guaranteedAbsorb++;
            }
        }
        int absorbedDamage = Math.Min(armorValue, Dice.Roll1D10()) + guaranteedAbsorb;

        int netDamage = ammoType switch
        {
            AmmoType.ArmorPenetration => Math.Max(0, (int)Math.Ceiling((damage * 2 - absorbedDamage) * 0.5)),
            AmmoType.Hardballs => Math.Max(0, damage * 2 - absorbedDamage),
            _ => Math.Max(0, damage - absorbedDamage) // Default case for Standard or null
        };

        // Apply the net damage to the body part
        bodyPart.TemporaryBodyPoints -= netDamage;
    }

    public int CalculateHitChance(Character characterCurrentTurn, int baseChance, Enviroment.Light? light, Enviroment.Weather? weather)
    {
        int strengthModifier = 0;

        if (light is null)
        {
            light = 0;
        }
        if (weather is null)
        {
            weather = 0;
        }

        if (characterCurrentTurn.MainHandEquipment is not null)
        {
            if (characterCurrentTurn.MainHandEquipment.StrengthRequirement > characterCurrentTurn.Strength)
            {
                strengthModifier = characterCurrentTurn.Strength - characterCurrentTurn.Strength;
            }
        }

        int lightModifier = (int)light;
        int weatherModifier = (int)weather;
        int stressModifier = (int)characterCurrentTurn.Stress;
        int woundsModifier = (int)characterCurrentTurn.Wounds;
        int rightArmModifier = (int)characterCurrentTurn.RightArmHasOneBodyPointRemaining;
        int leftArmModifier = (int)characterCurrentTurn.LeftArmHasOneBodyPointRemaining;

        int modifiedHitChance = baseChance -
                                lightModifier -
                                weatherModifier -
                                stressModifier -
                                woundsModifier -
                                rightArmModifier -
                                leftArmModifier -
                                strengthModifier;


        return modifiedHitChance;
    }

    public bool AvoidAttempt(Character target, FiringMode firingMode, Enviroment.Light light, Enviroment.Weather weather)
    {
        if (target.IsAvoiding is true && target.ActionsRemaining >= 1)
        {
            int roll = Dice.Roll1D20();
            if (firingMode is FiringMode.AreaSpray)
            {
                roll = roll + 3;
            }
            if (roll <= 10 && target.DefensiveBonus - (int)target.Stress - (int)target.Wounds - (int)weather - (int)light >= roll)
            {
                return true;
            }

        }
        return false;
    }

    public void AmmoHandler(Weapon weapon,
                            FiringMode firingMode,
                            RapidVolleyBulletsCount? rapidVolleyBulletsCount,
                            ref int baseChance, ref int shotsToCalculate,
                            bool secondaryModeActivated)
    {

        switch (firingMode)
        {
            case FiringMode.SingleRound:
                weapon.CurrentAmmo--;
                shotsToCalculate = 1;
                break;
            case FiringMode.Burst:
                if (weapon.CurrentAmmo >= 3)
                {
                    weapon.CurrentAmmo = weapon.CurrentAmmo - 3;
                    shotsToCalculate = 2;
                }
                else
                {
                    weapon.CurrentAmmo = 0;
                    shotsToCalculate = 1;
                }
                break;
            case FiringMode.FullAuto:
                if (weapon.CurrentAmmo >= 10)
                {
                    weapon.CurrentAmmo = weapon.CurrentAmmo - 10;
                    shotsToCalculate = 3;
                }
                else
                {
                    weapon.CurrentAmmo = 0;
                    shotsToCalculate = 1;
                }
                break;
            case FiringMode.RapidVolley:
                switch (rapidVolleyBulletsCount)
                {
                    case (RapidVolleyBulletsCount?)(int)RapidVolleyBulletsCount.Two:
                        weapon.CurrentAmmo = weapon.CurrentAmmo - 2;
                        shotsToCalculate = 2;
                        baseChance -= 4;
                        break;
                    case (RapidVolleyBulletsCount?)(int)RapidVolleyBulletsCount.Three:
                        weapon.CurrentAmmo = weapon.CurrentAmmo - 3;
                        shotsToCalculate = 3;
                        baseChance -= 6;
                        break;
                    case (RapidVolleyBulletsCount?)(int)RapidVolleyBulletsCount.Four:
                        weapon.CurrentAmmo = weapon.CurrentAmmo - 4;
                        shotsToCalculate = 4;
                        baseChance -= 8;
                        break;
                    case (RapidVolleyBulletsCount?)(int)RapidVolleyBulletsCount.Five:
                        weapon.CurrentAmmo = weapon.CurrentAmmo - 5;
                        shotsToCalculate = 5;
                        baseChance -= 10;
                        break;
                    default:
                        return;
                }
                break;
            case FiringMode.AreaSpray:
                if (weapon.CurrentAmmo >= 20)
                {
                    weapon.CurrentAmmo = weapon.CurrentAmmo - 20;
                    shotsToCalculate = 1;
                }
                else
                {
                    weapon.CurrentAmmo = 0;
                    shotsToCalculate = 1;
                }
                break;
            default:
                return;
        }

        // Check if ammo went below 0
        if (weapon.CurrentAmmo < 0)
        {
            weapon.CurrentAmmo = 0; // Reset ammo to 0
        }
    }
}
