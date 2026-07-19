using LinePutScript;
using System;
using System.Windows;

namespace VPet_Simulator.Windows.Interface
{
    /// <summary>
    /// Settings method interface
    /// </summary>
    public interface ISetting
    {
        /// <summary>
        /// Get the current zoom multiplier
        /// </summary>
        double ZoomLevel { get; }

        /// <summary>
        /// Set the zoom multiplier
        /// </summary>
        /// <param name="level">Zoom level</param>
        void SetZoomLevel(double level);

        /// <summary>
        /// Get the current playback volume
        /// </summary>
        double VoiceVolume { get; }

        /// <summary>
        /// Set the playback volume
        /// </summary>
        /// <param name="volume">Volume</param>
        void SetVoiceVolume(double volume);

        /// <summary>
        /// Get the current auto-save interval (minutes)
        /// </summary>
        int AutoSaveInterval { get; }

        /// <summary>
        /// Set the auto-save interval (minutes)
        /// </summary>
        /// <param name="interval">Save interval</param>
        void SetAutoSaveInterval(int interval);

        /// <summary>
        /// Get or set the maximum number of backup saves
        /// </summary>
        int BackupSaveMaxNum { get; set; }

        /// <summary>
        /// Get whether currently topmost
        /// </summary>
        bool TopMost { get; }

        /// <summary>
        /// Set whether topmost
        /// </summary>
        /// <param name="topMost">Whether topmost</param>
        void SetTopMost(bool topMost);

        /// <summary>
        /// Get or set the date the cache was last cleared
        /// </summary>
        DateTime LastCacheDate { get; set; }

        /// <summary>
        /// Get the current language
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Set the language
        /// </summary>
        /// <param name="language">Language code</param>
        void SetLanguage(string language);

        /// <summary>
        /// Get or set the hold duration counted as a long press (milliseconds)
        /// </summary>
        int PressLength { get; set; }

        /// <summary>
        /// Get or set the interaction interval
        /// </summary>
        int InteractionCycle { get; set; }

        /// <summary>
        /// Get the current calculation interval (seconds)
        /// </summary>
        double LogicInterval { get; }

        /// <summary>
        /// Set the calculation interval (seconds)
        /// </summary>
        /// <param name="interval">Calculation interval</param>
        void SetLogicInterval(double interval);

        /// <summary>
        /// Get whether movement is currently allowed
        /// </summary>
        bool AllowMove { get; }

        /// <summary>
        /// Set whether movement is allowed
        /// </summary>
        /// <param name="allowMove">Whether movement is allowed</param>
        void SetAllowMove(bool allowMove);

        /// <summary>
        /// Get whether smart move is currently enabled
        /// </summary>
        bool SmartMove { get; }

        /// <summary>
        /// Set whether smart move is enabled
        /// </summary>
        /// <param name="smartMove">Whether smart move is enabled</param>
        void SetSmartMove(bool smartMove);

        /// <summary>
        /// Get whether data features such as calculation are currently enabled
        /// </summary>
        bool EnableFunction { get; }

        /// <summary>
        /// Set whether data features such as calculation are enabled
        /// </summary>
        /// <param name="enableFunction">Whether the feature is enabled</param>
        void SetEnableFunction(bool enableFunction);

        /// <summary>
        /// Get the current smart move interval (seconds)
        /// </summary>
        int SmartMoveInterval { get; }

        /// <summary>
        /// Set the smart move interval (seconds)
        /// </summary>
        /// <param name="interval">Smart move interval</param>
        void SetSmartMoveInterval(int interval);

        /// <summary>
        /// Get or set whether the message box is external
        /// </summary>
        bool MessageBarOutside { get; set; }

        /// <summary>
        /// Get whether the game exit position is currently recorded
        /// </summary>
        bool StartRecordLast { get; set; }

        /// <summary>
        /// Get the last exit position
        /// </summary>
        Point StartRecordLastPoint { get; }

        /// <summary>
        /// Get or set the pet's startup position
        /// </summary>
        Point StartRecordPoint { get; set; }

        /// <summary>
        /// Get or set the real-time volume at which the music action runs
        /// </summary>
        double MusicCatch { get; set; }

        /// <summary>
        /// Get or set the real-time volume at which the special music action runs
        /// </summary>
        double MusicMax { get; set; }

        /// <summary>
        /// Get or set the pet's graphics rendering resolution; higher is sharper, takes effect after restart
        /// </summary>
        int Resolution { get; set; }

        /// <summary>
        /// Get or set whether the pet may auto-buy food
        /// </summary>
        bool AutoBuy { get; set; }

        /// <summary>
        /// Get or set whether the pet may auto-buy gifts
        /// </summary>
        bool AutoGift { get; set; }

        /// <summary>
        /// Get or set whether to hide the window in the task switcher (Alt+Tab); takes effect after restart
        /// </summary>
        bool HideFromTaskControl { get; set; }

        /// <summary>
        /// Read/write custom game settings (interface intended for mods)
        /// </summary>
        /// <param name="lineName">Game setting</param>
        /// <returns>The first Line with a matching name if found; otherwise a newly created Line with that name</returns>
        ILine this[string lineName] { get; set; }

        /// <summary>
        /// Allow interaction in multiplayer
        /// </summary>
        bool MPNOTouch { get; set; }
        /// <summary>
        /// Pet skin (not necessarily this one; falls back to the default index 0 if not found)
        /// </summary>
        string PetGraph { get; }
        /// <summary>
        /// Developer mode
        /// </summary>
        bool DeBug { get; set; }
    }

}
