using MutantChroniclesAPI.Model.WeaponModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MutantChroniclesAPI.Model.CharacterModel;

public class Character
{
    //----------------------------  Target Class  ----------------------------\\
    //  (Will be displayed at the Json Body's Bottom) 
    [JsonPropertyOrder(20)] public Target Target { get; set; }

    public Character() => Target = new Target(this);

    //----------------------------  Character Attributes (Required parameters)  ----------------------------\\
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public int Strength { get; set; }
    [Required] public int Physique { get; set; }
    [Required] public int Coordination { get; set; }
    [Required] public int Intelligence { get; set; }
    [Required] public int MentalStrength { get; set; }
    [Required] public int Personality { get; set; }

    //----------------------------  Character Bonuses; Scales with Attributes  ----------------------------\\

    public int OffensiveBonus { get; set; }
    public int ActionsPerRound { get; set; }
    public int DefensiveBonus { get; set; }
    public int PerceptionBonus { get; set; }
    public int InitiativeBonus { get; set; }

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

    public void CalculateMovementAllowance()
    {
        int movementAllowance = Coordination + Physique;
        int squares = 0; // 1 square equals 1.5 meters
        int meters = 0;

        switch (movementAllowance)
        {
            case int n when (n >= 2 && n <= 10):
                squares = 2;
                meters = 150;
                break;
            case int n when (n >= 11 && n <= 20):
                squares = 3;
                meters = 175;
                break;
            case int n when (n >= 21 && n <= 30):
                squares = 3;
                meters = 225;
                break;
            case int n when (n >= 31 && n <= 40):
                squares = 4;
                meters = 275;
                break;
            case int n when (n >= 41 && n <= 50):
                squares = 5;
                meters = 325;
                break;
            case int n when (n >= 51 && n <= 60):
                squares = 6;
                meters = 400;
                break;
            case int n when (n >= 61 && n <= 80):
                squares = 7;
                meters = 500;
                break;
            default:
                squares = (movementAllowance - 80) / 20 + 8;
                meters = squares * 150;
                break;
        }

        MovementAllowance = (squares, meters);

    }

    public void CalculateOffensiveBonus()
    {
        int offensiveBonus = Strength + Physique;

        switch (offensiveBonus)
        {
            case int n when n >= 2 && n <= 10:
                OffensiveBonus = -1;
                break;
            case int n when n >= 11 && n <= 20:
                OffensiveBonus = 0;
                break;
            case int n when n >= 21 && n <= 30:
                OffensiveBonus = 1;
                break;
            case int n when n >= 31 && n <= 40:
                OffensiveBonus = 2;
                break;
            case int n when n >= 41 && n <= 50:
                OffensiveBonus = 3;
                break;
            case int n when n >= 51 && n <= 60:
                OffensiveBonus = 4;
                break;
            case int n when n >= 61 && n <= 80:
                OffensiveBonus = 5;
                break;
            default:
                OffensiveBonus = (offensiveBonus - 80) / 20 + 8;
                break;
        }
    }

    public void CalculateActionsPerRound()
    {
        int actionsPerRound = Coordination + MentalStrength;

        int weightPenalty = CalculateWeightPenalty();

        switch (actionsPerRound)
        {
            case int n when n >= 2 && n <= 10:
                ActionsPerRound = 2 - weightPenalty;
                break;
            case int n when n >= 11 && n <= 20:
                ActionsPerRound = 3 - weightPenalty;
                break;
            case int n when n >= 21 && n <= 30:
                ActionsPerRound = 3 - weightPenalty;
                break;
            case int n when n >= 31 && n <= 40:
                ActionsPerRound = 4 - weightPenalty;
                break;
            case int n when n >= 41 && n <= 50:
                ActionsPerRound = 5 - weightPenalty;
                break;
            case int n when n >= 51 && n <= 60:
                ActionsPerRound = 6 - weightPenalty;
                break;
            case int n when n >= 61 && n <= 80:
                ActionsPerRound = 7 - weightPenalty;
                break;
            default:
                ActionsPerRound = (actionsPerRound - 80) / 20 + 8 - weightPenalty;
                break;
        }
    }

