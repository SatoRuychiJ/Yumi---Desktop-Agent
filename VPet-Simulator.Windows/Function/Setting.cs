using LinePutScript;
using LinePutScript.Dictionary;
using LinePutScript.Localization.WPF;
using Steamworks;
using System;
using System.Windows;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;

namespace VPet_Simulator.Windows
{
    /// <summary>
    /// Game settings
    /// </summary>
    internal class Setting : LPS_D, ISetting
    {
        MainWindow mw;
        /// <summary>
        /// Game settings
        /// </summary>
        public Setting(MainWindow mw, string lps) : base(lps)
        {
            var line = FindLine("zoomlevel");
            if (line == null)
                zoomlevel = 0.5;
            else
            {
                zoomlevel = line.InfoToDouble;
                if (zoomlevel < 0.1 || zoomlevel > 8)
                {
                    zoomlevel = 0.5;
                }
            }
            presslength = this["gameconfig"].GetInt("presslength", 300);
            intercycle = this["gameconfig"].GetInt("intercycle", 200);
            allowmove = !this["gameconfig"].GetBool("allowmove");
            smartmove = this["gameconfig"].GetBool("smartmove");
            // AIDeskPet: AI assistant mode disables data calculation by default (no hunger/illness); user can enable it manually in settings
            if (this["gameconfig"].Find("nofunction") == null)
                this["gameconfig"].SetBool("nofunction", true);
            enablefunction = !this["gameconfig"].GetBool("nofunction");
            autobuy = this["gameconfig"].GetBool("autobuy");
            autogift = this["gameconfig"].GetBool("autogift");
            autochangewindow = !this["gameconfig"].GetBool("autochangewindow");
            this.mw = mw;
        }


        private double zoomlevel = 0;
        /// <summary>
        /// Zoom ratio
        /// </summary>
        public double ZoomLevel
        {
            get
            {
                return zoomlevel;
            }
            set
            {
                FindorAddLine("zoomlevel").InfoToDouble = value;
                zoomlevel = value;
            }
        }
        /// <summary>
        /// Playback volume
        /// </summary>
        public double VoiceVolume
        {
            get => (double)GetFloat("voicevolume", 0.5);
            set => SetFloat("voicevolume", value);
        }
        /// <summary>
        /// Whether it is a larger screen
        /// </summary>
        public bool IsBiggerScreen
        {
            get => GetBool("bigscreen");
            set => SetBool("bigscreen", value);
        }
        /// <summary>
        /// Whether data collection is enabled
        /// </summary>
        public bool Diagnosis
        {
            get => this["diagnosis"].GetBool("enable");
            set => this["diagnosis"].SetBool("enable", value);
        }
        ///// <summary> // Tested: storing to memory has many benefits; not storing still uses a lot of memory, so just store it
        ///// Whether to store images in memory
        ///// </summary>
        //public bool StoreInMemory
        //{
        //    get => !this["set"].GetBool("storemem");
        //    set => this["set"].SetBool("storemem", value);
        //}
        /// <summary>
        /// Default mode when not in calculation mode
        /// </summary>
        public IGameSave.ModeType CalFunState
        {
            get => (IGameSave.ModeType)this[(gint)"calfunstate"];
            set => this[(gint)"calfunstate"] = (int)value;
        }
        /// <summary>
        /// Data collection frequency
        /// </summary>
        public int DiagnosisInterval
        {
            get => Math.Max(this["diagnosis"].GetInt("interval", 500), 20000);
            set => this["diagnosis"].SetInt("interval", value);
        }

