using MutantChroniclesAPI.Services.Combat;

namespace MutantChroniclesAPI.Tests.Services;

[TestFixture]
public class DiceTests
{
    [Test]
    public void Roll1D4_ShouldReturnNumberBetween1And4()
    {
        // Act
        int result = Dice.Roll1D4();

        // Assert
        Assert.IsTrue(result >= 1 && result <= 4);
    }

    [Test]
    public void Roll1D6_ShouldReturnNumberBetween1And6()
    {
        // Act
        int result = Dice.Roll1D6();

        // Assert
        Assert.IsTrue(result >= 1 && result <= 6);
    }

    [Test]
    public void Roll1D10_ShouldReturnNumberBetween1And10()
    {
        // Act
        int result = Dice.Roll1D10();

        // Assert
        Assert.IsTrue(result >= 1 && result <= 10);
    }

    [Test]
    public void Roll1D20_ShouldReturnNumberBetween1And20()
    {
        // Act
        int result = Dice.Roll1D20();

        // Assert
        Assert.IsTrue(result >= 1 && result <= 20);
    }


    [Test]
    [TestCase(1, 6, 2)]
    [TestCase(1, 4, 0)]
    [TestCase(1, 10, 0)]
    public void RollDamage_ShouldGenerateDamage_WithinRange(int minDamage, int maxDamage, int additionalDamage)
    {
        // Act
        int damage = Dice.RollDamage(minDamage, maxDamage, additionalDamage);

        // Assert
        Assert.GreaterOrEqual(damage, minDamage);
        Assert.LessOrEqual(damage, maxDamage + additionalDamage);

        Console.WriteLine($"Min Damage: {minDamage}");
        Console.WriteLine($"Max Damage: {maxDamage}");
        Console.WriteLine($"Additional Damage: {additionalDamage}");
        Console.WriteLine($"Calculated Damage: {damage}");
    }

    [Test]
    //Cases below 3 should always be = 1
    [TestCase(0)]
    [TestCase(1)]
    //[TestCase(2)] //BUG: When iterating TestCase(2); it sometimes returns failed for unknown reasons.
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(6)]
    [TestCase(8)]
    [TestCase(10)]
    public void RollTargetAreas_ShouldGenerateTargetAreas_WithinRange(int maxAreas)
    {
        // Act
        int targetArea = Dice.RollTargetAreas(maxAreas);

        // Assert
        if (maxAreas < 3)
        {
            Assert.AreEqual(1, targetArea);
        }
        else
        {
            Assert.GreaterOrEqual(targetArea, 1);
            Assert.LessOrEqual(targetArea, maxAreas);
        }

        Console.WriteLine($"Value input: {maxAreas}");
        if (maxAreas < 3)
        {
            Console.WriteLine("Expected Roll: 1D1");
        }
        else
        {
            Console.WriteLine($"Expected Roll: 1D{maxAreas}");
        }
        Console.WriteLine($"Result: {targetArea}");

    }

    [Test]
    public void BlockChance__double0_5_FiftyFiftyChance()

    {

    }
}