    public void CalculateDefensiveBonus()
    {
        int defensiveBonus = Coordination + Intelligence;

        switch (defensiveBonus)
        {
            case int n when n >= 2 && n <= 10:
                DefensiveBonus = 2;
                break;
            case int n when n >= 11 && n <= 20:
                DefensiveBonus = 3;
                break;
            case int n when n >= 21 && n <= 30:
                DefensiveBonus = 4;
                break;
            case int n when n >= 31 && n <= 40:
                DefensiveBonus = 5;
                break;
            case int n when n >= 41 && n <= 50:
                DefensiveBonus = 6;
                break;
            case int n when n >= 51 && n <= 60:
                DefensiveBonus = 7;
                break;
            case int n when n >= 61 && n <= 80:
                DefensiveBonus = 8;
                break;
            default:
                defensiveBonus = (defensiveBonus - 80) / 20 + 8;
                DefensiveBonus = defensiveBonus > 10 ? 10 : defensiveBonus;
                break;
        }
    }

    public void CalculatePerceptionBonus()
    {
        int perceptionBonus = Intelligence + MentalStrength;

        switch (perceptionBonus)
        {
            case int n when n >= 2 && n <= 10:
                PerceptionBonus = 2;
                break;
            case int n when n >= 11 && n <= 20:
                PerceptionBonus = 3;
                break;
            case int n when n >= 21 && n <= 30:
                PerceptionBonus = 4;
                break;
            case int n when n >= 31 && n <= 40:
                PerceptionBonus = 5;
                break;
            case int n when n >= 41 && n <= 50:
                PerceptionBonus = 6;
                break;
            case int n when n >= 51 && n <= 60:
                PerceptionBonus = 7;
                break;
            case int n when n >= 61 && n <= 80:
                PerceptionBonus = 8;
                break;
            default:
                PerceptionBonus = (perceptionBonus - 80) / 20 + 8;
                break;
        }
    }

    public void CalculateInitiativeBonus()
    {
        int initiativeBonus = Coordination + Personality;

        switch (initiativeBonus)
        {
            case int n when n >= 2 && n <= 10:
                InitiativeBonus = 1;
                break;
            case int n when n >= 11 && n <= 20:
                InitiativeBonus = 2;
                break;
            case int n when n >= 21 && n <= 30:
                InitiativeBonus = 3;
                break;
            case int n when n >= 31 && n <= 40:
                InitiativeBonus = 4;
                break;
            case int n when n >= 41 && n <= 50:
                InitiativeBonus = 5;
                break;
            case int n when n >= 51 && n <= 60:
                InitiativeBonus = 6;
                break;
            case int n when n >= 61 && n <= 80:
                InitiativeBonus = 7;
                break;
            default:
                InitiativeBonus = (initiativeBonus - 80) / 20 + 8;
                break;
        }
    }


    //----------------------------  Weapon Class  ----------------------------\\

    public Weapon MainHandEquipment { get; set; }
    public Weapon OffHandEquipment { get; set; }

    public bool DualWield { get; set; } = false;
    //public bool FlyingViperStyle { get; set; } = false; //Special Technique, future implementation (allows to make mainhand and offhand action at the same time)
    public bool TwoHanded { get; set; } = false;
    public void EquipWeaponInMainHand(Weapon weapon) => MainHandEquipment = weapon;
    public void EquipWeaponOffHand(Weapon weapon)
    {
        OffHandEquipment = weapon;
        if (MainHandEquipment is null)
        {
            MainHandEquipment = OffHandEquipment;
            OffHandEquipment = null!;
        }
    }

