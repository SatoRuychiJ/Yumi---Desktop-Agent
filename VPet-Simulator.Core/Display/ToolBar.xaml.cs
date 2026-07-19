using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static VPet_Simulator.Core.GraphHelper;
using static VPet_Simulator.Core.GraphInfo;
using static VPet_Simulator.Core.Main;
using Timer = System.Timers.Timer;

namespace VPet_Simulator.Core
{
    /// <summary>
    /// Interaction logic for ToolBar.xaml
    /// </summary>
    public partial class ToolBar : UserControl, IDisposable
    {
        Main m;
        public Timer CloseTimer;
        bool onFocus = false;
        Timer closePanelTimer;

        public ToolBar(Main m)
        {
            InitializeComponent();
            this.m = m;
            CloseTimer = new Timer()
            {
                Interval = 4000,
                AutoReset = false,
                Enabled = false
            };
            CloseTimer.Elapsed += Closetimer_Elapsed;
            closePanelTimer = new Timer();
            closePanelTimer.Elapsed += ClosePanelTimer_Tick;
            m.TimeUIHandle += M_TimeUIHandle;
            //LoadWork();
            LoadDIY();
        }
        public void LoadClean()
        {
            MenuWork.Click -= MenuWork_Click;
            MenuWork.Visibility = Visibility.Visible;
            MenuStudy.Click -= MenuStudy_Click;
            MenuStudy.Visibility = Visibility.Visible;
            MenuPlay.Click -= MenuPlay_Click;


            MenuWork.Items.Clear();
            MenuStudy.Items.Clear();
            MenuPlay.Items.Clear();
        }
        public void StartWork(Work w)
        {
            if (m.StartWork(w))
                Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Load default work
        /// </summary>
        public void LoadWork()
        {
            LoadClean();

            m.WorkList(out List<Work> ws, out List<Work> ss, out List<Work> ps);

            if (ws.Count == 0)
            {
                MenuWork.Visibility = Visibility.Collapsed;
            }
            else if (ws.Count == 1)
            {
                MenuWork.Click += MenuWork_Click;
                wwork = ws[0];
                MenuWork.Header = ws[0].NameTrans;
            }
            else
            {
                foreach (var w in ws)
                {
                    var mi = new MenuItem()
                    {
                        Header = w.NameTrans
                    };
                    mi.Click += (s, e) => StartWork(w);

                    MenuWork.Items.Add(mi);
                }
            }
            // AIDeskPet: Study feature removed, menu permanently hidden
            MenuStudy.Visibility = Visibility.Collapsed;
            if (ps.Count == 0)
            {
                MenuPlay.Visibility = Visibility.Collapsed;
            }
            else if (ps.Count == 1)
            {
                MenuPlay.Click += MenuPlay_Click;
                wplay = ps[0];
                MenuPlay.Header = ps[0].NameTrans;
            }
            else
            {
                foreach (var w in ps)
                {
                    var mi = new MenuItem()
                    {
                        Header = w.NameTrans
                    };
                    mi.Click += (s, e) => StartWork(w);
                    MenuPlay.Items.Add(mi);
                }
            }
        }
        /// <summary>
        /// Automatically show and hide the DIY menu
        /// </summary>
        public void LoadDIY()
        {
            // AIDeskPet: Feed+Interact removed, visible items = usage/system(+DIY) (DIY shown=3, hidden=2)
            if (MenuDIY.Items.Count > 0)
            {
                if (MenuDIY.Visibility == Visibility.Visible)
                    return;
                MenuDIY.Visibility = Visibility.Visible;
                ToolBarMenu.Tag = 3;
            }
            else
            {
                if (MenuDIY.Visibility == Visibility.Collapsed)
                    return;
                MenuDIY.Visibility = Visibility.Collapsed;
                ToolBarMenu.Tag = 2;
            }
        }
        private void MenuStudy_Click(object sender, RoutedEventArgs e)
        {
            StartWork(wstudy);
        }
        Work wwork;
        Work wstudy;
        Work wplay;


        private void MenuWork_Click(object sender, RoutedEventArgs e)
        {
            StartWork(wwork);
        }
        private void MenuPlay_Click(object sender, RoutedEventArgs e)
        {
            StartWork(wplay);
        }
        /// <summary>
        /// Refresh the display UI
        /// </summary>
        public void M_TimeUIHandle(Main m)
        {
            // AIDeskPet: Panel only has AI usage stats (spAIStats), refreshed by the AI plugin itself
        }

        private void ClosePanelTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (BdrPanel.IsMouseOver
                    || MenuPanel.IsMouseOver)
                {
                    closePanelTimer.Stop();
                    return;
                }
                BdrPanel.Visibility = Visibility.Collapsed;
            });
        }

        private void Closetimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (onFocus)
            {
                onFocus = false;
                CloseTimer.Start();
            }
            else
                Dispatcher.Invoke(() => this.Visibility = Visibility.Collapsed);
        }
        /// <summary>
        /// ToolBar show event
        /// </summary>
        public event Action EventShow;
        public void Show()
        {
            EventShow?.Invoke();
            if (m.UIGrid.Children.IndexOf(this) != m.UIGrid.Children.Count - 1)
            {
                Panel.SetZIndex(this, m.UIGrid.Children.Count);
            }
            Visibility = Visibility.Visible;
            if (CloseTimer.Enabled)
                onFocus = true;
            else
                CloseTimer.Start();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            CloseTimer.Enabled = false;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseTimer.Start();
        }

        private void MenuPanel_Click(object sender, RoutedEventArgs e)
        {
            m.Core.Controller.ShowPanel();
        }
        /// <summary>
        /// Window type
        /// </summary>
        public enum MenuType
        {
            /// <summary>
            /// Feed
            /// </summary>
            Feed,
            /// <summary>
            /// Interact
            /// </summary>
            Interact,
            /// <summary>
            /// Custom
            /// </summary>
            DIY,
            /// <summary>
            /// Settings
            /// </summary>
            Setting,
        }
        /// <summary>
        /// Add button
        /// </summary>
        /// <param name="parentMenu">Button location</param>
        /// <param name="displayName">Display name</param>
        /// <param name="clickCallback">Action</param>
        public void AddMenuButton(MenuType parentMenu,
            string displayName,
            Action clickCallback)
        {
            var menuItem = new MenuItem()
            {
                Header = displayName,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
            // AIDeskPet: In dark theme, explicitly set dynamic submenu items to bright purple foreground to avoid black text on black background
            menuItem.SetResourceReference(MenuItem.ForegroundProperty, "DARKPrimary");
            menuItem.Click += delegate
            {
                clickCallback?.Invoke();
            };
            switch (parentMenu)
            {
                case MenuType.Feed:
                    MenuFeed.Items.Add(menuItem);
                    break;
                case MenuType.Interact:
                    MenuInteract.Items.Add(menuItem);
                    break;
                case MenuType.DIY:
                    MenuDIY.Items.Add(menuItem);
                    LoadDIY();
                    break;
                case MenuType.Setting:
                    MenuSetting.Items.Add(menuItem);
                    break;
            }
        }

        // AIDeskPet: Status bar removed, related percentage text generator removed as well

        private Brush GetForeground(double value)
        {
            if (value >= .6)
            {
                return FindResource("SuccessProgressBarForeground") as Brush;
            }
            else if (value >= .3)
            {
                return FindResource("WarningProgressBarForeground") as Brush;
            }
            else
            {
                return FindResource("DangerProgressBarForeground") as Brush;
            }
        }
        /// <summary>
        /// MenuPanel show event
        /// </summary>
        public event Action EventMenuPanelShow;

        private void MenuPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            BdrPanel.Visibility = Visibility.Visible;
            M_TimeUIHandle(m);
            EventMenuPanelShow?.Invoke();
        }

        private void MenuPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            closePanelTimer.Start();
        }

        public void Dispose()
        {
            m = null;
            CloseTimer.Dispose();
            closePanelTimer.Dispose();
        }

        private void Sleep_Click(object sender, RoutedEventArgs e)
        {
            if (m.State == Main.WorkingState.Sleep)
            {
                if (m.Core.Save.Mode == IGameSave.ModeType.Ill)
                    return;
                m.State = WorkingState.Nomal;
                m.Display(GraphType.Sleep, AnimatType.C_End, m.DisplayNomal);
            }
            else if (m.State == Main.WorkingState.Nomal)
                m.DisplaySleep(true);
            else
            {
                m.WorkTimer.Stop(() => m.DisplaySleep(true), WorkTimer.FinishWorkInfo.StopReason.MenualStop);
            }
        }
    }
}
