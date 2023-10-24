using Microsoft.AspNetCore.Mvc;
using MutantChroniclesAPI.Controllers;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Repository;

namespace MutantChroniclesAPI.Tests.Controllers;


[TestFixture]
public class CharacterControllerTests
{

    [Test]
    public async Task InitializeCharacterController_Success()
    {
        //Arrange
        var controller = new CharacterController();

        //Act
        var result = controller;

        //Assert
        Assert.IsNotNull(result);

    }

    [Test]
    public async Task CreateCharacter_ValidInput_ReturnsOk()
    {
        // Arrange
        var controller = new CharacterController();
        var name = "John";
        var strength = 10;
        var physique = 8;
        var coordination = 7;
        var intelligence = 9;
        var mentalStrength = 6;
        var personality = 8;

        // Act
        var result = await controller.CreateCharacter(name,
                                                        strength,
                                                            physique,
                                                                coordination,
                                                                    intelligence,
                                                                        mentalStrength,
                                                                            personality) as OkObjectResult;
        var character = result.Value
                        as Character;

        // Assert
        Assert.IsNotNull(character);
        Assert.AreEqual("John", character.Name);
        Assert.AreEqual(10, character.Strength);
        Assert.AreEqual(8, character.Physique);
        Assert.AreEqual(7, character.Coordination);
        Assert.AreEqual(9, character.Intelligence);
        Assert.AreEqual(6, character.MentalStrength);
        Assert.AreEqual(8, character.Personality);

        Assert.IsNotNull(character.MovementAllowance);
    }


    [Test]
    public async Task DisplayCharacters_ReturnsOkWithListOfCharacters()
    {

        //Arrange
        var controller = new CharacterController();

        var characters = new List<Character>
        {
            new Character { Name = "Character1" },
                new Character { Name = "Character2" }
        };

        //Act
        var result = await controller.DisplayCharacter();

        //Assert
        Assert.AreEqual(2, characters.Count);

    }

}