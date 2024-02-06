using MutantChroniclesAPI.Controllers;
using MutantChroniclesAPI.Model.EnviromentModel;
using MutantChroniclesAPI.Services;
using NSubstitute;

namespace MutantChroniclesAPI.Tests.Controllers;

[TestFixture]
public class EnviromentControllerTests
{
    private EnviromentController controller;
    private EnviromentService enviromentService;

    [SetUp]
    public void SetUp()
    {
        controller = new EnviromentController(enviromentService);
        enviromentService = Substitute.For<EnviromentService>();
    }

    [Test]
    public void SetLight_WhenCalled_AppliesNewValue()
    {
        // Arrange: SetUp()
        var expectedLightValue = Enviroment.Light.FullMoonOutdoorsOrSinglecandleIndoors;
        // Act
        var result = enviromentService.SetLight(expectedLightValue);

        // Assert
        Assert.AreEqual(expectedLightValue, result);
    }

    [Test]
    public void SetWeather_WhenCalled_AppliesNewValue()
    {
        // Arrange: SetUp()
        var expectedWeatherValue = Enviroment.Weather.WindOrLightRain;

        // Act
        var result = enviromentService.SetWeather(expectedWeatherValue);

        // Assert
        Assert.AreEqual(expectedWeatherValue, result);
    }
}
