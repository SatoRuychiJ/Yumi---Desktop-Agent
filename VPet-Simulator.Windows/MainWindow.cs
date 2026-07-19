using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Dictionary;
using LinePutScript.Localization.WPF;
using NAudio.CoreAudioApi;
using Panuon.WPF.UI;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;
using static VPet_Simulator.Core.GraphHelper;
using static VPet_Simulator.Core.GraphInfo;
using static VPet_Simulator.Windows.Interface.Food;
using static VPet_Simulator.Windows.Interface.Photo.UnlockCondition;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using ContextMenu = System.Windows.Forms.ContextMenuStrip;
using Image = System.Windows.Controls.Image;
using Line = LinePutScript.Line;
using MenuItem = System.Windows.Forms.ToolStripMenuItem;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Timers.Timer;
using ToolBar = VPet_Simulator.Core.ToolBar;

namespace VPet_Simulator.Windows
{
    public partial class MainWindow : IMainWindow
    {

        /// <summary>
        /// Load theme
        /// </summary>
        /// <param name="themename">Theme name</param>
        public void LoadTheme(string themename)
        {
            Theme ctheme = Themes.Find(x => x.xName == themename);
            if (ctheme == null)
            {
                return;
            }
            Theme = ctheme;

            //Load image pack
            ImageSources.AddSources(ctheme.Images);

            //Shadow color
            Application.Current.Resources["ShadowColor"] = Function.HEXToColor('#' + ctheme.ThemeColor[(gstr)"ShadowColor"]);

            foreach (ILine lin in ctheme.ThemeColor.Assemblage.FindAll(x => !x.Name.Contains("Color")))
                Application.Current.Resources[lin.Name] = new SolidColorBrush(Function.HEXToColor('#' + lin.info));

            //System-generated colors
            Color c = Function.HEXToColor('#' + ctheme.ThemeColor["Primary"].info);
            c.A = 204;
            Application.Current.Resources["PrimaryTrans"] = new SolidColorBrush(c);
            c.A = 44;
            Application.Current.Resources["PrimaryTrans4"] = new SolidColorBrush(c);
            c.A = 170;
            Application.Current.Resources["PrimaryTransA"] = new SolidColorBrush(c);
            c.A = 238;
            Application.Current.Resources["PrimaryTransE"] = new SolidColorBrush(c);

            c = Function.HEXToColor('#' + ctheme.ThemeColor["Secondary"].info);
            c.A = 204;
            Application.Current.Resources["SecondaryTrans"] = new SolidColorBrush(c);
            c.A = 44;
            Application.Current.Resources["SecondaryTrans4"] = new SolidColorBrush(c);
            c.A = 170;
            Application.Current.Resources["SecondaryTransA"] = new SolidColorBrush(c);
            c.A = 238;
            Application.Current.Resources["SecondaryTransE"] = new SolidColorBrush(c);


            c = Function.HEXToColor('#' + ctheme.ThemeColor["DARKPrimary"].info);
            c.A = 204;
            Application.Current.Resources["DARKPrimaryTrans"] = new SolidColorBrush(c);
            c.A = 44;
            Application.Current.Resources["DARKPrimaryTrans4"] = new SolidColorBrush(c);
            c.A = 170;
            Application.Current.Resources["DARKPrimaryTransA"] = new SolidColorBrush(c);
            c.A = 238;
            Application.Current.Resources["DARKPrimaryTransE"] = new SolidColorBrush(c);
        }

        public void LoadFont(string fontname)
        {
            IFont cfont = Fonts.Find(x => x.Name == fontname);
            if (cfont == null)
            {
                return;
            }
            var font = cfont.Font;
            Application.Current.Resources["MainFont"] = font;
            Panuon.WPF.UI.GlobalSettings.Setting.FontFamily = font;
        }


        /// <summary>
        /// Get the auto-click text
        /// </summary>
        /// <returns>Speech content</returns>
        public ClickText GetClickText()
        {
            ClickText.DayTime dt;
            var now = DateTime.Now.Hour;
            if (now < 6)
                dt = ClickText.DayTime.Midnight;
            else if (now < 12)
                dt = ClickText.DayTime.Morning;
            else if (now < 18)
                dt = ClickText.DayTime.Afternoon;
            else
                dt = ClickText.DayTime.Night;

            ClickText.ModeType mt;
            switch (Core.Save.Mode)
            {
                case IGameSave.ModeType.PoorCondition:
                    mt = ClickText.ModeType.PoorCondition;
                    break;
                default:
                case IGameSave.ModeType.Nomal:
                    mt = ClickText.ModeType.Nomal;
                    break;
                case IGameSave.ModeType.Happy:
                    mt = ClickText.ModeType.Happy;
                    break;
                case IGameSave.ModeType.Ill:
                    mt = ClickText.ModeType.Ill;
                    break;
            }
            var list = ClickTexts.FindAll(x => x.DaiTime.HasFlag(dt) && x.Mode.HasFlag(mt) && x.CheckState(Main));
            if (list.Count == 0)
                return null;
            return list[Function.Rnd.Next(list.Count)];
        }
        private Image hashcheckimg;

        /// <summary>
        /// Disable HashCheck for this player
        /// If your mod is a cheat mod / contains cheat content, call this method before cheating
        /// </summary>
        public void HashCheckOff()
        {
            HashCheck = false;
        }
        /// <summary>
        /// Whether the save Hash check passed
        /// </summary>
        public bool HashCheck
        {
            get => GameSavesData.HashCheck;
            set
            {
                if (!value)
                {
                    GameSavesData.HashCheckOff();
                }
                // AIDeskPet: Save-verification pixel flag (hash.png) removed
            }
        }
        public void SetZoomLevel(double zl)
        {
            Set.ZoomLevel = zl;
            //this.Height = 500 * zl;
            MGrid.Width = 500 * zl;
            if (petHelper != null)
            {
                petHelper.Width = 50 * zl;
                petHelper.Height = 50 * zl;
                petHelper.ReloadLocation();
            }
        }



        //private DateTime timecount = DateTime.Now;
        /// <summary>
        /// Save settings
        /// </summary>
        public void Save()
        {
            //Save schedule
            ScheduleTask?.Save();

            //Save inventory
            foreach (var v in GameSavesData.Data.Assemblage.Keys.Where(x => x.StartsWith("item")))
                GameSavesData.Data.Remove(v);
            for (int i = 0; i < Items.Count; i++)
            {
                GameSavesData.Data.Add(LPSConvert.SerializeObjectToLine<Line>(Items[i], "item" + i.ToString()));
            }

            try
            {
                //Save plugins
                foreach (MainPlugin mp in Plugins)
                    mp.Save();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "由于插件引起的保存错误".Translate());
            }
            //Game save
            if (Set != null)
            {
                var st = Set.SaveTimesPP;
                if (Main != null)
                {
                    Set.VoiceVolume = Main.PlayVoiceVolume;
                    List<string> list = new List<string>();
                    Foods.FindAll(x => x.Star).ForEach(x => list.Add(x.Name));
                    Set["betterbuy"]["star"].info = string.Join(",", list);
                    //GameSavesData.Statistics[(gint)"stat_time"] = (int)(DateTime.Now - timecount).TotalMinutes;
                    //timecount = DateTime.Now;
                }
                Set.StartRecordLastPoint = new Point(Dispatcher.Invoke(() => Left), Dispatcher.Invoke(() => Top));
                if (PrefixSave == "" && File.Exists(ExtensionValue.BaseDirectory + @"\Setting.lps"))
                {//Backup for the main settings
                    if (new FileInfo(ExtensionValue.BaseDirectory + @"\Setting.lps").Length < 10)
                    {//File smaller than 10 bytes, likely corrupted
                        File.Delete(ExtensionValue.BaseDirectory + @"\Setting.lps");
                    }
                    else
                    {
                        if (File.Exists(ExtensionValue.BaseDirectory + @"\Setting.bkp"))
                            File.Delete(ExtensionValue.BaseDirectory + @"\Setting.bkp");
                        File.Move(ExtensionValue.BaseDirectory + @"\Setting.lps", ExtensionValue.BaseDirectory + @"\Setting.bkp");
                    }

                }
                File.WriteAllText(ExtensionValue.BaseDirectory + @$"\Setting{PrefixSave}.lps", Set.ToString());

                if (!Directory.Exists(ExtensionValue.BaseDirectory + @"\Saves"))
                    Directory.CreateDirectory(ExtensionValue.BaseDirectory + @"\Saves");
                if (!Directory.Exists(ExtensionValue.BaseDirectory + @"\Saves_BKP"))//Backup feature
                    Directory.CreateDirectory(ExtensionValue.BaseDirectory + @"\Saves_BKP");

                if (Core != null && Core.Save != null)
                {
                    var ds = new List<string>(Directory.GetFiles(ExtensionValue.BaseDirectory + @"\Saves", $"Save{PrefixSave}_*.lps")).OrderBy(x =>
                    {
                        if (int.TryParse(x.Split('_').Last().Split('.')[0], out int i))
                            return i;
                        return 0;
                    }).ToList();
                    while (ds.Count > Set.BackupSaveMaxNum)
                    {
                        File.Delete(ds[0]);
                        ds.RemoveAt(0);
                    }

                    if (File.Exists(ExtensionValue.BaseDirectory + $"\\Saves\\Save{PrefixSave}_{st}.lps"))
                        File.Delete(ExtensionValue.BaseDirectory + $"\\Saves\\Save{PrefixSave}_{st}.lps");

                    var saveslps = GameSavesData.ToLPS();
                    var savesdata = saveslps.ToString();

                    int hash = Math.Abs(saveslps.GetHashCode() % 255);
                    if (File.Exists(ExtensionValue.BaseDirectory + $"\\Saves_BKP\\Save{PrefixSave}_{hash:X}.lps"))
                        File.Delete(ExtensionValue.BaseDirectory + $"\\Saves_BKP\\Save{PrefixSave}_{hash:X}.lps");

                    //Save
                    File.WriteAllText(ExtensionValue.BaseDirectory + $"\\Saves\\Save{PrefixSave}_{st}.lps", savesdata);
                    //Backup
                    File.WriteAllText(ExtensionValue.BaseDirectory + $"\\Saves_BKP\\Save{PrefixSave}_{hash:X}.lps", savesdata);

                    if (File.Exists(ExtensionValue.BaseDirectory + @"\Save.lps"))
                    {
                        if (File.Exists(ExtensionValue.BaseDirectory + @"\Save.bkp"))
                            File.Delete(ExtensionValue.BaseDirectory + @"\Save.bkp");
                        File.Move(ExtensionValue.BaseDirectory + @"\Save.lps", ExtensionValue.BaseDirectory + @"\Save.bkp");
                    }

                    //Steam cloud save
                    if (IsSteamUser)
                    {
                        var steamsave = SteamRemoteStorage.Files.Where(x => x.StartsWith($"VPetCloud/Save{PrefixSave}_")).ToList();
                        if (steamsave.Count > Set.BackupSaveMaxNum)
                        {
                            steamsave = steamsave.OrderBy(x =>
                            {
                                if (int.TryParse(x.Split('_').Last().Split('.')[0], out int i))
                                    return i;
                                return 0;
                            }).ToList();
                            while (steamsave.Count > Set.BackupSaveMaxNum)
                            {
                                SteamRemoteStorage.FileDelete(steamsave[0]);
                                steamsave.RemoveAt(0);
                            }
                        }
                        SteamRemoteStorage.FileWrite($"VPetCloud/Save{PrefixSave}_{(DateTime.Now.Ticks / 60000):X}.lps", Encoding.UTF8.GetBytes(savesdata));
                    }
                }
            }
        }
        /// <summary>
        /// Reload the DIY button area
        /// </summary>
        public void LoadDIY()
        {
            Main.ToolBar.MenuDIY.Items.Clear();

            if (App.MutiSaves.Count > 1)
            {
                var list = App.MutiSaves.ToList();
                foreach (var win in App.MainWindows)
                {
                    list.Remove(win.PrefixSave);
                }
                list.Remove(PrefixSave);
                if (list.Count > 0)
                {
                    var menuItem = new System.Windows.Controls.MenuItem()
                    {
                        Header = "桌宠多开".Translate(),
                        HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                    };
                    foreach (var win in list)
                    {
                        var mo = new System.Windows.Controls.MenuItem()
                        {
                            Header = win.Translate(),
                            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                        };
                        mo.Click += (s, e) =>
                        {
                            if (App.MainWindows.FirstOrDefault(x => x.PrefixSave.Trim('-') == win) == null)
                            {
                                new MainWindow(win, this).Show();
                            }
                            menuItem.Items.Remove(s);
                        };
                        menuItem.Items.Add(mo);
                    }
                    Main.ToolBar.MenuDIY.Items.Add(menuItem);
                }
            }

            foreach (ISub sub in Set["diy"])
                Main.ToolBar.AddMenuButton(ToolBar.MenuType.DIY, sub.Name, () =>
                {
                    Main.ToolBar.Visibility = Visibility.Collapsed;
                    RunDIY(sub.Info);
                });

            //Load game Workshop plugins
            foreach (MainPlugin mp in Plugins)
                try//Don't remove try for DEBUG; not on the main thread so errors won't be shown
                {
                    mp.LoadDIY();
                }
                catch (Exception e)
                {
                    MessageBoxX.Show(e.ToString(), "由于插件引起的自定按钮加载错误".Translate() + '-' + mp.PluginName);
                }
            Main.ToolBar.LoadDIY();
        }
        /// <summary>
        /// Load the pet helper
        /// </summary>
        public void LoadPetHelper()
        {
            petHelper = new PetHelper(this);
            petHelper.Show();
        }

