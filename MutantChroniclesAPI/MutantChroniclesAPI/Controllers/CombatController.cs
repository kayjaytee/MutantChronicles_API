using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;
using MutantChroniclesAPI.Enums;
using MutantChroniclesAPI.Model;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Model.WeaponModel;
using MutantChroniclesAPI.Repository;
using MutantChroniclesAPI.Services.Combat;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static MC_Weapon_Calculator.Model.Ammo;
using static MutantChroniclesAPI.Model.CharacterModel.Target;

namespace MutantChroniclesAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CombatController : ControllerBase
{
    private static List<Character> combatants;
    private static List<Character> defeated;
    private static List<(Character character, int initiative)> initiativeOrder;
    private static Character characterCurrentTurn;

    private static int CurrentRound = 1;
    private static bool CombatInProgress;
    private static string combatStatus;
    private int initiative;


    [HttpPost("StartCombat")]
    public async Task<IActionResult> StartCombat()
    {
        try
        {
            if (CombatInProgress is true)
            {
                return BadRequest("Combat is already in progress");
            }
            if (CharacterRepository.Characters.Count < 1)
            {
                return NotFound("There are no characters created.");
            }

            CombatInProgress = true;
            characterCurrentTurn = null;

            // Set the combatants list with characters from the CharacterRepository
            combatants = CharacterRepository.Characters.ToList();
            defeated = new List<Character>();
            foreach (var character in combatants)
            {
                character.ActionsRemaining = character.ActionsPerRound;
                InitializeTemporaryBodyPoints(character.Target);
            }

            initiativeOrder = await InitiativeRoll(combatants);
            if (initiativeOrder.Count > 0)
            {
                characterCurrentTurn = initiativeOrder[0].character; // Set the character who won the initiative as current turn
            }

            combatStatus = StringBuilderFormatInitiative(initiativeOrder, CurrentRound, characterCurrentTurn);

            return Ok(combatStatus);

        }
        catch (Exception e)
        {
            return BadRequest(e);
        }


    }

    [HttpGet("DisplayTurn")]
    public async Task<IActionResult> DisplayTurn()
    {
        if (CombatInProgress is false)
        {
            return BadRequest("No combat in progress.");
        }
        else
        {
            return Ok(combatStatus);
        }

    }

    [HttpGet("DisplayStats")]
    public async Task<IActionResult> DisplayStats()
    {
        if (CombatInProgress is false)
        {
            return BadRequest("No combat in progress.");
        }
        else
        {
            var stringBuilder = new StringBuilder();
            foreach (var combatant in combatants)
            {
                stringBuilder.AppendLine($"Combatant: {combatant.Name}");

                stringBuilder.AppendLine($"Head BP {combatant.Target.Head.TemporaryBodyPoints} | AV: {combatant.Target.Head.ArmorValue}");
                stringBuilder.AppendLine($"Chest BP {combatant.Target.Chest.TemporaryBodyPoints} | AV: {combatant.Target.Chest.ArmorValue}");
                stringBuilder.AppendLine($"Stomach BP {combatant.Target.Stomach.TemporaryBodyPoints} | AV: {combatant.Target.Stomach.ArmorValue}");
                stringBuilder.AppendLine($"RightArm BP {combatant.Target.RightArm.TemporaryBodyPoints} | AV: {combatant.Target.RightArm.ArmorValue}");
                stringBuilder.AppendLine($"LeftArm BP {combatant.Target.LeftArm.TemporaryBodyPoints} | AV: {combatant.Target.LeftArm.ArmorValue}");
                stringBuilder.AppendLine($"RightLeg BP {combatant.Target.RightLeg.TemporaryBodyPoints} | AV: {combatant.Target.RightLeg.ArmorValue}");
                stringBuilder.AppendLine($"LeftLeg BP {combatant.Target.LeftLeg.TemporaryBodyPoints} | AV: {combatant.Target.LeftLeg.ArmorValue}");
                stringBuilder.AppendLine();
                if (combatant.EquippedWeapon is not null)
                {
                    stringBuilder.AppendLine($"Equipped Weapon: {combatant.EquippedWeapon.Description}: <{combatant.EquippedWeapon.Name}>");
                    stringBuilder.AppendLine($"Magazine: {combatant.EquippedWeapon.CurrentAmmo}/{combatant.EquippedWeapon.MagazineCapacity}");
                }
                else
                {
                    stringBuilder.AppendLine($"Equipped Weapon: Unarmed");
                }

                stringBuilder.AppendLine(); // Add a blank line between combatants
            }

            foreach (var combatant in defeated)
            {
                stringBuilder.AppendLine($"Defeated: {combatant.Name}");
                if (combatant.Target.Head.TemporaryBodyPoints <= 0)
                {
                    stringBuilder.AppendLine("Took a critical blow to the head.");
                }
                if (combatant.Target.Chest.TemporaryBodyPoints <= 0)
                {
                    stringBuilder.AppendLine("Took a critical blow to the chest.");
                }
                if (combatant.Target.Stomach.TemporaryBodyPoints <= 0)
                {
                    stringBuilder.AppendLine("Took a critical blow to the stomach");
                }

                stringBuilder.AppendLine(); // Add a blank line between combatants
            }


            var statsString = stringBuilder.ToString().Trim(); // Trim to remove leading/trailing whitespaces

            return Ok(statsString);
        }
    }


