using MutantChroniclesAPI.Interface;
using MutantChroniclesAPI.Services;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using MutantChroniclesAPI.Model.EnviromentModel;

namespace MutantChroniclesAPI.Tests.Interface;

[TestFixture]
public class IEnviromentServiceTests
{
    private IEnviromentService enviromentService;

    [SetUp]
    public void SetUp()
    {
        enviromentService = Substitute.For<IEnviromentService>();
    }

    [Test]
    public void GetLightModifiers_ReturnsLight()
    {
        //Arrange
        var expectedLight = Enviroment.Light.ASingleCandleInAGym;
        var mock = enviromentService.GetLightModifiers().Returns(expectedLight);


        //act
        var actualLight = enviromentService.GetLightModifiers();
        
        //assert
        Assert.AreEqual(expectedLight, actualLight);
    }

    [Test]
    public void GetWeatherModifiers_ReturnsWeather()
    {
        //Arrange
        var expectedWeather = Enviroment.Weather.SnowstormOrHailstorm;
        var mock = enviromentService.GetWeatherModifiers().Returns(expectedWeather);

        //act
        var actualWeather = enviromentService.GetWeatherModifiers();

        //assert
        Assert.AreEqual(expectedWeather, actualWeather);
    }

    [Test]
    public void SetLight_ChangesLightModifiers()
    {
        //Arrange
        var newLight = Enviroment.Light.DawnOrDuskOrSingleTorchIndoors;

        //Act
        var expectedLight = enviromentService.SetLight(newLight);
        var actualLight = enviromentService.GetLightModifiers();

        //Assert
        Assert.AreEqual(expectedLight, actualLight);
    }
}
