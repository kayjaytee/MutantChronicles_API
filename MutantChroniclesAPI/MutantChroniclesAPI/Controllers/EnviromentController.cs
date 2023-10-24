using Microsoft.AspNetCore.Mvc;
using MutantChroniclesAPI.Model;
using System.ComponentModel.DataAnnotations;
using static MutantChroniclesAPI.Model.Enviroment;

namespace MutantChroniclesAPI.Controllers;


[Route("api/[controller]")]
[ApiController]
public class EnviromentController : ControllerBase
{
    public static Enviroment.Light enviromentLight = Enviroment.Light.None;
    public static Enviroment.Weather enviromentWeather = Enviroment.Weather.None;

    [HttpPatch("light")]
    public async Task<IActionResult> SetLight([FromBody] Enviroment.Light light)
    {
        enviromentLight = light;
        return Ok(light.ToString());
    }

    [HttpPatch("weather")]
    public async Task<IActionResult> SetWeather([FromBody] Enviroment.Weather weather)
    {
        enviromentWeather = weather;
        return Ok(weather.ToString());
    }

}
