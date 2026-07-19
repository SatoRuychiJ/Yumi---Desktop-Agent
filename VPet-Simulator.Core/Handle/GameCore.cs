using System;
using System.Collections.Generic;
using System.Windows;

namespace VPet_Simulator.Core
{
    /// <summary>
    /// Resources used by the game
    /// </summary>
    public class GameCore
    {
        /// <summary>
        /// Controller
        /// </summary>
        public IController Controller;
        /// <summary>
        /// List of touch areas and events
        /// </summary>
        public List<TouchArea> TouchEvent = new List<TouchArea>();
        /// <summary>
        /// Graphics core
        /// </summary>
        public GraphCore Graph;
        /// <summary>
        /// Game data
        /// </summary>
        public IGameSave Save;
    }
    /// <summary>
    /// Touch area event
    /// </summary>
    public class TouchArea
    {
        /// <summary>
        /// Position
        /// </summary>
        public Point Locate;
        /// <summary>
        /// Size
        /// </summary>
        public Size Size;
        /// <summary>
        /// Action to run when triggered
        /// </summary>
        public Func<bool> DoAction;
        /// <summary>
        /// No: trigger immediately / Yes: trigger on long press
        /// </summary>
        public bool IsPress;
        /// <summary>
        /// Create a touch area event
        /// </summary>
        /// <param name="locate">Position</param>
        /// <param name="size">Size</param>
        /// <param name="doAction">Action to run when triggered</param>
        /// <param name="isPress">No: trigger immediately / Yes: trigger on long press</param>
        public TouchArea(Point locate, Size size, Func<bool> doAction, bool isPress = false)
        {
            Locate = locate;
            Size = size;
            DoAction = doAction;
            IsPress = isPress;
        }
        /// <summary>
        /// Determine whether this click event was successfully triggered
        /// </summary>
        /// <param name="point">Position</param>
        /// <returns>Whether successful</returns>
        public bool Touch(Point point)
        {
            double inx = point.X - Locate.X;
            double iny = point.Y - Locate.Y;
            return inx >= 0 && inx <= Size.Width && iny >= 0 && iny <= Size.Height;
        }
    }
}
