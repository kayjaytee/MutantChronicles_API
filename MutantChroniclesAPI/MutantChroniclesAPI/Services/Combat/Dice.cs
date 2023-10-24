namespace MutantChroniclesAPI.Services.Combat;

public class Dice
{
    private static readonly Random random = new Random();

    public static int Roll1D4()
    {
        return random.Next(1, 5);
    }

    public static int Roll1D6()
    {
        return random.Next(1, 7);
    }

    public static int Roll1D10()
    {
        return random.Next(1, 11);
    }

    public static int Roll1D20()
    {
        return random.Next(1, 21);
    }

    public static int RollDamage(int minDamage, int maxDamage, int additionalDamage)
    {
        return random.Next(minDamage, maxDamage + 1) + additionalDamage;
    }

    public static int RollTargetAreas(int maxAreas)
    {
        if (maxAreas <= 0)
        {
            return 1;
        }
        return random.Next(1, maxAreas + 1);
    }
}
