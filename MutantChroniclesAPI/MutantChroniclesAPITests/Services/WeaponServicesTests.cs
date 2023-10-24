using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Driver;
using MutantChroniclesAPI.Model.WeaponModel;
using MutantChroniclesAPI.Repository;
using MutantChroniclesAPI.Services;
using MutantChroniclesAPI.Services.Data;

namespace MutantChroniclesAPI.Tests.Services;


[TestFixture]
public class WeaponServiceTests
{
    private MongoDbRunner runner;
    private IMongoClient mongoClient;
    private IMongoDatabase mongoDatabase;
    private IMongoCollection<Weapon> weaponsCollection;
    private IOptions<MongoDBSettings> mongoDBSettings;

    private WeaponService weaponService;

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

        mongoDBSettings = Options.Create(settings);

        mongoClient = new MongoClient(test_ConnectionString);
        mongoDatabase = mongoClient.GetDatabase(test_DatabaseName);
        weaponsCollection = mongoDatabase.GetCollection<Weapon>(test_CollectionName);

        weaponService = new WeaponService(mongoDBSettings);


        CharacterRepository.Characters.Clear();
    }

    [Test]
    public void TestMongoDBConnection()
    {
        //Arrange: Setup()

        // Act
        var weaponCount = weaponService.GetAsync();

        // Assert
        Assert.IsNotNull(weaponCount, "Database connection test failed.");
    }

    [Test]
    public async Task GetAsync_ReturnListOfWeaponsAsync()
    {
        //Arrange: Setup()
        var expectedWeapons = new List<Weapon>
        {
            new Weapon { Name = "Weapon1" },
            new Weapon { Name = "Weapon2" }
        };
        await weaponsCollection.InsertManyAsync(expectedWeapons);

        // Act
        var result = await weaponService.GetAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<List<Weapon>>(result);
        Assert.AreEqual(expectedWeapons.Count, result.Count);
        Assert.AreEqual(expectedWeapons[0].Name, result[0].Name);
        Assert.AreEqual(expectedWeapons[1].Name, result[1].Name);
    }

    [Test]
    public async Task GetByNameAsync_WhenExactMatchExists_ShouldReturnWeapon()
    {
        // Arrange
        string targetWeaponName = "Bolter";
        var targetWeapon = new Weapon { Name = targetWeaponName };


        // Act
        await weaponsCollection.InsertOneAsync(targetWeapon);
        var result = await weaponService.GetByNameAsync(targetWeaponName);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(targetWeaponName, result.Name);
    }

    [Test]
    public async Task GetByNameAsync_WhenExactMatchDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        string nonExistentWeaponName = "NonExistentWeapon";

        // Act
        var result = await weaponService.GetByNameAsync(nonExistentWeaponName);

        // Assert
        Assert.IsNull(result);
    }

    [TearDown]
    public void TearDown()
    {
        runner.Dispose();
    }

}
