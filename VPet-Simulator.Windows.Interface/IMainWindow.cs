using LinePutScript.Dictionary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using VPet_Simulator.Core;
using static VPet_Simulator.Windows.Interface.ScheduleTask;

namespace VPet_Simulator.Windows.Interface
{
    /// <summary>
    /// Game main window
    /// </summary>
    public interface IMainWindow
    {
        /// <summary>
        /// Save prefix, used for running multiple instances; empty uses the default save, and when set the prefix is usually preceded by '-'
        /// </summary>
        string PrefixSave { get; }
        /// <summary>
        /// Startup arguments
        /// </summary>
        LPS_D Args { get; }
        /// <summary>
        /// Whether this is a Steam user
        /// </summary>
        bool IsSteamUser { get; }
        /// <summary>
        /// SteamID
        /// </summary>
        public ulong SteamID { get; }
        /// <summary>
        /// SteamAccountId
        /// </summary>
        public uint SteamAuthorID { get; }
        /// <summary>
        /// Game settings
        /// </summary>
        ISetting Set { get; }
        /// <summary>
        /// List of pet loaders
        /// </summary>
        List<PetLoader> Pets { get; }
        /// <summary>
        /// All available chat APIs
        /// </summary>
        List<ITalkAPI> TalkAPI { get; }
        /// <summary>
        /// The TalkBox currently in use
        /// </summary>
        ITalkAPI TalkBoxCurr { get; }
        /// <summary>
        /// Pet data core
        /// </summary>
        GameCore Core { get; }
        /// <summary>
        /// Pet main component
        /// </summary>
        Main Main { get; }
        /// <summary>
        /// Version number
        /// </summary>
        int version { get; }
        /// <summary>
        /// Version number
        /// </summary>
        string Version { get; }
        /// <summary>
        /// Last click time (Tick)
        /// </summary>
        long lastclicktime { get; set; }
        /// <summary>
        /// All third-party plugins
        /// </summary>
        List<MainPlugin> Plugins { get; }
        /// <summary>
        /// All food
        /// </summary>
        List<Food> Foods { get; }
        /// <summary>
        /// Lines said when food is needed
        /// </summary>
        List<LowText> LowFoodText { get; }
        /// <summary>
        /// Lines said when a drink is needed
        /// </summary>
        List<LowText> LowDrinkText { get; }
        /// <summary>
        /// Lines said when clicked
        /// </summary>
        List<ClickText> ClickTexts { get; }
        /// <summary>
        /// Lines for the player to choose to say
        /// </summary>
        List<SelectText> SelectTexts { get; }
        /// <summary>
        /// Get the auto-click text
        /// </summary>
        /// <returns>Speech content</returns>
        ClickText GetClickText();
        /// <summary>
        /// All photos
        /// </summary>
        List<Photo> Photos { get; }
        /// <summary>
        /// Image resources
        /// </summary>
        ImageResources ImageSources { get; }
        /// <summary>
        /// File resources, storing file paths; can be used by code plugin MODs
        /// </summary>
        Resources FileSources { get; }
        /// <summary>
        /// Set the game zoom multiplier
        /// </summary>
        /// <param name="zl">Zoom multiplier, range 0.1-10</param>
        void SetZoomLevel(double zl);
        /// <summary>
        /// Save settings
        /// </summary>
        void Save();
        /// <summary>
        /// Load DIY content
        /// </summary>
        void LoadDIY();
        /// <summary>
        /// Show the settings page
        /// </summary>
        /// <param name="page">Settings page</param>
        void ShowSetting(int page = -1);
        /// <summary>
        /// Show the ToBetterBuy page
        /// </summary>
        /// <param name="type">Food type</param>
        void ShowBetterBuy(Food.FoodType type);
        /// <summary>
        /// Show the photo gallery
        /// </summary>
        void ShowGallery();
        /// <summary>
        /// Close the pet
        /// </summary>
        void Close();
        /// <summary>
        /// Restart the pet
        /// </summary>
        void Restart();
        /// <summary>
        /// Mouse click-through
        /// </summary>
        bool MouseHitThrough { get; set; }

        /// <summary>
        /// Whether the save Hash check passed
        /// </summary>
        bool HashCheck { get; }

