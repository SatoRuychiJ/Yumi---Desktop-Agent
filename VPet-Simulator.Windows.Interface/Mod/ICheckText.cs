using LinePutScript.Converter;
using System;
using VPet_Simulator.Core;

namespace VPet_Simulator.Windows.Interface
{
    /// <summary>
    /// All checkable text formats
    /// </summary>
    public abstract class ICheckText : IText
    {
        [Line(ignoreCase: true)]
        public int mode { get; set; } = 7;
        /// <summary>
        /// Required state mode
        /// </summary>      
        public ModeType Mode
        {
            get => (ModeType)mode;
            set => mode = (int)value;
        }
        /// <summary>
        /// Pet state mode
        /// </summary>
        [Flags]
        public enum ModeType
        {
            /// <summary>
            /// Happy
            /// </summary>
            Happy = 1,
            /// <summary>
            /// Normal
            /// </summary>
            Nomal = 2,
            /// <summary>
            /// Poor condition
            /// </summary>
            PoorCondition = 4,
            /// <summary>
            /// Sick (in bed)
            /// </summary>
            Ill = 8
        }

        /// <summary>
        /// Likability requirement: minimum
        /// </summary>
        [Line(IgnoreCase = true)]
        public double LikeMin { get; set; } = 0;
        /// <summary>
        /// Likability requirement: maximum
        /// </summary>
        [Line(IgnoreCase = true)]
        public double LikeMax { get; set; } = int.MaxValue;
        /// <summary>
        /// Health requirement: minimum
        /// </summary>
        [Line(IgnoreCase = true)]
        public double HealthMin { get; set; } = 0;
        /// <summary>
        /// Health requirement: maximum
        /// </summary>
        [Line(IgnoreCase = true)]
        public double HealthMax { get; set; } = int.MaxValue;
        /// <summary>
        /// Level requirement: minimum
        /// </summary>
        [Line(IgnoreCase = true)] public double LevelMin { get; set; } = 0;
        /// <summary>
        /// Level requirement: maximum
        /// </summary>
        [Line(IgnoreCase = true)] public double LevelMax { get; set; } = int.MaxValue;
        /// <summary>
        /// Money requirement: minimum
        /// </summary>
        [Line(IgnoreCase = true)] public double MoneyMin { get; set; } = int.MinValue;
        /// <summary>
        /// Money requirement: maximum
        /// </summary>
        [Line(IgnoreCase = true)] public double MoneyMax { get; set; } = int.MaxValue;
        /// <summary>
        /// Food requirement: minimum
        /// </summary>
        [Line(IgnoreCase = true)] public double FoodMin { get; set; } = 0;
        /// <summary>
        /// Food requirement: maximum
        /// </summary>
        [Line(IgnoreCase = true)] public double FoodMax { get; set; } = int.MaxValue;
        /// <summary>
        /// Thirst requirement: minimum
        /// </summary>
        [Line(IgnoreCase = true)] public double DrinkMin { get; set; } = 0;
        /// <summary>
        /// Thirst requirement: maximum
        /// </summary>
        [Line(IgnoreCase = true)] public double DrinkMax { get; set; } = int.MaxValue;
        /// <summary>
        /// Mood requirement: minimum
        /// </summary>
        [Line(IgnoreCase = true)] public double FeelMin { get; set; } = 0;
        /// <summary>
        /// Mood requirement: maximum
        /// </summary>
        [Line(IgnoreCase = true)] public double FeelMax { get; set; } = int.MaxValue;
        /// <summary>
        /// Stamina requirement: minimum
        /// </summary>
        [Line(IgnoreCase = true)] public double StrengthMin { get; set; } = 0;
        /// <summary>
        /// Stamina requirement: maximum
        /// </summary>
        [Line(IgnoreCase = true)] public double StrengthMax { get; set; } = int.MaxValue;

        /// <summary>
        /// Check whether some of the states meet the requirements
        /// </summary> not all of them, because fetching each one individually is too inefficient
        public virtual bool CheckState(IGameSave save)
        {
            if (save.Likability < LikeMin || save.Likability > LikeMax)
                return false;
            if (save.Health < HealthMin || save.Health > HealthMax)
                return false;
            if (save.Level < LevelMin || save.Level > LevelMax)
                return false;
            if (save.Money < MoneyMin || save.Money > MoneyMax)
                return false;
            if (save.StrengthFood < FoodMin || save.StrengthFood > FoodMax)
                return false;
            if (save.StrengthDrink < DrinkMin || save.StrengthDrink > DrinkMax)
                return false;
            if (save.Feeling < FeelMin || save.Feeling > FeelMax)
                return false;
            if (save.Strength < StrengthMin || save.Strength > StrengthMax)
                return false;
            return true;
        }
        /// <summary>
        /// Check whether some of the states meet the requirements
        /// </summary> not all of them, because fetching each one individually is too inefficient
        public virtual bool CheckState(Main m) => CheckState(m.Core.Save);
    }
}