        /// <summary>
        /// Auto-save frequency (min)
        /// </summary>
        public int AutoSaveInterval
        {
            get => Math.Max(GetInt("autosave", 10), -1);
            set => SetInt("autosave", value);
        }
        /// <summary>
        /// Maximum number of backup saves
        /// </summary>
        public int BackupSaveMaxNum
        {
            get => Math.Max(GetInt("bakupsave", 50), 1);
            set => SetInt("bakupsave", value);
        }
        /// <summary>
        /// Whether to keep on top
        /// </summary>
        public bool TopMost
        {
            get => !GetBool("topmost");
            set => SetBool("topmost", !value);
        }
        /// <summary>
        /// Whether to show the pet help window
        /// </summary>
        public bool PetHelper
        {
            get => GetBool("pethelper");
            set => SetBool("pethelper", value);
        }
        /// <summary>
        /// Whether mouse click-through is enabled
        /// </summary>
        public bool HitThrough
        {
            get => GetBool("hitthrough");
            set => SetBool("hitthrough", value);
        }
        /// <summary>
        /// Last cache cleanup date
        /// </summary>
        public DateTime LastCacheDate
        {
            get => GetDateTime("lastcachedate", DateTime.MinValue);
            set => SetDateTime("lastcachedate", value);
        }
        /// <summary>
        /// Whether data collection is disabled (for the day)
        /// </summary>
        public bool DiagnosisDayEnable = true;
        /// <summary>
        /// Language
        /// </summary>
        public string Language
        {
            // AIDeskPet: English-only software, always English
            get => "en";
            set => this[(gstr)"language"] = value;
        }
        public string Font
        {
            get => GetString("font", "OPPOSans R");
            set => this[(gstr)"font"] = value;
        }
        public string Theme
        {
            get
            {
                var line = FindLine("theme");
                if (line == null)
                    return "terminal"; // AIDeskPet: default to terminal purple theme
                return line.Info;
            }
            set
            {
                FindorAddLine("theme").Info = value;
            }
        }
        /// <summary>
        /// Stored data of the current pet
        /// </summary>
        public ILine PetData_OLD => this["petdata"];
        /// <summary>
        /// Save sequence count++
        /// </summary>
        public int SaveTimesPP
        {
            get
            {
                int list = GetInt("savetimes", 100000) + 1;
                SetInt("savetimes", list);
                return list;
            }
        }
        /// <summary>
        /// Save sequence count
        /// </summary>
        public int SaveTimes
        {
            get => GetInt("savetimes", 100000);
            set => SetInt("savetimes", value);
        }

        private int presslength;
        private int intercycle;
        /// <summary>
        /// Duration to count as a long press, in milliseconds
        /// </summary>
        public int PressLength
        {
            get => presslength;
            set
            {
                presslength = value;
                this["gameconfig"].SetInt("presslength", value);
            }
        }
        /// <summary>
        /// Interaction cycle
        /// </summary>
        public int InteractionCycle
        {
            get => intercycle;
            set
            {
                intercycle = value;
                this["gameconfig"].SetInt("intercycle", value);
            }
        }
        /// <summary>
        /// Calculation interval (seconds)
        /// </summary>
        public double LogicInterval
        {
            get => this["gameconfig"].GetDouble("logicinterval", 15);
            set => this["gameconfig"].SetDouble("logicinterval", value);
        }

        /// <summary>
        /// Calculation interval
        /// </summary>
        public double PetHelpLeft
        {
            get => (double)this["pethelp"].GetFloat("left", 0);
            set => this["pethelp"].SetFloat("left", value);
        }
        /// <summary>
        /// Calculation interval
        /// </summary>
        public double PetHelpTop
        {
            get => (double)this["pethelp"].GetFloat("top", 0);
            set => this["pethelp"].SetFloat("top", value);
        }

        bool allowmove;
        /// <summary>
        /// Allow move events
        /// </summary>
        public bool AllowMove
        {
            get => allowmove;
            set
            {
                allowmove = value;
                this["gameconfig"].SetBool("allowmove", !value);
            }
        }
        bool smartmove;
        /// <summary>
        /// Smart move
        /// </summary>
        public bool SmartMove
        {
            get => smartmove;
            set
            {
                smartmove = value;
                this["gameconfig"].SetBool("smartmove", value);
            }
        }
        bool enablefunction;
        /// <summary>
        /// Enable data features such as calculation
        /// </summary>
        public bool EnableFunction
        {
            get => enablefunction;
            set
            {
                enablefunction = value;
                this["gameconfig"].SetBool("nofunction", !value);
            }
        }
        private bool autochangewindow;
        public bool AutoChangeWindow
        {
            get => !autochangewindow;
            set
            {
                autochangewindow = !value;
                this["gameconfig"].SetBool("autochangewindow", value);
            }
        }
        /// <summary>
        /// Smart move cycle (seconds)
        /// </summary>
        public int SmartMoveInterval
        {
            get => this["gameconfig"].GetInt("smartmoveinterval", 20 * 60);
            set => this["gameconfig"].SetInt("smartmoveinterval", value);
        }
        /// <summary>
        /// Message bar outside
        /// </summary>
        public bool MessageBarOutside
        {
            get => this["gameconfig"].GetBool("msgbarout");
            set => this["gameconfig"].SetBool("msgbarout", value);
        }
        /// <summary>
        /// Start on boot
        /// </summary>
        public bool StartUPBoot
        {
            get => this["gameconfig"].GetBool("startboot");
            set => this["gameconfig"].SetBool("startboot", value);
        }
        /// <summary>
        /// Start Steam on boot
        /// </summary>
        public bool StartUPBootSteam
        {
            get => !this["gameconfig"].GetBool("startbootsteam");
            set => this["gameconfig"].SetBool("startbootsteam", !value);
        }
        /// <summary>
        /// Selected desktop pet
        /// </summary>
        public string PetGraph
        {
            get => this["gameconfig"].GetString("petgraph", "aigirl");
            set => this["gameconfig"].SetString("petgraph", value);
        }

