using LinePutScript.Converter;

namespace VPet_Simulator.Windows.Interface
{
    /// <summary>
    /// Lines said automatically in a low state
    /// </summary>
    public class LowText : IText
    {
        /// <summary>
        /// State
        /// </summary>
        public enum ModeType
        {
            /// <summary>
            /// High state: happy/normal
            /// </summary>
            H,
            /// <summary>
            /// Low state: poor condition/sick
            /// </summary>
            L,
        }
        /// <summary>
        /// State
        /// </summary>
        [Line(IgnoreCase = true)] public ModeType Mode { get; set; } = ModeType.L;
        /// <summary>
        /// Stamina
        /// </summary>
        public enum StrengthType
        {
            /// <summary>
            /// Normal thirst/hunger
            /// </summary>
            L,
            /// <summary>
            /// Slightly thirsty/hungry
            /// </summary>
            M,
            /// <summary>
            /// Very thirsty/hungry
            /// </summary>
            S,
        }
        /// <summary>
        /// Stamina
        /// </summary>
        [Line(IgnoreCase = true)] public StrengthType Strength { get; set; } = StrengthType.S;
        /// <summary>
        /// Likability requirement
        /// </summary>
        public enum LikeType
        {
            /// <summary>
            /// No likability required
            /// </summary>
            N,
            /// <summary>
            /// Low likability requirement
            /// </summary>
            S,
            /// <summary>
            /// Medium likability requirement
            /// </summary>
            M,
            /// <summary>
            /// High likability
            /// </summary>
            L,
        }
        /// <summary>
        /// Likability requirement
        /// </summary>
        [Line(IgnoreCase = true)] public LikeType Like { get; set; } = LikeType.N;
    }
}