    [HttpPost("NextRound")]
    public async Task<IActionResult> NextRound()
    {
        if (CombatInProgress is false)
        {
            return BadRequest("No combat in progress.");
        }
        else
        {
            CheckForDefeatedCombatants();
            CurrentRound++;
            initiativeOrder = await InitiativeRoll(combatants);

            // Reset ActionsRemaining for all combatants
            foreach (var character in combatants)
            {
                character.ActionsRemaining = character.ActionsPerRound;
            }

            if (initiativeOrder.Count > 0)
            {
                characterCurrentTurn = initiativeOrder[0].character; // Set the character who won the initiative as current turn
            }

            combatStatus = StringBuilderFormatInitiative(initiativeOrder, CurrentRound, characterCurrentTurn);

            return Ok(combatStatus);
        }
    }
    /// <summary>
    /// Perform the action.
    /// </summary>
    /// <param name="baseChance">Base chance value.</param>
    /// <param name="firingMode">Firing mode.</param>
    /// <param name="bulletsCount">Number of bullets in RapidVolley (only for RapidVolley).</param>
    /// <param name="aim">Aim type.</param>
    /// <returns>Action result.</returns>
    [HttpOptions("Action/Shoot")]
    public async Task<IActionResult> ActionShoot(int baseChance,
                                            bool secondaryModeActivated,
                                            [Required][FromQuery] FiringMode firingMode,
                                            [Required][FromQuery] AimType aim,
                                            [FromQuery] string target,
                                            [FromQuery] decimal range,
                                            [FromQuery] RapidVolleyBulletsCount? rapidVolleyBulletsCount = null)
    {
        int shotsToCalculate = 0;

        if (characterCurrentTurn.ActionsRemaining <= 0)
        {
            return BadRequest(characterCurrentTurn.Name + " has ran out of actions.");
        }
        if (CombatInProgress is false)
        {
            return BadRequest("No combat in progress.");
        }
        if (firingMode is FiringMode.AreaSpray)
        {
            aim = AimType.Uncontrolled; //Area Spray is always considered Uncontrolled
        }

        var weapon = characterCurrentTurn.EquippedWeapon;
        if (weapon is null)
        {
            return BadRequest("Character is not carrying a ranged weapon.");
        }

        var targetCharacter = combatants.FirstOrDefault(x => string.Equals(x.Name, target, StringComparison.OrdinalIgnoreCase));
        if (targetCharacter is null || targetCharacter == characterCurrentTurn)
        {
            return BadRequest("Invalid target.");
        }
        if (IsFiringModeAllowed(weapon, firingMode, secondaryModeActivated) is false)
        {
            return BadRequest("Firing mode not allowed for this weapon.");
        }

        var aimingMode = ApplyAimingMode(aim, weapon, ref characterCurrentTurn, range, ref baseChance);
        if (aimingMode is BadRequestObjectResult)
        {
            return BadRequest(); //improve desc.
        }

        // Update ActionsRemaining in combatants list
        int characterIndex = combatants.FindIndex(x => x == characterCurrentTurn);
        if (characterIndex is not -1)
        {
            combatants[characterIndex].ActionsRemaining = characterCurrentTurn.ActionsRemaining;
        }

        var checkForEmptyMagazine = CheckForEmptyMagazine(weapon, secondaryModeActivated);
        if (checkForEmptyMagazine is OkObjectResult)
        {
            return Ok("Out of ammo! You need to reload your magazine.");
        }

        AmmoHandler(weapon, firingMode, rapidVolleyBulletsCount, ref baseChance, ref shotsToCalculate, secondaryModeActivated);

        StringBuilder resultBuilder = new StringBuilder();

        bool isCriticalHitApplied = false; //For controlling multipleTargetAreas. Only 1 body part can take critical damage per shot

        for (int i = 0; i < shotsToCalculate; i++)
        {

            int d20HitRoll = Dice.Roll1D20();

            bool isHit = d20HitRoll <= baseChance;

            bool critical = d20HitRoll == 1; // Check if it's a critical hit
            bool fumble = d20HitRoll == 20; // Check if user fumbled

            resultBuilder.AppendLine($"Chance of Success: {baseChance}");
            resultBuilder.AppendLine($"{characterCurrentTurn.Name.ToUpper()} fires at {targetCharacter.Name.ToUpper()} and...");
            resultBuilder.AppendLine($"Roll: {d20HitRoll}");


            if (fumble)
            {
                resultBuilder.AppendLine($"Whoops! You jammed your weapon...");
                int d10JamRoll = Dice.Roll1D10();
                if (d10JamRoll > characterCurrentTurn.EquippedWeapon.JammingFactor)
                {
                    resultBuilder.AppendLine($"...severly...");
                    characterCurrentTurn.EquippedWeapon.WeaponIsJammed = true;
                }

                int actionsLost = characterCurrentTurn.ActionsRemaining - Dice.Roll1D6();
                characterCurrentTurn.ActionsRemaining -= actionsLost;
                if (characterCurrentTurn.ActionsRemaining < 0)
                {
                    characterCurrentTurn.ActionsRemaining = 0;
                }
                resultBuilder.AppendLine($"...and lost {actionsLost} actions.");
                CheckForDefeatedCombatants();
                UpdateTurn(initiativeOrder);
                return Ok(resultBuilder.ToString());
            }
            if (critical)
            {
                resultBuilder.AppendLine($"CRITICAL HIT!");
            }
            else
            {
                resultBuilder.AppendLine(isHit ? "HITS!" : "MISS!");

            }

            if (isHit || d20HitRoll == 1)
            {

                int multipleTargetAreas = weapon.TargetMultipleHits > 0 ? Dice.RollTargetAreas(weapon.TargetMultipleHits) : 1;
                if (weapon.Category is Weapon.WeaponCategory.Shotgun)
                {
                    // For shotguns, always hit 2 body parts on Hits
                    multipleTargetAreas = 2;
                }

                for (int y = 0; y < multipleTargetAreas; y++)
                {

                    int d20Result = Dice.Roll1D20();
                    var hitLocation = targetCharacter.Target;


                    int damage = Dice.RollDamage(weapon.DamageMin,
                                    weapon.DamageMax,
                                    weapon.DamageAdded);

                    if (critical)
                    {
                        damage = weapon.DamageMax + weapon.DamageAdded;
                        isCriticalHitApplied = true;
                    }
                    resultBuilder.AppendLine($"{targetCharacter.Name.ToUpper()} takes {damage}");
                    switch (d20Result)
                    {
                        case int x when x <= 3:
                            ApplyDamageToBodyPart(hitLocation.LeftLeg, damage, weapon.AmmoType);
                            resultBuilder.AppendLine($"in Left Leg!");
                            break;
                        case int x when x <= 6:
                            ApplyDamageToBodyPart(hitLocation.RightLeg, damage, weapon.AmmoType);
                            resultBuilder.AppendLine($"in Right Leg!");
                            break;
                        case int x when x <= 8:
                            ApplyDamageToBodyPart(hitLocation.LeftArm, damage, weapon.AmmoType);
                            resultBuilder.AppendLine($"in Left Arm!");
                            break;
                        case int x when x <= 10:
                            ApplyDamageToBodyPart(hitLocation.RightArm, damage, weapon.AmmoType);
                            resultBuilder.AppendLine($"in Right Arm!");
                            break;
                        case int x when x <= 14:
                            ApplyDamageToBodyPart(hitLocation.Stomach, damage, weapon.AmmoType);
                            resultBuilder.AppendLine($"in Stomach!");
                            break;
                        case int x when x <= 19:
                            ApplyDamageToBodyPart(hitLocation.Chest, damage, weapon.AmmoType);
                            resultBuilder.AppendLine($"in Chest!");
                            break;
                        default: //when x == 20
                            ApplyDamageToBodyPart(hitLocation.Head, damage, weapon.AmmoType);
                            resultBuilder.AppendLine($"in Head!");
                            break;
                    }
                    resultBuilder.AppendLine();
                }

                isCriticalHitApplied = false; // Reset the flag for the next shot
            }
            else
            {
                if (firingMode == FiringMode.Burst || firingMode == FiringMode.FullAuto)
                {
                    // If burst/full auto fails due to a miss, break the loop
                    break;
                    resultBuilder.AppendLine($"(d20Result: {d20HitRoll})");
                }
            }

        }

        CheckForDefeatedCombatants();
        if (combatants.Count >= 2)
        {
            UpdateTurn(initiativeOrder);
        }
        return Ok(resultBuilder.ToString());
    }

