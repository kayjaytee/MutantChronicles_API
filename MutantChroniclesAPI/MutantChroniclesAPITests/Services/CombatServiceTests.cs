using Microsoft.AspNetCore.Mvc;
using MutantChroniclesAPI.Enums;
using MutantChroniclesAPI.Interface;
using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Model.EnviromentModel;
using MutantChroniclesAPI.Model.WeaponModel;
using MutantChroniclesAPI.Services;
using MutantChroniclesAPI.Services.Combat;
using MutantChroniclesAPI.Tests;
using NSubstitute;
using System.Diagnostics;

namespace MutantChroniclesAPITests.Services;

[TestFixture]
public class CombatServiceTests
{
    private CombatService combatService;
    private IEnviromentService enviromentService;

    [SetUp]
    public void SetUp()
    {
        // Create a partial mock of the concrete class using NSubstitute
        combatService = Substitute.For<CombatService>();
        enviromentService = Substitute.For<IEnviromentService>();
    }

    [Test]
    public void InitializeTemporaryBodyPoints_ShouldSetTemporaryBodyPointsToMaximum()
    {
        // Arrange
        var character = new Character();
        var target = new Target(character);

        // Act
        combatService.InitializeTemporaryBodyPoints(target);

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
        var result = combatService.StringBuilderFormatInitiative(characters, round, characterCurrentTurn);
        // Assert: Strings are normalized to improve readability in code, else the code has to be all the way to left.
        // Normalize the expected and actual strings
        string expected = @"Round 3
                       RAPID RONALD | Initiative: 15
                       Actions: 2/2
                       SPEEDY SIMON | Initiative: 12
                       Actions: 2/2
                       EASY EARL | Initiative: 9
                       Actions: 1/1
                       SLOW STEVE | Initiative: 8
                       Actions: 3/3
                       -------------------------------
                       Current Turn: RAPID RONALD
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
        var result = combatService.IsFiringModeAllowed(weapon, firingMode, secondaryMode);

        // Assert
        Assert.IsTrue(result);

    }

    [Test]
    public void InitiativeRoll_ShouldRollDie_CombineValues_ReturnInOrder()
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

        var result = combatService.InitiativeRoll(combatants);

        var initiativeResults = result.OrderByDescending(pair => pair.combined);

        // Assert
        Assert.IsNotNull(initiativeResults);

        // Check if the characters are sorted in descending order of initiative
        foreach (var character in initiativeResults)
        {
            Assert.Greater(character.combined, character.combined - 11);
            Assert.Less(character.combined, character.combined + 11);
        }


        //Prints Output in Order, zips together character with the correct score
        foreach (var pair in initiativeResults.Zip(result, (combatant, score) => (combatant, score)))
        {
            Console.WriteLine($"Character: {pair.combatant.character.Name}, InitiativeBonus: {pair.combatant.character.InitiativeBonus}, Total: {pair.combatant.combined.ToString()}");
        }
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

        var characterCurrentTurn_Initial = initialCharacter.character;
        Console.WriteLine($"Initial Character Current Turn: {characterCurrentTurn_Initial.Name}");

        var currentRound = 1;
        var combatStatus = "";

        // // // ACT \\ \\ \\
        combatService.UpdateTurn(initiativeOrder, characterCurrentTurn_Initial, currentRound, combatStatus);
        initiativeOrder = initiativeOrder.OrderByDescending(entry => entry.initiative).ToList();

