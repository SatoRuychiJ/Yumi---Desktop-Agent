using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Windows.Threading;
using VPet_Simulator.Core;
using static VPet_Simulator.Core.IGameSave;

namespace VPet_Simulator.Windows.Interface;

/// <summary>
/// Game save, desktop pet customized version
/// </summary>
public class GameSave_VPet : IGameSave
{
    /// <summary>
    /// Pet name
    /// </summary>
    [Line(name: "name")]
    public string Name { get; set; }
    /// <summary>
    /// Owner's name
    /// </summary>
    [Line(name: "hostname")]
    public string HostName { get; set; } = "";
    /// <summary>
    /// Money
    /// </summary>
    [Line(Type = LPSConvert.ConvertType.ToFloat, Name = "money")]
    public double Money { get; set; }


    double exp { get; set; }
    /// <summary>
    /// Level
    /// </summary>
    [Line]
    public int Level { get; set; } = 1;
    /// <summary>
    /// Level cap
    /// </summary>
    [Line]
    public int LevelMax { get; set; } = 0;
    /// <summary>
    /// Experience
    /// </summary>
    [Line(type: LPSConvert.ConvertType.ToFloat, name: "exp")]
    public double Exp
    {
        get => exp;
        set
        {
            int lun = LevelUpNeed();
            bool islevelup = false;
            bool islevelmaxup = false;
            int BeforeLevel = Level;
            int BeforeLevelMax = LevelMax;
            while (value >= lun)
            {
                islevelup = true;
                value -= lun;
                LikabilityMax += 10;
                if (++Level > 1000 + LevelMax * 100)
                {
                    LevelMax++;
                    islevelmaxup = true;
                    Level = 100 * LevelMax;
                }
                lun = LevelUpNeed();
            }
            exp = value;
            if (islevelup)
            {
                Event_LevelUp?.Invoke(new LevelUpEventArgs()
                {
                    IsLevelMaxUp = islevelmaxup,
                    BeforeLevel = BeforeLevel,
                    BeforeLevelMax = BeforeLevelMax
                });
            }
        }
    }
    public class LevelUpEventArgs : EventArgs
    {
        /// <summary>
        /// Whether leveled up
        /// </summary>
        public bool IsLevelUp => true;
        /// <summary>
        /// Whether the level cap increased
        /// </summary>
        public bool IsLevelMaxUp { get; set; }
        /// <summary>
        /// Previous level
        /// </summary>
        public int BeforeLevel { get; set; }
        /// <summary>
        /// Previous level cap
        /// </summary>
        public int BeforeLevelMax { get; set; }
    }
    public event Action<LevelUpEventArgs> Event_LevelUp;
    /// <summary>
    /// Total experience the player has gained
    /// </summary>
    public double TotalExpGained()
    {
        double totalExp = 0;
        // first, add the experience for LevelMax
        for (int i = 1; i <= LevelMax; i++)
        {
            for (int j = 100 * i + 1; j <= 1000 + 100 * i; j++)
                totalExp += 200 * j - 100;
        }
        // then, add the experience for the current level
        totalExp += (Level - 100 * LevelMax) * (200 * (Level - 1) - 100);
        // finally, add the remaining experience
        totalExp += Exp;
        return totalExp;
    }
    /// <summary>
    /// Experience required to level up
    /// </summary>
    public int LevelUpNeed() => 200 * Level - 100;
    /// <summary>
    /// Stamina 0-100
    /// </summary>
    public double Strength { get => strength; set => strength = Math.Min(StrengthMax, Math.Max(0, value)); }

    public double StrengthMax => 100 + (int)(Math.Pow(Level * (1 + LevelMax), 0.75) * 4);