    public void DualWieldCheck()
    {
        if (MainHandEquipment is not null && OffHandEquipment is not null)
        {
            DualWield = true;
            TwoHanded = false;
        }
        else
        {
            DualWield = false;
        }
    }


    //----------------------------  Armor Class  ----------------------------\\

    public List<Armor> Armor { get; set; } = new List<Armor>();

    public void UpdateArmorValuesForBodyParts()
    {
        foreach (var armor in Armor)
        {
            bool fireProof = false;
            switch (armor.Material)
            {
                case CharacterModel.Armor.ArmorMaterial.BulletProof:
                case CharacterModel.Armor.ArmorMaterial.LightCombat:
                case CharacterModel.Armor.ArmorMaterial.HeavyCombat:
                case CharacterModel.Armor.ArmorMaterial.ExtraHeavyCombat:

                    fireProof = true; break;

                default: break;
            }
            var armor100BlockChance = new ArmorValues(armor.Absorb, 1, true, true, fireProof);
            var armor50BlockChance = new ArmorValues(armor.Absorb, 0.5, true, true, fireProof);
            var armor25BlockChance = new ArmorValues(armor.Absorb, 0.25, true, true, fireProof);
            var shoulderAddsNoHeadProtection = new ArmorValues(armor.Absorb, 1, true, false, fireProof);

            switch (armor.Type)
            {
                case CharacterModel.Armor.ArmorType.Head:
                    Target.Head.ArmorValues.Add(armor100BlockChance);
                    break;

                case CharacterModel.Armor.ArmorType.Harness:
                    Target.Chest.ArmorValues.Add(armor100BlockChance);
                    Target.Stomach.ArmorValues.Add(armor100BlockChance);
                    break;

                case CharacterModel.Armor.ArmorType.Jacket:
                    Target.Chest.ArmorValues.Add(armor100BlockChance);
                    Target.Stomach.ArmorValues.Add(armor100BlockChance);
                    Target.RightArm.ArmorValues.Add(armor100BlockChance);
                    Target.LeftArm.ArmorValues.Add(armor100BlockChance);
                    break;

                case CharacterModel.Armor.ArmorType.Trenchcoat:
                    Target.Chest.ArmorValues.Add(armor100BlockChance);
                    Target.Stomach.ArmorValues.Add(armor100BlockChance);
                    Target.RightArm.ArmorValues.Add(armor100BlockChance);
                    Target.LeftArm.ArmorValues.Add(armor100BlockChance);
                    Target.RightLeg.ArmorValues.Add(armor50BlockChance);
                    Target.LeftLeg.ArmorValues.Add(armor50BlockChance);
                    break;

                case CharacterModel.Armor.ArmorType.Bodysuit:
                    Target.Chest.ArmorValues.Add(armor100BlockChance);
                    Target.Stomach.ArmorValues.Add(armor100BlockChance);
                    Target.RightArm.ArmorValues.Add(armor100BlockChance);
                    Target.LeftArm.ArmorValues.Add(armor100BlockChance);
                    Target.RightLeg.ArmorValues.Add(armor100BlockChance);
                    Target.LeftLeg.ArmorValues.Add(armor100BlockChance);
                    break;

                case CharacterModel.Armor.ArmorType.Arms:
                    Target.RightArm.ArmorValues.Add(armor100BlockChance);
                    Target.LeftArm.ArmorValues.Add(armor100BlockChance);
                    break;

                case CharacterModel.Armor.ArmorType.Gloves:
                    Target.RightArm.ArmorValues.Add(armor25BlockChance);
                    Target.LeftArm.ArmorValues.Add(armor25BlockChance);
                    break;

                case CharacterModel.Armor.ArmorType.Legs:
                    Target.RightLeg.ArmorValues.Add(armor100BlockChance);
                    Target.LeftLeg.ArmorValues.Add(armor100BlockChance);
                    break;

                case CharacterModel.Armor.ArmorType.Knee:
                    Target.RightLeg.ArmorValues.Add(armor25BlockChance);
                    Target.LeftLeg.ArmorValues.Add(armor25BlockChance);
                    break;

                case CharacterModel.Armor.ArmorType.Shoulders:
                    Target.Head.ArmorValues.Add(shoulderAddsNoHeadProtection);
                    Target.Chest.ArmorValues.Add(armor100BlockChance);
                    break;
            }
        }
    }

