namespace MutantChroniclesAPI.Model.CharacterModel;

public class Target
{
    private readonly Character character;

    public int MaximumBodyPoints { get; }
    public int TemporaryBodyPoints { get; private set; }

    public Target(Character character)
    {
        this.character = character;
        InitializeBodyParts();
        MaximumBodyPoints = CalculateMaximumBodyPoints();
        TemporaryBodyPoints = MaximumBodyPoints;
    }

    private void InitializeBodyParts()
    {
        Head = new BodyPart(0, 0, new List<ArmorValues>());
        Chest = new BodyPart(0, 0, new List<ArmorValues>());
        Stomach = new BodyPart(0, 0, new List<ArmorValues>());
        RightArm = new BodyPart(0, 0, new List<ArmorValues>());
        LeftArm = new BodyPart(0, 0, new List<ArmorValues>());
        RightLeg = new BodyPart(0, 0, new List<ArmorValues>());
        LeftLeg = new BodyPart(0, 0, new List<ArmorValues>());
    }

    private int CalculateMaximumBodyPoints()
    {

        int combinedstats = character.Physique + character.MentalStrength;

        switch (combinedstats)
        {
            case int n when (n >= 2 && n <= 10):
                Head.MaximumBodyPoints = 2;
                Chest.MaximumBodyPoints = 5;
                Stomach.MaximumBodyPoints = 4;
                RightArm.MaximumBodyPoints = 4;
                LeftArm.MaximumBodyPoints = 4;
                RightLeg.MaximumBodyPoints = 5;
                LeftLeg.MaximumBodyPoints = 5;
                break;

            case int n when (n >= 11 && n <= 20):
                Head.MaximumBodyPoints = 3;
                Chest.MaximumBodyPoints = 6;
                Stomach.MaximumBodyPoints = 5;
                RightArm.MaximumBodyPoints = 5;
                LeftArm.MaximumBodyPoints = 5;
                RightLeg.MaximumBodyPoints = 6;
                LeftLeg.MaximumBodyPoints = 6;
                break;

            case int n when (n >= 21 && n <= 30):
                Head.MaximumBodyPoints = 3;
                Chest.MaximumBodyPoints = 7;
                Stomach.MaximumBodyPoints = 6;
                RightArm.MaximumBodyPoints = 6;
                LeftArm.MaximumBodyPoints = 6;
                RightLeg.MaximumBodyPoints = 7;
                LeftLeg.MaximumBodyPoints = 7;
                break;

            case int n when (n >= 31 && n <= 40):
                Head.MaximumBodyPoints = 4;
                Chest.MaximumBodyPoints = 8;
                Stomach.MaximumBodyPoints = 7;
                RightArm.MaximumBodyPoints = 7;
                LeftArm.MaximumBodyPoints = 7;
                RightLeg.MaximumBodyPoints = 8;
                LeftLeg.MaximumBodyPoints = 8;
                break;

            case int n when (n >= 41 && n <= 50):
                Head.MaximumBodyPoints = 4;
                Chest.MaximumBodyPoints = 9;
                Stomach.MaximumBodyPoints = 8;
                RightArm.MaximumBodyPoints = 8;
                LeftArm.MaximumBodyPoints = 8;
                RightLeg.MaximumBodyPoints = 9;
                LeftLeg.MaximumBodyPoints = 9;
                break;

            case int n when (n >= 51 && n <= 60):
                Head.MaximumBodyPoints = 5;
                Chest.MaximumBodyPoints = 10;
                Stomach.MaximumBodyPoints = 9;
                RightArm.MaximumBodyPoints = 9;
                LeftArm.MaximumBodyPoints = 9;
                RightLeg.MaximumBodyPoints = 10;
                LeftLeg.MaximumBodyPoints = 10;
                break;

            default:
                int additionalPoints = (combinedstats - 60) / 10;
                Head.MaximumBodyPoints = 5 + (int)(additionalPoints * 0.5);
                Chest.MaximumBodyPoints = 10 + additionalPoints;
                Stomach.MaximumBodyPoints = 9 + additionalPoints;
                RightArm.MaximumBodyPoints = 9 + additionalPoints;
                LeftArm.MaximumBodyPoints = 9 + additionalPoints;
                RightLeg.MaximumBodyPoints = 10 + additionalPoints;
                LeftLeg.MaximumBodyPoints = 10 + additionalPoints;
                break;
        }

        return combinedstats;
    }


    //----------------------------  Body Parts  ----------------------------\\

    public class BodyPart
    {
        public int TemporaryBodyPoints { get; set; }
        public int MaximumBodyPoints { get; set; }

        public List<ArmorValues> ArmorValues { get; set; } //Inherits Absorbation from armor and applies relevant blockChance

        public BodyPart(int temporaryBodyPoints, int maximumBodyPoints, List<ArmorValues> armorValues)
        {
            TemporaryBodyPoints = temporaryBodyPoints;
            MaximumBodyPoints = maximumBodyPoints;
            ArmorValues = armorValues;
        }

        #region old solution with tuples
        //[JsonConverter(typeof(TupleToJsonArrayConverter))]
        //public List<(int Absorb, double BlockChance)> ArmorValues { get; set; }

        //public BodyPart(int temporaryBodyPoints, int maximumBodyPoints, List<(int Absorb, double BlockChance)> armorValues)
        //{
        //    TemporaryBodyPoints = temporaryBodyPoints;
        //    MaximumBodyPoints = maximumBodyPoints;
        //    ArmorValues = armorValues;
        //}
        #endregion

    }

    public BodyPart Head { get; private set; }
    public BodyPart Chest { get; private set; }
    public BodyPart Stomach { get; private set; }
    public BodyPart RightArm { get; private set; }
    public BodyPart LeftArm { get; private set; }
    public BodyPart RightLeg { get; private set; }
    public BodyPart LeftLeg { get; private set; }


}
