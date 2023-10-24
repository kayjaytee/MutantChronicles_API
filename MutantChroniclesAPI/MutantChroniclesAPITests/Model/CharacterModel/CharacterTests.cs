using MutantChroniclesAPI.Model.CharacterModel;
using MutantChroniclesAPI.Model.WeaponModel;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Text.Json;

namespace MutantChroniclesAPI.Tests.Model.CharacterModel;

[TestFixture]
public class CharacterTests
{
    private Character character;

    [SetUp]
    public void Setup()
    {
        character = new Character();
    }

    [Test]
    [TestCase(11,11, 1)]
    [TestCase(22,22, 3)]
    [TestCase(3,6, -1)]
    public void CalculateOffensiveBonus_STR_and_PHY_WithinRange_ReturnsCorrectValue(int STR, int PHY, int expected)
    {
        // Arrange
        character.Strength = STR;
        character.Physique = PHY;

        // Act
        character.CalculateOffensiveBonus();

        // Assert
        Assert.AreEqual(expected, character.OffensiveBonus);
        Console.WriteLine($"STR: {STR}, PHY: {PHY}, Combined Value: {STR+PHY}, Expected: {expected}, Result: {character.OffensiveBonus}");

    }

    [Test]
    [TestCase(10, 15, 3)]
    [TestCase(11, 22, 4)]
    [TestCase(30, 22, 6)]
    public void CalculateActionsPerRound_COR_and_MST_WithinRange_ReturnsCorrectValue(int COR, int MST, int expected)
    {
        // Arrange
        character.Coordination = COR;
        character.MentalStrength = MST;

        // Act
        character.CalculateActionsPerRound();

        // Assert
        Assert.AreEqual(expected, character.ActionsPerRound);
        Console.WriteLine($"COR: {COR}, MST: {MST}, Combined Value: {COR+MST}, Expected: {expected}, Result: {character.ActionsPerRound}");
    }

    [Test]
    [TestCase(9, 11, 3)]
    [TestCase(9, 11, 3)]
    [TestCase(9, 11, 3)]
    public void CalculateDefensiveBonus_COR_and_INT_WithinRange_ReturnsCorrectValue(int COR, int INTEL, int expected)
    {
        // Arrange
        character.Coordination = COR;
        character.Intelligence = INTEL;

        // Act
        character.CalculateDefensiveBonus();

        // Assert
        Assert.AreEqual(expected, character.DefensiveBonus);
        Console.WriteLine($"COR: {COR}, INT: {INTEL}, Combined Value: {COR + INTEL}, Expected: {expected}, Result: {character.DefensiveBonus}");
    }

    [Test]
    [TestCase(11, 12, 4)]
    [TestCase(11, 12, 4)]
    [TestCase(11, 12, 4)]
    public void CalculatePerceptionBonus_INT_and_MST_WithinRange_ReturnsCorrectValue(int INTEL, int MST, int expected)
    {
        // Arrange
        character.Intelligence = INTEL;
        character.MentalStrength = MST;

        // Act
        character.CalculatePerceptionBonus();

        // Assert
        Assert.AreEqual(expected, character.PerceptionBonus);
        Console.WriteLine($"INT: {INTEL}, MST: {MST}, Combined Value: {INTEL + MST}, Expected: {expected}, Result: {character.PerceptionBonus}");
    }

    [Test]
    [TestCase(11,11,3)]
    [TestCase(11,11,3)]
    [TestCase(11,11,3)]
    public void CalculateInitiativeBonus_COR_and_PER_WithinRange_ReturnsCorrectValue(int COR, int PER, int expected)
    {
        // Arrange
        character.Coordination = COR;
        character.Personality = PER;

        // Act
        character.CalculateInitiativeBonus();

        // Assert
        Assert.AreEqual(expected, character.InitiativeBonus);
        Console.WriteLine($"COR: {COR}, PER: {PER}, Combined Value: {COR + PER}, Expected: {expected}, Result: {character.InitiativeBonus}");
    }
 
