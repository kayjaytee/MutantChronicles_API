using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MutantChroniclesAPI.Enums;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Repository;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MutantChroniclesAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArmorController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateArmor(string name,
                                                 [Required][Range(1, int.MaxValue, ErrorMessage = "The armor value must be greater than or equal to 1.")] int armorValue,
                                                 [Required][FromQuery] ArmorType option,
                                                 string characterName)
    {
        Character character;
        Armor armor;
        double blockchance = 0.00;

        character = CharacterRepository.Characters.FirstOrDefault(x => string.Equals(x.Name, characterName, StringComparison.OrdinalIgnoreCase));
        if (character is null)
        {
            var suggestedCharacters = await SearchCharactersAsync(characterName);
            if (suggestedCharacters.Count > 0)
            {
                string suggestedNames = string.Join(", ", suggestedCharacters.Select(c => c.Name));
                return NotFound($"Character name could not be found. However, similar names such as '{suggestedNames}' where found. Is this what you're looking for?");
            }
            else
            {
                return NotFound("Character name could not be found.");
            }
        }
        switch (option)
        {
            case ArmorType.Head:
                armor = new Armor(name, armorValue, Armor.ArmorType.Head);
                break;
            case ArmorType.Harness:
                armor = new Armor(name, armorValue, Armor.ArmorType.Harness);
                break;
            case ArmorType.Jacket:
                armor = new Armor(name, armorValue, Armor.ArmorType.Jacket);
                break;
            case ArmorType.Trenchcoat:
                blockchance = 0.50;
                armor = new Armor(name, armorValue, Armor.ArmorType.Trenchcoat);
                break;
            case ArmorType.Bodysuit:
                armor = new Armor(name, armorValue, Armor.ArmorType.Bodysuit);
                break;
            case ArmorType.Arms:
                armor = new Armor(name, armorValue, Armor.ArmorType.Arms);
                break;
            case ArmorType.Gloves:
                blockchance = 0.25;
                armor = new Armor(name, armorValue, Armor.ArmorType.Gloves);
                break;
            case ArmorType.Legs:
                armor = new Armor(name, armorValue, Armor.ArmorType.Legs);
                break;
            case ArmorType.Knee:
                blockchance = 0.25;
                armor = new Armor(name, armorValue, Armor.ArmorType.Knee);
                break;
            default:
                return Problem();
        }
        character.Target = new Target(character);
        character.Armor.Add(armor);
        character.UpdateArmorValueForBodyParts();
        return Ok(character.Name + "\nArmor Details:" + armor.ToJson());
    }

    private async Task<List<Character>> SearchCharactersAsync(string query)
    {
        var regexPattern = new Regex(query, RegexOptions.IgnoreCase);
        var matchingCharacters = CharacterRepository.Characters.Where(c => regexPattern.IsMatch(c.Name)).ToList();

        return matchingCharacters;
    }

}
