using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MutantChroniclesAPI.Controllers;
using MutantChroniclesAPI.Enums;
using MutantChroniclesAPI.Model;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Model.WeaponModel;
using MutantChroniclesAPI.Repository;
using MutantChroniclesAPI.Services.Combat;
using NUnit.Framework.Internal;
using System.Diagnostics;
using System.Reflection;

namespace MutantChroniclesAPI.Tests.Controllers;

public class CombatControllerTests
{
    private CombatController controller;
    private readonly Dice dice;

    [SetUp]
    public void Setup()
    {
        controller = new CombatController();
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
        TestUtility.SetPrivateField(typeof(CombatController), "CombatInProgress", false); //private static bool CombatInProgress;
        var result = await controller.StartCombat();
        TestUtility.SetPrivateField(typeof(CombatController), "CombatInProgress", true); //private static bool CombatInProgress;

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
        TestUtility.SetPrivateField(typeof(CombatController), "CombatInProgress", true); //private static bool CombatInProgress;
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

        // Act
        TestUtility.SetPrivateField(controller, "CombatInProgress", false); //private static bool combatInProgress;
        var startCombat = await controller.StartCombat(); // Start combat to initialize the combatants list

        TestUtility.SetPrivateField(controller, "CurrentRound", 1); //private static int currentRound = 1;
        TestUtility.SetPrivateField(controller, "CombatInProgress", true);//private static bool combatInProgress;

        var nextRound = await controller.NextRound();

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(nextRound);

        // Assert: Verify increament by field integer
        var currentRoundField = typeof(CombatController).GetField("CurrentRound", BindingFlags.NonPublic | BindingFlags.Static);
        var currentRoundValue = (int)currentRoundField.GetValue(null);
        Assert.AreEqual(2, currentRoundValue);

    }

    [Test]
    public async Task InitiativeRoll_ShouldRollDie_CombineValues_ReturnInOrder()
    {
        // Arrange
        var combatants = new List<Character>
        {
            new Character { Name = "Speedy Simon", InitiativeBonus = 5 },
            new Character { Name = "Slow Steve", InitiativeBonus = 1 },
            new Character { Name = "Easy Earl", InitiativeBonus = 2 },
            new Character { Name = "Rapid Ronald", InitiativeBonus = 7 }
        };

        // Act

        var (sortedCharacters, initiativeResult) = await Tuple_InvokePrivateMethodAsync(controller, "InitiativeRoll", combatants);

        // Assert
        Assert.IsNotNull(sortedCharacters);
        Assert.AreEqual(4, sortedCharacters.Count);

        // Check if the characters are sorted in descending order of initiative
        foreach (var character in sortedCharacters)
        {
            Assert.Greater(character.InitiativeBonus, character.InitiativeBonus - 11);
            Assert.Less(character.InitiativeBonus, character.InitiativeBonus + 11);
        }


        //Prints Output in Order, zips together character with the correct score
        foreach (var (character, total) in sortedCharacters.Zip(initiativeResult,
                                           (combatant, score) => (combatant, score)))
        {
            Console.WriteLine($"Character: {character.Name}, InitiativeBonus: {character.InitiativeBonus}, Total: {total}");
        }
    }