    [Test]
    [TestCase(11, 11,  3,225)]
    [TestCase(11, 9,  3,175)]
    [TestCase(11, 11,  3,225)]
    public void CalculateMovementAllowance_PHY_and_COR_WithinRange_ReturnsCorrectValueAsTuple(int PHY, int COR, int squares, int mpm)
    {
        // Arrange
        character.Physique = PHY;
        character.Coordination = COR;

        // Act
        character.CalculateMovementAllowance();

        // Assert
        Assert.AreEqual((squares, mpm), character.MovementAllowance);
        Console.WriteLine($"PHY: {PHY}, COR: {COR}, Combined Value: {PHY + COR}, Expected: Squares: {squares}, Meters Per Minute: {mpm}, Result: Squares: {character.MovementAllowance.Squares}, Meters Per Minute: {character.MovementAllowance.Meters}");
    }

    [Test]
    public void MovementAllowance_WhenCalled_ReturnsSquaresAndMetersInJsonBody()
    {
        // Arrange
        var character = new Character
        {
            MovementAllowance = (2, 150) // Example values for squares and meters
        };

        // Act
        var json = JsonSerializer.Serialize(character.SerializedMovementAllowance);

        // Assert
        Assert.AreEqual("" +
            "{\"squares\":2," +
            "\"meters\":150}", json);
    }

    [Test]
    public void CalculateWeight_ReturnWeight()
    {
        // Arrange
        var character = new Character();
        var weapon = new Weapon { Weight = 2.5M };

        // Act
        character.EquipWeapon(weapon);
        character.CalculateWeight();

        // Assert
        Assert.AreEqual(2.5, character.WeightCarried);
    }

    [Test]
    [TestCase(10, 1.0, 0)]
    [TestCase(15, 30.4, 1)]
    [TestCase(4, 21.2, 2)]
    public void CalculateWeight_CalculatePenalty_ReturnPenalty(int STR, decimal weightCarried, int expectedPenalty)
    {
        // Arrange
        var character = new Character { Strength = STR, WeightCarried = weightCarried };

        // Act
        int actualPenalty = character.CalculateWeightPenalty();

        // Assert
        Assert.AreEqual(expectedPenalty, actualPenalty);

        Console.WriteLine($"Strength: {character.Strength}");
        Console.WriteLine($"Total Carried Weight: {character.WeightCarried} kg");
        Console.WriteLine($"Expected Penalty: {expectedPenalty}");
        Console.WriteLine($"Actual Penalty: {actualPenalty}");
    }


    [Test]
    public void UpdateArmorValueForBodyParts_AddsDifferentArmorTypes_UpdatesArmorValueCorrectly_BodyParts()
    {
        // Arrange
        var character = new Character();
   

        //Arrange: Add different armor sets to the character
        character.Armor.Add(new Armor("Cap", 1, Armor.ArmorType.Head));
        character.Armor.Add(new Armor("T-Shirt", 1, Armor.ArmorType.Harness));
        character.Armor.Add(new Armor("Jeans Jacket", 3, Armor.ArmorType.Jacket));
        character.Armor.Add(new Armor("Raincoat", 2, Armor.ArmorType.Trenchcoat));
        character.Armor.Add(new Armor("Gloves", 1, Armor.ArmorType.Gloves));
        character.Armor.Add(new Armor("Jeans", 4, Armor.ArmorType.Legs));

        // Act
        character.UpdateArmorValueForBodyParts();

        // Assert
        // Check if the armor values are updated correctly for each body part
        Assert.AreEqual(1, character.Target.Head.ArmorValue);
        Assert.AreEqual(6, character.Target.Chest.ArmorValue);
        Assert.AreEqual(6, character.Target.Stomach.ArmorValue);
        Assert.AreEqual(6, character.Target.RightArm.ArmorValue);
        Assert.AreEqual(6, character.Target.LeftArm.ArmorValue);
        Assert.AreEqual(6, character.Target.RightLeg.ArmorValue);
        Assert.AreEqual(6, character.Target.LeftLeg.ArmorValue);
    }

    [Test]
    public void EquipWeapon_ShouldSetEquippedWeapon()
    {
        // Arrange
        var character = new Character();
        var weapon = new Weapon { Name = "Test Weapon" };

        // Act
        character.EquipWeapon(weapon);

        // Assert
        Assert.AreEqual(weapon, character.EquippedWeapon);
    }
}