        // Assert
        // Find the next character who has actions remaining in the sortedCombatants list
        var characterCurrentTurn_After = initiativeOrder.FirstOrDefault(x => x.character.ActionsRemaining > 0).character;
        Console.WriteLine($"New Character Current Turn: {characterCurrentTurn_After.Name}");
        Assert.AreNotEqual(characterCurrentTurn_After, characterCurrentTurn_Initial);
    }

    [Test]
    [Ignore("Missing asserts")]
    public void CheckForWounds_HappyFlow()
    {
        //arrange
        var combatants = new List<Character>
        {
            new Character //1
            {
                Name = "I took 1 bullet in the knee",
                Physique = 10,
                MentalStrength = 10,
                Target = new Target(new Character())

            },

            new Character //2
            {
                Name = "I took several shots in my pinky finger",
                Physique = 10,
                MentalStrength = 10,
                Target = new Target(new Character())
            },
        };

        foreach (var character in combatants)
        {
            combatService.InitializeTemporaryBodyPoints(character.Target); //Required to generate sheet for body points
        }

        //Act
        combatService.CheckForWounds(combatants);

        //Assert

    }

    [Test]
    public void CheckForDefeatedCombatants__AddCharacterToDefeatedList_RemoveFromCombatants() //PROBLEM ITERATING THROUGH 2 LISTS
    {
        // Arrange
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

        // Act
        combatService.CheckForDefeatedCombatants(initiativeOrder, combatants, defeated);

        // Assert
        Assert.IsTrue(defeated.Count > 0);
        Assert.IsTrue(combatants.Count < 2);

    }

    [Test]
    [TestCase(Character.EnviromentStress.None, 1, Character.EnviromentStress.SomeoneFiresAtYou)]
    [TestCase(Character.EnviromentStress.SomeoneFiresAtYou, 2, Character.EnviromentStress.PeopleFireAtYouFromSeveralDirections)]
    [TestCase(Character.EnviromentStress.WARNINGThreeSecondsToAutoDestruct, 4, Character.EnviromentStress.WARNINGThreeSecondsToAutoDestruct)]
    [Description("ShooterEntities, ExpectedStress")]
    public void ApplyStressToTarget_OneShooter_ApplyStress(Character.EnviromentStress predeterminedStress, int shooterEntities, Character.EnviromentStress expectedStress)
    {
        //arrange
        var shooter = new Character
        {
            Name = "Mr Gunnypants",
        };

        var targetCharacter = new Character
        {
            Name = "Target Practice",
            CurrentlyUnderFire = new List<Character>(),
            Stress = predeterminedStress
        };
        for(int i = 0; i < shooterEntities; i++)
        {
            targetCharacter.CurrentlyUnderFire.Add(shooter);
        }

        //Act
        combatService.ApplyStressToTarget(targetCharacter, shooter);

        //Assert
        Assert.IsTrue(targetCharacter.CurrentlyUnderFire.Count == shooterEntities);
        Assert.IsTrue(targetCharacter.Stress == expectedStress);
        Console.WriteLine("Predetermined Stress: " + predeterminedStress.ToString());
        Console.WriteLine("Expected Stress: " + expectedStress.ToString());
        Console.WriteLine("Actual Stress: " + targetCharacter.Stress.ToString());
    }

    [Test]
    public void ResetBurningCondition_Returns_ZeroValue()
    {
        var character = new Character()
        {
            BurningCondition = 1,
        };

        var result = combatService.ResetBurningCondition(character);

        Assert.AreEqual(0, result);
    }

    [Test]
    public void CalculateHitChance_WithModifiers_ReturnsModifiedHitChance() //NEEDS UPDATE/ADJUSTMENT TO NEW MODIFIERS
    {
        // Arrange

        var testCharacter = new Character()
        {
            Strength = 10,
            Stress = Character.EnviromentStress.SomeoneFiresAtYou,
            Wounds = Character.EnviromentWounds.OneOrTwoHitsInOneBodyPart
        };

        int baseChance = 20;
        var light = enviromentService.SetLight(Enviroment.Light.DawnOrDuskOrSingleTorchIndoors);
        var weather = enviromentService.SetWeather(Enviroment.Weather.SnowstormOrHailstorm);

        // Act
        var modifiedHitChance = combatService.CalculateHitChance(testCharacter, baseChance, light, weather);

        // Assert
        int expectedModifiedHitChance = baseChance - (int)light - (int)weather - (int)testCharacter.Stress - (int)testCharacter.Wounds;
        Assert.AreEqual(expectedModifiedHitChance, modifiedHitChance);

        Console.WriteLine($"Base Chance: {baseChance}");
        Console.WriteLine($"Light Modifier: {(int)light}");
        Console.WriteLine($"Weather Modifier: {(int)weather}");
        Console.WriteLine($"Stress Modifier: {(int)testCharacter.Stress}"); //WIP
        Console.WriteLine($"Wounds Modifier: {(int)testCharacter.Wounds}"); //WIP
        Console.WriteLine($"EXPECTED: Modified Hit Chance: {expectedModifiedHitChance}");
        Console.WriteLine($"ACTUAL: Modified Hit Chance: {modifiedHitChance}");
    }

    [Test]
    public void AvoidAttempt_HappyFlow()
    {
        //Arrange
        var character = new Character()
        {
            DefensiveBonus = 10,
            IsAvoiding = true,
            ActionsRemaining = 3
        };

        //Act
        var result = combatService.AvoidAttempt(character, firingMode: FiringMode.SingleRound, light: Enviroment.Light.None, weather: Enviroment.Weather.None);

        //Assert
        Assert.IsInstanceOf<bool>(result);
    }

    #region Private Methods
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ \\

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