        /// <summary>
        /// Get the current system music playback volume
        /// </summary>
        float AudioPlayingVolume();
        /// <summary>
        /// Close indicator, default true
        /// </summary>
        bool CloseConfirm { get; }
        /// <summary>
        /// Disable HashCheck for this player
        /// If your mod is a cheat mod / contains cheat content, call this method before cheating
        /// </summary>
        void HashCheckOff();
        /// <summary>
        /// Windows opened by the game; they are all closed together on exit
        /// </summary>
        List<Window> Windows { get; set; }
        /// <summary>
        /// Game save data
        /// </summary>
        GameSave_v2 GameSavesData { get; }
        /// <summary>
        /// Main window Grid
        /// </summary>
        Grid MGHost { get; }
        /// <summary>
        /// Main window Pet Grid
        /// </summary>
        Grid PetGrid { get; }
        /// <summary>
        /// Triggered when a new multiplayer window (guest list) is created/joined
        /// Listen to this event if you want to write multiplayer features
        /// </summary>
        event Action<IMPWindows> MutiPlayerHandle;
        /// <summary>
        /// Triggered when a new multiplayer window (guest list) is created/joined
        /// Intended for MODs defining their own multiplayer window; normal multiplayer features do not need this
        /// </summary>
        /// <param name="mp"></param>
        void MutiPlayerStart(IMPWindows mp);

        /// <summary>
        /// Show the eating (sandwich) animation
        /// </summary>
        /// <param name="graphName">Sandwich animation name</param>
        /// <param name="imageSource">The image sandwiched in the middle</param>
        void DisplayFoodAnimation(string graphName, ImageSource imageSource);
        /// <summary>
        /// Use/consume an item (update: no money deducted) (does not include showing the animation)
        /// </summary>
        /// <param name="item">Item</param>
        void TakeItem(Food item);

        /// <summary>
        /// Show an input box
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="text">Text</param>
        /// <param name="defaulttext">Default text</param>
        /// <param name="ENDAction">End event</param>
        /// <param name="AllowMutiLine">Whether multi-line input is allowed</param>
        /// <param name="TextCenter">Center the text</param>
        /// <param name="CanHide">Whether it can be hidden</param>
        void ShowInputBox(string title, string text, string defaulttext, Action<string> ENDAction, bool AllowMutiLine = false, bool TextCenter = true, bool CanHide = false);
        /// <summary>
        /// UI thread invocation point
        /// </summary>
        Dispatcher Dispatcher { get; }
        /// <summary>
        /// Get info for all current MODs
        /// </summary>
        IEnumerable<IModInfo> ModInfo { get; }
        /// <summary>
        /// Get info for all currently enabled MODs
        /// </summary>
        IEnumerable<IModInfo> OnModInfo { get; }

        /// <summary>
        /// Locations of all MOD files
        /// </summary>
        List<DirectoryInfo> MODPath { get; }
        /// <summary>
        /// Schedule
        /// </summary>
        ScheduleTask ScheduleTask { get; }
        /// <summary>
        /// All available packages
        /// </summary>
        List<PackageFull> SchedulePackage { get; }
        /// <summary>
        /// Event: eating
        /// </summary>
        event Action<Food> Event_TakeItem;

        /// <summary>
        /// Event: new day
        /// </summary>
        event Action Event_NewDay;

        /// <summary>
        /// Dynamic resources, used by plugin MODs to store shared data
        /// </summary>
        Dictionary<string, object> DynamicResources { get; }

        /// <summary>
        /// Generate an authorization code (only for LB-related service verification)
        /// </summary>
        /// <returns></returns>
        Task<int> GenerateAuthKey();

        /// <summary>
        /// Invoke the Event_TakeItemHandle event
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="count">Count</param>
        /// <param name="from">Source</param>
        public void TakeItemHandle(Food item, int count, string from);

        /// <summary>
        /// Activity log, not saved
        /// </summary>
        public ObservableCollection<ActivityLog> ActivityLogs { get; }

        /// <summary>
        /// Inventory: items the pet owns
        /// </summary>
        public List<Item> Items { get; }

        /// <summary>
        /// Add an item to the inventory (auto-merge)
        /// </summary>
        /// <param name="item">Item</param>
        public void ItemsAdd(Item item);
    }
}
