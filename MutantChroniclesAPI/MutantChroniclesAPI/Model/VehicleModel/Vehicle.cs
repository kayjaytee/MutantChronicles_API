using MutantChroniclesAPI.Model.CharacterModel;
using System.Text.Json.Serialization;

namespace MutantChroniclesAPI.Model.VehicleModel;

public class Vehicle
{
    public string Name { get; set; }

    public class Dimensions
    {
        public decimal Length { get; set; }
        public decimal Height { get; set; }
        public decimal Width { get; set; }
    }

    public int MaxSpeed { get; set; } //CALCULATION IN THE SAME WAY AS MOVEMENT SPEED BUT IN M/HR == SQUARES

    /// <summary>
    /// The JSON body needs to return Squares and Meters in the MovementAllowance:
    /// It ignores the movement allowance, serializes it's content with a Dictionary and returns the squares and meters.
    /// </summary>
    [JsonIgnore] public (int Squares, int Meters) MovementAllowance { get; set; }

    [JsonPropertyName("MovementAllowance")]
    public Dictionary<string, int> SerializedMovementAllowance =>
        new Dictionary<string, int>
        {
            { "squares", MovementAllowance.Squares },
            { "meters", MovementAllowance.Meters }
        };
    public void CalculateVehicleMovementAllowance()
    {
        int vehicleSpeed = (int)(MaxSpeed * 16.6666667); // convert km/h to m/min
        int squares = 0; // 1 square equals 1.5 meters
        int meters = 0;

        switch (vehicleSpeed)
        {
            case int n when (n >= 2 && n <= 10):
                squares = 1;
                meters = 50;
                break;
            case int n when (n >= 11 && n <= 20):
                squares = 2;
                meters = 100;
                break;
            case int n when (n >= 21 && n <= 30):
                squares = 3;
                meters = 150;
                break;
            case int n when (n >= 31 && n <= 40):
                squares = 4;
                meters = 200;
                break;
            case int n when (n >= 41 && n <= 50):
                squares = 5;
                meters = 250;
                break;
            case int n when (n >= 51 && n <= 60):
                squares = 6;
                meters = 300;
                break;
            case int n when (n >= 61 && n <= 80):
                squares = 7;
                meters = 350;
                break;
            default:
                squares = (vehicleSpeed - 80) / 10 + 8;
                meters = squares * 50;
                break;
        }

        MovementAllowance = (squares, meters);

    }
    public double MaxFuel { get; set; }

    public class ArmourVehicle
    {
        public int All { get; set; }

        public int Front { get; set; }

        public int Rear { get; set; }

        public int Top { get; set; }
        public int Sides { get; set; }

    }

    public List<Armament>? Armaments { get; set; }

    public List<Character>? Crew { get; set; }


    //----------------------------  Temporary Effects  ----------------------------\\

    public bool BrokenCommunicationGear { get; set; } = false;

    public bool BrokenFuelHose { get; set; } = false;

    public bool BrokenBrakes { get; set; } = false;

    public short ExhaustPipe { get; set; } = 0;

    public bool EngineOffline { get; set; } = false;

    public bool BrokenGasRegulator { get; set; } = false;


}

public class Car : Vehicle
{
    public List<Character>? DriverSeat { get; set; }
    public List<Character>? FrontSeat { get; set; }
    public List<Character>? BackSeat { get; set; }


    //----------------------------  Temporary Effects  ----------------------------\\
    public bool BrokenWindShield { get; set; } = false;

    public bool BrokenTire { get; set; } = false;


}