        public void RunDIY(string content)
        {
            if (content.Contains(@":\"))
            {
                try
                {
                    if (!Set["v"][(gbol)"rundiy"])
                    {
                        MessageBoxX.Show("由于操作系统的设计，通过我们软件启动的程序可能会在任务管理器中归类为我们软件的子进程，这可能导致CPU/内存占用显示较高".Translate(),
                            "关于CPU/内存占用显示较高的一次性提示".Translate());
                        Set["v"][(gbol)"rundiy"] = true;
                    }
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = content;
                    startInfo.UseShellExecute = false;
                    Process.Start(startInfo);
                }
                catch
                {
                    try
                    {
                        try
                        {
                            Process.Start(content);
                        }
                        catch
                        {
                            var psi = new ProcessStartInfo
                            {
                                FileName = content,
                                UseShellExecute = true
                            };
                            Process.Start(psi);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBoxX.Show("快捷键运行失败:无法运行指定内容".Translate() + '\n' + e.Message);
                    }
                }
            }
            else if (content.Contains("://"))
            {
                try
                {
                    ExtensionFunction.StartURL(content);
                }
                catch (Exception e)
                {
                    MessageBoxX.Show("快捷键运行失败:无法运行指定内容".Translate() + '\n' + e.Message);
                }
            }
            else
            {
                try
                {
                    SendKeys.SendWait(content);
                }
                catch (Exception e)
                {
                    MessageBoxX.Show("快捷键运行失败:无法运行指定内容".Translate() + '\n' + e.Message);
                }
            }
        }

        public void ShowSetting(int page = -1)
        {
            if (page >= 0 && page <= 6)
                winSetting.MainTab.SelectedIndex = page;
            winSetting.Show();
        }
        public void ShowWorkMenu(Work.WorkType type)
        {
            if (winWorkMenu == null)
            {
                winWorkMenu = new winWorkMenu(this, type);
                winWorkMenu.Show();
            }
            else
            {
                winWorkMenu.LsbCategory.SelectedIndex = (int)type;
                winWorkMenu.Focus();
                winWorkMenu.Topmost = true;
            }
        }
        public void ShowBetterBuy(Food.FoodType type)
        {
            winBetterBuy.Show(type);
        }
        public void ShowGallery()
        {
            if (winGallery != null)
            {
                winGallery.Show();
                winGallery.Focus();
            }
            else
            {
                winGallery = new winGallery(this);
                winGallery.Show();
            }
        }
        int lowstrengthAskCountFood = 20;
        int lowstrengthAskCountDrink = 20;
        private void lowStrength()
        {
            var sm = Core.Save.StrengthMax;
            var sm75 = sm * 0.70;
            if (Set.AutoBuy && Core.Save.Money >= 100)
            {
                var havemoney = Core.Save.Money * 0.8;
                List<Food> food = Foods.FindAll(x => x.Price >= 2 && x.Health >= -5 && x.Exp >= -10 && x.Likability >= 0 && x.Price < havemoney //Pet won't eat negative-effect food
                 && !x.IsOverLoad() // Won't eat overloaded food
                );

                if ((Core.Save.StrengthFood + Core.Save.StoreStrengthFood) < sm75)
                {//When hungry, eat a proper meal
                    food = food.FindAll(x => x.Type == Food.FoodType.Meal && x.StrengthFood > Math.Min(sm * 0.20, 100));
                    if (food.Count == 0)
                        return;
                    var item = food[Function.Rnd.Next(food.Count)];
                    Core.Save.Money -= item.Price * 1.2;
                    TakeItemHandle(item, 1, "autofood");
                    TakeItem(item);
                    GameSavesData.Statistics[(gint)"stat_autobuy"]++;
                    Main.Display(item.GetGraph(), item.ImageSource, Main.DisplayToNomal);
                }
                else if ((Core.Save.StrengthDrink + Core.Save.StoreStrengthDrink) < sm75)
                {
                    food = food.FindAll(x => x.Type == Food.FoodType.Drink && x.StrengthDrink > Math.Min(sm * 0.20, 50));
                    if (food.Count == 0)
                        return;
                    var item = food[Function.Rnd.Next(food.Count)];
                    Core.Save.Money -= item.Price * 1.2;
                    TakeItemHandle(item, 1, "autodrink");
                    TakeItem(item);
                    GameSavesData.Statistics[(gint)"stat_autobuy"]++;
                    Main.Display(item.GetGraph(), item.ImageSource, Main.DisplayToNomal);
                }
                else if (Core.Save.Feeling < Core.Save.FeelingMax * 0.50)
                {
                    if (Set.AutoGift)
                    {
                        food = food.FindAll(x => x.Type == Food.FoodType.Gift && x.Feeling > Math.Min(Core.Save.FeelingMax * 0.10, 50));
                        if (food.Count == 0)
                            return;
                    }
                    else // Without auto-buy gifts, try auto-buying snacks to gain what little we can
                    {
                        food = food.FindAll(x => x.Type == Food.FoodType.Snack && x.Feeling > Math.Min(Core.Save.FeelingMax * 0.10, 40));
                        if (food.Count == 0)
                            return;
                    }
                    var item = food[Function.Rnd.Next(food.Count)];
                    Core.Save.Money -= item.Price * 1.2;
                    TakeItemHandle(item, 1, "autofeel");
                    TakeItem(item);
                    GameSavesData.Statistics[(gint)"stat_autogift"]++;
                    Main.Display(item.GetGraph(), item.ImageSource, Main.DisplayToNomal);
                }
            }
            else if (Core.Save.Mode == IGameSave.ModeType.Happy || Core.Save.Mode == IGameSave.ModeType.Nomal)
            {
                if (Core.Save.StrengthFood < sm75 && Function.Rnd.Next(lowstrengthAskCountFood--) == 0)
                {
                    lowstrengthAskCountFood = Set.InteractionCycle;
                    var like = Core.Save.Likability < 40 ? 0 : (Core.Save.Likability < 70 ? 1 : (Core.Save.Likability < 100 ? 2 : 3));
                    var txt = LowFoodText.FindAll(x => x.Mode == LowText.ModeType.H && (int)x.Like <= like);
                    if (txt.Count != 0)
                        if (Core.Save.StrengthFood > sm * 0.60)
                        {
                            txt = txt.FindAll(x => x.Strength == LowText.StrengthType.L);
                            if (txt.Count != 0)
                                Main.Say(txt[Function.Rnd.Next(txt.Count)].TranslateTextConvert(Main));
                        }
                        else if (Core.Save.StrengthFood > sm * 0.40)
                        {
                            txt = txt.FindAll(x => x.Strength == LowText.StrengthType.M);
                            if (txt.Count != 0)
                                Main.Say(txt[Function.Rnd.Next(txt.Count)].TranslateTextConvert(Main));
                        }
                        else
                        {
                            txt = txt.FindAll(x => x.Strength == LowText.StrengthType.S);
                            if (txt.Count != 0)
                                Main.Say(txt[Function.Rnd.Next(txt.Count)].TranslateTextConvert(Main));
                        }
                    Main.DisplayStopForce(() => Main.Display(GraphType.Switch_Hunger, AnimatType.Single, Main.DisplayToNomal));
                    return;
                }
                if (Core.Save.StrengthDrink < sm75 && Function.Rnd.Next(lowstrengthAskCountDrink--) == 0)
                {
                    lowstrengthAskCountDrink = Set.InteractionCycle;
                    var like = Core.Save.Likability < 40 ? 0 : (Core.Save.Likability < 70 ? 1 : (Core.Save.Likability < 100 ? 2 : 3));
                    var txt = LowDrinkText.FindAll(x => x.Mode == LowText.ModeType.H && (int)x.Like <= like);
                    if (txt.Count != 0)
                        if (Core.Save.StrengthDrink > sm * 0.60)
                        {
                            txt = txt.FindAll(x => x.Strength == LowText.StrengthType.L);
                            if (txt.Count != 0)
                                Main.Say(txt[Function.Rnd.Next(txt.Count)].TranslateTextConvert(Main));
                        }
                        else if (Core.Save.StrengthDrink > sm * 0.40)
                        {
                            txt = txt.FindAll(x => x.Strength == LowText.StrengthType.M);
                            if (txt.Count != 0)
                                Main.Say(txt[Function.Rnd.Next(txt.Count)].TranslateTextConvert(Main));
                        }
                        else
                        {
                            txt = txt.FindAll(x => x.Strength == LowText.StrengthType.S);
                            if (txt.Count != 0)
                                Main.Say(txt[Function.Rnd.Next(txt.Count)].TranslateTextConvert(Main));
                        }
                    Main.DisplayStopForce(() => Main.Display(GraphType.Switch_Thirsty, AnimatType.Single, Main.DisplayToNomal));
                    return;
                }
            }
            else
            {
                var sm20 = sm * 0.20;
                if (Core.Save.StrengthFood < sm * 0.60 && Function.Rnd.Next(lowstrengthAskCountFood--) == 0)
                {
                    lowstrengthAskCountFood = Set.InteractionCycle;
                    var like = Core.Save.Likability < 40 ? 0 : (Core.Save.Likability < 70 ? 1 : (Core.Save.Likability < 100 ? 2 : 3));
                    var txt = LowFoodText.FindAll(x => x.Mode == LowText.ModeType.L && (int)x.Like < like);
                    if (Core.Save.StrengthFood > sm * 0.40)
                    {
                        txt = txt.FindAll(x => x.Strength == LowText.StrengthType.L);
                        if (txt.Count != 0)
                            Main.Say(txt[Function.Rnd.Next(txt.Count)].TranslateTextConvert(Main));
                    }
                    else if (Core.Save.StrengthFood > sm20)
                    {
                        txt = txt.FindAll(x => x.Strength == LowText.StrengthType.M);
                        if (txt.Count != 0)
                            Main.Say(txt[Function.Rnd.Next(txt.Count)].TranslateTextConvert(Main));
                    }
                    else
                    {
                        txt = txt.FindAll(x => x.Strength == LowText.StrengthType.S);
                        if (txt.Count != 0)
                            Main.Say(txt[Function.Rnd.Next(txt.Count)].TranslateTextConvert(Main));
                    }
                    Main.DisplayStopForce(() => Main.Display(GraphType.Switch_Hunger, AnimatType.Single, Main.DisplayToNomal));
                    return;
                }
                if (Core.Save.StrengthDrink < sm * 0.60 && Function.Rnd.Next(lowstrengthAskCountDrink--) == 0)
                {
                    lowstrengthAskCountDrink = Set.InteractionCycle;
                    var like = Core.Save.Likability < 40 ? 0 : (Core.Save.Likability < 70 ? 1 : (Core.Save.Likability < 100 ? 2 : 3));
                    var txt = LowDrinkText.FindAll(x => x.Mode == LowText.ModeType.L && (int)x.Like < like);
                    if (Core.Save.StrengthDrink > sm * 0.40)
                    {
                        txt = txt.FindAll(x => x.Strength == LowText.StrengthType.L);
                        if (txt.Count != 0)
                            Main.Say(txt[Function.Rnd.Next(txt.Count)].TranslateTextConvert(Main));
                    }
                    else if (Core.Save.StrengthDrink > sm20)
                    {
                        txt = txt.FindAll(x => x.Strength == LowText.StrengthType.M);
                        if (txt.Count != 0)
                            Main.Say(txt[Function.Rnd.Next(txt.Count)].TranslateTextConvert(Main));
                    }
                    else
                    {
                        txt = txt.FindAll(x => x.Strength == LowText.StrengthType.S);
                        if (txt.Count != 0)
                            Main.Say(txt[Function.Rnd.Next(txt.Count)].TranslateTextConvert(Main));
                    }
                    Main.DisplayStopForce(() => Main.Display(GraphType.Switch_Thirsty, AnimatType.Single, Main.DisplayToNomal));
                    return;
                }
            }


        }
        /// <summary>
        /// Event: use item (triggered by every item use)
        /// </summary>
        public event Action<Food> Event_TakeItem;
        /// <summary>
        /// Event: use item (only called by auto-buy / better-buy) int: count string: source
        /// betterbuy: manual purchase via better-buy
        /// auto*: auto-buy (autofood/autodrink/autofeel) due to hunger/thirst/bad mood
        /// friend: gift from a friend (visitor list)
        /// *: called by other MODs
        /// </summary>
        public event Action<Food, int, string> Event_TakeItemHandle;
        /// <summary>
        /// Raise event Event_TakeItemHandle
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="count">Count</param>
        /// <param name="from">Source</param>
        public void TakeItemHandle(Food item, int count, string from)
        {
            Event_TakeItemHandle?.Invoke(item, count, from);
        }
        /// <summary>
        /// Use/consume item (does not include showing the animation)
        /// </summary>
        /// <param name="item">Item</param>
        public void TakeItem(Food item)
        {
            //Get the tired-of-eating time
            Main.LastInteractionTime = DateTime.Now;
            DateTime now = DateTime.Now;
            DateTime eattime = GameSavesData["buytime"].GetDateTime(item.Name, now);
            double eattimes = 0;
            if (eattime > now)
            {
                eattimes = (eattime - now).TotalHours;
            }
            double eatuseps;
            if (item.Type == FoodType.Gift)
                eatuseps = Math.Max(0.5, 1 - eattimes * eattimes * 0.01);
            else
                eatuseps = Math.Max(0.5, 1 - eattimes * eattimes * 0.02);
            //Start applying stats
            Core.Save.EatFood(item, eatuseps);
            //Tired of eating
            eattimes += Math.Max(0.5, Math.Min(4, 2 - (item.Likability + item.Feeling / 2) / 5));
            GameSavesData["buytime"].SetDateTime(item.Name, now.AddHours(eattimes));
            //Notify
            item.LoadEatTimeSource(this);
            item.NotifyOfPropertyChange("Description");

            ////After eating, remember to recalculate the state
            //Core.Save.Mode = Core.Save.CalMode();
            //Statistics
            GameSavesData.Statistics[(gint)"stat_buytimes"]++;
            GameSavesData.Statistics[(gint)("buy_" + item.Name)]++;
            GameSavesData.Statistics[(gdbe)"stat_betterbuy"] += item.Price;
            switch (item.Type)
            {
                case Food.FoodType.Food:
                    GameSavesData.Statistics[(gdbe)"stat_bb_food"] += item.Price;
                    break;
                case Food.FoodType.Drink:
                    GameSavesData.Statistics[(gdbe)"stat_bb_drink"] += item.Price;
                    break;
                case Food.FoodType.Drug:
                    GameSavesData.Statistics[(gdbe)"stat_bb_drug"] += item.Price;
                    GameSavesData.Statistics[(gdbe)"stat_bb_drug_exp"] += item.Exp;
                    break;
                case Food.FoodType.Snack:
                    GameSavesData.Statistics[(gdbe)"stat_bb_snack"] += item.Price;
                    break;
                case Food.FoodType.Functional:
                    GameSavesData.Statistics[(gdbe)"stat_bb_functional"] += item.Price;
                    break;
                case Food.FoodType.Meal:
                    GameSavesData.Statistics[(gdbe)"stat_bb_meal"] += item.Price;
                    break;
                case Food.FoodType.Gift:
                    GameSavesData.Statistics[(gdbe)"stat_bb_gift"] += item.Price;
                    GameSavesData.Statistics[(gdbe)"stat_bb_gift_like"] += item.Likability;
                    break;
            }

            Event_TakeItem?.Invoke(item);
        }

        public void RunAction(string action)
        {
            switch (action)
            {
                case "DisplayNomal":
                    Main.DisplayNomal();
                    break;
                case "DisplayToNomal":
                    Main.DisplayToNomal();
                    break;
                case "DisplayTouchHead":
                    Main.DisplayTouchHead();
                    break;
                case "DisplayTouchBody":
                    Main.DisplayTouchBody();
                    break;
                case "DisplayIdel":
                    Main.DisplayIdel();
                    break;
                case "DisplayIdel_StateONE":
                    Main.DisplayIdel_StateONE();
                    break;
                case "DisplaySleep":
                    Main.DisplaySleep();
                    break;
                case "DisplayRaised":
                    Main.DisplayRaised();
                    break;
                case "DisplayMove":
                    Main.DisplayMove();
                    break;
            }
        }
        /// <summary>
        /// Steam statistics-related changes
        /// </summary>
        private void Statistics_StatisticChanged(Statistics sender, string name, SetObject value)
        {
            if (name.StartsWith("stat_"))
            {
                SteamUserStats.SetStat(name, (int)value);
            }
        }
        /// <summary>
        /// Calculate statistics data
        /// </summary>
        private void StatisticsCalHandle()
        {
            var stat = GameSavesData.Statistics;
            var save = Core.Save;
            stat["stat_money"] = (SetObject)save.Money;
            stat["stat_level"] = save.Level;
            stat["stat_likability"] = save.Likability;

            stat[(gi64)"stat_total_time"] += (int)Set.LogicInterval;
            switch (Main.State)
            {
                case Main.WorkingState.Work:
                    if (Main.NowWork.Type == Work.WorkType.Work)
                        stat[(gi64)"stat_work_time"] += (int)Set.LogicInterval;
                    else
                        stat[(gi64)"stat_study_time"] += (int)Set.LogicInterval;
                    break;
                case Main.WorkingState.Sleep:
                    stat[(gi64)"stat_sleep_time"] += (int)Set.LogicInterval;
                    break;
            }
            if (save.Mode == IGameSave.ModeType.Ill)
            {
                if (save.Money < 100)
                    stat["stat_ill_nomoney"] = 1;
            }
            if (save.Money < save.Level)
            {
                stat["stat_level_g_money"] = 1;
            }
            if (save.Feeling < 1)
            {
                stat["stat_0_feel"] = 1;
                if (save.StrengthDrink < 1)
                    stat["stat_0_f_sd"] = 1;
            }
            if (save.Strength < 1 && save.Feeling < 1 && save.StrengthFood < 1 && save.StrengthDrink < 1)
                stat["stat_0_all"] = 1;
            if (save.StrengthFood < 1)
                stat["stat_0_strengthfood"] = 1;
            if (save.StrengthDrink < 1)
            {
                stat["stat_0_strengthdrink"] = 1;
                if (save.StrengthFood < 1)
                    stat["stat_0_sd_sf"] = 1;
            }
            var smm = save.StrengthMax - 1;
            if (save.Strength > smm && save.Feeling > save.FeelingMax - 1 && save.StrengthFood > smm && save.StrengthDrink > smm)
                stat[(gint)"stat_100_all"]++;

            if (IsSteamUser)
            {
                Task.Run(SteamUserStats.StoreStats);
            }
        }
        /// <summary>
        /// Load game save
        /// </summary>
        public bool SavesLoad(ILPS lps)
        {
            if (lps == null)
                return false;
            if (string.IsNullOrWhiteSpace(lps.ToString()))
                return false;
            GameSave_v2 tmp;
            if (GameSavesData != null)
                tmp = new GameSave_v2(lps, GameSavesData);
            else
            {
                var data = new LPS_D();
                foreach (var item in Set.PetData_OLD)
                {
                    if (item.Name.Contains("_"))
                    {
                        var strs = Sub.Split(item.Name, "_", 1);
                        data[strs[0]][(gstr)strs[1]] = item.Info;
                    }
                    else
                        data.Add(new Line(item.Name, item.Info));
                }
                tmp = new GameSave_v2(lps, null, olddata: data);
            }
            if (tmp.GameSave == null)
                return false;
            if (tmp.GameSave.Money == 0 && tmp.GameSave.Likability == 0 && tmp.GameSave.Exp == 0
                && tmp.GameSave.StrengthDrink == 0 && tmp.GameSave.StrengthFood == 0)//All data is 0, possibly a bug
                return false;
            if (tmp.GameSave.Exp < -1000000000)
            {
                tmp.GameSave.Exp = 1000000;
                tmp.Data[(gbol)"round"] = true;
                Dispatcher.Invoke(() => NoticeBox.Show("检测到经验值超过 9,223,372,036 导致算数溢出\n已经自动回正".Translate(), "数据溢出警告".Translate()));

            }
            if (tmp.GameSave.Money < -1000000000)
            {
                tmp.GameSave.Money = 100000;
                Dispatcher.Invoke(() => NoticeBox.Show("检测到金钱超过 9,223,372,036 导致算数溢出\n已经自动回正".Translate(), "数据溢出警告".Translate()));
            }

            if (tmp.Data[(gbol)"round"])
            {//Compensate for data overflow based on play time
                Dispatcher.Invoke(() => NoticeBox.Show("您以前遭遇过数据溢出, 已根据游戏时长自动添加进当前数值".Translate(), "数据溢出恢复".Translate()));
                var totalhour = (int)(tmp.Statistics[(gint)"stat_total_time"] / 3600);//Total play time / hours
                if (totalhour < 500)
                {
                    tmp.GameSave.Exp += totalhour * 200;
                }
                else
                {
                    double lm = Math.Sqrt(totalhour / 500);
                    tmp.GameSave.LevelMax += (int)lm;
                    tmp.GameSave.Exp += (totalhour % 500 + (lm - (int)lm) * 500) * 200;

                }
                tmp.GameSave.LikabilityMax += totalhour / 10;
                tmp.Data[(gbol)"round"] = false;
            }
            GameSavesData = tmp;
            Core.Save = tmp.GameSave;
            HashCheck = HashCheck;
            GameSavesData.GameSave.Event_LevelUp += LevelUP;
            return true;
        }



        private void Handle_Steam(Main obj)
        {
            string jointab = " ";
            if (winMutiPlayer != null)
            {
                if (winMutiPlayer.Joinable)
                    jointab += "可加入".Translate();
                SteamFriends.SetRichPresence("steam_player_group", winMutiPlayer.LobbyID.ToString("x"));
                SteamFriends.SetRichPresence("steam_player_group_size", winMutiPlayer.lb.MemberCount.ToString());
            }
            else
            {
                SteamFriends.SetRichPresence("steam_player_group_size", "0");
            }
            if (App.MainWindows.Count > 1)
            {
                if (App.MainWindows.FirstOrDefault() != this)
                {
                    return;
                }
                string str = "";
                int lv = 0;
                int workcount = 0;
                int sleepcount = 0;
                int musiccount = 0;
                int allcount = App.MainWindows.Count * 2 / 3;
                foreach (var item in App.MainWindows)
                {
                    if (item.GameSavesData == null || item.Main == null)
                        continue;
                    str += item.GameSavesData.GameSave.Name + ",";
                    if (item.HashCheck)
                    {
                        lv += item.GameSavesData.GameSave.Level;
                    }
                    else
                        lv = int.MinValue;
                    switch (item.Main.State)
                    {
                        case Main.WorkingState.Work:
                            workcount++;
                            break;
                        case Main.WorkingState.Sleep:
                            sleepcount++;
                            break;
                        case Main.WorkingState.Nomal:
                            if (item.Main.DisplayType.Name == "music")
                                musiccount++;
                            break;
                    }
                }
                SteamFriends.SetRichPresence("usernames", str.Trim(','));
                if (lv > 0)
                {
                    SteamFriends.SetRichPresence("lv", $" (lv{lv}/{App.MainWindows.Count})" + jointab);
                }
                else
                {
                    SteamFriends.SetRichPresence("lv", " " + jointab);
                }
                if (workcount > allcount)
                {
                    SteamFriends.SetRichPresence("steam_display", "#Status_MUTI_Work");
                }
                else if (sleepcount > allcount)
                {
                    SteamFriends.SetRichPresence("steam_display", "#Status_MUTI_Sleep");
                }
                else if (musiccount > allcount)
                {
                    SteamFriends.SetRichPresence("steam_display", "#Status_MUTI_Music");
                }
                else
                {
                    SteamFriends.SetRichPresence("steam_display", "#Status_MUTI_Play");
                }
            }
            else
            {
                if (HashCheck)
                {
                    SteamFriends.SetRichPresence("lv", $" (lv{GameSavesData.GameSave.Level})" + jointab);
                }
                else
                {
                    SteamFriends.SetRichPresence("lv", " " + jointab);
                }
                if (Core.Save.Mode == IGameSave.ModeType.Ill)
                {
                    SteamFriends.SetRichPresence("steam_display", "#Status_Ill");
                }
                else
                {
                    SteamFriends.SetRichPresence("mode", (Core.Save.Mode.ToString() + "ly").Translate());
                    switch (obj.State)
                    {
                        case Main.WorkingState.Work:
                            SteamFriends.SetRichPresence("work", obj.NowWork.NameTrans);
                            SteamFriends.SetRichPresence("steam_display", "#Status_Work");
                            break;
                        case Main.WorkingState.Sleep:
                            SteamFriends.SetRichPresence("steam_display", "#Status_Sleep");
                            break;
                        default:
                            if (obj.DisplayType.Name == "music")
                                SteamFriends.SetRichPresence("steam_display", "#Status_Music");
                            else
                            {
                                switch (obj.DisplayType.Type)
                                {
                                    case GraphType.Move:
                                        SteamFriends.SetRichPresence("idel", "乱爬".Translate());
                                        break;
                                    case GraphType.Idel:
                                    case GraphType.StateONE:
                                    case GraphType.StateTWO:
                                        SteamFriends.SetRichPresence("idel", "发呆".Translate());
                                        break;
                                    default:
                                        SteamFriends.SetRichPresence("idel", "闲逛".Translate());
                                        break;
                                }
                                SteamFriends.SetRichPresence("steam_display", "#Status_IDLE");
                            }
                            break;
                    }
                }
            }
        }
        private bool? AudioPlayingVolumeOK = null;
        /// <summary>
        /// Get the current system music playback volume
        /// </summary>
        public float AudioPlayingVolume()
        {
            if (AudioPlayingVolumeOK == null)
            {//First call checks whether it is supported
                try
                {//Subsequent error tolerance may be intermittent
                    using (var enumerator = new MMDeviceEnumerator())
                    {
                        if (enumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Console))
                        {
                            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                            AudioPlayingVolumeOK = true;
                            return device.AudioMeterInformation.MasterPeakValue;
                        }
                        else
                        {
                            AudioPlayingVolumeOK = false;
                            return -1;
                        }
                    }
                }
                catch
                {
                    AudioPlayingVolumeOK = false;
                    return -1;
                }
            }
            else if (AudioPlayingVolumeOK == false)
            {
                return -1;
            }
            try
            {//Subsequent error tolerance may be intermittent
                using (var enumerator = new MMDeviceEnumerator())
                {
                    if (enumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Console))
                    {
                        var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                        return device.AudioMeterInformation.MasterPeakValue;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            catch
            {
                return -1;
            }
        }
        /// <summary>
        /// Music detector
        /// </summary>
        private void Handle_Music(Main obj)
        {
            if (MusicTimer.Enabled == false && Core.Graph.FindGraphs("music", AnimatType.B_Loop, Core.Save.Mode) != null &&
                Main.IsIdel && AudioPlayingVolume() > Set.MusicCatch)
            {
                catch_MusicVolSum = 0;
                catch_MusicVolCount = 0;
                CurrMusicType = null;
                MusicTimer.Start();
                Task.Run(() =>
                {//Wait 3 seconds to see the recognition result
                    Thread.Sleep(3000);

                    if (CurrMusicType != null && Main.IsIdel)
                    {//Recognition passed, start the dancing animation
                        //Record statistics first
                        GameSavesData.Statistics[(gint)"stat_music"]++;
                        Main.Display(Core.Graph.FindGraph("music", AnimatType.A_Start, Core.Save.Mode), Display_Music);
                    }
                    else
                    { //Failed or something is blocking, stop detection
                        MusicTimer.Stop();
                    }
                });
            }
        }
        private void Display_Music()
        {
            if (CurrMusicType.HasValue)
            {
                if (CurrMusicType.Value)
                {//Play the more intense one
                    var mg = Core.Graph.FindGraph("music", AnimatType.Single, Core.Save.Mode);
                    mg ??= Core.Graph.FindGraph("music", AnimatType.B_Loop, Core.Save.Mode);
                    Main.Display(mg, Display_Music);
                }
                else
                {
                    Main.Display(Core.Graph.FindGraph("music", AnimatType.B_Loop, Core.Save.Mode), Display_Music);
                }
            }
            else
            {
                Main.Display("music", AnimatType.C_End, Main.DisplayToNomal);
            }
        }
        private void MusicTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!(Main.IsIdel || Main.DisplayType.Name == "music"))//Not music, interrupted
                return;
            catch_MusicVolSum += AudioPlayingVolume();
            catch_MusicVolCount++;
            if (catch_MusicVolCount >= 10)
            {
                double ans = catch_MusicVolSum / catch_MusicVolCount;
                catch_MusicVolSum /= 4;
                catch_MusicVolCount /= 4;
                if (ans > Set.MusicCatch)
                {
                    var bef = CurrMusicType;
                    CurrMusicType = ans > Set.MusicMax;
                    if (bef != null && bef != CurrMusicType)
                        Display_Music();
                    MusicTimer.Start();
                }
                else
                {
                    CurrMusicType = null;
                    if (Main.DisplayType.Name == "music")
                        Main.Display("music", AnimatType.C_End, Main.DisplayToNomal);
                }
            }
            else
            {
                MusicTimer.Start();
            }
        }

        public Timer MusicTimer;
        private double catch_MusicVolSum;
        private int catch_MusicVolCount;
        /// <summary>
        /// Current music playback state
        /// </summary>
        public bool? CurrMusicType { get; private set; }

        int LastDiagnosisTime = 0;

        /// <summary>
        /// Upload telemetry file
        /// </summary>
        public void DiagnosisUPLoad()
        {
            // AIDeskPet: Telemetry fully removed; no save/settings/SteamID uploaded to any server
            return;
        }
        /// <summary>
        /// Close indicator, defaults to true
        /// </summary>
        public bool CloseConfirm { get; private set; } = true;

        public List<ITalkAPI> TalkAPI { get; } = new List<ITalkAPI>();
        /// <summary>
        /// Index of the currently selected talk box
        /// </summary>
        public int TalkAPIIndex = -1;
        /// <summary>
        /// Current talk box
        /// </summary>
        public ITalkAPI TalkBoxCurr
        {
            get
            {
                if (TalkAPIIndex == -1)
                    return null;
                return TalkAPI[TalkAPIIndex];
            }
        }

        Grid IMainWindow.MGHost => MGHost;

        Grid IMainWindow.PetGrid => MGrid;
        internal MWController MWController { get; set; }
        /// <summary>
        /// Remove all chat talk boxes
        /// </summary>
        public void RemoveTalkBox()
        {
            if (TalkBox != null)
            {
                Main.ToolBar.MainGrid.Children.Remove(TalkBox);
                TalkBox = null;
            }
            if (TalkAPIIndex == -1)
                return;
            Main.ToolBar.MainGrid.Children.Remove(TalkAPI[TalkAPIIndex].This);
        }
        /// <summary>
        /// Load custom talk box
        /// </summary>
        public void LoadTalkDIY()
        {
            RemoveTalkBox();
            if (TalkAPIIndex == -1)
                return;
            Main.ToolBar.MainGrid.Children.Add(TalkAPI[TalkAPIIndex].This);
        }
        /// <summary>
        /// Overloaded work check
        /// </summary>
        public bool WorkCheck(Work work)
        {
            //Check whether it is overloaded
            if (HashCheck && work.IsOverLoad())
            {
                if (Set["gameconfig"].GetBool("noAutoCal"))
                {
                    if (MessageBoxX.Show("当前工作数据属性超模,是否继续工作?\n超模工作可能会导致游戏发生不可预料的错误\n超模工作不影响大部分成就解锁\n可以在设置中开启自动计算自动为工作设置合理数值"
                        .Translate(), "超模工作提醒".Translate(), MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        return false;
                    }
                    HashCheck = false;
                }
                else
                {
                    MessageBoxX.Show("当前工作数据属性超模,已自动取消".Translate(), "超模工作提醒".Translate());
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Game loading
        /// </summary>
        public void GameInitialization()
        {
            Console.SetOut(new ConsoleRedirectWriter(ActivityLogs));

            Console.WriteLine($"VPet Simulator {version}");

            App.MainWindows.Add(this);
            try
            {
                //Load game settings
                if (new FileInfo(ExtensionValue.BaseDirectory + @$"\Setting{PrefixSave}.lps").Exists)
                {
                    Set = new Setting(this, File.ReadAllText(ExtensionValue.BaseDirectory + @$"\Setting{PrefixSave}.lps"));
                }
                if (PrefixSave == "" && (Set == null || (Set != null && !Set["SingleTips"].GetBool("helloworld"))) && File.Exists(ExtensionValue.BaseDirectory + @"\Setting.bkp"))
                {//If settings are corrupted, read the backup settings
                    Set = new Setting(this, File.ReadAllText(ExtensionValue.BaseDirectory + @"\Setting.bkp"));
                }

                Set ??= new Setting(this, "Setting#VPET:|\n");

                var visualTree = new FrameworkElementFactory(typeof(Border));
                visualTree.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(BackgroundProperty));
                var childVisualTree = new FrameworkElementFactory(typeof(ContentPresenter));
                childVisualTree.SetValue(ClipToBoundsProperty, true);
                visualTree.AppendChild(childVisualTree);

                Template = new ControlTemplate
                {
                    TargetType = typeof(Window),
                    VisualTree = visualTree,
                };

                InitializeComponent();

                //MGrid.Height = 500 * Set.ZoomLevel;
                MGrid.Width = 500 * Set.ZoomLevel;
                if (Set.OpacityMain)
                    this.Opacity = Set.Opacity;

                double L = 0, T = 0;
                if (Set.StartRecordLast)
                {
                    var point = Set.StartRecordLastPoint;
                    if (point.X != 0 || point.Y != 0)
                    {
                        L = point.X;
                        T = point.Y;
                    }
                }
                else
                {
                    var point = Set.StartRecordPoint;
                    L = point.X; T = point.Y;
                }

                Left = L;
                Top = T;

                // control position inside bounds
                MWController = new MWController(this);
                Core.Controller = MWController;
                Task.Run(() =>
                {
                    double dist;
                    if ((dist = Core.Controller.GetWindowsDistanceLeft()) < 0)
                    {
                        Thread.Sleep(100);
                        Dispatcher.Invoke(() => Left -= dist);
                    }
                    if ((dist = Core.Controller.GetWindowsDistanceRight()) < 0)
                    {
                        Thread.Sleep(100);
                        Dispatcher.Invoke(() => Left += dist);
                    }
                    if ((dist = Core.Controller.GetWindowsDistanceUp()) < 0)
                    {
                        Thread.Sleep(100);
                        Dispatcher.Invoke(() => Top -= dist);
                    }
                    if ((dist = Core.Controller.GetWindowsDistanceDown()) < 0)
                    {
                        Thread.Sleep(100);
                        Dispatcher.Invoke(() => Top += dist);
                    }
                });
                if (Set.TopMost)
                {
                    Topmost = true;
                }

                //Close if it does not exist
                var modpath = new DirectoryInfo(ModPath + @"\0000_core\pet\vup");
                if (!modpath.Exists)
                {
                    MessageBoxX.Show("Missing module Core, cannot start.", "启动错误 boot error", Panuon.WPF.UI.MessageBoxIcon.Error);
                    Close();
                    return;
                }

            }
            catch (Exception e)
            {
                string errstr = "游戏发生错误,可能是".Translate() + (string.IsNullOrWhiteSpace(CoreMOD.NowLoading) ?
              "游戏或者MOD".Translate() : $"MOD({CoreMOD.NowLoading})") +
              "导致的\n请记录 错误信息截图和引发错误之前的操作 以便排查\n".Translate()
              + e.ToString();
                MessageBoxX.Show(errstr, "游戏致命性错误".Translate() + ' ' + "启动错误".Translate(), Panuon.WPF.UI.MessageBoxIcon.Error);
                Close();
            }
        }

        /// <summary>
        /// Startup method that supports multiple instances
        /// </summary>
        /// <param name="prefixsave">Save prefix</param>
        /// <param name="basemw">Base window</param>
        public MainWindow(string prefixsave, MainWindow basemw = null)
        {
            PrefixSave = prefixsave;
            if (prefixsave != string.Empty && !PrefixSave.StartsWith("-"))
                PrefixSave = '-' + prefixsave;

            IsSteamUser = App.MainWindows[0].IsSteamUser;

            //Process ARGS
            Args = new LPS_D();
            foreach (var str in App.Args)
            {
                Args.Add(new Line(str));
            }
            _dwmEnabled = Win32.Dwmapi.DwmIsCompositionEnabled();
            _hwnd = new WindowInteropHelper(this).EnsureHandle();

            GameInitialization();

            if (basemw != null)
            {
                Set["workshop"] = basemw.Set["workshop"];
                Set.Resolution = basemw.Set.Resolution;
            }


            //Load all MODs
            List<DirectoryInfo> Path = new List<DirectoryInfo>();
            Path.AddRange(new DirectoryInfo(ModPath).EnumerateDirectories());

            var workshop = Set["workshop"];
            foreach (ISub ws in workshop)
            {
                Path.Add(new DirectoryInfo(ws.Name));
            }


            Task.Run(() => GameLoad(Path));
        }
        /// <summary>
        /// MOD paths
        /// </summary>
        public List<DirectoryInfo> MODPath { get; private set; }

        public IEnumerable<IModInfo> ModInfo => CoreMODs;

        public IEnumerable<IModInfo> OnModInfo => CoreMODs.FindAll(x => x.IsOnMOD(this));

        /// <summary>
        /// Load game
        /// </summary>
        /// <param name="Path">MOD paths</param>
        public async Task GameLoad(List<DirectoryInfo> Path)
        {
            MODPath = Path.GroupBy(x => x.FullName).Select(group => group.First()).ToList();
            await Dispatcher.InvokeAsync(new Action(() => LoadingText.Content = "Loading MOD"));
            //Load mods
            foreach (DirectoryInfo di in MODPath)
            {
                if (!File.Exists(di.FullName + @"\info.lps"))
                    continue;
                await Dispatcher.InvokeAsync(new Action(() => LoadingText.Content = $"Loading MOD: {di.Name}"));
                CoreMODs.Add(new CoreMOD(di, this));
            }

            CoreMOD.NowLoading = null;

            //Determine whether the cache needs to be cleared
            if (App.MainWindows.Count == 1 && Set.LastCacheDate < CoreMODs.Max(x => x.CacheDate))
            {//Cache needs to be cleared
                Set.LastCacheDate = DateTime.Now;
                if (Directory.Exists(GraphCore.CachePath))
                {
                    Directory.Delete(GraphCore.CachePath, true);
                    Directory.CreateDirectory(GraphCore.CachePath);
                }
            }



            await Dispatcher.InvokeAsync(() =>
            {
                MessageBoxXSettings.Setting.OKButtonContent = "好的".Translate();
                MessageBoxXSettings.Setting.CancelButtonContent = "取消".Translate();
                MessageBoxXSettings.Setting.YesButtonContent = "是".Translate();
                MessageBoxXSettings.Setting.NoButtonContent = "否".Translate();
                PendingBoxSettings.Setting.CancelButtonContent = "取消".Translate();
                LoadingText.Content = "尝试加载游戏MOD".Translate();
            });

            //AIDeskPet: Legacy graphic-name compatibility, unified to point to aigirl
            if (Set.PetGraph == "默认虚拟桌宠" || Set.PetGraph == "vup")
                Set.PetGraph = "aigirl";

            //Current pet animation
            var petloader = Pets.Find(x => x.Name == Set.PetGraph);
            petloader ??= Pets[0];
            //Remove other-language content
            var tag = petloader.Config.Data.GetString("tag", "all").Split(',');
            LowDrinkText.RemoveAll(x => !x.FindTag(tag));
            LowFoodText.RemoveAll(x => !x.FindTag(tag));
            ClickTexts.RemoveAll(x => !x.FindTag(tag));
            SelectTexts.RemoveAll(x => !x.FindTag(tag));

            await Dispatcher.InvokeAsync(new Action(() => LoadingText.Content = "尝试加载游戏存档".Translate()));
            //Load save
            if (File.Exists(ExtensionValue.BaseDirectory + @"\Save.lps")) //An old save exists, prefer the old save
                try
                {
                    if (!SavesLoad(new LpsDocument(File.ReadAllText(ExtensionValue.BaseDirectory + @"\Save.lps"))))
                    {
                        //If loading the save failed, try loading the backup; if none, create a new one
                        LoadLatestSave(petloader.PetName);
                    }

                }
                catch (Exception ex)
                {
                    MessageBoxX.Show("Save file is corrupted and cannot be loaded.\n" + ex.Message, "存档损毁".Translate());
                    //If loading the save failed, try loading the backup; if none, create a new one
                    LoadLatestSave(petloader.PetName);
                }
            else
                //If loading the save failed, try loading the backup; if none, create a new one
                LoadLatestSave(petloader.PetName);

            //Load data normalization: food
            if (!Set["gameconfig"].GetBool("noAutoCal"))
            {
                foreach (Food f in Foods)
                {
                    if (f.IsOverLoad())
                    {
                        f.Price = Math.Max((int)f.RealPrice, 1);
                        f.isoverload = false;
                    }
                }
                //var food = new Food();
                foreach (var selet in SelectTexts)
                {
                    selet.Exp = Math.Max(Math.Min(selet.Exp, 1000), -1000);
                    selet.Feeling = Math.Max(Math.Min(selet.Feeling, 100), -100);
                    selet.Health = Math.Max(Math.Min(selet.Health, 100), -100);
                    selet.Likability = Math.Max(Math.Min(selet.Likability, 50), -50);
                    selet.Money = Math.Max(Math.Min(selet.Money, 1000), -1000);
                    selet.Strength = Math.Max(Math.Min(selet.Strength, 1000), -1000);
                    selet.StrengthDrink = Math.Max(Math.Min(selet.StrengthDrink, 1000), -1000);
                    selet.StrengthFood = Math.Max(Math.Min(selet.StrengthFood, 1000), -1000);
                }
                foreach (var selet in ClickTexts)
                {
                    selet.Exp = Math.Max(Math.Min(selet.Exp, 1000), -1000);
                    selet.Feeling = Math.Max(Math.Min(selet.Feeling, 1000), -1000);
                    selet.Health = Math.Max(Math.Min(selet.Health, 100), -100);
                    selet.Likability = Math.Max(Math.Min(selet.Likability, 50), -50);
                    selet.Money = Math.Max(Math.Min(selet.Money, 1000), -1000);
                    selet.Strength = Math.Max(Math.Min(selet.Strength, 1000), -1000);
                    selet.StrengthDrink = Math.Max(Math.Min(selet.StrengthDrink, 1000), -1000);
                    selet.StrengthFood = Math.Max(Math.Min(selet.StrengthFood, 1000), -1000);
                }
            }

            //Birthday cake defaults to fully filled
            var food = new Food()
            {
                Name = "生日蛋糕",
                Likability = 5,
                Exp = 1000,
                Feeling = 100,
                StrengthDrink = Core.Save.StrengthMax,
                StrengthFood = Core.Save.StrengthMax,
                Type = FoodType.Food,
                isoverload = false,
                Desc = "A special birthday cake."
            };
            food.LoadImageSource(this);
            food.Star = true;
            food.Price = (int)Math.Max(0, food.RealPrice * .5);
            Foods.Add(food);
            //Birthday cake defaults to fully filled
            food = new Food()
            {
                Name = "生日蛋糕2",//2nd surprise birthday cake
                Likability = Core.Save.Level / 10,
                Exp = Core.Save.Level,
                Feeling = Core.Save.FeelingMax / 20,
                StrengthDrink = Core.Save.StrengthMax / 20,
                StrengthFood = Core.Save.StrengthMax / 20,
                Type = FoodType.Food,
                isoverload = false,
                Desc = "A surprise cake. Each bite randomly refills a stat or grants a bonus, plus a mystery reward!"
            };
            food.LoadImageSource(this);
            food.Star = true;
            food.Price = food.RealPrice;
            Foods.Add(food);

            //First launch date
            if (GameSavesData.Data.FindLine("birthday") == null)
            {
                var sf = new FileInfo(ExtensionValue.BaseDirectory + @$"\Setting{PrefixSave}.lps");
                if (sf.Exists)
                {
                    GameSavesData[(gdat)"birthday"] = sf.CreationTime.Date;
                }
                else
                    GameSavesData[(gdat)"birthday"] = DateTime.Now.Date;
            }

            //Fill in supplementary data
            if (string.IsNullOrEmpty(GameSavesData.GameSave.HostName))
            {
                if (IsSteamUser)
                    GameSavesData.GameSave.HostName = SteamClient.Name;
                else
                    GameSavesData.GameSave.HostName = Environment.UserName;
            }

            //if (GameSavesData.Data.FindLine("HostBDay") == null)
            //{
            //    GameSavesData[(gdat)"HostBDay"] = GameSavesData[(gdat)"birthday"];
            //}


            AutoSaveTimer.Elapsed += AutoSaveTimer_Elapsed;

            if (GameSavesData.Statistics[(gdbe)"stat_bb_food"] < 0 || GameSavesData.Statistics[(gdbe)"stat_bb_drink"] < 0 || GameSavesData.Statistics[(gdbe)"stat_bb_drug"] < 0
                || GameSavesData.Statistics[(gdbe)"stat_bb_snack"] < 0 || GameSavesData.Statistics[(gdbe)"stat_bb_functional"] < 0 || GameSavesData.Statistics[(gdbe)"stat_bb_meal"] < 0
                || GameSavesData.Statistics[(gdbe)"stat_bb_gift"] < 0)
            {
                HashCheck = false;
            }

            if (Set.AutoSaveInterval > 0)
            {
                AutoSaveTimer.Interval = Set.AutoSaveInterval * 60000;
                AutoSaveTimer.Start();
            }
            ClickTexts.Add(new ClickText("Right-click me to open the menu."));
            ClickTexts.Add(new ClickText("You can change the display scale in the settings."));
            ClickTexts.Add(new ClickText("Don't want me wandering around? Set smart movement or turn movement off in the settings."));
            ClickTexts.Add(new ClickText("Thanks for spending time with me today."));
            //ClickTexts.Add(new ClickText("有建议/游玩反馈? 来 菜单-系统-反馈中心 反馈吧"));
            ClickTexts.Add(new ClickText("Press and hold my head to drag me anywhere you like."));

            ////Temporary chat content
            //ClickTexts.Add(new ClickText("主人，sbema秋季促销开始了哦，还有游戏大奖赛，快去给{name}去投一票吧。"));
            //ClickTexts.Add(new ClickText("主人主人，{name}参加了sbeam大奖赛哦，给人家投一票喵"));
            //ClickTexts.Add(new ClickText("那个。。主人。。\n人家参加了sbeam大奖赛哦。能不能。。给{name}投一票呢～"));
            //ClickTexts.Add(new ClickText("电脑里有一款《虚拟桌宠模拟器》的游戏正在参加2023的sbeam大奖赛，快来给桌宠投一票吧"));
            //"如果你觉得目前功能太少,那就多挂会机. 宠物会自己动的".Translate(),
            //"你知道吗? 你可以在设置里面修改游戏的缩放比例".Translate(),
            //"你现在乱点说话是说话系统的一部分,不过还没做,在做了在做了ing".Translate(),
            //"你添加了虚拟主播模拟器和虚拟桌宠模拟器到愿望单了吗? 快去加吧".Translate(),
            //"这游戏开发这么慢,都怪画师太咕了".Translate(),
            //"欢迎加入 虚拟主播模拟器群 430081239".Translate()

            await Dispatcher.InvokeAsync(new Action(() => LoadingText.Content = "尝试加载Steam内容".Translate()));
            //A small feature for streamers / video creators playing this game
            if (IsSteamUser)
            {
                ClickTexts.Add(new ClickText("关注 {0} 谢谢喵")
                {
                    TranslateText = "关注 {0} 谢谢喵".Translate(SteamClient.Name)
                });
                //Steam achievements
                GameSavesData.Statistics.StatisticChanged += Statistics_StatisticChanged;
                //Steam notifications
                SteamFriends.SetRichPresence("username", Core.Save.Name);
                SteamFriends.SetRichPresence("mode", (Core.Save.Mode.ToString() + "ly").Translate());
                SteamFriends.SetRichPresence("steam_display", "#Status_IDLE");
                SteamFriends.SetRichPresence("idel", "闲逛".Translate());
                if (HashCheck)
                {
                    SteamFriends.SetRichPresence("lv", $" (lv{GameSavesData.GameSave.Level})");
                }
                else
                {
                    SteamFriends.SetRichPresence("lv", " ");
                }
            }
            else
            {
                ClickTexts.Add(new ClickText("关注 {0} 谢谢喵")
                {
                    TranslateText = "关注 {0} 谢谢喵".Translate(Environment.UserName)
                });
            }

            //Load the music-recognition timer
            MusicTimer = new System.Timers.Timer(200)
            {
                AutoReset = false
            };
            MusicTimer.Elapsed += MusicTimer_Elapsed;


            //await Dispatcher.InvokeAsync(new Action(() => LoadingText.Content = "尝试加载游戏动画".Translate()));
            await Dispatcher.InvokeAsync(new Action(() => LoadingText.Content = "尝试加载动画和生成缓存\n该步骤可能会耗时比较长\n请耐心等待".Translate()));
            Core.Graph = petloader.Graph(Set.Resolution, Dispatcher);

            Main = await Dispatcher.InvokeAsync(() => new Main(Core));

            Main.LoadALL((c) =>
            {
                Dispatcher.Invoke(() => LoadingText.Content = "尝试加载动画和生成缓存\n该步骤可能会耗时比较长\n请耐心等待".Translate()
                + $"\n  {c} / {petloader.GraphCount}");
            }
            //#if NewYear
            //            , Core.Graph.FindGraph("newyear", AnimatType.Single, Core.Save.Mode)
            //#endif
            );
            Main.NoFunctionMOD = Set.CalFunState;
            await Dispatcher.InvokeAsync(() =>
              {
                  //Clear resources
                  Main.Resources = Application.Current.Resources;
                  Main.MsgBar.This.Resources = Application.Current.Resources;
                  Main.ToolBar.Resources = Application.Current.Resources;
                  Main.ToolBar.LoadClean();
                  Main.WorkList(out List<Work> ws, out List<Work> ss, out List<Work> ps);

                  //Load schedule
                  ScheduleTask = new ScheduleTask(this);

                  if (ws.Count == 0)
                  {
                      Main.ToolBar.MenuWork.Visibility = Visibility.Collapsed;
                  }
                  else
                  {
                      Main.ToolBar.MenuWork.MouseDoubleClick += (x, y) =>
                      {
                          Main.ToolBar.Visibility = Visibility.Collapsed;
                          ShowWorkMenu(Work.WorkType.Work);
                      };
                      Main.ToolBar.MenuWork.Click += (x, y) =>
                      {
                          Main.ToolBar.Visibility = Visibility.Collapsed;
                          if (Main.ToolBar.MenuWork.Items.Count == 0)
                              ShowWorkMenu(Work.WorkType.Work);
                      };
                  }
                  // AIDeskPet: Study feature removed
                  Main.ToolBar.MenuStudy.Visibility = Visibility.Collapsed;
                  if (ps.Count == 0)
                  {
                      Main.ToolBar.MenuPlay.Visibility = Visibility.Collapsed;
                  }
                  else
                  {
                      Main.ToolBar.MenuPlay.MouseDoubleClick += (x, y) =>
                      {
                          Main.ToolBar.Visibility = Visibility.Collapsed;
                          ShowWorkMenu(Work.WorkType.Play);
                      };
                      Main.ToolBar.MenuPlay.Click += (x, y) =>
                      {
                          Main.ToolBar.Visibility = Visibility.Collapsed;
                          if (Main.ToolBar.MenuPlay.Items.Count == 0) ShowWorkMenu(Work.WorkType.Play);
                      };
                  }
                  WorkStarMenu = new System.Windows.Controls.MenuItem()
                  {
                      Header = "收藏".Translate(),
                      HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                  };
                  foreach (var w in WorkStar())
                  {
                      var mi = new System.Windows.Controls.MenuItem()
                      {
                          Header = w.NameTrans
                      };
                      mi.Click += (s, e) => Main.ToolBar.StartWork(w.Double(Set["workmenu"].GetInt("double_" + w.Name, 1)));
                      WorkStarMenu.Items.Add(mi);
                  }
                  Main.ToolBar.MenuInteract.Items.Add(WorkStarMenu);

                  //Load theme:
                  LoadTheme(Set.Theme);
                  //Load font
                  LoadFont(Set.Font);

                  LoadingText.Content = "正在加载游戏\n该步骤可能会耗时比较长\n请耐心等待".Translate();


                  //Load data normalization: work
                  if (!Set["gameconfig"].GetBool("noAutoCal"))
                  {
                      foreach (var work in Core.Graph.GraphConfig.Works)
                      {
                          if (work.LevelLimit > 200)//Imported max reasonable work level cannot exceed 200
                              work.LevelLimit = 200;
                          work.FixOverLoad();//Imported work defaults to 1.2x
                      }
                  }
                  //Load data normalization: auto-work
                  foreach (var stp in SchedulePackage)
                      stp.FixOverLoad();


                  var m = new System.Windows.Controls.MenuItem()
                  {
                      Header = "MOD管理".Translate(),
                      HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
                  };
                  m.Click += (x, y) =>
                  {
                      Main.ToolBar.Visibility = Visibility.Collapsed;
                      winSetting.MainTab.SelectedIndex = 5;
                      winSetting.Show();
                  };
                  Main.FunctionSpendHandle += lowStrength;
                  Main.WorkTimer.E_FinishWork += WorkTimer_E_FinishWork;
                  Main.ToolBar.MenuMODConfig.Items.Add(m);

                  //Load game Workshop plugins
                  foreach (MainPlugin mp in Plugins)
                      try //Don't remove try for DEBUG; on the main thread errors won't be shown either
                      {
                          mp.LoadPlugin();
                      }
                      catch (Exception e)
                      {
                          NoticeBox.Show("由于插件引起的游戏启动错误".Translate() + "\n" + e.ToString(), "由于插件引起的游戏启动错误".Translate() + '-' + mp.PluginName);
                      }
                  Foods.ForEach(item => item.LoadImageSource(this));
                  Photos.ForEach(item => item.LoadUserInfo(this));

                  //Load inventory
                  foreach (var line in GameSavesData.Data.Assemblage.Where(x => x.Key.StartsWith("item")))
                  {
                      var itm = Item.CreateItem(this, line.Value);
                      itm.LoadSource(this);
                      ItemsAdd(itm);
                  }

                  // AIDeskPet: Removed vup-exclusive legacy items (money/toy system, original IP easter eggs)
                  //Daily gift box
                  everydaygift();



                  Main.TimeHandle += Handle_Music;
                  if (IsSteamUser)
                      Main.TimeHandle += Handle_Steam;
                  // AIDeskPet: Disabled reporting diagnostic data to the original dev team's server


                  // AIDeskPet: Level/money panel display removed


                  switch (Set["CGPT"][(gstr)"type"])
                  {
                      case "DIY":
                          TalkAPIIndex = TalkAPI.FindIndex(x => x.APIName == Set["CGPT"][(gstr)"DIY"]);
                          LoadTalkDIY();
                          break;
                      //case "API":
                      //    TalkBox = new TalkBoxAPI(this);
                      //    Main.ToolBar.MainGrid.Children.Add(TalkBox);
                      //    break;
                      case "LB":
                          //if (IsSteamUser)
                          //{
                          //    TalkBox = new TalkSelect(this);
                          //    Main.ToolBar.MainGrid.Children.Add(TalkBox);
                          //}
                          TalkBox = new TalkSelect(this);
                          Main.ToolBar.MainGrid.Children.Add(TalkBox);
                          break;
                  }

                  //Window widgets
                  winSetting = new winGameSetting(this);
                  winBetterBuy = new winBetterBuy(this);

                  Main.DefaultClickAction = () =>
                  {
                      if (new TimeSpan(DateTime.Now.Ticks - lastclicktime).TotalSeconds > 20)
                      {
                          lastclicktime = DateTime.Now.Ticks;
                          var rt = GetClickText();
                          if (rt != null)
                          {
                              //Chat effects
                              if (rt.Exp != 0)
                              {
                                  if (rt.Exp > 0)
                                  {
                                      GameSavesData.Statistics[(gint)"stat_say_exp_p"]++;
                                  }
                                  else
                                      GameSavesData.Statistics[(gint)"stat_say_exp_d"]++;
                              }
                              if (rt.Likability != 0)
                              {
                                  if (rt.Likability > 0)
                                      GameSavesData.Statistics[(gint)"stat_say_like_p"]++;
                                  else
                                      GameSavesData.Statistics[(gint)"stat_say_like_d"]++;
                              }
                              if (rt.Money != 0)
                              {
                                  if (rt.Money > 0)
                                      GameSavesData.Statistics[(gint)"stat_say_money_p"]++;
                                  else
                                      GameSavesData.Statistics[(gint)"stat_say_money_d"]++;
                              }
                              Main.Core.Save.EatFood(rt);
                              Main.Core.Save.Money += rt.Money;
                              Main.SayRnd(rt.TranslateTextConvert(Main), desc: rt.FoodToDescription());
                          }
                      }
                  };
                  Main.PlayVoiceVolume = Set.VoiceVolume;
                  Main.FunctionSpendHandle += StatisticsCalHandle;
                  DisplayGrid.Child = Main;
                  Task.Run(async () =>
                  {
                      while (!Main.IsWorking)
                      {
                          Thread.Sleep(100);
                      }
                      await Dispatcher.InvokeAsync(async () =>
                      {
                          while (LoadingText.Visibility != Visibility.Collapsed)
                          {
                              LoadingText.Visibility = Visibility.Collapsed;
                              await Task.Delay(1000);
                          }
                      });
                  });

                  Main.ToolBar.AddMenuButton(ToolBar.MenuType.Setting, "退出桌宠".Translate(), () => { Main.ToolBar.Visibility = Visibility.Collapsed; Close(); });
                  if (Set.DeBug)
                      Main.ToolBar.AddMenuButton(ToolBar.MenuType.Setting, "开发控制台".Translate(), () => { Main.ToolBar.Visibility = Visibility.Collapsed; new winConsole(this).Show(); });
                  // AIDeskPet: Photo gallery feature removed
                  // AIDeskPet: Tutorial / feedback center (original team service) removed
                  Main.ToolBar.AddMenuButton(ToolBar.MenuType.Setting, "设置面板".Translate(), () =>
                  {
                      Main.ToolBar.Visibility = Visibility.Collapsed;
                      winSetting.Show();
                      winSetting.Activate();
                  });

                  //this.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Res/TopLogo2019.PNG")));

                  //Main.ToolBar.AddMenuButton(VPet_Simulator.Core.ToolBar.MenuType.Feed, "喂食测试", () =>
                  //    {
                  //        Main.ToolBar.Visibility = Visibility.Collapsed;
                  //        IRunImage eat = (IRunImage)Core.Graph.FindGraph(GraphType.Eat, GameSave.ModeType.Nomal);
                  //        var b = Main.FindDisplayBorder(eat);
                  //        eat.Run(b, new BitmapImage(new Uri("pack://application:,,,/Res/汉堡.png")), Main.DisplayToNomal);
                  //    }
                  //);
                  Main.ToolBar.AddMenuButton(ToolBar.MenuType.Feed, "吃饭".Translate(), () =>
                  {
                      winBetterBuy.Show(Food.FoodType.Meal);
                  });
                  Main.ToolBar.AddMenuButton(ToolBar.MenuType.Feed, "喝水".Translate(), () =>
                  {
                      winBetterBuy.Show(Food.FoodType.Drink);
                  });
                  Main.ToolBar.AddMenuButton(ToolBar.MenuType.Feed, "收藏".Translate(), () =>
                  {
                      winBetterBuy.Show(Food.FoodType.Star);
                  });
                  Main.ToolBar.AddMenuButton(ToolBar.MenuType.Feed, "药品".Translate(), () =>
                  {
                      winBetterBuy.Show(Food.FoodType.Drug);
                  });
                  Main.ToolBar.AddMenuButton(ToolBar.MenuType.Feed, "礼品".Translate(), () =>
                  {
                      winBetterBuy.Show(Food.FoodType.Gift);
                  });
                  Main.ToolBar.AddMenuButton(ToolBar.MenuType.Feed, "背包".Translate(), () =>
                  {
                      if (winInventory != null && !winInventory.IsClosed)
                          winInventory.Show();
                      else
                      {
                          winInventory = new winInventory(this);
                          winInventory.Show();
                      }
                  });
                  Main.SetMoveMode(Set.AllowMove, Set.SmartMove, Set.SmartMoveInterval * 1000);
                  Main.SetLogicInterval((int)(Set.LogicInterval * 1000));
                  if (Set.MessageBarOutside)
                      Main.MsgBar.SetPlaceOUT();

                  Main.WorkCheck = WorkCheck;

                  //Load icon
                  notifyIcon = new NotifyIcon();
                  notifyIcon.Text = "AIDeskPet" + PrefixSave;
                  ContextMenu m_menu;

                  if (Set.PetHelper)
                      LoadPetHelper();



                  m_menu = new ContextMenu();
                  m_menu.Opening += (x, y) => { GameSavesData.Statistics[(gint)"stat_menu_pop"]++; };
                  var hitThrough = new MenuItem("鼠标穿透".Translate(), null, (x, y) => { SetTransparentHitThrough(); })
                  {
                      Name = "NotifyIcon_HitThrough",
                      Checked = HitThrough
                  };
                  m_menu.Items.Add(hitThrough);
                  var topmost = new MenuItem("置于顶层".Translate(), null, (x, y) =>
                  {
                      Topmost = ((MenuItem)x).Checked;
                  })
                  {
                      Name = "NotifyIcon_TopMost",
                      CheckOnClick = true,
                      Checked = Topmost
                  };
                  m_menu.Items.Add(topmost);
                  m_menu.Items.Add(new MenuItem("重置位置与状态".Translate(), null, (x, y) =>
                  {
                      Main.CleanState();
                      Main.DisplayToNomal();
                      Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
                      Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
                  }));
                  // AIDeskPet: Tutorial / feedback center removed
                  if (Set.DeBug)
                      m_menu.Items.Add(new MenuItem("开发控制台".Translate(), null, (x, y) => { new winConsole(this).Show(); }));

                  m_menu.Items.Add(new MenuItem("设置面板".Translate(), null, (x, y) =>
                  {
                      winSetting.Show();
                  }));
                  m_menu.Items.Add(new MenuItem("重启桌宠".Translate(), null, (x, y) => Restart()));
                  m_menu.Items.Add(new MenuItem("退出桌宠".Translate(), null, (x, y) => Close()));

                  LoadDIY();

                  notifyIcon.ContextMenuStrip = m_menu;

                  notifyIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/vpeticon.ico")).Stream);

                  notifyIcon.Visible = true;
                  notifyIcon.BalloonTipClicked += (a, b) =>
                  {
                      winSetting.Show();
                  };
                  if (Set.StartUPBoot == true && !Set["v"][(gbol)"newverstartup"])
                  {//Update to the latest version's startup-boot method
                      try
                      {
                          winSetting.GenStartUP();
                          Set["v"][(gbol)"newverstartup"] = true;
                      }
                      catch
                      {

                      }
                  }


                  //Achievements and statistics
                  GameSavesData.Statistics[(gint)"stat_open_times"]++;
                  Main.MoveTimer.Elapsed += MoveTimer_Elapsed;
                  Main.SayProcess.Add(Main_OnSay);
                  Main.Event_TouchHead += Main_Event_TouchHead;
                  Main.Event_TouchBody += Main_Event_TouchBody;

                  HashCheck = HashCheck;

                  //Add face-pinch animation (if any)
                  if (Core.Graph.GraphConfig.Data.ContainsLine("pinch"))
                  {
                      var pin = Core.Graph.GraphConfig.Data["pinch"];
                      Main.Core.TouchEvent.Insert(0, new TouchArea(
                          new Point(pin[(gdbe)"px"], pin[(gdbe)"py"]), new Size(pin[(gdbe)"sw"], pin[(gdbe)"sh"])
                          , DisplayPinch, true));
                  }


                  if (Set.HitThrough)
                  {
                      if (!Set["v"][(gbol)"HitThrough"])
                      {
                          Set["v"][(gbol)"HitThrough"] = true;
                          Set.HitThrough = false;
                      }
                      else
                          SetTransparentHitThrough();
                  }

                  if (File.Exists(ExtensionValue.BaseDirectory + @"\Tutorial.html") && Set["SingleTips"].GetDateTime("tutorial") <= new DateTime(2023, 10, 20) && App.MainWindows.Count == 1)
                  {
                      Set["SingleTips"].SetDateTime("tutorial", DateTime.Now);
                      if (LocalizeCore.CurrentCulture == "zh-Hans")
                          ExtensionFunction.StartURL(ExtensionValue.BaseDirectory + @"\Tutorial.html");
                      else if (LocalizeCore.CurrentCulture == "zh-Hant")
                          ExtensionFunction.StartURL(ExtensionValue.BaseDirectory + @"\Tutorial_zht.html");
                      else
                          ExtensionFunction.StartURL(ExtensionValue.BaseDirectory + @"\Tutorial_en.html");
                  }
                  if (!Set["SingleTips"].GetBool("helloworld"))
                  {
                      Task.Run(() =>
                      {
                          Thread.Sleep(2000);
                          Set["SingleTips"].SetBool("helloworld", true);
                          NoticeBox.Show("欢迎使用 AIDeskPet!\n如果遇到助手爬不见了,可以在我这里设置居中或退出".Translate(),
                             "你好".Translate() + (IsSteamUser ? SteamClient.Name : Environment.UserName), Panuon.WPF.UI.MessageBoxIcon.Info, true, 5000);
                          //Thread.Sleep(2000);
                          //Main.SayRnd("欢迎使用虚拟桌宠模拟器\n这是个中期的测试版,若有bug请多多包涵\n欢迎加群虚拟主播模拟器430081239或在菜单栏-管理-反馈中提交bug或建议".Translate());
                      });
                  }
                  if (Set["v"][(gint)"rank"] != DateTime.Now.Year && GameSavesData.Statistics[(gint)"stat_total_time"] > 3600)
                  {//Annual report reminder
                      Task.Run(() =>
                      {
                          Thread.Sleep(Function.Rnd.Next(200000, 400000));
                          Set["v"][(gint)"rank"] = DateTime.Now.Year;
                          var btn = Dispatcher.Invoke(() =>
                          {
                              var button = new System.Windows.Controls.Button()
                              {
                                  Content = "点击前往查看".Translate(),
                                  FontSize = 20,
                                  HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                                  Background = Function.ResourcesBrush(Function.BrushType.PrimaryDark),
                                  Foreground = Function.ResourcesBrush(Function.BrushType.PrimaryText),
                              };
                              button.Click += (x, y) =>
                              {
                                  var panelWindow = new winCharacterPanel(this);
                                  panelWindow.MainTab.SelectedIndex = 1;
                                  panelWindow.Show();
                                  Main.MsgBar.ForceClose();
                              };
                              return button;
                          });
                          Main.Say("哼哼~主人，我的考试成绩出炉了哦，快来和我一起看我的成绩单喵".Translate(), btn, "shining");
                      });
                  }
                  //Birthday setup reminder
                  if (GameSavesData.Data.FindLine("HostBDay") == null)
                  {
                      Task.Run(() =>
                      {
                          Thread.Sleep(Function.Rnd.Next(100000, 200000));
                          var btn = Dispatcher.Invoke(() =>
                          {
                              var button = new System.Windows.Controls.Button()
                              {
                                  Content = "设置".Translate(),
                                  FontSize = 20,
                                  HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                                  Background = Function.ResourcesBrush(Function.BrushType.PrimaryDark),
                                  Foreground = Function.ResourcesBrush(Function.BrushType.PrimaryText),
                              };
                              button.Click += (x, y) =>
                              {
                                  ShowSetting(2);
                              };
                              return button;
                          });
                          Main.Say("不要忘记设置生日时间哦 {0}，我会偷偷给你准备礼物的。".Translate(GameSavesData.GameSave.HostName), btn, "shining");
                      });
                  }
                  else
                  {
                      var bdt = GameSavesData.GetDateTime("HostBDay");
                      if (DateTime.Now.Month == bdt.Month && DateTime.Now.Day == bdt.Day)
                      {
                          Task.Run(() =>
                          {
                              Thread.Sleep(Function.Rnd.Next(100000, 200000));
                              HostBDay();
                          });
                      }
                  }

#if BDAY
                  if (DateTime.Now < new DateTime(2025, 8, 22) && DateTime.Now >= new DateTime(2025, 8, 14))
                  {
                      food.Star = true;
                      Task.Run(() =>
                      {
                          Thread.Sleep(10000);
                          var btn = Dispatcher.Invoke(() =>
                          {
                              var button = new System.Windows.Controls.Button()
                              {
                                  Content = "查看生日公告/视频".Translate(),
                                  FontSize = 20,
                                  HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                                  Background = Function.ResourcesBrush(Function.BrushType.PrimaryDark),
                                  Foreground = Function.ResourcesBrush(Function.BrushType.PrimaryText),
                              };
                              button.Click += (x, y) =>
                              {
                                  if (LocalizeCore.CurrentCulture.StartsWith("zh"))
                                      ExtensionFunction.StartURL("https://www.bilibili.com/opus/1100685352151023623");
                                  else
                                      { /* AIDeskPet: Original game news external link removed */ }
                              };
                              return button;
                          });
                          string bdt;
                          switch (DateTime.Now.Day)
                          {
                              case 14:
                                  bdt = "Wait, is today my birthday? I completely forgot! Thank you! Another year has gone by, and I hope we stay together for many more.";
                                  break;
                              case 15:
                                  bdt = "How was yesterday? Did you have a good time? If it wasn't enough, that's fine, let's celebrate again!";
                                  break;
                              case 16:
                                  bdt = "What, the birthday is already over? Time to get back to the schedule? I'm not quite ready yet!";
                                  break;
                              case 17:
                                  bdt = "I'm having birthday withdrawals! I think I'll only feel better if you feed me some cake.";
                                  break;
                              case 18:
                                  bdt = "I'm all grown up now, I don't need birthdays... those childish things... I really don't... okay maybe I do.";
                                  break;
                              case 19:
                                  bdt = "Don't worry, I've figured it out! As long as we're together, every day feels like a birthday!";
                                  break;
                              default:
                                  bdt = "It's been so long, it must almost be the anniversary again! What, only a week has passed? Can't we skip ahead to the next birthday?";
                                  break;

                          }
                          Main.Say(bdt.Translate(), btn, "self");
                          //Main.Say(bdt.Translate(), "self");
                      });
                  }
#endif
                  newday = DateTime.Now.Day;
                  Main.TimeHandle += NewDayHandle;
                  Event_NewDay += () =>
                  {
                      var bdt = GameSavesData.GetDateTime("HostBDay");
                      if (DateTime.Now.Month == bdt.Month && DateTime.Now.Day == bdt.Day)
                      {
                          HostBDay();
                      }
                  };
                  Event_NewDay += everydaygift;

                  //Special feature for the birthday cake
                  Event_TakeItem += MainWindow_Event_TakeItem;
                  //Add purchase event
                  Event_TakeItemHandle += (item, count, from) => ActivityLogs.Add(new ActivityLog("take_" + from, item.TranslateName, count.ToString()));
                  //Add work event
                  Main.Event_WorkStart += (work) => ActivityLogs.Add(new ActivityLog("work_start", work.NameTrans));
                  Main.Event_WorkEnd += (workinfo) => ActivityLogs.Add(new ActivityLog("work_end", workinfo.work.NameTrans, workinfo.Reason.ToString(), workinfo.spendtime.ToString("f0"), workinfo.count.ToString("f0")));
                  Main.SayProcess.Add((sayinfo) =>
                  {
                      Task.Run(async () =>
                      {
                          ActivityLogs.Add(new ActivityLog("petsay", await sayinfo.GetSayText()));
                      });
                  });
                  //if (DateTime.Now.DayOfYear == 1)
                  //{
                  //    Task.Run(() =>
                  //    {
                  //        Thread.Sleep(5000);
                  //        Main.SayRnd("25年都跨过去了, 还有什么是跨不过的呢? {0}这一年辛苦了! 新年请多多指教!".Translate(GameSavesData.GameSave.HostName));
                  //    });
                  //}
                  //Fix the issue where the 2026 New Year bug invalidated HashCheck
                  //In short, restore HashCheck once for all users who have the "2026 New Year" photo, as a bonus, since anyone with this photo was basically in the bug window
                  //Anyone who sees this code, please don't share it, to avoid abuse
                  //var photo25 = Photos.Find(x => x.Name == "2026跨年");
                  //if (photo25?.IsUnlock == true && GameSavesData.HashCheck == false && GameSavesData.Data["debug"][(gbol)"fix26"] == false)
                  //{
                  //    GameSave_v2 ogs = GameSavesData;
                  //    GameSavesData = new GameSave_v2(ogs.GameSave.Name);
                  //    GameSavesData.Data = ogs.Data;
                  //    GameSavesData.GameSave = ogs.GameSave;
                  //    GameSavesData.Statistics = ogs.Statistics;
                  //    HashCheck = true;
                  //}
                  //GameSavesData.Data["debug"][(gbol)"fix26"] = true;
                  if (GameSavesData.HashCheck == false && GameSavesData["debug"].Find("losthash") == null)
                  {
                      GameSavesData["debug"][(gdat)"losthash"] = DateTime.Now;
                  }
#if NewYear
                  //New Year only feature
                  if (DateTime.Now < new DateTime(2026, 2, 25))
                  {
                      Event_NewDay += NewYearSay;
                      Task.Run(() =>
                      {
                          Thread.Sleep(5000);
                          NewYearSay();
                      });
                  }
#endif
                  //MOD errors
                  foreach (CoreMOD cm in CoreMODs)
                      if (!cm.SuccessLoad)
                          if (cm.Tag.Contains("该模组已损坏"))
                              MessageBoxX.Show("模组 {0} 插件损坏\n虚拟桌宠模拟器未能成功加载该插件\n请联系MOD作者修复该问题".Translate(cm.Name) + '\n' + cm.ErrorMessage, "该模组已损坏".Translate());
                          else if (Set.IsPassMOD(cm.Name) || !string.IsNullOrEmpty(cm.ErrorMessage))
                              MessageBoxX.Show("模组 {0} 的代码插件损坏\n虚拟桌宠模拟器未能成功加载该插件\n请联系MOD作者修复该问题".Translate(cm.Name) + '\n' + cm.ErrorMessage, "{0} 未加载代码插件".Translate(cm.Name));
                          else if (Set.IsMSGMOD(cm.Name))
                              MessageBoxX.Show("由于 {0} 包含代码插件\n虚拟桌宠模拟器已自动停止加载该插件\n请手动前往设置允许启用该mod 代码插件".Translate(cm.Name), "{0} 未加载代码插件".Translate(cm.Name));
                  //Animation errors
                  if (Main.ErrorMessage.Count != 0)
                  {
                      var errstr = string.Join("\n------\n", Main.ErrorMessage);
                      if (errstr.Contains("0000_core"))
                      {
                          MessageBoxX.Show("动画加载错误,请尝试以下解决方法修复问题:\n\t1. 删除游戏根目录`Cache`文件夹\n\t2. 删除游戏根目录`mod\\0000_core\\pet`文件夹,并在Steam验证游戏完整性".Translate(), "动画加载错误".Translate());
                          // AIDeskPet: Feedback center (winReport) removed, local prompt only
                      }
                      else
                          MessageBoxX.Show("动画加载错误\n虚拟桌宠模拟器未能成功加载该动画\n请联系MOD作者修复该问题".Translate() + '\n' + errstr, "动画加载错误".Translate());

                      Main.ErrorMessage.Clear();
                  }
                  //Load game Workshop plugins
                  foreach (MainPlugin mp in Plugins)
                      try //Don't remove try for DEBUG; on the main thread errors won't be shown either
                      {
                          mp.GameLoaded();
                      }
                      catch (Exception e)
                      {
                          NoticeBox.Show("由于插件引起的游戏启动错误".Translate() + "\n" + e.ToString(), "由于插件引起的游戏启动错误".Translate() + '-' + mp.PluginName);
                      }

                  //Everything written here is shared functionality; for features limited to the first MW, go to

                  if (GameSavesData.GameSave.Likability < 520)
                      Core.Graph.GraphsName[GraphType.Idel].Remove("like520");
                  else if (Core.Graph.FindGraph("like520", AnimatType.Single, IGameSave.ModeType.Happy) != null)
                  {
                      Event_NewDay += like520;
                      like520();
                  }
                  if (Set.DeBug)
                      ActivityLogs.CollectionChanged += ActivityLogs_WriteFile;
              });


            ////Game tips
            //if (Set["SingleTips"][(gint)"open"] == 0 && Set.StartUPBoot == true && Set.StartUPBootSteam == true)
            //{
            //    await Dispatcher.InvokeAsync(new Action(() =>
            //    {
            //        MessageBoxX.Show("检测到您开启了开机启动, 以下是开机启动相关提示信息: (仅显示一次)".Translate() + "\n------\n" +
            //             "游戏开机启动的实现方式是创建快捷方式,不是注册表,更健康,所以游戏卸了也不知道\n如果游戏打不开,可以去这里手动删除游戏开机启动快捷方式:\n%appdata%\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\".Translate()
            //          , "关于卸载不掉的问题是因为开启了开机启动".Translate(), Panuon.WPF.UI.MessageBoxIcon.Info);
            //        Set["SingleTips"][(gint)"open"] = 1;
            //    }));
            //}

        }
        public static object LogsLock = new object();
        private void ActivityLogs_WriteFile(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                List<string> sb = new List<string>();
                foreach (ActivityLog log in e.NewItems)
                {
                    sb.Add(log.ToString(Main));
                }
                string logPath = ExtensionValue.BaseDirectory + $"\\Logs{PrefixSave}.txt";
                lock (LogsLock)
                {
                    if (File.Exists(logPath) && new FileInfo(logPath).Length > 1024 * 1024)
                    {
                        var allLines = File.ReadAllLines(logPath);
                        if (allLines.Length > 2000)
                        {
                            File.WriteAllLines(logPath, allLines.Skip(allLines.Length - 2000));
                        }
                    }
                    File.AppendAllLines(logPath, sb);
                }
            }
        }

        private void everydaygift()
        {
            if (Set["dailydata"][(gint)"everydaygift"] == DateTime.Now.DayOfYear)
            {
                return;
            }
            Set["dailydata"][(gint)"everydaygift"] = DateTime.Now.DayOfYear;
            var itm = new Item
            {
                Name = "每日礼包",
                Desc = "物品系统附赠的每日礼包, 打开后会获得3个随机物品. 教程还送礼物, 萝莉丝真大方.".Translate(),
                ItemType = "Mail",
                Price = 15,
            };
            Dispatcher.Invoke(() => itm.LoadSource(this));
            ItemsAdd(itm);
        }


        private void like520()
        {
            var date = DateTime.Now.Day + DateTime.Now.Month * 100;
            var thisy77 = GetLunarDate(7, 7);
            if (date == 520 || date == 521 || date == 214 || date == (thisy77.Day + thisy77.Month * 100))
            {
                Task.Run(() =>
                {
                    Thread.Sleep(52000);
                    Main.Display("like520", AnimatType.Single, Main.DisplayNomal);
                });
            }
        }

        private void MainWindow_Event_TakeItem(Food obj)
        {
            switch (obj.Name)
            {
                case "生日蛋糕2":
                    //Update the birthday cake's attributes and price
                    obj.Exp = Core.Save.Level;
                    obj.Likability = Core.Save.LikabilityMax / 20;
                    obj.StrengthDrink = Core.Save.StrengthMax / 20;
                    obj.StrengthFood = Core.Save.StrengthMax / 20;
                    obj.isoverload = false;
                    obj.Price = (int)Math.Max(0, obj.RealPrice * .5);
                    switch (Function.Rnd.Next(15))
                    {
                        case 1:
                        case 2:
                        case 3:
                            Core.Save.Strength = Core.Save.StrengthMax;
                            Main.LabelDisplayShow("{0}充满抛瓦!".Translate(Core.Save.Name), 3000);
                            break;
                        case 4:
                        case 5:
                            Core.Save.Feeling = Core.Save.FeelingMax;
                            Main.LabelDisplayShow("{0}今天也是好心情!".Translate(Core.Save.Name), 3000);
                            break;
                        case 6:
                        case 7:
                            Core.Save.StrengthFood = Core.Save.StrengthMax;
                            Main.LabelDisplayShow("{0}吃饱了!".Translate(Core.Save.Name), 3000);
                            break;
                        case 8:
                        case 9:
                            Core.Save.StrengthDrink = Core.Save.StrengthMax;
                            Main.LabelDisplayShow("{0}加满水了!".Translate(Core.Save.Name), 3000);
                            break;
                        case 10:
                            int get = (Function.Rnd.Next(Core.Save.LevelUpNeed() * (GameSavesData.GameSave.LevelMax + 1)) / 200 + 1) * 100;
                            Core.Save.Exp += get;
                            Main.LabelDisplayShow("{0}经验 +{1} 告辞".Translate(Core.Save.Name, get.ToString("N0")), 4000);
                            break;
                        case 11:
                            get = (Function.Rnd.Next(Core.Save.LevelUpNeed() * (GameSavesData.GameSave.LevelMax + 1)) / 500 + 1) * 10;
                            Core.Save.Exp += get;
                            Main.LabelDisplayShow("{0}在马路边捡到{1}金钱".Translate(Core.Save.Name, get.ToString("N0")), 4000);
                            break;
                        case 12:
                            if (Function.Rnd.Next(3) != 0)
                            {//Roll again, give likability
                                get = Function.Rnd.Next((int)Core.Save.LikabilityMax / 25) + 1;
                                Core.Save.Likability += get;
                                Main.LabelDisplayShow("{0}更喜欢{1}了".Translate(Core.Save.Name, Core.Save.HostName), 4000);
                                break;
                            }
                            var photos = Photos.FindAll(x => x.IsUnlock == false && x.UnlockAble.Lock == false);
                            if (photos.Count > 0)
                            {
                                var tempphoto = photos.FindAll(x => x.UnlockAble.Time != null || x.UnlockAble.Date != null || x.UnlockAble.Holiday != HolidayType.None);
                                if (tempphoto.Count > 0)//Prioritize unlocking time/date/holiday photos
                                    photos = tempphoto;
                                else
                                {
                                    tempphoto = photos.FindAll(x => x.UnlockAble.SellBoth == false && (x.UnlockAble.Feeling > 10 || x.UnlockAble.Likability >= 10 || x.UnlockAble.Money >= 10));
                                    if (tempphoto.Count > 0)//Then unlock likability/money/fullness/thirst photos
                                        photos = tempphoto;
                                }

                                var photo = photos[Function.Rnd.Next(photos.Count)];
                                photo.Unlock(this);
                                Main.LabelDisplayShow("{0}收到了新照片".Translate(Core.Save.Name) + '\n' + photo.Name, 4000);
                            }
                            else
                                goto case 11;
                            break;
                        default:
                            Main.LabelDisplayShow("{0}获得了谢谢惠顾".Translate(Core.Save.Name), 4000);
                            break;
                    }
                    break;
            }
        }

        public event Action<IMPWindows> MutiPlayerHandle;
        public void MutiPlayerStart(IMPWindows mp)
        {
            MutiPlayerHandle?.Invoke(mp);
        }

        /// <summary>
        /// Whether to show the eating animation
        /// </summary>
        bool showeatanm = true;
        /// <summary>
        /// Show the eating (sandwich) animation
        /// </summary>
        /// <param name="graphName">Sandwich animation name</param>
        /// <param name="imageSource">Image sandwiched in the middle</param>
        public void DisplayFoodAnimation(string graphName, ImageSource imageSource)
        {
            if (showeatanm)
            {//Show animation
                showeatanm = false;
                Main.Display(graphName, imageSource, () =>
                {
                    showeatanm = true;
                    if (Core.Controller.EnableFunction)
                    {
                        var newmod = Core.Save.CalMode();
                        if (Core.Save.Mode != newmod)
                        {
                            //Tweak the parameters so the switch animation still plays
                            Main.DisplayType.Type = GraphType.Default;
                            //Switch display animation
                            Main.PlaySwitchAnimat(Core.Save.Mode, newmod);
                            Core.Save.Mode = newmod;
                        }
                        else
                            Main.DisplayToNomal();
                    }
                    else
                        Main.DisplayToNomal();
                });
            }
            else
            {//If not showing the animation, check whether there is an override
                if (Main.DisplayType.Animat != AnimatType.Single && Main.DisplayType.Name != graphName)
                {
                    showeatanm = true;
                }
            }
        }

        public void HostBDay()
        {
            var petloader = Pets.Find(x => x.Name == Set.PetGraph);
            petloader ??= Pets[0];

            string sbv = "Special_Birthday_Voice_" + petloader.Name;
            string sbv_trans = sbv.Translate(GameSavesData.GameSave.HostName);
            if (sbv == sbv_trans)
            {
                Main.Say("今天是{0}的生日！祝{0}生日快乐！".Translate(GameSavesData.GameSave.HostName), "bday", true);
            }
            else
            {
                Main.Say(sbv_trans, "bday");
                Dispatcher.Invoke(() =>
                {
                    var panelWindow = new winCharacterPanel(this);
                    panelWindow.MainTab.SelectedIndex = 2;
                    panelWindow.Show();
                });
            }
        }



        int newday = 0;
        private void NewDayHandle(Main main)
        {
            if (DateTime.Now.Hour == 0 && newday != DateTime.Now.Day)
            {//Day rollover
                newday = DateTime.Now.Day;
                Event_NewDay?.Invoke();
            }
        }
        /// <summary>
        /// Event: a new day
        /// </summary>
        public event Action Event_NewDay;
#if NewYear
        /// <summary>
        /// New Year greeting
        /// </summary>
        private void NewYearSay()
        {
            string sayny;
            switch (newday)
            {

                case 16:
                    sayny = "白龙马，蹄朝西~马儿你跑快点啊~神马都是浮云~\n小马萝莉斯祝主人马年顺利，万事顺利，前途无阻，万马奔腾不停歇！".Translate();
                    break;
                case 17:
                    sayny = "马什么梅？什么冬梅？马冬什么？\n演员萝莉斯祝主人马年大智，学业进步，智商增加，考试满分！".Translate();
                    break;
                case 18:
                    sayny = "老马啊！！！哎！老马啊——！\n主播萝莉斯祝主人马年快乐，心情轻松快乐，烦恼统统飞走！".Translate();
                    break;
                case 19:
                    sayny = "我大意了啊没有闪，小主人你不讲武德！吃我闪电五连鞭！\n马掌门人萝莉斯祝主人马年安康，功夫有长进，技术有进步，能力会出众！".Translate();
                    break;
                case 20:
                    sayny = "哈基米曼波~马儿跳马儿跳~\n六星萝莉斯祝主人马年大运，抽卡出金一发入魂，装备掉落出红满仓！".Translate();
                    break;
                case 21:
                    sayny = "哎致命空枪，哎打腿没死，哎又空枪。我柜子动了我不玩了。\n游戏萝莉斯祝主人马年变强，枪枪爆头好运连连，把把第一永不马枪！".Translate();
                    break;
                case 22:
                    sayny = "待我高头大马，许你十里桃花！\n马猴烧酒萝莉斯祝主人马年马上有对象！千里姻缘一马牵，万水千山有马子！".Translate();
                    break;
                default:
                case 23:
                    sayny = "马喽的命也是命！\n打工人萝莉斯祝主人马年发财，返工赚大钱，今年一定发！马到成功！".Translate();
                    break;
            }
            Main.SayRnd(sayny);
        }
#endif
        /// <summary>
        /// Display the face-pinch state
        /// </summary>
        public bool DisplayPinch()
        {
            if (Core.Graph.FindGraphs("pinch", AnimatType.A_Start, Core.Save.Mode) == null)
            {
                return false;
            }
            Main.CountNomal = 0;

            if (Core.Controller.EnableFunction && Core.Save.Strength >= 10 && Core.Save.Feeling < Core.Save.FeelingMax)
            {
                Core.Save.StrengthChange(-2);
                Core.Save.FeelingChange(1);
                Core.Save.Mode = Core.Save.CalMode();
                Main.LabelDisplayShowChangeNumber(LocalizeCore.Translate("体力-{0:f0} 心情+{1:f0}"), 2, 1);
            }
            if (Main.DisplayType.Name == "pinch")
            {
                if (Main.DisplayType.Animat == AnimatType.A_Start)
                    return false;
                else if (Main.DisplayType.Animat == AnimatType.B_Loop)
                    if (Dispatcher.Invoke(() => Main.PetGrid.Tag) is IGraph ig && ig.GraphInfo.Name == "pinch" && ig.GraphInfo.Animat == AnimatType.B_Loop)
                    {
                        ig.SetContinue();
                        return true;
                    }
                    else if (Dispatcher.Invoke(() => Main.PetGrid2.Tag) is IGraph ig2 && ig2.GraphInfo.Name == "pinch" && ig2.GraphInfo.Animat == AnimatType.B_Loop)
                    {
                        ig2.SetContinue();
                        return true;
                    }
            }
            Main_Event_TouchHead();
            Main_Event_TouchBody();
            Main.Display("pinch", AnimatType.A_Start, () =>
               Main.Display("pinch", AnimatType.B_Loop, DisplayPinch_loop));
            return true;
        }
        private void DisplayPinch_loop()
        {
            if (Main.isPress && Main.DisplayType.Name == "pinch" && Main.DisplayType.Animat == AnimatType.B_Loop)
            {
                if (Core.Controller.EnableFunction && Core.Save.Strength >= 10 && Core.Save.Feeling < Core.Save.FeelingMax)
                {
                    Core.Save.StrengthChange(-2);
                    Core.Save.FeelingChange(1);
                    Core.Save.Mode = Core.Save.CalMode();
                    Main.LabelDisplayShowChangeNumber(LocalizeCore.Translate("体力-{0:f0} 心情+{1:f0}"), 2, 1);
                }
                Main.Display("pinch", AnimatType.B_Loop, DisplayPinch_loop);
            }
            else
            {
                Main.DisplayCEndtoNomal("pinch");
            }
        }
        /// <summary>
        /// Get starred (favorited) works
        /// </summary>
        public List<Work> WorkStar()
        {
            List<Work> works = new List<Work>();
            foreach (var work in Core.Graph.GraphConfig.Works)
            {
                if (Set["work_star"].GetBool(work.Name))
                    works.Add(work);
            }
            return works;
        }
        public System.Windows.Controls.MenuItem WorkStarMenu;

        public void LevelUP(GameSave_VPet.LevelUpEventArgs args)
        {
            var gf = Core.Graph.FindGraph("levelup", GraphInfo.AnimatType.Single, GameSavesData.GameSave.Mode);
            if (gf != null)
            {
                Task.Run(() =>
                {
                    Thread.Sleep(5000);
                    Main.Display(gf, Main.DisplayToNomal);
                });
            }
            if (args.IsLevelMaxUp)
            {//Notify the user the level cap has increased
                Task.Run(() =>
                {
                    Thread.Sleep(5000);
                    Dispatcher.Invoke(() =>
                    {
                        Main.Say("邦邦咔邦,{0}等级突破了!".Translate(Name));
                        MessageBoxX.Show("系统提示\n您的桌宠等级已经突破\nLv{0}→LV{1} x{2}\n已突破为尊贵的x{3}阶".Translate(
                            1000 + args.BeforeLevelMax * 100, 100 * GameSavesData.GameSave.LevelMax, GameSavesData.GameSave.LevelMax),
                            "桌宠等级突破".Translate());
                    });
                });
            }
        }

        public void CheckGalleryUnlock()
        {
            var ps = Photos.FindAll(x => !x.IsUnlock && !x.UnlockAble.SellBoth && x.UnlockAble.Check(GameSavesData));
            if (ps.Count == 0) return;
            StringBuilder sb = new StringBuilder();
            foreach (Photo p in ps)
            {
                sb.Append(", ");
                p.Unlock(this);
                sb.Append(p.TranslateName);
            }
            ActivityLogs.Add(new ActivityLog("photo_unlock", sb.ToString().AsSpan(2).ToString()));
            Dispatcher.Invoke(() =>
            NoticeBox.Show(string.Concat(sb.ToString().AsSpan(2), "\n", "以上照片已解锁".Translate()), "新的照片已解锁".Translate()
            , Panuon.WPF.UI.MessageBoxIcon.Info, true, 5000));
        }
        static readonly DateTime StartDate = new(2023, 8, 14, 0, 0, 0, DateTimeKind.Utc);
        static int authheycache;
        static DateTime GetDateFromAuthKey(int authKey)
        {
            // Parse the number of hours from the auth key
            int hoursSince2020 = authKey / 10000;

            // Calculate the date and time
            DateTime date = StartDate.AddHours(hoursSince2020);

            return date;
        }
        public async Task<int> GenerateAuthKey()
        {
            if (!IsSteamUser)
                return 0;

            bool genck = false;
            long steamId = (long)SteamClient.SteamId.Value;

            while (true)
            {
                if (authheycache != 0)
                {
                    DateTime dt = GetDateFromAuthKey(authheycache);
                    if (!(dt > DateTime.UtcNow.AddDays(1) || dt < DateTime.UtcNow.AddHours(-2)))
                    {
                        return authheycache;
                    }
                }

                // Add ConfigureAwait(false)
                Leaderboard? leaderboard = await SteamUserStats
                    .FindLeaderboardAsync("chatgpt_auth")
                    .ConfigureAwait(false);

                if (!leaderboard.HasValue)
                    return 0;

                var lb = leaderboard.Value;

                // Add ConfigureAwait(false)
                LeaderboardEntry[] key = await lb
                    .GetScoresAroundUserAsync(0, 0)
                    .ConfigureAwait(false);

                if (key == null || key.Length == 0 || genck)
                {
                    int hoursSince2020 = (int)(DateTime.UtcNow - StartDate).TotalHours;
                    authheycache = hoursSince2020 * 10000 + Function.Rnd.Next(10000);
                    await lb.ReplaceScore(authheycache).ConfigureAwait(false);
                    return authheycache;
                }
                else
                {
                    authheycache = key.First().Score;
                    genck = true;
                }
            }
        }
        /// <summary>
        /// Add item to inventory (auto-merge)
        /// </summary>
        /// <param name="item">Item</param>
        public void ItemsAdd(Item item)
        {
            var sameitem = Items.Find(x => x.Name == item.Name);
            if (sameitem != null)
            {
                sameitem.Count += item.Count;
            }
            else
            {
                Items.Add(item);
            }
        }
    }
}