    [HttpPost("Action/Reload")]
    public async Task<IActionResult> ActionReload(int baseChance)
    {
        if (characterCurrentTurn.EquippedWeapon is null)
        {
            return BadRequest("You don't have a weapon equipped");
        }
        if (characterCurrentTurn.EquippedWeapon.WeaponIsJammed is true)
        {
            return BadRequest("Your weapon is jammed!");
        }

        if (characterCurrentTurn.ActionsRemaining < characterCurrentTurn.EquippedWeapon.ReloadingTime)
        {
            return BadRequest("You don't have enough actions to start reloading your weapon");
        }

        int d20HitRoll = Dice.Roll1D20();
        bool isSuccessful = d20HitRoll <= baseChance;

        if (d20HitRoll == 20)
        {
            characterCurrentTurn.ActionsRemaining -= characterCurrentTurn.EquippedWeapon.ReloadingTime;
            UpdateTurn(initiativeOrder);
            return Ok("$Whoops! Your magazine dropped on the ground...");
        }
        else if (isSuccessful || d20HitRoll != 20)
        {
            characterCurrentTurn.ActionsRemaining -= characterCurrentTurn.EquippedWeapon.ReloadingTime;
            characterCurrentTurn.EquippedWeapon.CurrentAmmo = characterCurrentTurn.EquippedWeapon.MagazineCapacity;
            UpdateTurn(initiativeOrder);
            return Ok($"{characterCurrentTurn.EquippedWeapon.Name} reloaded with a new magazine!");
        }
        else
        {
            characterCurrentTurn.ActionsRemaining -= characterCurrentTurn.EquippedWeapon.ReloadingTime;
            UpdateTurn(initiativeOrder);
            return Ok("$You didn't fit the magazine properly...Try agian!");
        }


    }

