using Microsoft.AspNetCore.Mvc;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Repository;

namespace MutantChroniclesAPI.Controllers;


[Route("api/[controller]")]
[ApiController]

public class CharacterController : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> CreateCharacter(string name,
                                                        int strength,
                                                            int physique,
                                                                int coordination,
                                                                    int intelligence,
                                                                        int mentalstrength,
                                                                            int personality)
    {
        try
        {
            var character = new Character
            {
                Name = name,
                Strength = strength,
                Physique = physique,
                Coordination = coordination,
                Intelligence = intelligence,
                MentalStrength = mentalstrength,
                Personality = personality,
                OffensiveBonus = strength + physique,
                ActionsPerRound = coordination + intelligence,
                PerceptionBonus = intelligence + mentalstrength,
                DefensiveBonus = coordination + personality,
            };

            character.CalculateOffensiveBonus();
            character.CalculateActionsPerRound();
            character.CalculatePerceptionBonus();
            character.CalculateDefensiveBonus();
            character.CalculateInitiativeBonus();
            character.CalculateMovementAllowance();
            character.CalculateWeightPenalty();

            (int squares, int meters) movementAllowance = character.MovementAllowance;

            //----------------------------  Initialize Target System for Character  ----------------------------\\

            var target = new Target(character);
            character.Target = target;

            CharacterRepository.Characters.Add(character);

            return Ok(character);
        }
        catch (Exception e)
        { return BadRequest(e); }

    }

    [HttpPost("test")]
    public async Task<IActionResult> CreateTestCharacter(string name)
    {
        try
        {
            var random = new Random();

            // Generate random stats between 3 and 20
            int strength = random.Next(3, 21);
            int physique = random.Next(3, 21);
            int coordination = random.Next(3, 21);
            int intelligence = random.Next(3, 21);
            int mentalstrength = random.Next(3, 21);
            int personality = random.Next(3, 21);


            var character = new Character
            {
                Name = name,
                Strength = strength,
                Physique = physique,
                Coordination = coordination,
                Intelligence = intelligence,
                MentalStrength = mentalstrength,
                Personality = personality,
                OffensiveBonus = strength + physique,
                ActionsPerRound = coordination + intelligence,
                PerceptionBonus = intelligence + mentalstrength,
                DefensiveBonus = coordination + personality,
            };

            character.CalculateOffensiveBonus();
            character.CalculateActionsPerRound();
            character.CalculatePerceptionBonus();
            character.CalculateDefensiveBonus();
            character.CalculateInitiativeBonus();
            character.CalculateMovementAllowance();
            character.CalculateWeightPenalty();

            (int squares, int meters) movementAllowance = character.MovementAllowance;

            //----------------------------  Initialize Target System for Character  ----------------------------\\

            var target = new Target(character);
            character.Target = target;

            CharacterRepository.Characters.Add(character);

            return Ok(character);
        }
        catch (Exception e)
        { return BadRequest(e); }

    }


    [HttpGet]
    public async Task<IActionResult> DisplayCharacter()
    {
        return Ok(CharacterRepository.Characters.ToList()); //JSONtoTUPLEConverter
    }
}
