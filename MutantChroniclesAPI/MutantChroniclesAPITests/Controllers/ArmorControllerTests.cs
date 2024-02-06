using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;
using MongoDB.Bson;
using MutantChroniclesAPI.Controllers;
using MutantChroniclesAPI.Enums;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Repository;
using NSubstitute;
using NSubstitute.ClearExtensions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;

namespace MutantChroniclesAPI.Tests.Controllers;

[TestFixture]
public class ArmorControllerTests
{

    private ArmorController controller;
    private List<Character> characters;
    private JsonSerializerOptions? options;

    [SetUp]
    public void Setup()
    {
        controller = new ArmorController();
        characters = new List<Character>();
    }

    [Test]
    public async Task InitializeArmorController_Success()
    {
        //Arrange
        var controller = new ArmorController();

        //Act
        var result = controller;

        //Assert
        Assert.IsNotNull(result);

    }

    [Test]
    public async Task CreateArmor__Name_ArmorValue_ArmorType__IsApplied_Async()
    {
        // Arrange
        string characterName = "TestCharacter";
        string armorName = "TestArmor";
        int armorValue = 10;
        ArmorType armorType = (ArmorType)(int)ArmorType.Head;
        ArmorMaterial armorMaterial = (ArmorMaterial)(int)ArmorMaterial.ExtraHeavyCombat;

        var testCharacter = new Character { Name = characterName };
        CharacterRepository.Characters.Add(testCharacter);

        // Act
        var result = await controller.CreateArmor(armorName, armorValue, armorType, armorMaterial, characterName);


        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
    }


    [Test]
    public async Task SearchCharactersAsync_WhenCharacterNotFound_ShouldReturnSimilarMatches()
    {
        // Arrange
        string query = "joh";
        characters = new List<Character>
        {
            new Character { Name = "Johnny Katana" },
            new Character { Name = "Jeff Goldblum" },
            new Character { Name = "Johanna Joy" }
        };

        // Act
        CharacterRepository.Characters.AddRange(characters);
        var matchingCharacters = await TestUtility
                                 .InvokePrivateMethodAsync<List<Character>>
                                 (controller, "SearchCharactersAsync", query);

        // Assert
        Assert.AreEqual(2, matchingCharacters.Count);
        Assert.IsTrue(matchingCharacters.Any(c => c.Name == "Johnny Katana"));
        Assert.IsTrue(matchingCharacters.Any(c => c.Name == "Johanna Joy"));
    }

    [Test]
    public async Task SearchCharactersAsync_WhenCharacterNotFound_QueryDoesntFindMatch_ShouldReturnNotFound()
    {
        // Arrange
        string query = "Foo";
        characters = new List<Character>
        {
            new Character { Name = "Kung Fu" },
            new Character { Name = "Ping Pong" },
            new Character { Name = "Moo Fighters" }
        };


        // Act
        var matchingCharacters = await TestUtility
                         .InvokePrivateMethodAsync<List<Character>>
                         (controller, "SearchCharactersAsync", query);

        // Assert
        Assert.IsEmpty(matchingCharacters);
    }


    [TearDown]
    public void TearDown()
    {
        CharacterRepository.Characters.Clear();
    }
}
