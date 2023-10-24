using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Driver;
using MutantChroniclesAPI.Controllers;
using MutantChroniclesAPI.Model;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Model.WeaponModel;
using MutantChroniclesAPI.Repository;
using MutantChroniclesAPI.Services;
using MutantChroniclesAPI.Services.Data;
using MutantChroniclesAPI.Tests;
using NSubstitute;
using NSubstitute.ClearExtensions;
using System.Reflection;

namespace MutantChroniclesAPI.Tests.Controllers;

[TestFixture]
public class WeaponControllerTests
{
    private MongoDbRunner runner;
    private IMongoClient mongoClient;
    private IMongoDatabase mongoDatabase;
    private IMongoCollection<Weapon> weaponsCollection;
    private IOptions<MongoDBSettings> mongoDBSettings;
    private WeaponService weaponService;

    private WeaponController weaponController;

    [SetUp]
    public void Setup()
    {
        ///<Mongo2Go: Mock/Subtitute Setup - Summary>
        ///
        ///  To properly test a MongoDB Server integration, the runner creates a in-memory server which is a temporary instance.
        ///  that simulates the behavior of a real MongoDB-server.
        /// 
        /// 
        /// </Mongo2Go: Mock/Subtitute Setup - Summary>

        runner = MongoDbRunner.Start();

        var test_ConnectionString = runner.ConnectionString;
        var test_DatabaseName = "test_database";
        var test_CollectionName = "test_collection";

        var settings = new MongoDBSettings
        {
            ConnectionString = test_ConnectionString,
            DatabaseName = test_DatabaseName,
            CollectionName = test_CollectionName
        };

        mongoClient = new MongoClient(test_ConnectionString);
        mongoDatabase = mongoClient.GetDatabase(test_DatabaseName);
        weaponsCollection = mongoDatabase.GetCollection<Weapon>(test_CollectionName);

        mongoDBSettings = Options.Create(settings);

        weaponService = new WeaponService(mongoDBSettings);
        weaponController = new WeaponController(weaponService);
    }

    [Test]
    public async Task AppliesWeaponServiceAsync()
    {
        //Arrange: Setup()

        //Act
        var result = await weaponController.GetAllWeapons();

        //Assert
        Assert.NotNull(result);
    }

    [Test]
    public async Task GetAllWeapons_ReturnsListOfWeapons()
    {
        // Arrange
        var expectedWeapons = new List<Weapon>
        {
            new Weapon { Name = "Weapon1" },
            new Weapon { Name = "Weapon2" },
            new Weapon { Name = "Weapon3" }
        };
        await weaponsCollection.InsertManyAsync(expectedWeapons);

        // Act
        var result = await weaponController.GetAllWeapons();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedWeapons.Count, result.Count);
    }

    [Test]
    public async Task SearchWeapons_WithValidQuery_ReturnsOkWithMatchingWeapons()
    {
        // Arrange
        string query = "test";

        var expectedWeapons = new List<Weapon>
        {
            new Weapon { Name = "TestWeapon1" },
            new Weapon { Name = "TestWeapon2" },
            new Weapon { Name = "MockWeaponX" },
        };
        await weaponsCollection.InsertManyAsync(expectedWeapons);

        // Act
        var result = await weaponController.SearchWeapons(query);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        var actualWeapons = okResult.Value as List<Weapon>;

        Assert.AreEqual(2, actualWeapons.Count);

        Assert.IsTrue(actualWeapons.Any(x => x.Name == "TestWeapon1"));
        Assert.IsTrue(actualWeapons.Any(x => x.Name == "TestWeapon2"));
        Assert.IsFalse(actualWeapons.Any(x => x.Name == "MockWeaponX")); //This should be ignored by the SearchQuery since it doesn't match
    }

    [Test]
    public async Task EquipWeapon_CharacterFoundAndWeaponFound_EquipsWeaponAndReturnsOk()
    {
        // Arrange
    
        string characterName = "TestCharacter";
        Character testCharacter = new Character { Name = characterName };

        string weaponName = "TestWeapon";
        Weapon testWeapon = new Weapon { Name = weaponName };
        weaponsCollection.InsertOne(testWeapon);

        CharacterRepository.Characters.Add(testCharacter);

        // Act
        var result = await weaponController.EquipWeapon(weaponName, characterName);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        Assert.AreEqual(testWeapon.Name, testCharacter.EquippedWeapon.Name);
    }

    [Test]
    public void UpdateCharacter_EquipsWeapon_CalculatesStats()
    {
        // Arrange
        Character testCharacter = new Character { Strength = 15,
                                                  Coordination = 15,
                                                  Intelligence = 15,
                                                  MentalStrength = 15 };

        Weapon testWeapon = new Weapon { Weight = 2.5M };

        // Act
        TestUtility.InvokePrivateMethod(weaponController, "UpdateCharacter", testCharacter, testWeapon);

        // Assert
        Assert.AreEqual(2.5M, testCharacter.WeightCarried);
        Assert.AreEqual(0, testCharacter.CalculateWeightPenalty());
        Assert.AreEqual(3 - 0, testCharacter.ActionsPerRound);
    }

    [Test]
    public async Task EquipWeapon_CharacterNotFound_ReturnsNotFound()
    {
        // Arrange
        string weaponName = "TestWeapon";
        string characterName = "UnknownCharacter";

        // Act
        IActionResult result = await weaponController.EquipWeapon(weaponName, characterName);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
        var notFoundResult = (NotFoundObjectResult)result;
        Assert.AreEqual("Character name could not be found.", notFoundResult.Value);
    }

    [Test]
    public async Task SearchCharactersAsync_WhenCharacterNotFound_ShouldReturnSimilarMatches() //Sometimes throws fail, possibly due to error with TearDown()
    {
        // Arrange
        string query = "joh";
        var characters = new List<Character>
        {
            new Character { Name = "Johnny Katana" },
            new Character { Name = "Jeff Goldblum" },
            new Character { Name = "John Blaster" }
        };


        // Act
        CharacterRepository.Characters.AddRange(characters);
        var matchingCharacters = await TestUtility
                                .InvokePrivateMethodAsync<List<Character>>
                                (weaponController, "SearchCharactersAsync", query);

        // Assert
        Assert.AreEqual(2, matchingCharacters.Count);
        Assert.IsTrue(matchingCharacters.Any(x => x.Name == "Johnny Katana"));
        Assert.IsTrue(matchingCharacters.Any(x => x.Name == "John Blaster"));
    }


}