    [Line(Type = LPSConvert.ConvertType.ToFloat, IgnoreCase = true)]
    protected double strength { get; set; }
    /// <summary>
    /// Stamina to be replenished, slowly added to the pet over time
    /// </summary>//makes the game more engaging
    [Line(Type = LPSConvert.ConvertType.ToFloat, IgnoreCase = true)]
    public double StoreStrength { get; set; }
    /// <summary>
    /// Stamina change
    /// </summary>
    public double ChangeStrength { get; set; } = 0;
    public void StrengthChange(double value)
    {
        ChangeStrength += value;
        Strength += value;
    }
    /// <summary>
    /// Fullness
    /// </summary>
    public double StrengthFood
    {
        get => strengthFood; set
        {
            value = Math.Min(StrengthMax, value);
            if (value <= 0)
            {
                Health += value;
                strengthFood = 0;
            }
            else
                strengthFood = value;
        }
    }
    [Line(Type = LPSConvert.ConvertType.ToFloat)]
    protected double strengthFood { get; set; }
    /// <summary>
    /// Fullness to be replenished, slowly added to the pet over time
    /// </summary>//makes the game more engaging
    [Line(Type = LPSConvert.ConvertType.ToFloat)]
    public double StoreStrengthFood { get; set; }
    public void StrengthChangeFood(double value)
    {
        ChangeStrengthFood += value;
        StrengthFood += value;
    }
    /// <summary>
    /// Food change
    /// </summary>
    public double ChangeStrengthFood { get; set; } = 0;
    /// <summary>
    /// Thirst
    /// </summary>
    public double StrengthDrink
    {
        get => strengthDrink; set
        {
            value = Math.Min(StrengthMax, value);
            if (value <= 0)
            {
                Health += value;
                strengthDrink = 0;
            }
            else
                strengthDrink = value;
        }
    }

    [Line(Type = LPSConvert.ConvertType.ToFloat)]
    protected double strengthDrink { get; set; }
    /// <summary>
    /// Thirst to be replenished, slowly added to the pet over time
    /// </summary>//makes the game more engaging
    [Line(Type = LPSConvert.ConvertType.ToFloat)]
    public double StoreStrengthDrink { get; set; }
    /// <summary>
    /// Thirst change
    /// </summary>
    public double ChangeStrengthDrink { get; set; } = 0;
    public void StrengthChangeDrink(double value)
    {
        ChangeStrengthDrink += value;
        StrengthDrink += value;
    }
    /// <summary>
    /// Mood
    /// </summary>
    public double Feeling
    {
        get => feeling; set
        {

            value = Math.Min(FeelingMax, value);
            if (value <= 0)
            {
                Health += value / 2;
                Likability += value / 2;
                feeling = 0;
            }
            else
                feeling = value;
        }
    }

    [Line(Type = LPSConvert.ConvertType.ToFloat)]
    protected double feeling { get; set; }
    /// <summary>
    /// Mood change
    /// </summary>
    public double ChangeFeeling { get; set; } = 0;
    public void FeelingChange(double value)
    {
        ChangeFeeling += value;
        Feeling += value;
    }
    /// <summary>
    /// Health (sickness) (hidden)
    /// </summary>
    public double Health { get => health; set => health = Math.Min(100, Math.Max(0, value)); }

    [Line(Type = LPSConvert.ConvertType.ToFloat)]
    protected double health { get; set; }
    /// <summary>
    /// Likability (hidden) (accumulated value)
    /// </summary>
    public double Likability
    {
        get => likability; set
        {
            var max = LikabilityMax;
            value = Math.Max(0, value);
            if (value > max)
            {
                likability = max;
                Health += value - max;
            }
            else
                likability = value;
        }
    }

    [Line(Type = LPSConvert.ConvertType.ToFloat)]
    protected double likability { get; set; }

