using MutantChroniclesAPI.Converter;
using MutantChroniclesAPI.Enums;
using NUnit.Framework;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static MutantChroniclesAPI.Converter.StringEnumConverterWithDescription;

namespace MutantChroniclesAPI.Tests.Converter;

[TestFixture]
public class StringEnumConverterWithDescriptionTests
{
    private StringEnumConverterWithDescription converter;

    [SetUp]
    public void SetUp()
    {
        converter = new StringEnumConverterWithDescription();
    }

    [Test]
    public void CanConvert_EnumType_True()
    {
        // Arrange
        var enumType = typeof(ArmorType);

        // Act
        var result = converter.CanConvert(enumType);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void CannotConvert_EnumType_ReturnFalse()
    {
        // Arrange
        var notEnumType = typeof(int);

        // Act
        var result = converter.CanConvert(notEnumType);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    [TestCase("\"Harness\"", ArmorType.Harness)] // JSON representation of the enum value
    [TestCase("\"Head\"", ArmorType.Head)] // JSON representation of the enum value
    public void Read_WhenValidJsonStringProvided_ShouldReturnCorrectEnumValue(string json, ArmorType expected)
    {
        // Arrange: Setup/Testcase/Parameters

        // Act
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read(); // Move to the value token
        var result = converter.Read(ref reader, typeof(ArmorType), null);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    [TestCase("\"Harness\"", ArmorType.Harness)] // The enum value to write as JSON
    [TestCase("\"Head\"", ArmorType.Head)] // The enum value to write as JSON
    public void Write_ConvertsEnumToString(string expected, ArmorType value)
    {
        // Arrange: Setup/Testcase/Parameters
        var stream = new MemoryStream();
        var writer = new Utf8JsonWriter(stream);

        // The enum value to write as JSON

        // Act
        converter.Write(writer, value, null);
        writer.Flush();
        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.AreEqual(expected, json);
    }

    [Test]
    public void GetEnumDescription_WhenEnumValueHasDescription_ShouldReturnDescription()
    {
        // Arrange
        var converter = new StringEnumConverterWithDescription();
        var value = ArmorType.Harness; // An enum value with a description attribute

        // Act
        var result = TestUtility.InvokePrivateMethod<string>(converter, "GetEnumDescription", value);

        // Assert
        Assert.AreEqual("Harness", result);
    }

    [Test]
    public void GetEnumDescription_WhenEnumValueHasNoDescription_ShouldReturnEnumValueToString()
    {
        // Arrange
        var converter = new StringEnumConverterWithDescription();
        var value = ArmorType.Arms; // An enum value without a description attribute

        // Act
        var result = TestUtility.InvokePrivateMethod<string>(converter, "GetEnumDescription", value);

        // Assert
        Assert.AreEqual("Arms", result);
    }


}
