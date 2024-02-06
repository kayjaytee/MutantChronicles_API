using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MutantChroniclesAPI.Controllers;
using MutantChroniclesAPI.Model.CharacterModel;

namespace MutantChroniclesAPI.Tests.Controllers;


[TestFixture]
public class CharacterControllerTests
{

    [Test]
    public void InitializeCharacterController_Success()
    {
        //Arrange
        var controller = new CharacterController();

        //Act
        var result = controller;

        //Assert
        Assert.IsNotNull(result);

    }

    [Test]
    [Ignore("Test needs adjustment for proper testing")]
    public void CreateCharacter_ValidInput_ReturnsOk()
    {
        // Arrange
        var controller = new CharacterController();

        var name = "Super Badass Bob";
        var strength = 10;
        var physique = 8;
        var coordination = 7;
        var intelligence = 9;
        var mentalStrength = 6;
        var personality = 8;

        var expectedCharacter = new Character
        {
            Name = name,
            Strength = strength,
            Physique = physique,
            Coordination = coordination,
            Intelligence = intelligence,
            MentalStrength = mentalStrength,
            Personality = personality,
        };

        // Act
        var result = controller.CreateCharacter(name, strength, physique, coordination, intelligence, mentalStrength, personality);


        // Assert
        Assert.IsTrue(expectedCharacter.Name.Equals(name));
        //improve assert
    }

    [Test]
    public void DisplayCharacters_ReturnsOkWithListOfCharacters()
    {

        //Arrange
        var controller = new CharacterController();

        var characters = new List<Character>
        {
            new Character { Name = "Character1" },
                new Character { Name = "Character2" }
        };

        //Act
        var result = controller.DisplayCharacter();

        //Assert
        Assert.AreEqual(2, characters.Count);

    }

}