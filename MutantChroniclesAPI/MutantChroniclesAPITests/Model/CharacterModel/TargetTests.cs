

using MutantChroniclesAPI.Model.CharacterModel;
using System.Reflection;

namespace MutantChroniclesAPI.Tests.Model.CharacterModel;

public class TargetTests
{
    private Character character;
    private Target target;

    [Test]
    public void InitializeBodyParts_AllBodyPartsAreInitialized()
    {
        // Arrange
        var character = new Character();
        var target = new Target(character);

        // Act
        TestUtility.InvokePrivateMethod(target, "InitializeBodyParts");

        // Assert
        CollectionAssert.AllItemsAreNotNull(new List<object>
        {
            target.Head,
            target.Chest,
            target.Stomach,
            target.RightArm,
            target.LeftArm,
            target.RightLeg,
            target.LeftLeg
        });
    }
    [Test]
    public void CalculateMaximumBodyPoints_Character_PHY_and_MST_ReturnsCorrectValues()
    {
        // Arrange
        var character = new Character
        {
            Physique = 11,
            MentalStrength = 11
        };

        // Act
        var target = new Target(character);
        character.Target = target;

        // Assert: BodyPoints (Total)
        Assert.AreEqual(22, character.Target.MaximumBodyPoints);
        Assert.AreEqual(22, character.Target.TemporaryBodyPoints);

        // Assert: BodyPoints (Body Parts)
        var expected = new int[] { 3, 7, 6, 6, 6, 7, 7 };
        var actual = new int[]
        {
            character.Target.Head.MaximumBodyPoints,
            character.Target.Chest.MaximumBodyPoints,
            character.Target.Stomach.MaximumBodyPoints,
            character.Target.RightArm.MaximumBodyPoints,
            character.Target.LeftArm.MaximumBodyPoints,
            character.Target.RightLeg.MaximumBodyPoints,
            character.Target.LeftLeg.MaximumBodyPoints
        };
        Assert.AreEqual(expected, actual);
    }

}