    [HttpOptions("Action/Melee")]
    public async Task<IActionResult> ActionMelee(int baseChance, [FromQuery] string target)
    {
        //ADJUSTMENTS
        if (characterCurrentTurn.ActionsRemaining <= 0)
        {
            return BadRequest(characterCurrentTurn.Name + " has ran out of actions.");
        }
        if (CombatInProgress is false)
        {
            return BadRequest("No combat in progress.");
        }
        Weapon weapon = characterCurrentTurn.EquippedWeapon;
        Character targetCharacter = combatants.FirstOrDefault(x => x.Name == target);

        if (targetCharacter is null || targetCharacter == characterCurrentTurn)
        {
            return BadRequest("Invalid target.");
        }

        characterCurrentTurn.ActionsRemaining--;

        // Update ActionsRemaining in combatants list
        int characterIndex = combatants.FindIndex(x => x == characterCurrentTurn);
        if (characterIndex is not -1)
        {
            combatants[characterIndex].ActionsRemaining = characterCurrentTurn.ActionsRemaining;
        }

        UpdateTurn(initiativeOrder);
        StringBuilder resultBuilder = new StringBuilder();

        int d20HitRoll = Dice.Roll1D20();
        bool isHit = d20HitRoll <= baseChance;

        bool critical = d20HitRoll == 1; // Check if it's a critical hit
        bool fumble = d20HitRoll == 20; // Check if user fumbled

        resultBuilder.AppendLine($"Chance of Success: {baseChance}");
        resultBuilder.AppendLine($"{characterCurrentTurn.Name.ToUpper()} hits {targetCharacter.Name.ToUpper()} in melee and...");
        resultBuilder.AppendLine($"Roll: {d20HitRoll}");

        if (critical)
        {
            resultBuilder.AppendLine($"CRITICAL HIT!");
            //Missing damage calculation for crit
        }
        else
        {
            resultBuilder.AppendLine(isHit ? "HITS!" : "MISS!");
        }

        if (isHit || d20HitRoll == 1)
        {

            int d20Result = Dice.Roll1D20();
            var hitLocation = targetCharacter.Target;
            int damage = 0;

            if (weapon is not null)
            {
                damage = CalculateMeleeAttack(characterCurrentTurn, weapon);
            }
            if (weapon is null)
            {
                damage = Dice.RollDamage(1, 3, characterCurrentTurn.OffensiveBonus);

            }


            resultBuilder.AppendLine($"{targetCharacter.Name.ToUpper()} takes {damage}");
            switch (d20Result)
            {
                case int x when x <= 3:
                    ApplyDamageToBodyPart(hitLocation.LeftLeg, damage, null);
                    resultBuilder.AppendLine($"in Left Leg!");
                    break;
                case int x when x <= 6:
                    ApplyDamageToBodyPart(hitLocation.RightLeg, damage, null);
                    resultBuilder.AppendLine($"in Right Leg!");
                    break;
                case int x when x <= 9:
                    ApplyDamageToBodyPart(hitLocation.LeftArm, damage, null);
                    resultBuilder.AppendLine($"in Left Arm!");
                    break;
                case int x when x <= 12:
                    ApplyDamageToBodyPart(hitLocation.RightArm, damage, null);
                    resultBuilder.AppendLine($"in Right Arm!");
                    break;
                case int x when x <= 15:
                    ApplyDamageToBodyPart(hitLocation.Stomach, damage, null);
                    resultBuilder.AppendLine($"in Stomach!");
                    break;
                case int x when x <= 18:
                    ApplyDamageToBodyPart(hitLocation.Chest, damage, null);
                    resultBuilder.AppendLine($"in Chest!");
                    break;
                default: //when x == 20
                    ApplyDamageToBodyPart(hitLocation.Head, damage, null);
                    resultBuilder.AppendLine($"in Head!");
                    break;
            }
        }
        return Ok(resultBuilder.ToString());
    }


