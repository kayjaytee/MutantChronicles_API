using MutantChroniclesAPI.Model.EnviromentModel;
using MutantChroniclesAPI.Services;

namespace MutantChroniclesAPI.Interface;

public interface IEnviromentService
{
    public Enviroment.Light GetLightModifiers();
    public Enviroment.Weather GetWeatherModifiers();
    public Enviroment.Light SetLight(Enviroment.Light light);
    public Enviroment.Weather SetWeather(Enviroment.Weather weather);
}
