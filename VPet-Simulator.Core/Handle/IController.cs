namespace VPet_Simulator.Core
{
    /// <summary>
    /// Pet controller, must be implemented by the host
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// Move the pet position (zoom ratio applied automatically)
        /// </summary>
        /// <param name="X">X axis</param>
        /// <param name="Y">Y axis</param>
        void MoveWindows(double X, double Y);

        /// <summary>
        /// Get the pet distance from the left edge
        /// </summary>
        double GetWindowsDistanceLeft();
        /// <summary>
        /// Get the pet distance from the right edge
        /// </summary>
        double GetWindowsDistanceRight();
        /// <summary>
        /// Get the pet distance from the top edge
        /// </summary>
        double GetWindowsDistanceUp();
        /// <summary>
        /// Get the pet distance from the bottom edge
        /// </summary>
        double GetWindowsDistanceDown();
        /// <summary>
        /// Get whether the screen the pet is on is the active screen
        /// </summary>
        /// <returns></returns>
        bool IfInActivateScreen() => true;
        /// <summary>
        /// Set the current screen as the active screen (if the pet spans multiple screens, the primary screen takes precedence)
        /// </summary>
        void SetNowScreenActivate() { }
        ///// <summary>
        ///// Window width
        ///// </summary>
        //double WindowsWidth { get; set; }
        ///// <summary>
        ///// Window height
        ///// </summary>
        //double WindowsHight { get; set; }
        /// <summary>
        /// Zoom ratio
        /// </summary>
        double ZoomRatio { get; }
        /// <summary>
        /// Press duration to count as a long press, in milliseconds
        /// </summary>
        int PressLength { get; }

        /// <summary>
        /// Show the panel window
        /// </summary>
        void ShowPanel();

        /// <summary>
        /// Re-align to the edge when at the border, to avoid being blocked
        /// </summary>
        void ResetPosition();
        /// <summary>
        /// Check whether the pet is at the edge
        /// </summary>
        bool CheckPosition();

        /// <summary>
        /// Enable data features such as calculation
        /// </summary>
        bool EnableFunction { get; }
        /// <summary>
        /// Interaction cycle
        /// </summary>
        int InteractionCycle { get; }

        /// <summary>
        /// Whether edge repositioning is enabled
        /// </summary>
        bool RePositionActive { get; set; }

        /// <summary>
        /// Whether to automatically switch the active screen
        /// </summary>
        bool AutoChangeWindow => false;
    }
}
