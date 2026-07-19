using LinePutScript;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;

namespace VPet_Simulator.Windows
{
    /// <summary>
    /// Interaction logic for TalkSelect.xaml
    /// </summary>
    public partial class TalkSelect : UserControl
    {// Chat box using the new option-based approach

        /// <summary>
        /// Options currently in the list
        /// </summary>
        List<SelectText> textList = new List<SelectText>();
        /// <summary>
        /// Things already said
        /// </summary>
        HashSet<string> textSaid = new HashSet<string>();
        /// <summary>
        /// Next refresh time
        /// </summary>
        public DateTime RelsTime;
        private DateTime lastAddTime;

        MainWindow mw;
        public TalkSelect(MainWindow mw)
        {
            InitializeComponent();
            this.mw = mw;
            mw.Main.ToolBar.EventShow += RelsSelect;
            RelsSelect();
        }


        /// <summary>
        /// Refresh all current options
        /// </summary>
        public void RelsSelect()
        {
            if (RelsTime < DateTime.Now)
            {
                // Refresh options
                RelsTime = DateTime.Now.AddMinutes(10);// Refresh every 10 minutes; each chat adds 5 minutes
                lastAddTime = DateTime.Now;
                textList.Clear();
                textSaid.Clear();
                // Randomly pick options
                var list = mw.SelectTexts.ToList();
                while (list.Count > 0 && textList.Count < 5)
                {
                    int sid = Function.Rnd.Next(list.Count);
                    var select = list[sid];
                    list.RemoveAt(sid);
                    if (textList.Find(x => x.Choose == select.Choose) == null && select.CheckState(mw.Main))
                    {
                        textList.Add(select);
                    }
                }
            }
            // Refresh display
            if (textList.Count > 0)
            {
                tbTalk.Items.Clear();
                foreach (var item in textList)
                {
                    if (!textSaid.Contains(item.Choose))
                    {
                        tbTalk.Items.Add(item.TranslateChoose);
                    }
                }
                btn_Send.IsEnabled = true;
            }
            else
            {
                tbTalk.Items.Clear();
                tbTalk.Items.Add("没有可以说的话".Translate());
                btn_Send.IsEnabled = false;
            }
            double min = (RelsTime - DateTime.Now).TotalMinutes;
            double interval = (RelsTime - lastAddTime).TotalMinutes;
            double progress = 1 - min / interval;
            progress = Math.Min(1, Math.Max(0, progress));
            PrograssUsed.Value = progress;
            PrograssUsed.ToolTip = "下次刷新剩余时间: {0:f1}分钟".Translate(min);
        }

        private void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            if (tbTalk.SelectedIndex == -1 || tbTalk.Text == "没有可以说的话".Translate() || textList.Count == 0)
            {
                return;
            }
            mw.Main.ToolBar.Visibility = Visibility.Collapsed;
            var say = textList[tbTalk.SelectedIndex];
            textList.RemoveAt(tbTalk.SelectedIndex);

            // Add log
            mw.ActivityLogs.Add(new ActivityLog("hostsay",say.TranslateChoose));

            // Chat effect
            if (say.Exp != 0)
            {
                if (say.Exp > 0)
                {
                    mw.GameSavesData.Statistics[(gint)"stat_say_exp_p"]++;
                }
                else
                    mw.GameSavesData.Statistics[(gint)"stat_say_exp_d"]++;
            }
            if (say.Likability != 0)
            {
                if (say.Likability > 0)
                    mw.GameSavesData.Statistics[(gint)"stat_say_like_p"]++;
                else
                    mw.GameSavesData.Statistics[(gint)"stat_say_like_d"]++;
            }
            if (say.Money != 0)
            {
                if (say.Money > 0)
                    mw.GameSavesData.Statistics[(gint)"stat_say_money_p"]++;
                else
                    mw.GameSavesData.Statistics[(gint)"stat_say_money_d"]++;
            }
            mw.Main.Core.Save.EatFood(say);
            mw.Main.Core.Save.Money += say.Money;

            
            textSaid.Add(say.Choose);
            RelsTime = RelsTime.AddMinutes(5);
            lastAddTime = DateTime.Now;

            mw.Main.SayRnd(say.TranslateTextConvert(mw.Main), desc: say.FoodToDescription());
            if (say.ToTags.Count > 0)
            {
                var list = mw.SelectTexts.FindAll(x => x.ContainsTag(say.ToTags)).ToList();
                while (list.Count > 0)
                {
                    int sid = Function.Rnd.Next(list.Count);
                    var select = list[sid];
                    list.RemoveAt(sid);
                    if (textList.Find(x => x.Choose == select.Choose) == null && !textSaid.Contains(select.Choose) && select.CheckState(mw.Main))
                    {
                        textList.Add(select);
                        break;
                    }
                }
            }
            RelsSelect();
        }
    }
}
