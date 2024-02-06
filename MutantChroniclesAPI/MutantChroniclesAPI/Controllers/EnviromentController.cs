using Microsoft.AspNetCore.Mvc;
using MutantChroniclesAPI.Model.EnviromentModel;
using MutantChroniclesAPI.Services;

namespace MutantChroniclesAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EnviromentController : ControllerBase
{
    private readonly EnviromentService _enviromentService;

    //public static Enviroment.Light enviromentLight = Enviroment.Light.None;
    //public static Enviroment.Weather enviromentWeather = Enviroment.Weather.None;

    public EnviromentController(EnviromentService enviromentService)
    {
        _enviromentService = enviromentService;
    }

    [HttpPatch("light/set")]
    public async Task<IActionResult> SetLight([FromBody] Enviroment.Light light)
    {
        _enviromentService.SetLight(light);
        return Ok(light.ToString() + " " + (int)light);
    }

    [HttpPatch("weather/set")]
    public async Task<IActionResult> SetWeather([FromBody] Enviroment.Weather weather)
    {
        _enviromentService.SetWeather(weather);
        return Ok(weather.ToString() + " " + (int)weather);
    }

    [HttpGet("light/get")]
    public async Task<IActionResult> GetLight()
    {
        var light = _enviromentService.GetLightModifiers();
        return Ok("Current Light: " + light.ToString() + " " + (int)light);
    }

}