        /// <summary>
        /// Whether to record the game exit position (default: yes)
        /// </summary>
        public bool StartRecordLast
        {
            get => !this["startrecordlast"].GetBool("enable");
            set => this["startrecordlast"].SetBool("enable", !value);
        }
        /// <summary>
        /// Recorded last exit position
        /// </summary>
        public Point StartRecordLastPoint
        {
            get
            {
                var line = FindLine("startrecordlast");
                if (line == null)
                    return new Point(100, 100);
                return new Point(line.GetDouble("x", 100), line.GetDouble("y", 100));
            }
            set
            {
                var line = FindorAddLine("startrecordlast");
                line.SetDouble("x", Math.Min(Math.Max(value.X, -65000), 65000));
                line.SetDouble("y", Math.Min(Math.Max(value.Y, -65000), 65000));
            }
        }
        /// <summary>
        /// Configured startup position of the desktop pet
        /// </summary>
        public Point StartRecordPoint
        {
            get
            {
                var line = FindLine("startrecord");
                if (line == null)
                    return StartRecordLastPoint;
                return new Point(line.GetDouble("x", 0), line.GetDouble("y", 0));
            }
            set
            {
                var line = FindorAddLine("startrecord");
                line.SetDouble("x", Math.Min(Math.Max(value.X, -65000), 65000));
                line.SetDouble("y", Math.Min(Math.Max(value.Y, -65000), 65000));
            }
        }
        /// <summary>
        /// Triggers the music action when the live playback volume reaches this value
        /// </summary>
        public double MusicCatch
        {
            get => Math.Max(this["gameconfig"].GetDouble("musiccatch", 0.3), 0.02);
            set => this["gameconfig"].SetDouble("musiccatch", value);
        }
        /// <summary>
        /// Triggers the special music action when the live playback volume reaches this value
        /// </summary>
        public double MusicMax
        {
            get => Math.Max(this["gameconfig"].GetDouble("musicmax", 0.70), 0.02);
            set => this["gameconfig"].SetDouble("musicmax", value);
        }
        /// <summary>
        /// Rendering resolution of the desktop pet graphics; higher is sharper
        /// </summary>
        public int Resolution
        {
            get => this["gameconfig"].GetInt("resolution", 500);
            set => this["gameconfig"].SetInt("resolution", value);
        }

        bool autobuy;
        /// <summary>
        /// Allow the desktop pet to auto-buy food
        /// </summary>
        public bool AutoBuy
        {
            get => autobuy;
            set
            {
                autobuy = value;
                this["gameconfig"].SetBool("autobuy", value);
            }
        }
        bool autogift;
        /// <summary>
        /// Allow the desktop pet to auto-buy gifts
        /// </summary>
        public bool AutoGift
        {
            get => autogift;
            set
            {
                autogift = value;
                this["gameconfig"].SetBool("autogift", value);
            }
        }
        /// <summary>
        /// Hide the window in the task switcher (Alt+Tab)
        /// </summary>
        public bool HideFromTaskControl
        {
            get => this["gameconfig"].GetBool("hide_from_task_control");
            set => this["gameconfig"].SetBool("hide_from_task_control", value);
        }

        public bool MoveAreaDefault
        {
            get
            {
                var line = FindLine("movearea");
                if (line == null)
                    return true;
                return line.GetBool("set");
            }
            set
            {
                var line = FindorAddLine("movearea");
                line.SetBool("set", value);
            }
        }
        public System.Drawing.Rectangle MoveArea
        {
            get
            {
                var line = FindLine("movearea");
                if (line == null)
                    return default(System.Drawing.Rectangle);
                return new System.Drawing.Rectangle(
                    line.GetInt("x", 0),
                    line.GetInt("y", 0),
                    line.GetInt("w", 114),
                    line.GetInt("h", 514)
                );
            }
            set
            {
                var line = FindorAddLine("movearea");
                line.SetInt("x", value.X);
                line.SetInt("y", value.Y);
                line.SetInt("w", value.Width);
                line.SetInt("h", value.Height);
            }
        }
        /// <summary>
        /// Message bar outside
        /// </summary>
        public bool MPNOTouch
        {
            get => this["mutiplay"].GetBool("notouch");
            set => this["mutiplay"].SetBool("notouch", value);
        }
        public bool DeBug
        {
            get => this[(gbol)"debug"];
            set => this[(gbol)"debug"] = value;
        }