    //----------------------------  Carrying Weight  ----------------------------\\
    public decimal WeightCarried { get; set; }

    public void CalculateWeight()
    {
        decimal weaponWeight = MainHandEquipment.Weight;
        WeightCarried = weaponWeight;
    }

    public int CalculateWeightPenalty()
    {
        int penalty = 0;
        int overburdenThreshold = Strength * 2;

        if (WeightCarried > overburdenThreshold)
        {

            if (WeightCarried <= Strength * 4)
            {
                penalty = 1;
            }
            else if (WeightCarried <= Strength * 6)
            {
                penalty = 2;
            }
            else if (WeightCarried <= Strength * 10)
            {
                penalty = 3;
            }
            else
            {
                penalty = 4;
            }

        }

        return penalty;
    }


    //----------------------------  Temporary Effects  ----------------------------\\

    public int ActionsRemaining { get; set; }

    public List<Character> CurrentlyUnderFire { get; set; } = new List<Character>();

    public bool IsAvoiding { get; set; }
    public bool TakingCover { get; set; }

    public short BurningCondition { get; set; } = 0;

    public EnviromentWounds Wounds { get; set; }
    public EnviromentStress Stress { get; set; }

    public HeadOneBodyPointRemaining HeadHasOneBodyPointRemaining { get; private set; }
    public ChestOneBodyPointRemaining ChestHasOneBodyPointRemaining { get; private set; }
    public StomachOneBodyPointRemaining StomachHasOneBodyPointRemaining { get; private set; }
    public RightArmOneBodyPointRemaining RightArmHasOneBodyPointRemaining { get; private set; }
    public LeftArmOneBodyPointRemaining LeftArmHasOneBodyPointRemaining { get; private set; }
    public RightLegOneBodyPointRemaining RightLegHasOneBodyPointRemaining { get; private set; }
    public LeftLegOneBodyPointRemaining LeftLegHasOneBodyPointRemaining { get; private set; }

    public enum EnviromentWounds
    {
        None = 0,
        OneOrTwoHitsInOneBodyPart = 1,
        ThreeOrFourHitsInOneBodyPart = 2,
        YouAreWoundedInMoreThanOneBodyPart = 3,
        OneBodyPartHasZeroBodyPointsLeft = 4,
        TwoOrMoreBodyPartsHaveNoBodyPointsLeft = 5
    }
    public enum EnviromentStress
    {
        None = 0,
        SomeoneFiresAtYou = 1,
        PeopleFireAtYouFromSeveralDirections = 2,
        WARNINGThreeSecondsToAutoDestruct = 3,
        YourClothesAreOnFire = 4,
        YouAreMidairFallingTowardCertainDeath = 5
    }

    #region Effects of Damage - Actions
    public enum HeadOneBodyPointRemaining
    {
        False = 0,
        True = -1
    }
    public enum ChestOneBodyPointRemaining
    {
        False = 0,
        True = -1
    }
    public enum StomachOneBodyPointRemaining
    {
        False = 0,
        True = -1
    }
    #endregion
    #region Effects of Damage - Arms
    public enum RightArmOneBodyPointRemaining
    {
        False = 0,
        True = -5
    }
    public enum LeftArmOneBodyPointRemaining
    {
        False = 0,
        True = -5
    }
    #endregion
    #region Effects of Damage - Movement
    public enum RightLegOneBodyPointRemaining
    {
        False = 0,
        True = -1
    }
    public enum LeftLegOneBodyPointRemaining
    {
        False = 0,
        True = -1
    }

    #endregion



}