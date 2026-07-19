using LinePutScript;
using LinePutScript.Converter;
using System;
using static VPet_Simulator.Core.IGameSave;

namespace VPet_Simulator.Core
{
    /// <summary>
    /// Game save
    /// </summary>
    public class GameSave : IGameSave
    {
        /// <summary>
        /// Pet name
        /// </summary>
        [Line(name: "name")]
        public string Name { get; set; }
        public string HostName { get; set; }

        /// <summary>
        /// Money
        /// </summary>
        [Line(Type = LPSConvert.ConvertType.ToFloat, Name = "money")]
        public double Money { get; set; }
        /// <summary>
        /// Experience
        /// </summary>
        [Line(type: LPSConvert.ConvertType.ToFloat, name: "exp")] public double Exp { get; set; }
        /// <summary>
        /// Level
        /// </summary>
        public int Level => Exp < 0 ? 1 : (int)(Math.Sqrt(Exp) / 10) + 1;
        /// <summary>
        /// Experience needed to level up
        /// </summary>
        /// <returns></returns>
        public int LevelUpNeed() => (int)(Math.Pow((Level) * 10, 2));
        /// <summary>
        /// Strength 0-100
        /// </summary>
        public double Strength { get => strength; set => strength = Math.Min(StrengthMax, Math.Max(0, value)); }

        public double StrengthMax { get; } = 100;

        [Line(Type = LPSConvert.ConvertType.ToFloat, IgnoreCase = true)]
        protected double strength { get; set; }
        /// <summary>
        /// Strength to be replenished, slowly added to the pet over time
        /// </summary>//makes the game more engaging
        [Line(Type = LPSConvert.ConvertType.ToFloat, IgnoreCase = true)]
        public double StoreStrength { get; set; }
        /// <summary>
        /// Change in strength
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
                value = Math.Min(100, value);
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
        /// Change in food
        /// </summary>
        public double ChangeStrengthFood { get; set; } = 0;
        /// <summary>
        /// Thirst
        /// </summary>
        public double StrengthDrink
        {
            get => strengthDrink; set
            {
                value = Math.Min(100, value);
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
        /// Change in thirst
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

                value = Math.Min(100, value);
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
        /// Change in mood
        /// </summary>
        public double ChangeFeeling { get; set; } = 0;
        public void FeelingChange(double value)
        {
            ChangeFeeling += value;
            Feeling += value;
        }
        /// <summary>
        /// Health (illness) (hidden)
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
        /// Retrieve stored strength
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

        public double LikabilityMax => 90 + Level * 10;

        public double FeelingMax => 100;

        public double ExpBonus => 1;

        /// <summary>
        /// Calculate the pet's current state
        /// </summary>
        public ModeType CalMode()
        {
            int realhel = 60 - (Feeling >= 80 ? 12 : 0) - (Likability >= 80 ? 12 : (Likability >= 40 ? 6 : 0));
            //start from the worst case first
            if (Health <= realhel)
            {
                //narrowed down to either poor condition or ill
                if (Health <= realhel / 2)
                {//ill
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
        public GameSave(string name)
        {
            Name = name;
            Money = 1000;
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
        public GameSave()
        {
            //Money = line.GetFloat("money");
            //Name = line.Info;
            //Exp = line.GetInt("exp");
            //Strength = line.GetFloat("strength");
            //StrengthDrink = line.GetFloat("strengthdrink");
            //StrengthFood = line.GetFloat("strengthfood");
            //Feeling = line.GetFloat("feeling");
            //Health = line.GetFloat("health");
            //Likability = line.GetFloat("likability");
            //Mode = CalMode();
        }
        /// <summary>
        /// Load save
        /// </summary>
        public static GameSave Load(ILine data) => LPSConvert.DeserializeObject<GameSave>(data);
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
}