    [HttpPost("Action/Unjam")]
    public async Task<IActionResult> ActionUnjam(int baseChance)
    {
        if (characterCurrentTurn.EquippedWeapon is null)
        {
            return BadRequest("You don't have a weapon equipped");
        }
        if (characterCurrentTurn.EquippedWeapon.WeaponIsJammed is false)
        {
            return BadRequest("Your weapon is not jammed.");
        }

        characterCurrentTurn.ActionsRemaining = 0;
        int d20HitRoll = Dice.Roll1D20();
        bool isSuccessful = d20HitRoll <= baseChance;

        if (d20HitRoll == 1)
        {
            RemoveWeaponJam();
            UpdateTurn(initiativeOrder);
            return Ok("With critical success, the weapon got unjammed and is now ready for use agian!");
        }
        if (isSuccessful || d20HitRoll != 20)
        {
            characterCurrentTurn.EquippedWeapon.SuccessfulUnjamAttempts++;
            if (characterCurrentTurn.EquippedWeapon.SuccessfulUnjamAttempts > 2)
            {
                RemoveWeaponJam();
                UpdateTurn(initiativeOrder);
                return Ok("Weapon unjammed successfully.");
            }
            UpdateTurn(initiativeOrder);
            return Ok("You were successful, but need more time to unjam the weapon.");
        }
        UpdateTurn(initiativeOrder);
        return Ok("You were unsuccessful and have to attempt next turn.");
    }

    #region Combat Methods

    private IActionResult CheckForEmptyMagazine(Weapon weapon, bool secondaryModeActivated)
    {
        //Check for empty magazine-clip
        if (weapon.SecondaryMode is not null)
        {
            if (weapon.SecondaryMode.CurrentAmmo <= 0 && secondaryModeActivated is true)
            {
                return Ok("Out of ammo! You need to reload your magazine.");
            }
        }
        if (weapon.CurrentAmmo <= 0)
        {
            return Ok("Out of ammo! You need to reload your magazine.");
        }

        return null; //Continue
    }
    private IActionResult AmmoHandler(Weapon weapon,
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
                        return BadRequest("Invalid number of bullets for rapid volley.");
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
                return BadRequest();
        }

        // Check if ammo went below 0
        if (weapon.CurrentAmmo < 0)
        {
            weapon.CurrentAmmo = 0; // Reset ammo to 0
        }

