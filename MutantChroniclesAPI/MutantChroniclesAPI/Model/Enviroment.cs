namespace MutantChroniclesAPI.Model;

public class Enviroment
{
    public enum Light
    {
        None = 0,
        DawnOrDuskOrSingleTorchIndoors = 1,
        FullMoonOutdoorsOrSinglecandleIndoors = 2,
        ASingleCandleInAGym = 3,
        ShroudedMoonLightOutdoors = 4,
        PitchBlackOrBlindfolded = 5
    }

    public enum Weather
    {
        None = 0,
        WindOrLightRain = 1,
        HeavyWindOrHeavyRain = 2,
        GaleSnowfallOrHail = 3,
        SnowstormOrHailstorm = 4,
        HurricaneOrBlizzard = 5
    }
}
