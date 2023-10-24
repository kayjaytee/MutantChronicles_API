using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Model.WeaponModel;
using MutantChroniclesAPI.Repository;
using MutantChroniclesAPI.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MutantChroniclesAPI.Controllers;


[Route("api/[controller]")]
[ApiController]

public class WeaponController : ControllerBase
{
    private readonly WeaponService _weaponService;

    public WeaponController(WeaponService weaponService)
    {
        _weaponService = weaponService;
    }

    [HttpGet("get")]
    public async Task<List<Weapon>> GetAllWeapons() =>
      await _weaponService.GetAsync();

    [HttpGet("search")]
    public async Task<IActionResult> SearchWeapons(string query)
    {
        var matchingWeapons = await _weaponService.SearchAsync(query);
        return Ok(matchingWeapons);
    }

    [HttpPatch("equip/character")]
    public async Task<IActionResult> EquipWeapon([Required] string weaponName,
                                                 [Required] string characterName)
    {
        Character character;

        character = CharacterRepository.Characters.FirstOrDefault(x => x.Name.Equals(characterName, StringComparison.OrdinalIgnoreCase));
        if (character is null)
        {
            var suggestedCharacters = await SearchCharactersAsync(characterName);
            if (suggestedCharacters.Count > 0)
            {
                string suggestedNames = string.Join(", ", suggestedCharacters.Select(c => c.Name));
                return NotFound($"Character name could not be found. However, similar names such as '{suggestedNames}' was found. Is this what you're looking for?");
            }
            else
            {
                return NotFound("Character name could not be found.");
            }
        }

        var weapon = await _weaponService.GetByNameAsync(weaponName);
        if (weapon is null)
        {
            var searchWeapon = await _weaponService.SearchAsync(weaponName);
            if (searchWeapon.Count > 0)
            {
                string suggestedNames = string.Join(", ", searchWeapon.Select(x => x.Name));
                return NotFound($"Weapon '{weaponName}' could not be found. However, similar weapons with similar names such as '{suggestedNames}' was found. Is this what you're looking for?");
            }
            else
            {
                return NotFound($"Weapon '{weaponName}' could not be found.");
            }
        }
        weapon.CurrentAmmo = weapon.MagazineCapacity;
        weapon.AmmoType = MC_Weapon_Calculator.Model.Ammo.AmmoType.Standard; //WIP
        UpdateCharacter(character, weapon);
        return Ok($"Character '{characterName}': is now equipped with a '{weapon.Description} <<{weapon.Name}>>.'");

    }

    private void UpdateCharacter(Character character, Weapon weapon)
    {
        character.EquipWeapon(weapon);
        character.CalculateWeight();
        character.CalculateWeightPenalty();
        character.CalculateActionsPerRound();
        character.CalculateMovementAllowance();
    }

    private async Task<List<Character>> SearchCharactersAsync(string query)
    {
        var regexPattern = new Regex(query, RegexOptions.IgnoreCase);
        var matchingCharacters = CharacterRepository.Characters.Where(c => regexPattern.IsMatch(c.Name)).ToList();

        return matchingCharacters;
    }
}