        return Ok();
    }

    private void RemoveWeaponJam()
    {
        characterCurrentTurn.EquippedWeapon.WeaponIsJammed = false;
        characterCurrentTurn.EquippedWeapon.SuccessfulUnjamAttempts = 0;
    }
    private int CalculateMeleeAttack(Character character, Weapon weapon)
    {
        int damage = character.OffensiveBonus;

        //if bayonet == true > +2 dmg

        switch (weapon.Weight)
        {
            case decimal weight when weight < 5:
                damage += Dice.Roll1D4();
                break;
            case decimal weight when weight >= 5 && weight <= 20:
                damage += Dice.Roll1D4() + 1;
                break;
            case decimal weight when weight > 20:
                damage += Dice.Roll1D6();
                break;
            default:
                break;
        }

        return damage;

    }
    private void InitializeTemporaryBodyPoints(Target target)
    {
        target.Head.TemporaryBodyPoints = target.Head.MaximumBodyPoints;
        target.Chest.TemporaryBodyPoints = target.Chest.MaximumBodyPoints;
        target.Stomach.TemporaryBodyPoints = target.Stomach.MaximumBodyPoints;
        target.RightArm.TemporaryBodyPoints = target.RightArm.MaximumBodyPoints;
        target.LeftArm.TemporaryBodyPoints = target.LeftArm.MaximumBodyPoints;
        target.RightLeg.TemporaryBodyPoints = target.RightLeg.MaximumBodyPoints;
        target.LeftLeg.TemporaryBodyPoints = target.LeftLeg.MaximumBodyPoints;
    }
    private async Task<List<(Character character, int combined)>> InitiativeRoll(List<Character> combatants)
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
    private string StringBuilderFormatInitiative(List<(Character character, int initiative)> sortedCharacters, int round, Character? currentCharacterTurn)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Round {round}");

        foreach (var (character, initiative) in sortedCharacters)
        {
            stringBuilder.AppendLine($"{character.Name.ToUpper()} | Initiative: {initiative}");
            stringBuilder.AppendLine($"Actions: {character.ActionsRemaining}/{character.ActionsPerRound}");
            stringBuilder.AppendLine();
        }

        if (currentCharacterTurn is not null)
        {
            stringBuilder.AppendLine("-------------------------------");
            stringBuilder.AppendLine($"Current Turn: {currentCharacterTurn.Name.ToUpper()}");
            stringBuilder.AppendLine("-------------------------------");
        }

        var formatString = stringBuilder.ToString().Trim(); // Trim to remove leading/trailing whitespaces

        return formatString;
    }
    private bool IsFiringModeAllowed(Weapon weapon, FiringMode firingMode, bool secondaryMode)
    {
        var function = weapon.WeaponFunctionalityEnum;
        if (secondaryMode is true)
        {
            function = weapon.SecondaryMode.WeaponFunctionalityEnum;
        }

        switch (function)
        {
            case Weapon.WeaponFunctionality.Manual:
                return firingMode == FiringMode.SingleRound;

            case Weapon.WeaponFunctionality.SemiAutomatic:
                return firingMode == FiringMode.SingleRound ||
                       firingMode == FiringMode.RapidVolley;

            case Weapon.WeaponFunctionality.FullAutomatic:
                return true; // All firing modes are allowed

            case Weapon.WeaponFunctionality.SemiAutomaticWith3RoundBurst:
                return firingMode == FiringMode.SingleRound ||
                       firingMode == FiringMode.RapidVolley ||
                       firingMode == FiringMode.Burst;

            default:
                return false;
        }
    }
    private IActionResult ApplyAimingMode(AimType aim, Weapon weapon, ref Character characterCurrentTurn, decimal range, ref int baseChance)
    {
        switch (aim)
        {
            case AimType.Uncontrolled:
                if (characterCurrentTurn.ActionsRemaining < 1)
                    return BadRequest("You don't have enough actions!");

                switch (range)
                {
                    case decimal x when x < 3m:
                        baseChance -= 3;
                        break;
                    case decimal x when x <= 7.5m:
                        break;
                    case decimal x when x <= 12m:
                        baseChance--;
                        break;
                    case decimal x when x < 16.5m:
                        baseChance -= 2;
                        break;
                    case decimal x when x < 21m:
                        baseChance -= 3;
                        break;
                    case decimal x when x < 25.5m:
                        baseChance -= 4;
                        break;
                    case decimal x when x < 30m:
                        baseChance -= 5;
                        break;
                    default:
                        decimal rangeBeyond30m = range - 30m;
                        int additionalPenalty = (int)Math.Ceiling(rangeBeyond30m / 4.5m);
                        baseChance -= (5 + additionalPenalty);
                        break;
                }

                characterCurrentTurn.ActionsRemaining--;
                break;
            case AimType.Aimed:
                if (weapon.SpecialSightModifiers.LaserSight)
                {
                    if (characterCurrentTurn.ActionsRemaining < 1)
                        return BadRequest("You don't have enough actions!");
                }
                else if (characterCurrentTurn.ActionsRemaining < 2)
                {
                    return BadRequest("You don't have enough actions!");
                }

                switch (range)
                {
                    case decimal x when x >= 3m && x <= 150m:
                        break;
                    case decimal x when x <= 300m:
                        baseChance -= 3;
                        break;
                    case decimal x when x <= 450m:
                        baseChance -= 6;
                        break;
                    case decimal x when x <= 750m:
                        baseChance -= 9;
                        break;
                    case decimal x when x <= 1050m:
                        baseChance -= 12;
                        break;
                    default:
                        decimal rangeBeyond1050m = range - 1050m;
                        int additionalPenalty = (int)Math.Ceiling(rangeBeyond1050m / 300m);
                        baseChance -= additionalPenalty * 3;
                        break;
                }

                characterCurrentTurn.ActionsRemaining -= weapon.SpecialSightModifiers.LaserSight ? 1 : 2;

                break;
            case AimType.AccurateAimed:

                baseChance += 3;

                if (weapon.SpecialSightModifiers.LaserSight)
                {
                    if (characterCurrentTurn.ActionsRemaining < 2)
                        return BadRequest("You don't have enough actions!");
                }
                else if (characterCurrentTurn.ActionsRemaining < 3)
                {
                    return BadRequest("You don't have enough actions!");
                }

                switch (range)
                {
                    case decimal x when x >= 3m && x <= 150m:
                        break;
                    case decimal x when x <= 300m:
                        baseChance -= 3;
                        break;
                    case decimal x when x <= 450m:
                        baseChance -= 6;
                        break;
                    case decimal x when x <= 750m:
                        baseChance -= 9;
                        break;
                    case decimal x when x <= 1050m:
                        baseChance -= 12;
                        break;
                    default:
                        decimal rangeBeyond1050m = range - 1050m;
                        int additionalPenalty = (int)Math.Ceiling(rangeBeyond1050m / 300m);
                        baseChance -= additionalPenalty * 3;
                        break;
                }
                characterCurrentTurn.ActionsRemaining -= weapon.SpecialSightModifiers.LaserSight ? 2 : 3;
                break;
            default:
                return BadRequest("Invalid aiming mode!");
        }
        return null;
    }
    private async Task<int> CalculateHitChance(int baseChance, Enviroment.Light light, Enviroment.Weather weather)
    {

        int lightModifier = (int)light;
        int weatherModifier = (int)weather;

        int modifiedHitChance = baseChance - lightModifier - weatherModifier;

        return modifiedHitChance;
    }
    private void ApplyDamageToBodyPart(BodyPart bodyPart, int damage, AmmoType? ammoType)
    {
        int armorValue = bodyPart.ArmorValue;
        int absorbedDamage = Math.Min(armorValue, Dice.Roll1D10());

        if (ammoType is AmmoType.ArmorPenetration)
        {
            int doubledDamage = damage * 2;

            if (absorbedDamage >= doubledDamage)
            {
                bodyPart.TemporaryBodyPoints -= (damage - absorbedDamage);
            }
            else
            {
                int remainingDamage = (int)Math.Ceiling((doubledDamage - absorbedDamage) * 0.5);
                bodyPart.TemporaryBodyPoints -= remainingDamage;
            }
        }
        else if (ammoType is AmmoType.Hardballs)
        {
            bodyPart.TemporaryBodyPoints -= (damage * 2 - absorbedDamage);
        }
        else
        {
            bodyPart.TemporaryBodyPoints -= (damage - absorbedDamage);
        }
    }
    private void UpdateTurn(List<(Character character, int initiative)> initiativeOrder)
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

        combatStatus = StringBuilderFormatInitiative(initiativeOrder, CurrentRound, characterCurrentTurn);
    }
    private void CheckForDefeatedCombatants()
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

    #endregion


}
