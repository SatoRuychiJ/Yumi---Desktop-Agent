using LinePutScript.Localization.WPF;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using static VPet_Simulator.Core.GraphHelper;
using static VPet_Simulator.Core.GraphInfo;
using static VPet_Simulator.Core.WorkTimer.FinishWorkInfo;

namespace VPet_Simulator.Core
{
    /// <summary>
    /// Interaction logic for WorkTimer.xaml
    /// </summary>
    public partial class WorkTimer : Viewbox
    {
        Main m;
        public WorkTimer(Main m)
        {
            InitializeComponent();
            this.m = m;
            //data-related calculations moved to MainLogic
            //this only displays the UI
            m.TimeUIHandle += M_TimeUIHandle;
        }
        /// <summary>
        /// Display mode
        /// 0 = default
        /// 1 = remaining time
        /// 2 = earned (money/level)
        /// </summary>
        public int DisplayType = 0;
        /// <summary>
        /// Accumulated money/experience earned
        /// </summary>
        public double GetCount;
        /// <summary>
        /// Start time
        /// </summary>
        public DateTime StartTime;
        /// <summary>
        /// Finished work info
        /// </summary>
        public struct FinishWorkInfo
        {
            /// <summary>
            /// The completed work
            /// </summary>
            public Work work;
            /// <summary>
            /// Income from the completed work
            /// </summary>
            public double count;
            /// <summary>
            /// Time spent on the completed work (minutes)
            /// </summary>
            public double spendtime;
            /// <summary>
            /// Reason for stopping the work
            /// </summary>
            public enum StopReason
            {
                /// <summary>
                /// Completed by time running out
                /// </summary>
                TimeFinish,
                /// <summary>
                /// Manually stopped by the player
                /// </summary>
                MenualStop,
                /// <summary>
                /// Stopped due to state, etc.
                /// </summary>
                StateFail,
                /// <summary>
                /// Other reasons
                /// </summary>
                Other,
            }
            /// <summary>
            /// Stop reason
            /// </summary>
            public StopReason Reason;
            /// <summary>
            /// Finished work info
            /// </summary>
            /// <param name="work">the current work</param>
            /// <param name="count">current profit (bonus calculated automatically)</param>
            public FinishWorkInfo(Work work, double count, StopReason reason)
            {
                this.work = work;
                this.count = count * (1 + work.FinishBonus);
                this.spendtime = work.Time;
                this.Reason = reason;
            }
            /// <summary>
            /// Finished work info
            /// </summary>
            /// <param name="work">the current work</param>
            /// <param name="count">current profit (bonus calculated automatically)</param>
            public FinishWorkInfo(Work work, double count, DateTime starttime, StopReason reason)
            {
                this.work = work;
                this.count = count * (1 + work.FinishBonus);
                this.spendtime = DateTime.Now.Subtract(starttime).TotalMinutes;
                this.Reason = reason;
            }
        }
        /// <summary>
        /// UI-related display
        /// </summary>
        /// <param name="m"></param>
        private void M_TimeUIHandle(Main m)
        {
            if (Visibility == Visibility.Collapsed) return;
            TimeSpan ts = DateTime.Now - StartTime;
            TimeSpan tleft;
            if (ts.TotalMinutes > m.NowWork.Time)
            {
                //finished studying, stop
                //ts = TimeSpan.FromMinutes(MaxTime);
                //tleft = TimeSpan.Zero;
                //PBLeft.Value = MaxTime;
                FinishWorkInfo fwi = new FinishWorkInfo(m.NowWork, GetCount, FinishWorkInfo.StopReason.TimeFinish);
                if (m.NowWork.Type == Work.WorkType.Work)
                {
                    m.Core.Save.Money += GetCount * m.NowWork.FinishBonus;
                    Stop(() => m.SayRnd(LocalizeCore.Translate("{2}完成啦, 累计赚了 {0:f2} 金钱\n共计花费了{1}分钟", fwi.count,
                        fwi.spendtime, fwi.work.NameTrans), true), StopReason.TimeFinish);
                }
                else
                {
                    m.Core.Save.Exp += GetCount * m.NowWork.FinishBonus;
                    Stop(() => m.SayRnd(LocalizeCore.Translate("{2}完成啦, 累计获得 {0:f2} 经验\n共计花费了{1}分钟", fwi.count,
                        fwi.spendtime, fwi.work.NameTrans), true), StopReason.TimeFinish);
                }
                return;
            }
            else
            {
                tleft = TimeSpan.FromMinutes(m.NowWork.Time) - ts;
                PBLeft.Value = ts.TotalMinutes;
            }
            switch (DisplayType)
            {
                default:
                case 0:
                    ShowTimeSpan(ts); break;
                case 1:
                    ShowTimeSpan(tleft); break;
                case 2:
                    tNumber.Text = GetCount.ToString("f0");
                    if (m.NowWork.Type == Work.WorkType.Work)
                        tNumberUnit.Text = LocalizeCore.Translate("钱");
                    else
                        tNumberUnit.Text = "EXP";
                    break;
                case 3:
                    break;
            }
        }
        public void ShowTimeSpan(TimeSpan ts)
        {
            if (ts.TotalSeconds < 90)
            {
                tNumber.Text = ts.TotalSeconds.ToString("f1");
                tNumberUnit.Text = LocalizeCore.Translate("秒");
            }
            else if (ts.TotalMinutes < 90)
            {
                tNumber.Text = ts.TotalMinutes.ToString("f1");
                tNumberUnit.Text = LocalizeCore.Translate("分钟");
            }
            else
            {
                tNumber.Text = ts.TotalHours.ToString("f1");
                tNumberUnit.Text = LocalizeCore.Translate("小时");
            }
        }
        public void DisplayUI()
        {
            if (DisplayType == 3)
            {
                btnSwitch.Opacity = 0.5;
                DisplayBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnSwitch.Opacity = 1;
                DisplayBorder.Visibility = Visibility.Visible;
                btnStop.Content = LocalizeCore.Translate("停止") + m.NowWork.NameTrans;
                switch (DisplayType)
                {
                    default:
                    case 0:
                        tNow.Text = LocalizeCore.Translate("当前已{0}", m.NowWork.NameTrans);
                        break;
                    case 1:
                        tNow.Text = LocalizeCore.Translate("剩余{0}时间", m.NowWork.NameTrans);
                        break;
                    case 2:
                        if (m.NowWork.Type == Work.WorkType.Work)
                            tNow.Text = LocalizeCore.Translate("累计金钱收益");
                        else
                            tNow.Text = LocalizeCore.Translate("获得经验值");
                        break;
                }
            }
            M_TimeUIHandle(m);
        }
        private void SwitchState_Click(object sender, RoutedEventArgs e)
        {
            DisplayType++;
            if (DisplayType >= 4)
                DisplayType = 0;
            DisplayUI();
        }
        public void Start(Work work)
        {
            //if (state == Main.WorkingState.Nomal)
            //    return;
            Visibility = Visibility.Visible;
            m.State = Main.WorkingState.Work;
            m.NowWork = work;
            StartTime = DateTime.Now;
            GetCount = 0;

            work.SetStyle(this);
            work.Display(m);
            m.Event_WorkStartInvoke(work);

            PBLeft.Maximum = work.Time;
            DisplayUI();
        }
        /// <summary>
        /// Stop working
        /// </summary>
        /// <param name="then"></param>
        public void Stop(Action @then = null, StopReason reason = StopReason.MenualStop)
        {
            if (m.State == Main.WorkingState.Work && m.NowWork != null)
            {
                FinishWorkInfo fwi = new FinishWorkInfo(m.NowWork, GetCount, StartTime, reason);
                E_FinishWork?.Invoke(fwi);
            }
            Visibility = Visibility.Collapsed;
            m.State = Main.WorkingState.Nomal;
            m.Display(m.NowWork.Graph, AnimatType.C_End, then ?? m.DisplayNomal);
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Stop(reason: StopReason.MenualStop);
        }
        /// <summary>
        /// Invoked when the task is completed
        /// </summary>
        public event Action<FinishWorkInfo> E_FinishWork;
    }
}
