using MutantChroniclesAPI.Model.EnviromentModel;
using MutantChroniclesAPI.Services;
using NSubstitute;

namespace MutantChroniclesAPITests.Services;

[TestFixture]
public class EnviromentServiceTests
{
    private EnviromentService enviromentService;

    [SetUp]
    public void SetUp()
    {
        // Create a partial mock of the concrete class using NSubstitute
        enviromentService = Substitute.For<EnviromentService>();
    }

    [Test]
    public void GetLightModifiers_Returns_Light()
    {
        //Arrange
        var expectedLight = Enviroment.Light.ASingleCandleInAGym;
        var mock = enviromentService.SetLight(expectedLight);

        //act
        var actualLight = enviromentService.GetLightModifiers();
        
        //assert
        Assert.AreEqual(expectedLight, actualLight);
    }

    [Test]
    public void GetWeatherModifiers_Returns_Weather()
    {
        // Arrange
        var expectedWeather = Enviroment.Weather.SnowstormOrHailstorm;
        var mock = enviromentService.SetWeather(expectedWeather);

        // Act
        var actualWeather = enviromentService.GetWeatherModifiers();

        // Assert
        Assert.AreEqual(expectedWeather, actualWeather);
    }
}


