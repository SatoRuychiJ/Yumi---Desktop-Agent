namespace VPet_Simulator.Windows.Interface
{
    /// <summary>
    /// This is the main body of the plugin. Please inherit this class
    /// </summary>
    public abstract class MainPlugin
    {
        /// <summary>
        /// MOD name. Used to locate the plugin by name; must match the MOD name
        /// </summary>
        public abstract string PluginName { get; }
        /// <summary>
        /// Main window. Provides the host's features and settings; most parameters and calls live here
        /// </summary>
        public IMainWindow MW;
        /// <summary>
        /// MOD plugin initialization
        /// </summary>
        /// <param name="mainwin">Main window</param>
        /// Do not load game or player data here; use only for initialization. Note: there is no UI thread at this point
        /// To load data (CORE)/game (SAVE), use LoadPlugin
        /// To get/set data after loading, use GameLoaded
        public MainPlugin(IMainWindow mainwin)
        {
            //At this point the main window, player, Core, etc. are all null; do not load game or player data
            MW = mainwin;
        }
        ///// <summary>//TODO
        ///// Load game theme
        ///// </summary>
        ///// <param name="theme">Theme</param>
        //public virtual void LoadTheme(Theme theme) { }
        /// <summary>
        /// Initialize the program and read the save
        /// </summary>
        /// e.g. add your own Tick to mw.Main.EventTimer
        /// e.g. create desktop controls that use the UI
        /// e.g. add a custom item type creation method to Item.Creators
        public virtual void LoadPlugin() { }
        /// <summary>
        /// State after the game has finished loading. Allows modifying already-loaded content
        /// </summary>
        public virtual void GameLoaded() { }

        /// <summary>
        /// Game end (can save or clear, etc., though saving has a dedicated Save())
        /// </summary>
        public virtual void EndGame() { }

        /// <summary>
        /// Save the game (can write GameSave.Other to store settings and data, etc.)
        /// </summary>
        public virtual void Save() { }

        /// <summary>
        /// Open the code plugin settings
        /// </summary>
        public virtual void Setting() { }
        /// <summary>
        /// Reload DIY buttons; add custom buttons here if needed
        /// </summary>
        public virtual void LoadDIY() { }
    }
}
