using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace VPet_Simulator.Core
{
    /// <summary>
    /// Animation display interface
    /// </summary>
    public interface IGraph : IEquatable<object>, IDisposable
    {
        /// <summary>
        /// Run this animation from the start
        /// </summary>
        /// <param name="EndAction">Stop action</param>
        /// <param name="parant">Display location</param>
        void Run(Decorator parant, Action EndAction = null);

        /// <summary>
        /// Whether to loop playback
        /// </summary>
        bool IsLoop { get; set; }

        /// <summary>
        /// Whether preparation is complete
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Whether loading failed
        /// </summary>
        bool IsFail { get; }
        /// <summary>
        /// Failure error message
        /// </summary>
        string FailMessage { get; }
        /// <summary>
        /// Information about this animation
        /// </summary>
        GraphInfo GraphInfo { get; }

        /// <summary>
        /// Current animation playback state and control
        /// </summary>
        TaskControl Control { get; }

        /// <summary>
        /// Stop the animation
        /// </summary>
        /// <param name="StopEndAction">Whether to skip running the end animation when stopping</param>
        void Stop(bool StopEndAction)
        {
            if (Control == null)
                return;
            if (StopEndAction)
                Control.EndAction = null;
            Control.Type = TaskControl.ControlType.Stop;
        }
        /// <summary>
        /// Set to continue playback
        /// </summary>
        void SetContinue()
        {
            Control.Type = TaskControl.ControlType.Continue;
        }

        /// <summary>
        /// Animation file path, may be a folder or a file
        /// </summary>
        string Path { get; }
        /// <summary>
        /// Last-used timestamp, used to decide whether resources need to be released
        /// </summary>
        long LastUseTimeTicks => 0;

        /// <summary>
        /// Update the last-used time to the current time, so idle cache cleanup can decide whether to clean up
        /// </summary>
        void Touch() { }
        /// <summary>
        /// Clean up idle cache; if this animation has been unused for a long time, release its resources
        /// </summary>
        /// <param name="nowTicks">Current time</param>
        void CleanupIdleCache(long nowTicks) { }

        /// <summary>
        /// Indicates ImageRun support
        /// </summary>
        public interface IRunImage : IGraph
        {
            /// <summary>
            /// Run this animation from the start
            /// </summary>
            /// <param name="parant">Display location</param>
            /// <param name="EndAction">End method</param>
            /// <param name="image">Additional image</param>
            void Run(Decorator parant, ImageSource image, Action EndAction = null);
        }

        /// <summary>
        /// Animation control class
        /// </summary>
        public class TaskControl
        {
            /// <summary>
            /// Current animation playback state
            /// </summary>
            public bool PlayState => Type != ControlType.Status_Stoped && Type != ControlType.Stop;
            /// <summary>
            /// Set to continue playback
            /// </summary>
            public void SetContinue() { Type = ControlType.Continue; }
            /// <summary>
            /// Stop playback
            /// </summary>
            public void Stop(Action endAction = null) { EndAction = endAction; Type = ControlType.Stop; }
            /// <summary>
            /// Control type
            /// </summary>
            public enum ControlType
            {
                /// <summary>
                /// Maintain the status quo, no override applied
                /// </summary>
                Status_Quo,
                /// <summary>
                /// Stop the current animation
                /// </summary>
                Stop,
                /// <summary>
                /// Continue playback after completion, effective only once, then reverts to Status_Quo
                /// </summary>
                Continue,
                /// <summary>
                /// Animation has stopped
                /// </summary>
                Status_Stoped,
            }
            /// <summary>
            /// End action
            /// </summary>
            public Action EndAction;
            /// <summary>
            /// Control type
            /// </summary>
            public ControlType Type = ControlType.Status_Quo;
            /// <summary>
            /// Provides operations and end action for the animation control class
            /// </summary>
            /// <param name="endAction"></param>
            public TaskControl(Action endAction = null)
            {
                EndAction = endAction;
            }
        }
    }
}
