using MutantChroniclesAPI.Interface;
using MutantChroniclesAPI.Model.EnviromentModel;

namespace MutantChroniclesAPI.Services;

public class EnviromentService : IEnviromentService
{

    public static Enviroment.Light Light { get; set; }
    public static Enviroment.Weather Weather { get; set; }
    public Enviroment.Weather GetWeatherModifiers()
    {
        return Weather;
    }
    public Enviroment.Light GetLightModifiers()
    {
        return Light;
    }
    public Enviroment.Light SetLight(Enviroment.Light light)
    {
        Light = light;
        return light;
    }
    public Enviroment.Weather SetWeather(Enviroment.Weather weather)
    {
        Weather = weather;
        return weather;
    }

}

