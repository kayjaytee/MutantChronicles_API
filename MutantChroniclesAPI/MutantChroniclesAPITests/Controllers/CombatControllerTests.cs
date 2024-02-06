using MC_Weapon_Calculator.Model;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MutantChroniclesAPI.Controllers;
using MutantChroniclesAPI.Enums;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Model.EnviromentModel;
using MutantChroniclesAPI.Model.WeaponModel;
using MutantChroniclesAPI.Repository;
using MutantChroniclesAPI.Services;
using MutantChroniclesAPI.Services.Combat;
using NSubstitute;
using NUnit.Framework.Internal;
using System.Diagnostics;
using System.Reflection;

namespace MutantChroniclesAPI.Tests.Controllers;

public class CombatControllerTests
{
    private CombatController controller;
    private EnviromentService enviromentService;
    private CombatService combatService;
    private Dice dice;

    [SetUp]
    public void Setup()
    {
        enviromentService = Substitute.For<EnviromentService>();
        combatService = Substitute.For<CombatService>();
        controller = new CombatController(enviromentService, combatService);
    }

    [Test]
    public async Task StartCombat_ShouldCreateNewCombatRound_WithNewCopyOf_CharacterRepository()
    {
        // Arrange
        var combatants = new List<Character>
        {
            new Character { Name = "Speedy Simon", InitiativeBonus = 5 },
            new Character { Name = "Slow Steve", InitiativeBonus = 1 },
            new Character { Name = "Easy Earl", InitiativeBonus = 2 },
            new Character { Name = "Rapid Ronald", InitiativeBonus = 7 }
        };
        CharacterRepository.Characters.AddRange(combatants);

        // Act
        TestUtility.SetPrivateField(controller, "combatInProgress", false); //private static bool CombatInProgress;
        var result = await controller.StartCombat();
        TestUtility.SetPrivateField(controller, "combatInProgress", true); //private static bool CombatInProgress;

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task DisplayTurn_ReturnOk()
    {
        // Arrange
        var characters = new List<Character>
        {
            new Character { Name = "Speedy Simon", InitiativeBonus = 5 },
            new Character { Name = "Slow Steve", InitiativeBonus = 1 },
            new Character { Name = "Easy Earl", InitiativeBonus = 2 },
            new Character { Name = "Rapid Ronald", InitiativeBonus = 7 }
        };
        CharacterRepository.Characters.AddRange(characters);


        // Act
        TestUtility.SetPrivateField(typeof(CombatController), "combatInProgress", true); //private static bool CombatInProgress;
        var result = await controller.DisplayTurn();

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task WhenCombatIsInProgress_ClickNextRound_ShouldIncrementRound_ReRollInitiative_ReturnOk()
    {
        // Arrange
        var combatants = new List<Character>
        {
            new Character { Name = "Speedy Simon", InitiativeBonus = 5 },
            new Character { Name = "Slow Steve", InitiativeBonus = 1 },
            new Character { Name = "Easy Earl", InitiativeBonus = 2 },
            new Character { Name = "Rapid Ronald", InitiativeBonus = 7 }
        };

        CharacterRepository.Characters.AddRange(combatants);

        TestUtility.SetPrivateField(controller, "combatInProgress", false); //private static bool combatInProgress;
        var startCombat = await controller.StartCombat(); // Start combat to initialize the combatants list

        TestUtility.SetPrivateField(controller, "currentRound", 1); //private static int currentRound = 1;
        TestUtility.SetPrivateField(controller, "combatInProgress", true);//private static bool combatInProgress;

        //Act
        var nextRound = await controller.NextRound();

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(nextRound);

        // Assert: Verify increament by field integer
        var currentRoundField = typeof(CombatController).GetField("currentRound", BindingFlags.NonPublic | BindingFlags.Static);
        var currentRoundValue = (int)currentRoundField.GetValue(null);
        Assert.AreEqual(2, currentRoundValue);

    }



    #region Shooting Test Cases
    [Test]
    public async Task Action_Shoot_HappyFlow()
    {
        //Arrange

        var lightMock = enviromentService.SetLight(Enviroment.Light.None);
        var weatherMock = enviromentService.SetWeather(Enviroment.Weather.None);

        var combatants = new List<Character>
        {
            new Character { Name = "Badass Boris" },
            new Character { Name = "Tommy Targetsson", CurrentlyUnderFire = new List<Character>() },
        };
        var firstCharacter = combatants.First();

        var defeated = new List<Character>();

        var initiativeOrder = new List<(Character, int initiative)>
        {
           (combatants[0], 10), // Badass Boris with initiative 10
           (combatants[1], 5)   // Tommy Targetsson with initiative 5
        };

        var testWeapon = new Weapon
        {
            Name = "testWeapon",
            MagazineCapacity = 10,
            CurrentAmmo = 10,
            WeaponFunctionalityEnum = 0
        };

        var characterCurrentTurn = firstCharacter;
        characterCurrentTurn.ActionsRemaining = 1;
        characterCurrentTurn.MainHandEquipment = testWeapon;
        var combatInprogress = true;

        int baseChance = 99; //Guaranteed success
        var aim = AimType.Uncontrolled;
        var firingMode = FiringMode.SingleRound;
        RapidVolleyBulletsCount? rapidVolleyBulletsCount = null;

        var target = "Tommy Targetsson";
        var range = 2m;
        var secondaryModeActivated = false;

        #region Arrange: Private Fields

        TestUtility.SetPrivateField(controller, "characterCurrentTurn", characterCurrentTurn);
        TestUtility.SetPrivateField(controller, "combatInProgress", combatInprogress);
        TestUtility.SetPrivateField(controller, "combatants", combatants);
        TestUtility.SetPrivateField(controller, "defeated", combatants);
        TestUtility.SetPrivateField(controller, "initiativeOrder", initiativeOrder);

        #endregion

        //Act

        var sut = await controller.ActionShoot(baseChance, secondaryModeActivated, firingMode, aim, target, range, rapidVolleyBulletsCount);

        //Assert

        Assert.IsTrue(baseChance > 1);
        Assert.IsTrue(combatInprogress);
        Assert.IsTrue(characterCurrentTurn.MainHandEquipment.CurrentAmmo > 0);
        Assert.IsNotNull(characterCurrentTurn);
        Assert.IsInstanceOf<OkObjectResult>(sut);

    }

    [Test]
    public async Task Action_SecondaryFiringMode_HappyFlow() //Same process as above, but for secondaryFiringMode
    {
        //Arrange

        var combatants = new List<Character>
        {
            new Character { Name = "Badass Boris" },
            new Character { Name = "Tommy Targetsson" },
        };
        var firstCharacter = combatants.First();

        var defeated = new List<Character>();

        var initiativeOrder = new List<(Character, int initiative)>
        {
           (combatants[0], 10), // Badass Boris with initiative 10
           (combatants[1], 5)   // Tommy Targetsson with initiative 5
        };

        var testWeapon = new Weapon
        {
            Name = "testWeapon",
            SecondaryMode = new SecondaryMode
            {
                NameDescription = "testSecondaryMode",
                Equipped = true,
                MagazineCapacity = 10,
                CurrentAmmo = 10,
                WeaponFunctionalityEnum = 0
            }
        };

        var characterCurrentTurn = firstCharacter;
        characterCurrentTurn.ActionsRemaining = 1;
        characterCurrentTurn.MainHandEquipment = testWeapon;
        var combatInprogress = true;

        int baseChance = 99; //Guaranteed success
        var aim = AimType.Uncontrolled;
        var firingMode = FiringMode.SingleRound;
        RapidVolleyBulletsCount? rapidVolleyBulletsCount = null;

        var target = "Tommy Targetsson";
        var range = 2m;
        var secondaryModeActivated = true;

        //Act

        TestUtility.SetPrivateField(controller, "characterCurrentTurn", characterCurrentTurn);
        TestUtility.SetPrivateField(controller, "combatInProgress", combatInprogress);
        TestUtility.SetPrivateField(controller, "combatants", combatants);
        TestUtility.SetPrivateField(controller, "defeated", combatants);
        TestUtility.SetPrivateField(controller, "initiativeOrder", initiativeOrder);

        var result = await controller.ActionShoot(baseChance, secondaryModeActivated, firingMode, aim, target, range, rapidVolleyBulletsCount);

        //Assert

        Assert.IsTrue(baseChance > 1);
        Assert.IsTrue(combatInprogress);
        Assert.IsTrue(characterCurrentTurn.MainHandEquipment.SecondaryMode.CurrentAmmo > 0);
        Assert.IsNotNull(characterCurrentTurn);
        Assert.IsInstanceOf<OkObjectResult>(result);

    }

    [Test]
    public async Task Action_Shoot_NoActionsRemaining_ReturnBadRequest()
    {

        //Arrange: Input Parameters
        var baseChance = 99; //High chance to test a working flow
        var firingMode = FiringMode.SingleRound;
        var aim = AimType.Uncontrolled;
        var target = "ValidTarget";
        var range = 10.0m;
        var rapidVolleyBulletsCount = RapidVolleyBulletsCount.Three; //This will not affect the shot unless FiringMode is declared same.
        var secondaryModeActivated = false;

        //Arrange: Field
        var characterCurrentTurn = new Character();
        characterCurrentTurn.ActionsRemaining = 0;

        var combatInprogress = true;

        //Act
        TestUtility.SetPrivateField(controller, "characterCurrentTurn", characterCurrentTurn);
        TestUtility.SetPrivateField(controller, "combatInProgress", combatInprogress);
        var result = await controller.ActionShoot(baseChance, secondaryModeActivated, firingMode, aim, target, range, rapidVolleyBulletsCount);

        //Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        Assert.IsInstanceOf<IActionResult>(result);

    }

    [Test]
    public async Task Action_Shoot_NoWeaponEquipped_ReturnsBadRequest()
    {
        //Arrange
        var baseChance = 99; //High chance to test a working flow
        var firingMode = FiringMode.SingleRound;
        var aim = AimType.Uncontrolled;
        var target = "ValidTarget";
        var range = 10.0m;
        var rapidVolleyBulletsCount = RapidVolleyBulletsCount.Three; //This will not affect the shot unless FiringMode is declared same.
        var secondaryModeActivated = false;

        var characterCurrentTurn = new Character();
        characterCurrentTurn.MainHandEquipment = null!;

        //Act
        TestUtility.SetPrivateField(controller, "characterCurrentTurn", characterCurrentTurn);
        var result = await controller.ActionShoot(baseChance, secondaryModeActivated, firingMode, aim, target, range, rapidVolleyBulletsCount);

        //Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        Assert.IsInstanceOf<IActionResult>(result);
        Assert.IsNull(characterCurrentTurn.MainHandEquipment);
    }

    [Test]
    public async Task Action_Shoot_NoAmmo_ReturnsBadRequest()
    {
        //Arrange
        var baseChance = 99; //High chance to test a working flow
        var firingMode = FiringMode.SingleRound;
        var aim = AimType.Uncontrolled;
        var target = "ValidTarget";
        var range = 10.0m;
        var rapidVolleyBulletsCount = RapidVolleyBulletsCount.Three; //This will not affect the shot unless FiringMode is declared same.
        var secondaryModeActivated = false;

        var characterCurrentTurn = new Character();
        characterCurrentTurn.MainHandEquipment = new Weapon
        {
            Name = "testWeapon",
            MagazineCapacity = 10,
            CurrentAmmo = 0,

        };

        //Act
        TestUtility.SetPrivateField(controller, "characterCurrentTurn", characterCurrentTurn);
        var result = await controller.ActionShoot(baseChance, secondaryModeActivated, firingMode, aim, target, range, rapidVolleyBulletsCount);

        //Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        Assert.IsInstanceOf<IActionResult>(result);
        Assert.IsTrue(characterCurrentTurn.MainHandEquipment.CurrentAmmo <= 0);
    }

    #endregion Shooting Test Cases

    [Test]
    public async Task Action_Reload__HappyFlow()
    {

        // Arrange

        int baseChance = 99; //High value ensures success
        var secondaryModeActivated = false; //Reloads the main firing magazine and not any secondary weapon mode.
        var reloadingTime = 2; // Assuming reloading takes 2 actions

        var equippedWeapon = new Weapon { MagazineCapacity = 10, ReloadingTime = reloadingTime, AmmoType = Ammo.AmmoType.Standard };
        var character = new Character
        {
            Physique = 10,
            MentalStrength = 10,
            ActionsRemaining = reloadingTime + 1,
            MainHandEquipment = equippedWeapon,


        };
        var target = new Target(character);


        foreach (var bodyPart in new[]
        { character.Target.Head,
              character.Target.Chest,
              character.Target.Stomach,
              character.Target.RightArm,
              character.Target.LeftArm,
              character.Target.RightLeg,
              character.Target.LeftLeg })
        {
            bodyPart.MaximumBodyPoints = 5;
            bodyPart.TemporaryBodyPoints = 5;
        }

        var initiativeOrder = new List<(Character, int initiative)>();
        var ammoType = Ammo.AmmoType.Standard;

        TestUtility.SetPrivateField(controller, "characterCurrentTurn", character); // private field characterCurrentTurn
        TestUtility.SetPrivateField(controller, "initiativeOrder", initiativeOrder);

        // Act
        var result = await controller.ActionReload(baseChance, secondaryModeActivated, ammoType);

        // Assert
        Console.WriteLine($"Magazine Capicity: {equippedWeapon.MagazineCapacity}");
        Console.WriteLine($"Current Ammo: {equippedWeapon.CurrentAmmo}");
        Assert.IsNotNull(result);

    }

    [Test]
    public void Action_Unjam_Weapon_()
    {

    }

    [Test]
    public void ApplyAimingMode_InvalidAimType_ReturnsBadRequest()
    {
        // Arrange
        int baseChance = 20;
        decimal range = 2m;

        AimType invalidAim = (AimType)99;
        Weapon weapon = new Weapon();
        Character character = new Character();

        // Act
        var result = TestUtility.InvokePrivateMethod<IActionResult>(controller, "ApplyAimingMode", invalidAim, weapon, character, range, baseChance);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public void ApplyAimingMode_Uncontrolled_NotEnoughActions_ReturnsBadRequest()
    {
        // Arrange
        int baseChance = 20;
        decimal range = 2m;

        AimType aim = AimType.Uncontrolled;
        Weapon weapon = new Weapon();
        Character character = new Character { ActionsRemaining = 0 }; // Set ActionsRemaining to 0

        // Act
        IActionResult result = TestUtility.InvokePrivateMethod<IActionResult>(controller, "ApplyAimingMode", aim, weapon, character, range, baseChance);


        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public void ApplyAimingMode_Aimed_Successful()
    {
        // Arrange
        int baseChance = 20;
        decimal range = 99m;

        AimType aim = AimType.Aimed;
        Weapon weapon = new Weapon { SpecialSightModifiers = new SpecialSightModifiers { LaserSight = true } };
        Character character = new Character { ActionsRemaining = 2 };

        // Act
        var result = TestUtility.InvokePrivateMethod<IActionResult>(controller, "ApplyAimingMode", aim, weapon, character, range, baseChance);

        // Assert
        Assert.IsNull(result);
        Assert.AreEqual(1, character.ActionsRemaining); // Check if ActionsRemaining has been decremented
    }

    [Test]
    [TestCase(4)]
    [TestCase(15)]
    [TestCase(25)]
    public void CalculateMeleeDamage_CalculatesWeight_ReturnsDamageDice_WithOffensiveBonus(decimal weight)
    {
        // Arrange
        var testWeapon = new Weapon { Weight = weight };
        var testCharacter = new Character { OffensiveBonus = 5, MainHandEquipment = testWeapon };

        // Act
        TestUtility.SetPrivateField(controller, "characterCurrentTurn", testCharacter);
        int damage = TestUtility.InvokePrivateMethod<int>(controller, "CalculateMeleeAttack", testCharacter, testWeapon);
        int damage_excluding_offensivebonus = damage - testCharacter.OffensiveBonus;

        // Assert
        Console.WriteLine($"Weapon weight: {weight} KG, Calculated damage: {damage}, Excluding Offensive Bonus: {damage - testCharacter.OffensiveBonus}");
        switch (weight)
        {
            case decimal x when x < 5:
                Assert.IsTrue(damage_excluding_offensivebonus >= 1 && damage_excluding_offensivebonus <= 4);
                break;

            case decimal x when x >= 5 && x <= 20:
                Assert.IsTrue(damage_excluding_offensivebonus >= 1 && damage_excluding_offensivebonus <= 5);
                break;

            case decimal x when x > 20:
                Assert.IsTrue(damage_excluding_offensivebonus >= 1 && damage_excluding_offensivebonus <= 6);
                break;

            default:
                Assert.Fail("Unexpected weight value");
                break;
        }

    }

    [Test]
    public void CalculateMeleeDamage_CalculatesChainBayonet_ReturnsDamageDice_WithOffensiveBonus()
    {
        // Arrange
        var testCharacter = new Character { OffensiveBonus = 5 };
        var chainBayonet = new ChainBayonet
        {
            Equipped = true,
            NameDescription = "TestBayonet",
            DamageMin = 1,
            DamageMax = 6,
            DamageAdded = 1
        };
        var testWeapon = new Weapon { ChainBayonet = chainBayonet };
        TestUtility.SetPrivateField(controller, "characterCurrentTurn", testCharacter);

        // Act
        int damage = TestUtility.InvokePrivateMethod<int>(controller, "CalculateMeleeAttack", testCharacter, testWeapon);
        int damage_excluding_offensivebonus = damage - testCharacter.OffensiveBonus;

        // Assert
        Console.WriteLine($"Weapon ChainBayonet: '{chainBayonet.NameDescription}', Calculated damage: {damage}, Excluding Offensive Bonus: {damage - testCharacter.OffensiveBonus}");
        Assert.IsTrue(damage_excluding_offensivebonus >= chainBayonet.DamageMin &&
                      damage_excluding_offensivebonus <= chainBayonet.DamageMax + chainBayonet.DamageAdded,
                      "Unexpected damage value for ChainBayonet");

    }

    [Test]
    public void EndCombat_ClearsData()
    {
        //Arrange
        var defeated = new List<Character>();
        var combatants = new List<Character>
        {
            new Character //1
            {
                Name = "Character 1",
                Target = new Target(new Character())
            },

            new Character //2
            {
                Name = "Character 2",
                Target = new Target(new Character())
            },
        };
        var initiativeOrder = new List<(Character character, int initiative)>
        {
              (combatants[0], 2),
              (combatants[1], 1)
        };

        TestUtility.SetPrivateField(controller, "combatInProgress", true);
        TestUtility.SetPrivateField(controller, "combatants", combatants);
        TestUtility.SetPrivateField(controller, "defeated", defeated);
        TestUtility.SetPrivateField(controller, "initiativeOrder", initiativeOrder);


        //Act
        var sut = controller.EndCombat();

        //Assert
        Assert.IsEmpty(defeated);
        Assert.IsEmpty(combatants);
        Assert.IsEmpty(initiativeOrder);
    }



}