        public double Opacity
        {
            get => Math.Min(Math.Max(this["gameconfig"].GetDouble("opacity", 0.6), 0.05), 1);
            set => this["gameconfig"].SetDouble("opacity", value);
        }
        public bool OpacityMain
        {
            get => this["gameconfig"].GetBool("opacitymain");
            set => this["gameconfig"].SetBool("opacitymain", value);
        }
        public bool OpacityHitThrough
        {
            get => !this["gameconfig"].GetBool("opacityhitthrough");
            set => this["gameconfig"].SetBool("opacityhitthrough", !value);
        }

        public int GameScreenIndex
        {
            get => this["gameconfig"].GetInt("gamescreenindex", 0);
            set => this["gameconfig"].SetInt("gamescreenindex", value);
        }

        public long SteamID
        {
            get => this[(gi64)"steamid"];
            set => this[(gi64)"steamid"] = value;
        }

        /// <summary>
        /// Read/write custom game settings (an interface intended for mods)
        /// </summary>
        /// <param name="lineName">Game setting</param>
        /// <returns>The first Line with a matching name if found; otherwise a newly created Line with that name</returns>
        ILine ISetting.this[string lineName]
        {
            get
            {
                if (lineName == "onmod")
                    return new Line("onmod", "true");
                return FindorAddLine(lineName);
            }
            set
            {
                if (value.Name == "onmod")
                    return;
                AddorReplaceLine(value);
            }
        }

        public void SetZoomLevel(double level) => mw.SetZoomLevel(level);

        public void SetVoiceVolume(double volume) { VoiceVolume = volume; mw.Main.PlayVoiceVolume = volume; }

        public void SetAutoSaveInterval(int interval)
        {
            AutoSaveInterval = interval;
            if (AutoSaveInterval > 0)
            {
                mw.AutoSaveTimer.Interval = AutoSaveInterval * 60000;
                mw.AutoSaveTimer.Start();
            }
            else
            {
                mw.AutoSaveTimer.Stop();
            }
        }

        public void SetTopMost(bool topMost)
        {
            TopMost = true;
            mw.Topmost = topMost;
        }

        public void SetLanguage(string language)
        {
            var petloader = mw.Pets.Find(x => x.Name == PetGraph);
            petloader ??= mw.Pets[0];
            bool ischangename = mw.Core.Save.Name == petloader.PetName.Translate();
            LocalizeCore.LoadCulture(language);
            Language = LocalizeCore.CurrentCulture;
            if (ischangename)
            {
                mw.Core.Save.Name = petloader.PetName.Translate();
                if (mw.IsSteamUser)
                    SteamFriends.SetRichPresence("username", mw.Core.Save.Name);
            }
        }

        public void SetLogicInterval(double interval)
        {
            LogicInterval = interval;
            mw.Main.SetLogicInterval((int)(interval * 1000));
        }

        public void SetAllowMove(bool allowMove)
        {
            AllowMove = allowMove;
            mw.Main.SetMoveMode(AllowMove, SmartMove, SmartMoveInterval * 1000);
        }

        public void SetSmartMove(bool smartMove)
        {
            SmartMove = smartMove;
            mw.Main.SetMoveMode(AllowMove, SmartMove, SmartMoveInterval * 1000);
        }

        public void SetEnableFunction(bool enableFunction)
        {
            EnableFunction = enableFunction;
            if (!enableFunction)
            {
                if (mw.Main.State != Main.WorkingState.Nomal)
                {
                    mw.Main.WorkTimer.Visibility = Visibility.Collapsed;
                    mw.Main.State = Main.WorkingState.Nomal;
                }
            }
        }

        public void SetSmartMoveInterval(int interval)
        {
            SmartMoveInterval = interval;
            mw.Main.SetMoveMode(AllowMove, SmartMove, SmartMoveInterval * 1000);
        }
    }
}
