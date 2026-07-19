namespace VPet_Simulator.Core
{
    /// <summary>
    /// Food interface
    /// </summary>
    public interface IFood
    {
        /// <summary>
        /// Experience points
        /// </summary>
        int Exp { get; }

        /// <summary>
        /// Stamina 0-100
        /// </summary>
        double Strength { get; }
        /// <summary>
        /// Fullness
        /// </summary>
        double StrengthFood { get; }
        /// <summary>
        /// Thirst
        /// </summary>
        double StrengthDrink { get; }

        /// <summary>
        /// Mood
        /// </summary>
        double Feeling { get; }

        /// <summary>
        /// Health
        /// </summary>
        double Health { get; }

        /// <summary>
        /// Likability
        /// </summary>
        double Likability { get; }
    }
}
