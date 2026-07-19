using LinePutScript.Converter;
using System;
using VPet_Simulator.Core;
using static VPet_Simulator.Core.Main;

namespace VPet_Simulator.Windows.Interface
{
    /// <summary>
    /// Random chatter triggered when the pet is clicked
    /// </summary>
    public class ClickText : ICheckText, IFood
    {
        public ClickText()
        {

        }
        public ClickText(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Specifies which work state to speak in; empty means any, sleep means while sleeping
        /// </summary>
        [Line(ignoreCase: true)]
        public string Working { get; set; } = null;

        /// <summary>
        /// Date range
        /// </summary>
        [Flags]
        public enum DayTime
        {
            Morning = 1,
            Afternoon = 2,
            Night = 4,
            Midnight = 8,
        }
        /// <summary>
        /// Current time
        /// </summary>
        [Line(ignoreCase: true)]
        private int dayTime { get; set; } = 15;
        /// <summary>
        /// Date range
        /// </summary>      
        public DayTime DaiTime
        {
            get => (DayTime)dayTime;
            set => dayTime = (int)value;
        }

        /// <summary>
        /// Work state
        /// </summary>
        [Line(IgnoreCase = true)]
        public WorkingState State { get; set; } = WorkingState.Nomal;


        /// <summary>
        /// Check whether some of the states meet the requirements
        /// </summary> not all of them, because fetching each one individually is too inefficient
        public override bool CheckState(Main m)
        {
            if (!base.CheckState(m))
                return false;

            if (string.IsNullOrWhiteSpace(Working))
            {
                if (State != m.State)
                    return false;
            }
            else
            {
                if (m.State != WorkingState.Work)
                    return false;
                if (m.NowWork.Name != Working)
                    return false;
            }
            return true;
        }
        [Line(ignoreCase: true)]
        public double Money { get; set; }

        [Line(ignoreCase: true)]
        public int Exp { get; set; }
        [Line(ignoreCase: true)]
        public double Strength { get; set; }
        [Line(ignoreCase: true)]
        public double StrengthFood { get; set; }
        [Line(ignoreCase: true)]
        public double StrengthDrink { get; set; }
        [Line(ignoreCase: true)]
        public double Feeling { get; set; }
        [Line(ignoreCase: true)]
        public double Health { get; set; }
        [Line(ignoreCase: true)]
        public double Likability { get; set; }
    }
}
