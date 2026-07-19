using LinePutScript;

namespace VPet_Simulator.Core
{
    /// <summary>
    /// Game save
    /// </summary>
    public interface IGameSave
    {
        /// <summary>
        /// Pet name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Owner's title
        /// </summary>
        string HostName { get; set; }
        /// <summary>
        /// Money
        /// </summary>
        double Money { get; set; }
        /// <summary>
        /// Experience
        /// </summary>
        double Exp { get; set; }
        /// <summary>
        /// Experience bonus
        /// </summary>
        double ExpBonus { get; }
        /// <summary>
        /// Level
        /// </summary>
        int Level { get; }
        /// <summary>
        /// Experience needed to level up
        /// </summary>
        /// <returns></returns>
        int LevelUpNeed();
        /// <summary>
        /// Strength 0-100
        /// </summary>
        double Strength { get; set; }
        /// <summary>
        /// Maximum strength
        /// </summary>
        double StrengthMax { get; }
        /// <summary>
        /// Strength to be replenished, slowly added to the pet over time
        /// </summary>//makes the game more engaging
        double StoreStrength { get; set; }
        /// <summary>
        /// Change in strength
        /// </summary>
        double ChangeStrength { get; set; }
        /// <summary>
        /// Modify strength
        /// </summary>
        /// <param name="value"></param>
        void StrengthChange(double value);
        /// <summary>
        /// Fullness
        /// </summary>
        double StrengthFood { get; set; }
        /// <summary>
        /// Fullness to be replenished, slowly added to the pet over time
        /// </summary>//makes the game more engaging
        double StoreStrengthFood { get; set; }
        void StrengthChangeFood(double value);
        /// <summary>
        /// Change in food
        /// </summary>
        double ChangeStrengthFood { get; set; }
        /// <summary>
        /// Thirst
        /// </summary>
        double StrengthDrink { get; set; }

        /// <summary>
        /// Thirst to be replenished, slowly added to the pet over time
        /// </summary>//makes the game more engaging
        double StoreStrengthDrink { get; set; }
        /// <summary>
        /// Change in thirst
        /// </summary>
        double ChangeStrengthDrink { get; set; }
        /// <summary>
        /// Modify thirst
        /// </summary>
        void StrengthChangeDrink(double value);
        /// <summary>
        /// Modify mood
        /// </summary>
        void FeelingChange(double value);
        /// <summary>
        /// Change in mood
        /// </summary>
        double ChangeFeeling { get; set; }
        /// <summary>
        /// Mood
        /// </summary>
        double Feeling { get; set; }
        /// <summary>
        /// Maximum mood
        /// </summary>
        double FeelingMax { get; }

        /// <summary>
        /// Health (illness) (hidden)
        /// </summary>
        double Health { get; set; }

        /// <summary>
        /// Likability (hidden) (accumulated value)
        /// </summary>
        double Likability { get; set; }
        /// <summary>
        /// Likability (hidden) (maximum value)
        /// </summary>
        double LikabilityMax { get; }

        /// <summary>
        /// Clear changes
        /// </summary>
        void CleanChange();
        /// <summary>
        /// Retrieve stored strength
        /// </summary>
        void StoreTake();
        /// <summary>
        /// Eat food
        /// </summary>
        /// <param name="food">Food class</param>
        void EatFood(IFood food);
        /// <summary>
        /// Pet's current state
        /// </summary>
        ModeType Mode { get; set; }
        /// <summary>
        /// Pet state mode
        /// </summary>
        public enum ModeType
        {
            /// <summary>
            /// Happy
            /// </summary>
            Happy,
            /// <summary>
            /// Normal
            /// </summary>
            Nomal,
            /// <summary>
            /// Poor condition
            /// </summary>
            PoorCondition,
            /// <summary>
            /// Ill (bedridden)
            /// </summary>
            Ill
        }

        /// <summary>
        /// Calculate the pet's current state
        /// </summary>
        ModeType CalMode();

        /// <summary>
        /// Save
        /// </summary>
        /// <returns>Save line</returns>
        Line ToLine();
    }
}
