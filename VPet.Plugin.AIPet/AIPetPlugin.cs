using System;
using System.Timers;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.AIPet
{
    /// <summary>
    /// AI desktop pet plugin entry point
    /// </summary>
    public class AIPetPlugin : MainPlugin
    {
        public override string PluginName => "AIPet";

        public AISetting Config { get; private set; }
        public AIController Controller { get; private set; }
        public AITalkBox TalkBox { get; private set; }

        private Timer proactiveTimer;
        private winAISetting settingWindow;

        public AIPetPlugin(IMainWindow mainwin) : base(mainwin) { }

        public override void LoadPlugin()
        {
            Config = new AISetting(MW);
            Controller = new AIController(MW, this, Config);
            MW.Dispatcher.Invoke(() =>
            {
                TalkBox = new AITalkBox(this);
                MW.TalkAPI.Add(TalkBox);
            });
        }

        public override void GameLoaded()
        {
            // Interaction event hooks (bilingual EN/ZH; descriptions omit the address term to avoid grammar issues)
            bool En() => Controller.IsEnglish;
            MW.Main.Event_TouchHead += () =>
                Controller.HandleEvent(En() ? "The user patted your head" : "对方摸了摸你的头", probability: 0.4, cooldownSeconds: 120);
            MW.Main.Event_TouchBody += () =>
                Controller.HandleEvent(En() ? "The user gave you a poke" : "对方戳了戳你", probability: 0.4, cooldownSeconds: 120);
            MW.Event_TakeItem += food =>
                Controller.HandleEvent(En() ? $"The user fed you \"{food.TranslateName}\"" : $"对方喂你吃了「{food.TranslateName}」", probability: 0.5, cooldownSeconds: 180);
            MW.Event_NewDay += () =>
                Controller.HandleEvent(En() ? "A new day has started — greet the user good morning" : "新的一天开始了，跟对方道个早安吧", probability: 1.0, cooldownSeconds: 0);
            MW.Main.Event_WorkEnd += info =>
                Controller.HandleEvent(En() ? $"You just finished the task \"{info.work.NameTrans}\" (+{info.count:f0})" : $"你刚完成了工作「{info.work.NameTrans}」，赚到了{info.count:f0}", probability: 0.5, cooldownSeconds: 300);

            // Proactive behavior timer (checks once per minute)
            proactiveTimer = new Timer(60_000) { AutoReset = true };
            proactiveTimer.Elapsed += (_, _) =>
            {
                try { Controller.ProactiveTick(); } catch { }
            };
            proactiveTimer.Start();

            // Panel: API/token usage statistics
            MW.Dispatcher.Invoke(BuildStatsPanel);
            MW.Main.ToolBar.EventMenuPanelShow += () => MW.Dispatcher.Invoke(RefreshStatsPanel);
        }

        private System.Windows.Controls.TextBlock tbModel, tbToday, tbTotal;

        private void BuildStatsPanel()
        {
            var sp = MW.Main.ToolBar.spAIStats;
            sp.Children.Clear();
            System.Windows.Controls.TextBlock Mk(double size = 20)
            {
                var tb = new System.Windows.Controls.TextBlock
                {
                    FontSize = size,
                    Margin = new System.Windows.Thickness(0, 2, 0, 0),
                    TextWrapping = System.Windows.TextWrapping.NoWrap,
                    TextTrimming = System.Windows.TextTrimming.CharacterEllipsis,
                    FontFamily = new System.Windows.Media.FontFamily("Cascadia Mono, Consolas, Microsoft YaHei UI"),
                };
                tb.SetResourceReference(System.Windows.Controls.TextBlock.ForegroundProperty, "DARKPrimary");
                sp.Children.Add(tb);
                return tb;
            }
            var title = Mk(22);
            title.Text = "PS> ai-usage --stats";
            title.FontWeight = System.Windows.FontWeights.Bold;
            tbModel = Mk();
            tbToday = Mk();
            tbTotal = Mk();
            RefreshStatsPanel();
        }

        private void RefreshStatsPanel()
        {
            if (tbModel == null) return;
            var u = Controller.Usage;
            tbModel.Text = $"model   {Config.Model}";
            tbToday.Text = $"today   {u.TodayRequests} calls · ↑{FmtTok(u.TodayIn)} ↓{FmtTok(u.TodayOut)}";
            tbTotal.Text = $"total   ↑{FmtTok(u.TotalIn)} ↓{FmtTok(u.TotalOut)}";
        }

        private static string FmtTok(long n) =>
            n >= 1_000_000 ? $"{n / 1_000_000.0:f1}M tok" : n >= 1000 ? $"{n / 1000.0:f1}k tok" : $"{n} tok";

        public override void Setting()
        {
            MW.Dispatcher.Invoke(() =>
            {
                if (settingWindow == null || !settingWindow.IsLoaded)
                {
                    settingWindow = new winAISetting(this);
                    settingWindow.Show();
                }
                else
                {
                    settingWindow.Activate();
                }
            });
        }

        public override void Save()
        {
            Controller?.SaveData();
        }

        public override void EndGame()
        {
            proactiveTimer?.Stop();
            proactiveTimer?.Dispose();
            Controller?.SaveData();
        }
    }
}