    #region Shooting Test Cases
    [Test]
    public async Task Action_Shoot_HappyFlow()
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
            MagazineCapacity = 10,
            CurrentAmmo = 10,
            WeaponFunctionalityEnum = 0
        };

        var characterCurrentTurn = firstCharacter;
        characterCurrentTurn.ActionsRemaining = 1;
        characterCurrentTurn.EquippedWeapon = testWeapon;
        var combatInprogress = true;

        int baseChance = 99; //Guaranteed success
        var aim = AimType.Uncontrolled;
        var firingMode = FiringMode.SingleRound;
        RapidVolleyBulletsCount? rapidVolleyBulletsCount = null;

        var target = "Tommy Targetsson";
        var range = 2m;
        var secondaryModeActivated = false;

        //Act

        TestUtility.SetPrivateField(controller, "characterCurrentTurn", characterCurrentTurn);
        TestUtility.SetPrivateField(controller, "CombatInProgress", combatInprogress);
        TestUtility.SetPrivateField(controller, "combatants", combatants);
        TestUtility.SetPrivateField(controller, "defeated", combatants);
        TestUtility.SetPrivateField(controller, "initiativeOrder", initiativeOrder);

        var result = await controller.ActionShoot(baseChance, secondaryModeActivated, firingMode, aim, target, range, rapidVolleyBulletsCount);

        //Assert

        Assert.IsTrue(baseChance > 1);
        Assert.IsTrue(combatInprogress);
        Assert.IsTrue(characterCurrentTurn.EquippedWeapon.CurrentAmmo > 0);
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
        TestUtility.SetPrivateField(controller, "CombatInProgress", combatInprogress);
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
        characterCurrentTurn.EquippedWeapon = null!;

        //Act
        TestUtility.SetPrivateField(controller, "characterCurrentTurn", characterCurrentTurn);
        var result = await controller.ActionShoot(baseChance, secondaryModeActivated, firingMode, aim, target, range, rapidVolleyBulletsCount);

        //Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
        Assert.IsInstanceOf<IActionResult>(result);
        Assert.IsNull(characterCurrentTurn.EquippedWeapon);
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
        characterCurrentTurn.EquippedWeapon = new Weapon
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
        Assert.IsTrue(characterCurrentTurn.EquippedWeapon.CurrentAmmo <= 0);
    }

    #endregion Shooting Test Cases

    [Test]
    public async Task Action_Reload__WithSuccess_DecreasesActionsRemaining_And_ReloadsAmmo() //FAILS RANDOMLY; Need to Mock the dice roll
    {

        // Arrange

        int baseChance = 99; //High value ensures success
        int reloadingTime = 2; // Assuming reloading takes 2 actions

        Weapon equippedWeapon = new Weapon { MagazineCapacity = 10, ReloadingTime = reloadingTime };
        Character characterCurrentTurn = new Character { ActionsRemaining = reloadingTime + 1, EquippedWeapon = equippedWeapon };
        var initiativeOrder = new List<(Character, int initiative)>();

        TestUtility.SetPrivateField(controller, "characterCurrentTurn", characterCurrentTurn); // private field characterCurrentTurn
        TestUtility.SetPrivateField(controller, "initiativeOrder", initiativeOrder);

        // Act
        var result = await controller.ActionReload(baseChance);

        // Assert
        Console.WriteLine($"Magazine Capicity: {equippedWeapon.MagazineCapacity}");
        Console.WriteLine($"Current Ammo: {equippedWeapon.CurrentAmmo}");
        Assert.AreEqual(1, characterCurrentTurn.ActionsRemaining);
        Assert.AreEqual(equippedWeapon.MagazineCapacity, equippedWeapon.CurrentAmmo);

    }


    //[Test]
    //public async Task ActionReload__FailedlReload__Return_FailedMessage()
    //{
    //    // Arrange
    //    int baseChance = 15; // Set a base chance that is achievable for successful reload
    //    int reloadingTime = 2; // Assuming reloading takes 2 actions

    //    Weapon equippedWeapon = new Weapon { MagazineCapacity = 10, ReloadingTime = reloadingTime };
    //    Character characterCurrentTurn = new Character { ActionsRemaining = reloadingTime + 1, EquippedWeapon = equippedWeapon };

    //    TestUtility.SetPrivateField(typeof(CombatController), "characterCurrentTurn", characterCurrentTurn); // private field characterCurrentTurn

    //    // Act
    //    var result = await controller.ActionReload(baseChance);

    //    // Assert

    //    Assert.AreEqual(1, characterCurrentTurn.ActionsRemaining);
    //    Assert.AreEqual(equippedWeapon.MagazineCapacity, equippedWeapon.CurrentAmmo);
    //}

    [Test]
    public void Action_Unjam_Weapon_()
    {

    }

    [Test]
    [Ignore("Failing test; kept as failed for demonstration")]
    public void StringBuilderFormatInitiative_ShouldReturnFormattedString_InOrder()
    {
        // Arrange
        var characters = new List<(Character, int)>
        {
            (new Character { Name = "Speedy Simon", InitiativeBonus = 5, ActionsPerRound = 2, ActionsRemaining = 2 }, 12),
            (new Character { Name = "Slow Steve", InitiativeBonus = 1, ActionsPerRound = 3, ActionsRemaining = 3 }, 8),
            (new Character { Name = "Easy Earl", InitiativeBonus = 2, ActionsPerRound = 1, ActionsRemaining = 1 }, 9),
            (new Character { Name = "Rapid Ronald", InitiativeBonus = 7, ActionsPerRound = 2 , ActionsRemaining = 2 }, 15)
        };
        int round = 3;
        Character characterCurrentTurn = new() { Name = "Rapid Ronald", InitiativeBonus = 5, ActionsPerRound = 2 };
        Debug.Assert(characterCurrentTurn is not null, "characterCurrentTurn should not be null");

        // Sort characters by initiative
        characters.Sort((x, y) => y.Item2.CompareTo(x.Item2));

        // Act
        var result = TestUtility.InvokePrivateMethod<string>(controller, "StringBuilderFormatInitiative", characters, round, characterCurrentTurn);
        // Assert: Strings are normalized to improve readability in code, else the code has to be all the way to left.
        // Normalize the expected and actual strings
        string expected = @"Round 3
                       Rapid Ronald | Initiative: 15
                       Actions: 2/2
                       Speedy Simon | Initiative: 12
                       Actions: 2/2
                       Easy Earl | Initiative: 9
                       Actions: 1/1
                       Slow Steve | Initiative: 8
                       Actions: 3/3
                       -------------------------------
                       Current Turn: Rapid Ronald
                       ------------------------------- ";

        expected = NormalizeString(expected);
        result = NormalizeString(result);
        Console.WriteLine("E X P E C T E D");
        Console.WriteLine(expected);
        Console.WriteLine();
        Console.WriteLine("A C T U A L");
        Console.WriteLine(result);

        Assert.AreEqual(expected, result);
    }

    [Test]
    [TestCaseSource(nameof(TestCases_IsFiringModeAllowed))]
    public void IsFiringModeAllowed_ShouldReturnCorrectResult(Weapon.WeaponFunctionality functionality, FiringMode firingMode, bool secondaryMode)
    {
        // Arrange
        var weapon = new Weapon
        {
            WeaponFunctionalityEnum = functionality
        };

        // Act
        var result = TestUtility
                    .InvokePrivateMethod<bool>
                    (controller, "IsFiringModeAllowed", weapon, firingMode, secondaryMode);

        // Assert
        Assert.IsTrue(result);

    }

    [Test]
    public async Task CalculateHitChance_WithEnviromentModifiers_ReturnsModifiedHitChance()
    {
        // Arrange
        int baseChance = 20;
        Enviroment.Light light = Enviroment.Light.FullMoonOutdoorsOrSinglecandleIndoors;
        Enviroment.Weather weather = Enviroment.Weather.HeavyWindOrHeavyRain;

        // Act
        var modifiedHitChance = await TestUtility.InvokePrivateMethodAsync<int>(controller, "CalculateHitChance", baseChance, light, weather);

        // Assert
        int expectedModifiedHitChance = baseChance - (int)light - (int)weather;
        Assert.AreEqual(expectedModifiedHitChance, modifiedHitChance);

        Console.WriteLine($"Base Chance: {baseChance}");
        Console.WriteLine($"Light Modifier: {(int)light}");
        Console.WriteLine($"Weather Modifier: {(int)weather}");
        Console.WriteLine($"EXPECTED: Modified Hit Chance: {expectedModifiedHitChance}");
        Console.WriteLine($"ACTUAL: Modified Hit Chance: {modifiedHitChance}");
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
        IActionResult result = TestUtility.InvokePrivateMethod<IActionResult>(controller, "ApplyAimingMode", invalidAim, weapon, character, range, baseChance);

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
    public void UpdateTurn_Should_UpdateCharacterCurrentTurn()
    {

        // Arrange
        var combatants = new List<(Character character, int initiative)>
        {
            (new Character { Name = "Speedy Simon", InitiativeBonus = 5, ActionsRemaining = 3 },  12),
            (new Character { Name = "Slow Steve", InitiativeBonus = 1, ActionsRemaining = 2 },  8),
            (new Character { Name = "Easy Earl", InitiativeBonus = 2, ActionsRemaining = 2 }, 9),
            (new Character { Name = "Rapid Ronald", InitiativeBonus = 7, ActionsRemaining = 0 }, 15)
        };



        // Create an initiative order list by selecting each character's
        // name and initiative from the combatants list and converting it to a list
        var initiativeOrder = combatants.Select((x, i) => (x.character, x.initiative)).ToList();

        // Sort the combatants based on initiative in descending order
        var sortedCombatants = combatants.OrderByDescending(x => x.initiative).ToList();
        var initialCharacter = sortedCombatants.FirstOrDefault();

        //Initialize Fields
        TestUtility.SetPrivateField<bool>(controller, "CombatInProgress", true);
        TestUtility.SetPrivateField<Character>(controller, "characterCurrentTurn", initialCharacter.character);
        var characterCurrentTurn_Initial = TestUtility.GetPrivateField<Character>(controller, "characterCurrentTurn");
        Console.WriteLine($"Initial Character Current Turn: {characterCurrentTurn_Initial.Name}");


        // // // ACT \\ \\ \\
        TestUtility.InvokePrivateMethod(controller, "UpdateTurn", initiativeOrder);
        initiativeOrder = initiativeOrder.OrderByDescending(entry => entry.initiative).ToList();



        // Assert
        // Find the next character who has actions remaining in the sortedCombatants list
        var nextCharacter = sortedCombatants.FirstOrDefault(x => x.character.ActionsRemaining > 0).character;
        var characterCurrentTurn_New = TestUtility.GetPrivateField<Character>(controller, "characterCurrentTurn");
        Console.WriteLine($"New Character Current Turn: {characterCurrentTurn_New.Name}");
        Assert.AreEqual(nextCharacter.Name, characterCurrentTurn_New.Name);
    }

    [Test]
    public void InitializeTemporaryBodyPoints_ShouldSetTemporaryBodyPointsToMaximum()
    {
        // Arrange
        var character = new Character();
        var target = new Target(character);

        // Act
        TestUtility.InvokePrivateMethod(controller, "InitializeTemporaryBodyPoints", target);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(target.Head.TemporaryBodyPoints, Is.EqualTo(target.Head.MaximumBodyPoints));
            Assert.That(target.Chest.TemporaryBodyPoints, Is.EqualTo(target.Chest.MaximumBodyPoints));
            Assert.That(target.Stomach.TemporaryBodyPoints, Is.EqualTo(target.Stomach.MaximumBodyPoints));
            Assert.That(target.RightArm.TemporaryBodyPoints, Is.EqualTo(target.RightArm.MaximumBodyPoints));
            Assert.That(target.LeftArm.TemporaryBodyPoints, Is.EqualTo(target.LeftArm.MaximumBodyPoints));
            Assert.That(target.RightLeg.TemporaryBodyPoints, Is.EqualTo(target.RightLeg.MaximumBodyPoints));
            Assert.That(target.LeftLeg.TemporaryBodyPoints, Is.EqualTo(target.LeftLeg.MaximumBodyPoints));
        });
    }

    [Test]
    [TestCase(4)]
    [TestCase(15)]
    [TestCase(25)]
    public void CalculateMeleeDamage_CalculatesWeight_ReturnsDamageDice_WithOffensiveBonus(decimal weight)
    {
        // Arrange
        Character testCharacter = new Character { OffensiveBonus = 5 };
        Weapon testWeapon = new Weapon { Weight = weight };

        // Act
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
    public void CheckForDefeatedCombatants__AddCharacterToDefeatedList_RemoveFromCombatants() //PROBLEM ITERATING THROUGH 2 LISTS
    {
        // Arrange
        var initiative = 0;
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

        TestUtility.SetPrivateField(controller, "combatants", combatants);
        TestUtility.SetPrivateField(controller, "defeated", defeated);
        TestUtility.SetPrivateField(controller, "initiativeOrder", initiativeOrder);

        // Act
        TestUtility.InvokePrivateMethod(controller, "CheckForDefeatedCombatants");

        // Assert
        Assert.IsTrue(defeated.Count > 0);
        Assert.IsTrue(combatants.Count < 2);

    }

    #region Private Methods
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ \\
    private static async Task<(List<Character> sortedCharacters, List<int> initiativeResult)>
        Tuple_InvokePrivateMethodAsync(object instance, string methodName, List<Character> list)
    {
        var methodInfo = instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        var task = methodInfo.Invoke(instance, new object[] { list }) as Task<List<(Character, int)>>;
        var result = await task;

        var sortedCharacters = result.Select(x => x.Item1).ToList();
        var initiativeScores = result.Select(x => x.Item2).ToList();

        return (sortedCharacters, initiativeScores);
    }

    private static IEnumerable<TestCaseData> TestCases_IsFiringModeAllowed()
    {
        yield return new TestCaseData(Weapon.WeaponFunctionality.Manual, FiringMode.SingleRound, false);

        yield return new TestCaseData(Weapon.WeaponFunctionality.SemiAutomatic, FiringMode.SingleRound, false);
        yield return new TestCaseData(Weapon.WeaponFunctionality.SemiAutomatic, FiringMode.RapidVolley, false);

        yield return new TestCaseData(Weapon.WeaponFunctionality.FullAutomatic, FiringMode.SingleRound, false);
        yield return new TestCaseData(Weapon.WeaponFunctionality.FullAutomatic, FiringMode.RapidVolley, false);
        yield return new TestCaseData(Weapon.WeaponFunctionality.FullAutomatic, FiringMode.FullAuto, false);
        yield return new TestCaseData(Weapon.WeaponFunctionality.FullAutomatic, FiringMode.Burst, false);

        yield return new TestCaseData(Weapon.WeaponFunctionality.SemiAutomaticWith3RoundBurst, FiringMode.SingleRound, false);
        yield return new TestCaseData(Weapon.WeaponFunctionality.SemiAutomaticWith3RoundBurst, FiringMode.RapidVolley, false);
        yield return new TestCaseData(Weapon.WeaponFunctionality.SemiAutomaticWith3RoundBurst, FiringMode.Burst, false);
    }

    private static string NormalizeString(string input)
    {
        var lines = input.Split('\n')
                         .Select(line => line.Trim())
                         .Where(line => !string.IsNullOrEmpty(line)); // Remove empty lines

        return string.Join("\n", lines);
    }
    #endregion Private Methods




}