    /// <summary>
    /// Clear changes
    /// </summary>
    public void CleanChange()
    {
        ChangeStrength /= 2;
        ChangeFeeling /= 2;
        ChangeStrengthDrink /= 2;
        ChangeStrengthFood /= 2;
    }
    /// <summary>
    /// Retrieve the stored stamina
    /// </summary>
    public void StoreTake()
    {
        const int t = 10;

        var s = StoreStrength / t;
        StoreStrength -= s;
        if (Math.Abs(StoreStrength) < 1)
            StoreStrength = 0;
        else
            StrengthChange(s);

        s = StoreStrengthDrink / t;
        StoreStrengthDrink -= s;
        if (Math.Abs(StoreStrengthDrink) < 1)
            StoreStrengthDrink = 0;
        else
            StrengthChangeDrink(s);

        s = StoreStrengthFood / t;
        StoreStrengthFood -= s;
        if (Math.Abs(StoreStrengthFood) < 1)
            StoreStrengthFood = 0;
        else
            StrengthChangeFood(s);
    }
    /// <summary>
    /// Eat food
    /// </summary>
    /// <param name="food">Food class</param>
    public void EatFood(IFood food)
    {
        Exp += food.Exp;
        var tmp = food.Strength / 2;
        StrengthChange(tmp);
        StoreStrength += tmp;
        tmp = food.StrengthFood / 2;
        StrengthChangeFood(tmp);
        StoreStrengthFood += tmp;
        tmp = food.StrengthDrink / 2;
        StrengthChangeDrink(tmp);
        StoreStrengthDrink += tmp;
        FeelingChange(food.Feeling);
        Health += food.Health;
        Likability += food.Likability;
    }
    /// <summary>
    /// Pet's current state
    /// </summary>
    [Line(name: "mode")]
    public ModeType Mode { get; set; } = ModeType.Nomal;
    [Line]
    public double LikabilityMax { get; set; } = 100;

    public double FeelingMax => 100 + (int)(Math.Pow(Level * (1 + LevelMax), 0.75) * 2);
    /// <summary>
    /// Experience bonus TODO
    /// </summary>
    public double ExpBonus { get; set; } = 1;

    /// <summary>
    /// Calculate the pet's current state
    /// </summary>
    public ModeType CalMode()
    {
        int realhel = 60 - (Feeling / FeelingMax >= 80 ? 12 : 0) - (Likability >= 80 ? 12 : (Likability >= 40 ? 6 : 0));
        //start from the worst
        if (Health <= realhel)
        {
            //we can narrow it down to either poor condition or sickness
            if (Health <= realhel / 2)
            {//sick
                return ModeType.Ill;
            }
            else
            {
                return ModeType.PoorCondition;
            }
        }
        //then decide whether happy or normal
        double realfel = .90 - (Likability >= 80 ? .20 : (Likability >= 40 ? .10 : 0));
        double felps = Feeling / FeelingMax;
        if (felps >= realfel)
        {
            return ModeType.Happy;
        }
        else if (felps <= realfel / 2)
        {
            return ModeType.PoorCondition;
        }
        return ModeType.Nomal;
    }
    /// <summary>
    /// New game
    /// </summary>
    public GameSave_VPet(string name)
    {
        Name = name;
        Money = 100;
        Exp = 0;
        Strength = 100;
        StrengthFood = 100;
        StrengthDrink = 100;
        Feeling = 60;
        Health = 100;
        Likability = 0;
        Mode = CalMode();
    }
    /// <summary>
    /// Load save
    /// </summary>
    public GameSave_VPet()
    {
    }
    /// <summary>
    /// Load save
    /// </summary>
    public static GameSave_VPet Load(ILine data) => LPSConvert.DeserializeObject<GameSave_VPet>(data);
    /// <summary>
    /// Save
    /// </summary>
    /// <returns>Save line</returns>
    public Line ToLine()
    {
        //Line save = new Line("vpet", Name);
        //save.SetFloat("money", Money);
        //save.SetInt("exp", Exp);
        //save.SetFloat("strength", Strength);
        //save.SetFloat("strengthdrink", StrengthDrink);
        //save.SetFloat("strengthfood", StrengthFood);
        //save.SetFloat("feeling", Feeling);
        //save.SetFloat("health", Health);
        //save.SetFloat("Likability", Likability);
        return LPSConvert.SerializeObject(this, "vpet");
    }

}
