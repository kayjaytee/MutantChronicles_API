using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;
using MutantChroniclesAPI.Enums;
using MutantChroniclesAPI.Interface;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Model.EnviromentModel;
using MutantChroniclesAPI.Model.WeaponModel;
using MutantChroniclesAPI.Repository;
using MutantChroniclesAPI.Services.Combat;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text;
using static MC_Weapon_Calculator.Model.Ammo;

[assembly: InternalsVisibleTo("MutantChroniclesAPI.Tests")]

namespace MutantChroniclesAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CombatController : ControllerBase
{

    private static List<Character> combatants;
    private static List<Character> defeated;
    private static List<(Character character, int initiative)> initiativeOrder;
    private static Character characterCurrentTurn;
    private static Character characterPreviousTurn; //WIP

    private static int currentRound = 1;
    private static bool combatInProgress;
    private static string combatStatus;
    private static IEnviromentService _enviromentService;
    private static ICombatService _combatService;
    private int initiative;
    public CombatController(IEnviromentService enviromentService,
                            ICombatService combatService)
    {
        _enviromentService = enviromentService;
        _combatService = combatService;
    }


    [HttpPost("StartCombat")]
    public async Task<IActionResult> StartCombat()
    {
        try
        {
            if (combatInProgress is true)
            {
                return BadRequest("Combat is already in progress");
            }
            if (CharacterRepository.Characters.Count < 1)
            {
                return NotFound("There are no characters created.");
            }

            combatInProgress = true;
            characterCurrentTurn = null;

            // Set the combatants list with characters from the CharacterRepository
            combatants = CharacterRepository.Characters.ToList();
            defeated = new List<Character>();
            foreach (var character in combatants)
            {
                character.ActionsRemaining = character.ActionsPerRound;
                _combatService.InitializeTemporaryBodyPoints(character.Target);
            }

            initiativeOrder = _combatService.InitiativeRoll(combatants);
            if (initiativeOrder.Count > 0)
            {
                characterCurrentTurn = initiativeOrder[0].character; // Set the character who won the initiative as current turn
            }

            combatStatus = _combatService.StringBuilderFormatInitiative(initiativeOrder, currentRound, characterCurrentTurn);

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
        if (combatInProgress is false)
        {
            return BadRequest("No combat in progress.");
        }
        else
        {
            return Ok(combatStatus);
        }

    }

    [HttpGet("DisplayStats")]
    public async Task<IActionResult> DisplayStats() //TEST IMPROVEMENT: BETTER SETUP OPTIONS
    {
        if (combatInProgress is false)
        {
            return BadRequest("No combat in progress.");
        }
        else
        {
            var stringBuilder = new StringBuilder();
            foreach (var combatant in combatants)
            {
                stringBuilder.AppendLine($"Combatant: {combatant.Name}");
                stringBuilder.AppendLine($"Equipment: {combatant.Armor.ToList()}");

                stringBuilder.AppendLine($"Head BP {combatant.Target.Head.TemporaryBodyPoints}/{combatant.Target.Head.MaximumBodyPoints} | AV: {combatant.Target.Head.ArmorValues.Sum(x => x.Absorb)}");
                stringBuilder.AppendLine($"Chest BP {combatant.Target.Chest.TemporaryBodyPoints}/{combatant.Target.Chest.MaximumBodyPoints} | AV: {combatant.Target.Chest.ArmorValues.Sum(x => x.Absorb)}");
                stringBuilder.AppendLine($"Stomach BP {combatant.Target.Stomach.TemporaryBodyPoints}/{combatant.Target.Stomach.MaximumBodyPoints} | AV: {combatant.Target.Stomach.ArmorValues.Sum(x => x.Absorb)}");
                stringBuilder.AppendLine($"RightArm BP {combatant.Target.RightArm.TemporaryBodyPoints}/{combatant.Target.RightArm.MaximumBodyPoints} | AV: {combatant.Target.RightArm.ArmorValues.Sum(x => x.Absorb)}");
                stringBuilder.AppendLine($"LeftArm BP {combatant.Target.LeftArm.TemporaryBodyPoints}/{combatant.Target.LeftArm.MaximumBodyPoints} | AV: {combatant.Target.LeftArm.ArmorValues.Sum(x => x.Absorb)}");
                stringBuilder.AppendLine($"RightLeg BP {combatant.Target.RightLeg.TemporaryBodyPoints}/{combatant.Target.RightLeg.MaximumBodyPoints} | AV: {combatant.Target.RightLeg.ArmorValues.Sum(x => x.Absorb)}");
                stringBuilder.AppendLine($"LeftLeg BP {combatant.Target.LeftLeg.TemporaryBodyPoints}/{combatant.Target.LeftLeg.MaximumBodyPoints} | AV: {combatant.Target.LeftLeg.ArmorValues.Sum(x => x.Absorb)}");
                stringBuilder.AppendLine();

                stringBuilder.AppendLine($"Character Stress: {combatant.Stress.ToString()} (-{(int)combatant.Stress})");
                stringBuilder.AppendLine($"Character Wounds: {combatant.Wounds.ToString()} (-{(int)combatant.Wounds})");

                stringBuilder.AppendLine();
                if (combatant.MainHandEquipment is not null)
                {
                    stringBuilder.AppendLine($"Equipped Weapon (in Main Hand): {combatant.MainHandEquipment.Description}: <{combatant.MainHandEquipment.Name}>");
                    stringBuilder.AppendLine($"Magazine: {combatant.MainHandEquipment.CurrentAmmo}/{combatant.MainHandEquipment.MagazineCapacity} (Ammo Type: '{combatant.MainHandEquipment.AmmoType}')");
                    if (combatant.MainHandEquipment.SecondaryMode is not null && combatant.MainHandEquipment.SecondaryMode.Equipped is true)
                    {
                        stringBuilder.AppendLine($"Secondary Firing Mode: <{combatant.MainHandEquipment.SecondaryMode.NameDescription}>");
                        stringBuilder.AppendLine($"Magazine: {combatant.MainHandEquipment.SecondaryMode.CurrentAmmo}/{combatant.MainHandEquipment.SecondaryMode.MagazineCapacity}");
                    }
                }
                if (combatant.OffHandEquipment is not null)
                {
                    stringBuilder.AppendLine($"Equipped Weapon (in Off Hand): {combatant.OffHandEquipment.Description}: <{combatant.OffHandEquipment.Name}>");
                    stringBuilder.AppendLine($"Magazine: {combatant.OffHandEquipment.CurrentAmmo}/{combatant.OffHandEquipment.MagazineCapacity} (Ammo Type: '{combatant.OffHandEquipment.AmmoType}')");
                    if (combatant.OffHandEquipment.SecondaryMode is not null && combatant.OffHandEquipment.SecondaryMode.Equipped is true)
                    {
                        stringBuilder.AppendLine($"Secondary Firing Mode: <{combatant.OffHandEquipment.SecondaryMode.NameDescription}>");
                        stringBuilder.AppendLine($"Magazine: {combatant.OffHandEquipment.SecondaryMode.CurrentAmmo}/{combatant.OffHandEquipment.SecondaryMode.MagazineCapacity}");
                    }
                }
                else
                {
                    stringBuilder.AppendLine($"Equipped Weapon: Unarmed");
                }

                stringBuilder.AppendLine(); // Add a blank line between combatants
            }

            stringBuilder.AppendLine($"Weather Modifier: {_enviromentService.GetWeatherModifiers()} -({(int)_enviromentService.GetWeatherModifiers()})");
            stringBuilder.AppendLine($"Light Modifier: {_enviromentService.GetLightModifiers()} -({(int)_enviromentService.GetLightModifiers()})");

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
        if (combatInProgress is false)
        {
            return BadRequest("No combat in progress.");
        }
        else
        {
            _combatService.CheckForDefeatedCombatants(initiativeOrder, combatants, defeated);
            currentRound++;
            initiativeOrder = _combatService.InitiativeRoll(combatants);
            foreach (var character in combatants)
            {
                character.CurrentlyUnderFire.Clear();
                switch (character.Stress)
                {
                    //Reset Stress from being fired at
                    case Character.EnviromentStress.SomeoneFiresAtYou:
                    case Character.EnviromentStress.PeopleFireAtYouFromSeveralDirections:
                        character.Stress = 0;
                        break;

                    default:
                        break;
                }
            }

            // Reset ActionsRemaining for all combatants
            foreach (var character in combatants)
            {
                character.ActionsRemaining = character.ActionsPerRound;
            }

            if (initiativeOrder.Count > 0)
            {
                characterCurrentTurn = initiativeOrder[0].character; // Set the character who won the initiative as current turn
            }
            _combatService.CheckForWounds(combatants);
            combatStatus = _combatService.StringBuilderFormatInitiative(initiativeOrder, currentRound, characterCurrentTurn);

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
        try
        {
            var weapon = characterCurrentTurn.MainHandEquipment;
            var targetCharacter = combatants.FirstOrDefault(x => string.Equals(x.Name, target, StringComparison.OrdinalIgnoreCase));
            int shotsToCalculate = 0;
            _combatService.CheckForWounds(combatants);

            #region BadRequests
            if (characterCurrentTurn.ActionsRemaining <= 0)
            {
                return BadRequest(characterCurrentTurn.Name + " has ran out of actions.");
            }
            if (combatInProgress is false)
            {
                return BadRequest("No combat in progress.");
            }

            if (weapon is null)
            {
                return BadRequest("Character is not carrying a ranged weapon.");
            }

            if (targetCharacter is null || targetCharacter == characterCurrentTurn)
            {
                return BadRequest("Invalid target.");
            }
            if (_combatService.IsFiringModeAllowed(weapon, firingMode, secondaryModeActivated) is false)
            {
                return BadRequest("Firing mode not allowed for this weapon.");
            }

            var aimingMode = ApplyAimingMode(aim, weapon, ref characterCurrentTurn, range, ref baseChance);
            if (aimingMode is BadRequestObjectResult)
            {
                return BadRequest(); //improve desc.
            }

            if (firingMode is FiringMode.AreaSpray)
            {
                aim = AimType.Uncontrolled; //Area Spray is always considered Uncontrolled
            }
            #endregion BadRequests

            // Update ActionsRemaining in combatants list
            int characterIndex = combatants.FindIndex(x => x == characterCurrentTurn);
            if (characterIndex is not -1)
            {
                combatants[characterIndex].ActionsRemaining = characterCurrentTurn.ActionsRemaining;
            }

            switch (_combatService.IsMagazineEmptyCheck(weapon, secondaryModeActivated))
            {
                case true:
                    return Ok("CLICK! Out of ammo! You need to reload your magazine.");
                case false:
                    break;
            }

            _combatService.AmmoHandler(weapon, firingMode, rapidVolleyBulletsCount, ref baseChance, ref shotsToCalculate, secondaryModeActivated);

            StringBuilder resultBuilder = new StringBuilder();

            bool isCriticalHitApplied = false; //For controlling multipleTargetAreas. Only 1 body part can take critical damage per shot

            _combatService.CalculateHitChance(characterCurrentTurn, baseChance,
            (Enviroment.Light)_enviromentService.GetLightModifiers(),
            (Enviroment.Weather)_enviromentService.GetWeatherModifiers());


            for (int i = 0; i < shotsToCalculate; i++)
            {

                int d20HitRoll = Dice.Roll1D20();

                bool isHit = d20HitRoll <= baseChance;

                bool critical = d20HitRoll == 1; // Check if it's a critical hit
                bool fumble = d20HitRoll == 20; // Check if user fumbled

                resultBuilder.AppendLine($"Chance of Success: {baseChance}");
                resultBuilder.AppendLine($"Modifiers:");
                resultBuilder.AppendLine($"Wounds: {characterCurrentTurn.Wounds.ToString()}, (-{((int)characterCurrentTurn.Wounds)})");
                resultBuilder.AppendLine($"Stress: {characterCurrentTurn.Stress.ToString()}, (-{((int)characterCurrentTurn.Stress)})");
                resultBuilder.AppendLine($"Light: {_enviromentService.GetLightModifiers()}, (-{(int)_enviromentService.GetLightModifiers()}");
                resultBuilder.AppendLine($"Weather: {_enviromentService.GetWeatherModifiers()}, (-{(int)_enviromentService.GetWeatherModifiers()}");
                resultBuilder.AppendLine();
                resultBuilder.AppendLine($"{characterCurrentTurn.Name.ToUpper()} fires at {targetCharacter.Name.ToUpper()} and...");
                resultBuilder.AppendLine($"Roll: {d20HitRoll}");

                _combatService.ApplyStressToTarget(targetCharacter, characterCurrentTurn);

                if (fumble)
                {
                    resultBuilder.AppendLine($"Whoops! You jammed your weapon...");
                    int d10JamRoll = Dice.Roll1D10();
                    if (d10JamRoll >= characterCurrentTurn.MainHandEquipment.JammingFactor)
                    {
                        resultBuilder.AppendLine($"...the magazine is completely stuck.");
                    }

                    int actionsLost = characterCurrentTurn.ActionsRemaining - Dice.Roll1D6();
                    characterCurrentTurn.ActionsRemaining -= actionsLost;
                    if (characterCurrentTurn.ActionsRemaining < 0)
                    {
                        characterCurrentTurn.ActionsRemaining = 0;
                    }
                    resultBuilder.AppendLine($"...and lost {actionsLost} actions.");
                    characterCurrentTurn.MainHandEquipment.WeaponIsJammed = true;
                    characterCurrentTurn.MainHandEquipment.SuccessfulUnjamAttempts = 0;
                    _combatService.CheckForDefeatedCombatants(initiativeOrder, combatants, defeated);
                    _combatService.UpdateTurn(initiativeOrder, characterCurrentTurn, currentRound, combatStatus);
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

                        if (targetCharacter.IsAvoiding is true)
                        {
                            resultBuilder.AppendLine($"({targetCharacter.Name.ToUpper()} attempts to take cover and avoid the incoming attack...");

                            switch (_combatService.AvoidAttempt(targetCharacter, firingMode,
                                       _enviromentService.GetLightModifiers(),
                                       _enviromentService.GetWeatherModifiers()))
                            {
                                case true:

                                    switch (firingMode)
                                    {
                                        case FiringMode.SingleRound:
                                            //Avoid 1 roll
                                            break;
                                        case FiringMode.Burst:
                                            //Avoid 1 roll, for both attacks
                                            break;
                                        case FiringMode.FullAuto:
                                        case FiringMode.RapidVolley:
                                            //Seperate Avoid rolls must be made for each successful attack roll
                                            break;
                                        case FiringMode.AreaSpray:
                                            //Avoid 1 roll with +3 bonus
                                            break;
                                    }
                                    if (characterCurrentTurn.MainHandEquipment.Category is Weapon.WeaponCategory.Shotgun)
                                    {

                                    }
                                    else
                                    {

                                    }
                                    break;

                                case false:
                                    resultBuilder.AppendLine($"({targetCharacter.Name.ToUpper()} tries to take cover but fails.");
                                    break;
                            }


                        }

                        if (critical)
                        {
                            damage = weapon.DamageMax + weapon.DamageAdded;
                            isCriticalHitApplied = true;
                        }
                        if (weapon.Category is Weapon.WeaponCategory.Shotgun &&
                            targetCharacter.IsAvoiding is true &&
                            _combatService.AvoidAttempt(targetCharacter, firingMode,
                            _enviromentService.GetLightModifiers(),
                            _enviromentService.GetWeatherModifiers()) is true)
                        {
                            //SHOTGUN: IF AVOIDANCE IS SUCCESSFUL FOR SHOTGUN, STILL TAKE HALF DAMAGE
                            damage = (int)(damage * 0.5);
                        }

                        resultBuilder.AppendLine($"{targetCharacter.Name.ToUpper()} takes {damage}...");

                        switch (d20Result)
                        {
                            case int x when x <= 3:
                                _combatService.ApplyDamageToBodyPart(hitLocation.LeftLeg, damage, weapon.AmmoType);
                                resultBuilder.AppendLine($"in Left Leg!");
                                break;
                            case int x when x <= 6:
                                _combatService.ApplyDamageToBodyPart(hitLocation.RightLeg, damage, weapon.AmmoType);
                                resultBuilder.AppendLine($"in Right Leg!");
                                break;
                            case int x when x <= 8:
                                _combatService.ApplyDamageToBodyPart(hitLocation.LeftArm, damage, weapon.AmmoType);
                                resultBuilder.AppendLine($"in Left Arm!");
                                break;
                            case int x when x <= 10:
                                _combatService.ApplyDamageToBodyPart(hitLocation.RightArm, damage, weapon.AmmoType);
                                resultBuilder.AppendLine($"in Right Arm!");
                                break;
                            case int x when x <= 14:
                                _combatService.ApplyDamageToBodyPart(hitLocation.Stomach, damage, weapon.AmmoType);
                                resultBuilder.AppendLine($"in Stomach!");
                                break;
                            case int x when x <= 19:
                                _combatService.ApplyDamageToBodyPart(hitLocation.Chest, damage, weapon.AmmoType);
                                resultBuilder.AppendLine($"in Chest!");
                                break;
                            default: //when x == 20
                                _combatService.ApplyDamageToBodyPart(hitLocation.Head, damage, weapon.AmmoType);
                                resultBuilder.AppendLine($"in Head!");
                                break;
                        }
                        resultBuilder.AppendLine();
                    }

                    isCriticalHitApplied = false; // Reset the flag for the next shot
                }
                else
                {
                    if (firingMode is FiringMode.Burst || firingMode is FiringMode.FullAuto)
                    {
                        // If burst/full auto fails due to a miss, break the loop
                        break;
                    }
                    resultBuilder.AppendLine($"(d20Result: {d20HitRoll})");
                }

            }

            _combatService.CheckForDefeatedCombatants(initiativeOrder, combatants, defeated);
            _combatService.CheckForWounds(combatants);
            if (combatants.Count >= 2)
            {
                _combatService.UpdateTurn(initiativeOrder, characterCurrentTurn, currentRound, combatStatus);
            }
            return Ok(resultBuilder.ToString());
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }

    [HttpPost("Action/Reload")]
    public async Task<IActionResult> ActionReload(int baseChance, bool? secondaryModeActivated, AmmoType ammotype)
    {
        if (characterCurrentTurn.MainHandEquipment is null)
        {
            return BadRequest("You don't have a weapon equipped");
        }

        if (characterCurrentTurn.Target.RightArm.TemporaryBodyPoints <= 0 &&
           characterCurrentTurn.Target.LeftArm.TemporaryBodyPoints <= 0)
        {
            return BadRequest("Your arms are critically wounded and cannot be used!");
        }

        if (characterCurrentTurn.ActionsRemaining < characterCurrentTurn.MainHandEquipment.ReloadingTime)
        {
            return BadRequest("You don't have enough actions to start reloading your weapon");
        }

        _combatService.CalculateHitChance(characterCurrentTurn, baseChance,
        (Enviroment.Light)(_enviromentService.GetLightModifiers()),
        (Enviroment.Weather)(_enviromentService.GetWeatherModifiers()));

        switch (secondaryModeActivated)
        {
            case true:

                if (characterCurrentTurn.MainHandEquipment.SecondaryMode.WeaponIsJammed is true)
                {
                    return BadRequest("Your weapon is jammed!");
                }

                break;

            case false:
            case null:

                if (characterCurrentTurn.MainHandEquipment.WeaponIsJammed is true)
                {
                    return BadRequest("Your weapon is jammed!");
                }

                break;
        }

        int d20HitRoll = Dice.Roll1D20();
        bool isSuccessful = d20HitRoll <= baseChance;

        if (d20HitRoll == 20)
        {
            characterCurrentTurn.ActionsRemaining -= characterCurrentTurn.MainHandEquipment.ReloadingTime;
            _combatService.UpdateTurn(initiativeOrder, characterCurrentTurn, currentRound, combatStatus);
            return Ok("$Whoops! Your magazine dropped on the ground...");
        }
        else if (isSuccessful || d20HitRoll != 20)
        {
            characterCurrentTurn.ActionsRemaining -= characterCurrentTurn.MainHandEquipment.ReloadingTime;
            if (secondaryModeActivated is true)
            {
                if (characterCurrentTurn.MainHandEquipment.Category is Weapon.WeaponCategory.AssaultRifle
                    && characterCurrentTurn.MainHandEquipment.SecondaryMode is not null)
                {
                    switch (characterCurrentTurn.MainHandEquipment.SecondaryMode.NameDescription)
                    {
                        case "M50": break;
                        case "Shogun": break;
                        case "AR3000": break;
                        case "Panzerknacker": break;
                        case "Volcano": break;
                        default: break;

                            //Unfinished: The the weapons listed above are cases where ammo is only loaded 1 by 1, instead of whole magazine for secondary firing mode.
                    }
                }
                characterCurrentTurn.MainHandEquipment.SecondaryMode.CurrentAmmo = characterCurrentTurn.MainHandEquipment.SecondaryMode.MagazineCapacity;
            }
            else
            {
                if (characterCurrentTurn.MainHandEquipment.Category is Weapon.WeaponCategory.Shotgun //Shotguns reload 1 ammo per round, with small expections
                && characterCurrentTurn.MainHandEquipment.Name.Equals("Mandible") is false)
                {
                    characterCurrentTurn.MainHandEquipment.CurrentAmmo++;
                    characterCurrentTurn.MainHandEquipment.AmmoType = ammotype;
                }
                else
                {
                    characterCurrentTurn.MainHandEquipment.CurrentAmmo = characterCurrentTurn.MainHandEquipment.MagazineCapacity;
                    characterCurrentTurn.MainHandEquipment.AmmoType = ammotype;
                }
            }

            _combatService.UpdateTurn(initiativeOrder, characterCurrentTurn, currentRound, combatStatus);
            return Ok($"{characterCurrentTurn.MainHandEquipment.Name} reloaded with a new magazine!");
        }
        else
        {
            characterCurrentTurn.ActionsRemaining -= characterCurrentTurn.MainHandEquipment.ReloadingTime;
            _combatService.UpdateTurn(initiativeOrder, characterCurrentTurn, currentRound, combatStatus);
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
        if (combatInProgress is false)
        {
            return BadRequest("No combat in progress.");
        }
        Weapon weapon = characterCurrentTurn.MainHandEquipment;
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

        _combatService.UpdateTurn(initiativeOrder, characterCurrentTurn, currentRound, combatStatus);
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
                    _combatService.ApplyDamageToBodyPart(hitLocation.LeftLeg, damage, null);
                    resultBuilder.AppendLine($"in Left Leg!");
                    break;
                case int x when x <= 6:
                    _combatService.ApplyDamageToBodyPart(hitLocation.RightLeg, damage, null);
                    resultBuilder.AppendLine($"in Right Leg!");
                    break;
                case int x when x <= 9:
                    _combatService.ApplyDamageToBodyPart(hitLocation.LeftArm, damage, null);
                    resultBuilder.AppendLine($"in Left Arm!");
                    break;
                case int x when x <= 12:
                    _combatService.ApplyDamageToBodyPart(hitLocation.RightArm, damage, null);
                    resultBuilder.AppendLine($"in Right Arm!");
                    break;
                case int x when x <= 15:
                    _combatService.ApplyDamageToBodyPart(hitLocation.Stomach, damage, null);
                    resultBuilder.AppendLine($"in Stomach!");
                    break;
                case int x when x <= 18:
                    _combatService.ApplyDamageToBodyPart(hitLocation.Chest, damage, null);
                    resultBuilder.AppendLine($"in Chest!");
                    break;
                default: //when x == 20
                    _combatService.ApplyDamageToBodyPart(hitLocation.Head, damage, null);
                    resultBuilder.AppendLine($"in Head!");
                    break;
            }
        }
        return Ok(resultBuilder.ToString());
    }


    [HttpPost("Action/Unjam")]
    public async Task<IActionResult> ActionUnjam(int baseChance, bool? secondaryModeActivated)
    {
        if (characterCurrentTurn.MainHandEquipment is null)
        {
            return BadRequest("You don't have a weapon equipped");
        }

        _combatService.CalculateHitChance(characterCurrentTurn, baseChance, light: 0, weather: 0);

        switch (secondaryModeActivated)
        {
            case true:

                if (characterCurrentTurn.MainHandEquipment.SecondaryMode.WeaponIsJammed is false)
                {
                    return BadRequest("Your weapon's secondary firing mode is not jammed.");
                }

                break;

            case false:
            case null:

                if (characterCurrentTurn.MainHandEquipment.WeaponIsJammed is false)
                {
                    return BadRequest("Your weapon is not jammed.");
                }

                break;
        }

        characterCurrentTurn.ActionsRemaining = 0;
        int d20HitRoll = Dice.Roll1D20();
        bool isSuccessful = d20HitRoll <= baseChance;

        if (d20HitRoll == 1)
        {
            _combatService.RemoveWeaponJam(characterCurrentTurn, secondaryModeActivated);
            _combatService.UpdateTurn(initiativeOrder, characterCurrentTurn, currentRound, combatStatus);
            return Ok("With critical success, the weapon is unjammed and now ready for use agian!");
        }
        if (isSuccessful || d20HitRoll != 20)
        {
            characterCurrentTurn.MainHandEquipment.SuccessfulUnjamAttempts++;
            if (characterCurrentTurn.MainHandEquipment.SuccessfulUnjamAttempts > 2)
            {
                _combatService.RemoveWeaponJam(characterCurrentTurn, secondaryModeActivated);
                _combatService.UpdateTurn(initiativeOrder, characterCurrentTurn, currentRound, combatStatus); //bloat? UpdateTurn
                return Ok("Weapon unjammed successfully.");
            }
            _combatService.UpdateTurn(initiativeOrder, characterCurrentTurn, currentRound, combatStatus);
            return Ok("You were successful, but need more time to unjam the weapon.");
        }
        _combatService.UpdateTurn(initiativeOrder, characterCurrentTurn, currentRound, combatStatus);
        return Ok("You were unsuccessful and have to attempt next turn.");
    }


    [HttpPatch("Enviroment/WARNINGThreeSecondsToAutoDestruct/TurnOn")] //NEEDS TEST
    public async Task<IActionResult> EnviromentWARNINGThreeSecondsToAutoDestruct_ON()
    {
        foreach (var character in combatants)
        {
            switch (character.Stress)
            {
                case Character.EnviromentStress.YourClothesAreOnFire:
                case Character.EnviromentStress.YouAreMidairFallingTowardCertainDeath:
                    break;

                default:
                    character.Stress = Character.EnviromentStress.WARNINGThreeSecondsToAutoDestruct;
                    break;

            }
        }

        return Ok("WARNING WARNING! (Enviroment Stress Applied, -3 modifier applied to all");
    }

    [HttpPatch("Enviroment/WARNINGThreeSecondsToAutoDestruct/TurnOff")] //NEEDS TEST
    public async Task<IActionResult> EnviromentWARNINGThreeSecondsToAutoDestruct_OFF()
    {
        foreach (var character in combatants)
        {
            character.Stress = Character.EnviromentStress.None;
            _combatService.ApplyStressToTarget(character, characterCurrentTurn);
        }

        return Ok("Stress warning is normalized.");
    }

    [HttpPost("EndCombat")]
    public async Task<IActionResult> EndCombat()
    {
        if (combatInProgress is false)
        {
            return BadRequest("No combat in progress.");
        }

        combatants.Clear();
        initiativeOrder.Clear();
        defeated.Clear();
        characterCurrentTurn = null!;
        combatStatus = "";
        currentRound = 1;

        return Ok("Combat has ended");
    }

    [HttpPost("InputDamageManually")]
    public async Task<IActionResult> InputDamageManually([Required][FromQuery] string target, [Required] int damage, string? bodypart)
    {
        if (combatInProgress is false)
        {
            return BadRequest("No combat in progress.");
        }
        Character targetCharacter = combatants.FirstOrDefault(x => x.Name == target);

        if (targetCharacter is null)
        {
            return BadRequest("Invalid target.");
        }

        return null;
    }
    #region Combat Methods


    private int CalculateMeleeAttack(Character character, Weapon weapon)
    {
        int damage = characterCurrentTurn.OffensiveBonus;

        if (weapon.ChainBayonet is not null && weapon.ChainBayonet.Equipped is true)
        {
            damage += Dice.RollDamage(weapon.ChainBayonet.DamageMin, weapon.ChainBayonet.DamageMax, weapon.ChainBayonet.DamageAdded);
        }
        else
        {
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
        }

        if (weapon is null)
        {
            damage = Dice.RollDamage(1, 3, characterCurrentTurn.OffensiveBonus);
        }

        return damage;

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
        return null!;
    }
    //Not properly teste
    #endregion


}
