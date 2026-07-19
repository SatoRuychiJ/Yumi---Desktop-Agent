using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static VPet_Simulator.Core.GraphHelper;
using static VPet_Simulator.Core.GraphInfo;
using static VPet_Simulator.Core.WorkTimer;
using Timer = System.Timers.Timer;

namespace VPet_Simulator.Core
{
    public partial class Main
    {
        public const int TreeRND = 5;

        /// <summary>
        /// Handles the spoken content
        /// </summary>
        [Obsolete("Use SayProcess instead")]
        public event Action<string> OnSay;
        /// <summary>
        /// Last interaction time
        /// </summary>
        public DateTime LastInteractionTime { get; set; } = DateTime.Now;
        /// <summary>
        /// Event timer
        /// </summary>
        public Timer EventTimer = new Timer(15000)
        {
            AutoReset = true,
            Enabled = true
        };
        /// <summary>
        /// Speak, using a random expression
        /// </summary>
        public void SayRnd(string text, bool force = false, string desc = null)
        {
            Say(text, SayRndFunction(text), force, desc);
        }

        /// <summary>
        /// Process sayInfo, using a random expression
        /// </summary>
        /// <param name="sayInfo">SayInfoWithStream Class, provides basic stream info and basic methods</param>
        public void SayRnd(SayInfoWithStream sayInfo)
        {
            Task.Run(() =>
            {
                while (!sayInfo.IsFinishGen && Function.ComCheck(sayInfo.CurrentText.ToString()) < 4 && sayInfo.CurrentText.Length < 80)
                {
                    Thread.Sleep(100);
                }
                sayInfo.GraphName = SayRndFunction(sayInfo.CurrentText.ToString());
                if (sayInfo.IsFinishGen)
                    Say(sayInfo.ToNoneStream().Result);
                else
                    Say(sayInfo);
            });
        }
        /// <summary>
        /// Random expression method; modify this to use a specific type of speaking expression
        /// </summary>
        public Func<string, string> SayRndFunction;
        /// <summary>
        /// Speech processing (please do not block this handler)
        /// </summary>
        public List<Action<SayInfo>> SayProcess = new List<Action<SayInfo>>();


        /// <summary>
        /// Streaming speech
        /// </summary>
        /// <param name="sayInfoWithStream">Speech info</param>
        public void Say(SayInfoWithStream sayInfoWithStream)
        {
            Task.Run(() =>
            {
                sayInfoWithStream.Event_Finish += (text) => OnSay?.Invoke(text);

                if (sayInfoWithStream.IsFinishGen)
                {
                    OnSay?.Invoke(sayInfoWithStream.CurrentText.ToString());
                }

                SayProcess.ForEach(a => a.Invoke(sayInfoWithStream));

                if (sayInfoWithStream.Force || !string.IsNullOrWhiteSpace(sayInfoWithStream.GraphName) && DisplayType.Type == GraphType.Default)// idle is not used here because idle includes studying, etc.
                    Display(sayInfoWithStream.GraphName, AnimatType.A_Start, () =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MsgBar.Show(Core.Save.Name, sayInfoWithStream);
                        });
                        DisplayBLoopingForce(sayInfoWithStream.GraphName);
                    });
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        MsgBar.Show(Core.Save.Name, sayInfoWithStream);
                    });
                }
            });
        }
        /// <summary>
        /// Normal speech
        /// </summary>
        /// <param name="sayinfo">Speech info</param>
        public void Say(SayInfoWithOutStream sayinfo)
        {
            Task.Run(() =>
            {
                OnSay?.Invoke(sayinfo.Text);

                SayProcess.ForEach(a => a.Invoke(sayinfo));

                if (sayinfo.Force || !string.IsNullOrWhiteSpace(sayinfo.GraphName) && DisplayType.Type == GraphType.Default)// idle is not used here because idle includes studying, etc.
                    Display(sayinfo.GraphName, AnimatType.A_Start, () =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MsgBar.Show(Core.Save.Name, sayinfo.Text, sayinfo.GraphName, sayinfo.MsgContent ?? (string.IsNullOrWhiteSpace(sayinfo.Desc) ? null :
                                new TextBlock() { Text = sayinfo.Desc, FontSize = 20, ToolTip = sayinfo.Desc, HorizontalAlignment = HorizontalAlignment.Right }));
                        });
                        DisplayBLoopingForce(sayinfo.GraphName);
                    });
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        MsgBar.Show(Core.Save.Name, sayinfo.Text, sayinfo.GraphName, msgContent: sayinfo.MsgContent ?? (string.IsNullOrWhiteSpace(sayinfo.Desc) ? null :
                            new TextBlock() { Text = sayinfo.Desc, FontSize = 20, ToolTip = sayinfo.Desc, HorizontalAlignment = HorizontalAlignment.Right }));
                    });
                }
            });
        }
        /// <summary>
        /// Speak
        /// </summary>
        /// <param name="text">Speech content</param>
        /// <param name="graphname">Graphic name</param>
        /// <param name="desc">Description</param>
        /// <param name="force">Force display of the graphic</param>
        public void Say(string text, string graphname = null, bool force = false, string desc = null) => Say(new SayInfoWithOutStream()
        {
            Text = text,
            GraphName = graphname,
            Desc = desc,
            Force = force,
            MsgContent = null
        });
        /// <summary>
        /// Speak
        /// </summary>
        /// <param name="text">Speech content</param>
        /// <param name="graphname">Graphic name</param>
        /// <param name="msgcontent">Message content</param>
        /// <param name="force">Force display of the graphic</param>
        public void Say(string text, UIElement msgcontent, string graphname = null, bool force = false) => Say(new SayInfoWithOutStream()
        {
            Text = text,
            GraphName = graphname,
            Desc = null,
            Force = force,
            MsgContent = msgcontent
        });

        int labeldisplaycount = 100;
        int labeldisplayhash = 0;
        Timer labeldisplaytimer = new Timer(10)
        {
            AutoReset = true,
        };
        double labeldisplaychangenum1 = 0;
        double labeldisplaychangenum2 = 0;
        /// <summary>
        /// Show the message popup Label
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="time">Duration</param>
        public void LabelDisplayShow(string text, int time = 2000)
        {
            labeldisplayhash = text.GetHashCode();
            Dispatcher.Invoke(() =>
            {
                LabelDisplayText.Text = text;
                LabelDisplay.Opacity = 1;
                LabelDisplay.Visibility = Visibility.Visible;
                labeldisplaycount = time / 10;
                labeldisplaytimer.Start();
            });
        }
        /// <summary>
        /// Show the message popup Label, automatically accumulating value changes
        /// </summary>
        /// <param name="text">Text, using {0:f2}</param>
        /// <param name="changenum1">Change value 1</param>
        /// <param name="changenum2">Change value 2</param>
        /// <param name="time">Duration</param>
        public void LabelDisplayShowChangeNumber(string text, double changenum1, double changenum2 = 0, int time = 2000)
        {
            if (labeldisplayhash == text.GetHashCode())
            {
                labeldisplaychangenum1 += changenum1;
                labeldisplaychangenum2 += changenum2;
            }
            else
            {
                labeldisplaychangenum1 = changenum1;
                labeldisplaychangenum2 = changenum2;
                labeldisplayhash = text.GetHashCode();
            }
            Dispatcher.Invoke(() =>
            {
                LabelDisplayText.Text = string.Format(text, labeldisplaychangenum1, labeldisplaychangenum2);
                LabelDisplay.Opacity = 1;
                LabelDisplay.Visibility = Visibility.Visible;
                labeldisplaycount = time / 10;
                labeldisplaytimer.Start();
            });
        }
        public Work NowWork;
        /// <summary>
        /// Calculate related data based on consumption
        /// </summary>
        /// <param name="TimePass">Elapsed time multiplier</param>
        public void FunctionSpend(double TimePass)
        {
            Core.Save.CleanChange();
            Core.Save.StoreTake();
            double freedrop = (DateTime.Now - LastInteractionTime).TotalMinutes;
            if (freedrop < 1)
                freedrop = 0;
            else
                freedrop = Math.Min(Math.Sqrt(freedrop) * TimePass / 4, Core.Save.FeelingMax / 800);
            double sm25 = Core.Save.StrengthMax * 0.25;
            double sm50 = Core.Save.StrengthMax * 0.5;
            double sm60 = Core.Save.StrengthMax * 0.6;
            double sm75 = Core.Save.StrengthMax * 0.75;

            switch (State)
            {
                case WorkingState.Empty:
                    break;
                case WorkingState.Sleep:
                    // Sleep: slowly recover everything (except mood, but mood won't drop)
                    Core.Save.StrengthChange(TimePass * 2);
                    Core.Save.StrengthChangeFood(TimePass);
                    if (Core.Save.StrengthFood <= sm25)
                    {// Low state: 2x recovery speed
                        Core.Save.StrengthChangeFood(TimePass);
                    }
                    else if (Core.Save.StrengthFood >= sm75)
                        Core.Save.Health += TimePass * 2;
                    Core.Save.StrengthChangeDrink(TimePass);
                    if (Core.Save.StrengthDrink >= sm25)
                    {
                        Core.Save.StrengthChangeDrink(TimePass);
                    }
                    else if (Core.Save.StrengthDrink >= sm75)
                        Core.Save.Health += TimePass * 2;
                    LastInteractionTime = DateTime.Now;
                    break;
                case WorkingState.Work:
                    if (NowWork == null)
                        break;
                    var needfood = TimePass * NowWork.StrengthFood;
                    var needdrink = TimePass * NowWork.StrengthDrink;

                    double efficiency = 0;
                    int addhealth = -2;


                    var nsfood = needfood * .3;
                    var nsdrink = needdrink * .3;
                    if (Core.Save.Strength > sm25 + nsfood + nsdrink)
                    {// Can use strength to reduce some consumption and increase efficiency
                        Core.Save.StrengthChange(-nsfood - nsdrink);
                        efficiency += 0.1;
                        needfood -= nsfood;
                        needdrink -= nsdrink;
                    }

                    if (Core.Save.StrengthFood <= sm25)
                    {// Low state: low efficiency
                        Core.Save.StrengthChangeFood(-needfood / 2);
                        efficiency += 0.2;
                        if (Core.Save.Strength >= needfood)
                        {
                            Core.Save.StrengthChange(-needfood);
                            efficiency += 0.1;
                        }
                        addhealth -= 2;
                    }
                    else
                    {
                        Core.Save.StrengthChangeFood(-needfood);
                        efficiency += 0.4;
                        if (Core.Save.StrengthFood >= sm60)
                        {
                            addhealth += Function.Rnd.Next(1, 3);
                            efficiency += 0.1;
                        }
                    }
                    if (Core.Save.StrengthDrink <= sm25)
                    {// Low state: low efficiency
                        Core.Save.StrengthChangeDrink(-needdrink / 2);
                        efficiency += 0.2;
                        if (Core.Save.Strength >= needdrink)
                        {
                            Core.Save.StrengthChange(-needdrink);
                            efficiency += 0.1;
                        }
                        addhealth -= 2;
                    }
                    else
                    {
                        Core.Save.StrengthChangeDrink(-needdrink);
                        efficiency += 0.4;
                        if (Core.Save.StrengthDrink >= sm60)
                        {
                            addhealth += Function.Rnd.Next(1, 3);
                            efficiency += 0.1;
                        }
                    }
                    if (addhealth > 0)
                        Core.Save.Health += addhealth * TimePass;
                    var addmoney = Math.Max(0, TimePass * NowWork.MoneyBase * (2 * efficiency - 0.5));
                    if (NowWork.Type == Work.WorkType.Work)
                        Core.Save.Money += addmoney;
                    else
                        Core.Save.Exp += addmoney;
                    WorkTimer.GetCount += addmoney;
                    if (NowWork.Type == Work.WorkType.Play)
                    {
                        LastInteractionTime = DateTime.Now;
                        Core.Save.FeelingChange(-NowWork.Feeling * TimePass);
                    }
                    else
                        Core.Save.FeelingChange(-freedrop * (0.5 + NowWork.Feeling / 2));
                    break;
                default:// Default
                    // Miscellaneous consumption such as eating and drinking
                    addhealth = -2;
                    if (Core.Save.StrengthFood >= sm50)
                    {
                        Core.Save.StrengthChangeFood(-TimePass);
                        Core.Save.StrengthChange(TimePass);
                        if (Core.Save.StrengthFood >= sm75)
                            addhealth += Function.Rnd.Next(1, 3);
                    }
                    else if (Core.Save.StrengthFood <= sm25)
                    {
                        Core.Save.Health -= Function.Rnd.NextDouble() * TimePass;
                        addhealth -= 2;
                    }
                    if (Core.Save.StrengthDrink >= sm50)
                    {
                        Core.Save.StrengthChangeDrink(-TimePass);
                        Core.Save.StrengthChange(TimePass);
                        if (Core.Save.StrengthDrink >= sm75)
                            addhealth += Function.Rnd.Next(1, 3);
                    }
                    else if (Core.Save.StrengthDrink <= sm25)
                    {
                        Core.Save.Health -= Function.Rnd.NextDouble() * TimePass;
                        addhealth -= 2;
                    }
                    if (addhealth > 0)
                        Core.Save.Health += addhealth * TimePass;
                    Core.Save.StrengthChangeFood(-TimePass);
                    Core.Save.StrengthChangeDrink(-TimePass);
                    Core.Save.FeelingChange(-freedrop);
                    break;
            }

            //if (Core.GameSave.Strength <= 40)
            //{
            //    Core.GameSave.Health -= Function.Rnd.Next(0, 1);
            //}
            Core.Save.Exp += TimePass;
            // Feeling boosts likability
            if (Core.Save.Feeling >= Core.Save.FeelingMax * 0.75)
            {
                if (Core.Save.Feeling >= Core.Save.FeelingMax * 0.90)
                {
                    Core.Save.Likability += TimePass;
                }
                Core.Save.Exp += TimePass * 2;
                Core.Save.Health += TimePass;
            }
            else if (Core.Save.Feeling <= 25) // No multiplier here; give a bit more benefit to the upper bound
            {
                Core.Save.Likability -= TimePass;
                Core.Save.Exp -= TimePass;
            }
            if (Core.Save.StrengthDrink <= sm25)
            {
                Core.Save.Health -= Function.Rnd.Next(0, 1) * TimePass;
                Core.Save.Exp -= TimePass;
            }
            else if (Core.Save.StrengthDrink >= sm75)
                Core.Save.Health += Function.Rnd.Next(0, 1) * TimePass;

            FunctionSpendHandle?.Invoke();
            var newmod = Core.Save.CalMode();
            if (Core.Save.Mode != newmod)
            {
                // Switch the displayed animation
                PlaySwitchAnimat(Core.Save.Mode, newmod);

                Core.Save.Mode = newmod;
            }
            // Play the stop-working animation depending on the situation
            if (Core.Save.Mode == IGameSave.ModeType.Ill && State == WorkingState.Work)
            {
                Dispatcher.Invoke(() => WorkTimer.Stop(reason: FinishWorkInfo.StopReason.StateFail));
            }
        }
        /// <summary>
        /// Play the switch animation
        /// </summary>
        /// <param name="before">State before switching</param>
        /// <param name="after">State after switching</param>
        public void PlaySwitchAnimat(IGameSave.ModeType before, IGameSave.ModeType after)
        {
            if (!(DisplayType.Type == GraphType.Default || DisplayType.Type == GraphType.Switch_Down || DisplayType.Type == GraphType.Switch_Up))
            {
                return;
            }
            if (before == after)
            {
                DisplayToNomal();
                return;
            }
            if (before < after)
            {
                Display(Core.Graph.FindGraph(Core.Graph.FindName(GraphType.Switch_Down), AnimatType.Single, before),
                    () => PlaySwitchAnimat((IGameSave.ModeType)(((int)before) + 1), after));
            }
            else
            {
                Display(Core.Graph.FindGraph(Core.Graph.FindName(GraphType.Switch_Up), AnimatType.Single, before),
                    () => PlaySwitchAnimat((IGameSave.ModeType)(((int)before) - 1), after));
            }
        }
        /// <summary>
        /// State calculation handler
        /// </summary>
        public event Action FunctionSpendHandle;
        /// <summary>
        /// Interface for random display requests (return: whether successful)
        /// </summary>
        public List<Func<bool>> RandomInteractionAction = new List<Func<bool>>();
        /// <summary>
        /// Determine whether it is in the idle state
        /// </summary>
        public bool IsIdel => (DisplayType.Type == GraphType.Default || DisplayType.Type == GraphType.Work) && !isPress;

        /// <summary>
        /// Automatically triggers calculation at the specified interval; can be calculated manually after disabling EventTimer
        /// </summary>
        public void EventTimer_Elapsed()
        {
            // All handlers
            TimeHandle?.Invoke(this);
            if (Core.Controller.EnableFunction)
            {
                FunctionSpend(0.05);
            }
            else
            {
                //Core.Save.Mode = GameSave.ModeType.Happy;
                //Core.GameSave.Mode = GameSave.ModeType.Ill;
                Core.Save.Mode = NoFunctionMOD;
            }

            //UIHandle
            Dispatcher.Invoke(() => TimeUIHandle?.Invoke(this));

            if (IsIdel)
            {
                int rnddisplay = Math.Max(20, Core.Controller.InteractionCycle - CountNomal);
                if (DisplayType.Type == GraphType.Work)
                    rnddisplay = 2 * rnddisplay + 20;
                switch (Function.Rnd.Next(rnddisplay))
                {
                    case 0:
                    case 1:
                    case 2:
                        // Show movement
                        DisplayMove();
                        break;
                    case 3:
                    case 4:
                    case 5:
                        // Show idle
                        DisplayIdel();
                        break;
                    case 6:
                        DisplayIdel_StateONE();
                        break;
                    case 7:
                        DisplaySleep();
                        break;
                    case 8:
                    case 9:
                    case 10:
                        // Give other displays a chance
                        var list = RandomInteractionAction.ToList();
                        for (int i = Function.Rnd.Next(list.Count); 0 != list.Count; i = Function.Rnd.Next(list.Count))
                        {
                            var act = list[i];
                            if (act.Invoke())
                            {
                                break;
                            }
                            else
                            {
                                list.RemoveAt(i);
                            }
                        }
                        break;
                }
            }
        }
        /// <summary>
        /// Edge check and re-centering detection; if near an edge, enter side-hide mode, otherwise re-center
        /// </summary>
        /// <returns>Whether it successfully entered side-hide mode</returns>
        private bool MoveSideHideCheck()
        {
            if (Core.Controller.IfInActivateScreen() == false && Core.Controller.AutoChangeWindow == true)
            {
                Core.Controller.SetNowScreenActivate();
            }
            // Check whether near an edge; if so, enter side-hide mode
            if (Core.Controller.GetWindowsDistanceLeft() < -50 * Core.Controller.ZoomRatio)
            {
                // Check whether there is a SideLoad
                if (Core.Graph.FindName(GraphType.SideHide_Left_Main) != null)
                {
                    Core.Controller.MoveWindows(-Core.Controller.GetWindowsDistanceLeft() / Core.Controller.ZoomRatio - Core.Graph.GraphConfig.Data["side"][(gdbe)"left"], 0);
                    if (Core.Controller.GetWindowsDistanceDown() < 0) Core.Controller.MoveWindows(0, Core.Controller.GetWindowsDistanceDown() / Core.Controller.ZoomRatio - 100);
                    else if (Core.Controller.GetWindowsDistanceUp() < 0) Core.Controller.MoveWindows(0, -Core.Controller.GetWindowsDistanceUp() / Core.Controller.ZoomRatio);
                    Display(GraphType.SideHide_Left_Main, AnimatType.A_Start, DisplayBLoopingForce);
                    return true;
                }
                else if (Core.Controller.RePositionActive)
                {// If not, re-center
                    Core.Controller.MoveWindows(-Core.Controller.GetWindowsDistanceLeft() / Core.Controller.ZoomRatio, 0);
                }
            }   
            else if (Core.Controller.GetWindowsDistanceRight() < -50 * Core.Controller.ZoomRatio)
            {
                if (Core.Graph.FindName(GraphType.SideHide_Right_Main) != null)
                {
                    Core.Controller.MoveWindows(Core.Controller.GetWindowsDistanceRight() / Core.Controller.ZoomRatio + 500 - Core.Graph.GraphConfig.Data["side"][(gdbe)"right"], 0);
                    if (Core.Controller.GetWindowsDistanceDown() < 0) Core.Controller.MoveWindows(0, Core.Controller.GetWindowsDistanceDown() / Core.Controller.ZoomRatio - 100);
                    else if (Core.Controller.GetWindowsDistanceUp() < 0) Core.Controller.MoveWindows(0, -Core.Controller.GetWindowsDistanceUp() / Core.Controller.ZoomRatio);
                    Display(GraphType.SideHide_Right_Main, AnimatType.A_Start, DisplayBLoopingForce);
                    return true;
                }
                else if (Core.Controller.RePositionActive)
                {
                    Core.Controller.MoveWindows(Core.Controller.GetWindowsDistanceRight() / Core.Controller.ZoomRatio, 0);
                }
            }
            return false;
        }

        /// <summary>
        /// Fixed-point movement position vector
        /// </summary>
        public Point MoveTimerPoint = new Point(0, 0);
        /// <summary>
        /// Fixed-point movement timer
        /// </summary>
        public Timer MoveTimer = new Timer();
        /// <summary>
        /// Set the calculation interval
        /// </summary>
        /// <param name="Interval">Calculation interval</param>
        public void SetLogicInterval(int Interval)
        {
            EventTimer.Interval = Interval;
        }
        private Timer SmartMoveTimer = new Timer(20 * 60)
        {
            AutoReset = true,
        };
        /// <summary>
        /// Whether smart movement is enabled
        /// </summary>
        private bool SmartMove;
        /// <summary>
        /// Set the movement mode
        /// </summary>
        /// <param name="AllowMove">Allow movement</param>
        /// <param name="smartMove">Enable smart movement</param>
        /// <param name="SmartMoveInterval">Smart movement cycle</param>
        public void SetMoveMode(bool AllowMove, bool smartMove, int SmartMoveInterval)
        {
            MoveTimer.Enabled = false;
            if (AllowMove)
            {
                MoveTimerSmartMove = true;
                if (smartMove)
                {
                    SmartMoveTimer.Interval = SmartMoveInterval;
                    SmartMoveTimer.Start();
                    SmartMove = true;
                }
                else
                {
                    SmartMoveTimer.Enabled = false;
                    SmartMove = false;
                }
            }
            else
            {
                MoveTimerSmartMove = false;
            }
        }
        /// <summary>
        /// Current state
        /// </summary>
        public WorkingState State = WorkingState.Nomal;

        /// <summary>
        /// The current state
        /// </summary>
        public enum WorkingState
        {
            /// <summary>
            /// Default: doing nothing
            /// </summary>
            Nomal,
            /// <summary>
            /// Working / studying
            /// </summary>
            Work,
            /// <summary>
            /// Sleeping
            /// </summary>
            Sleep,
            /// <summary>
            /// Traveling
            /// </summary>
            Travel,
            /// <summary>
            /// Other state; leaves a slot for developers to compute
            /// </summary>
            Empty,
        }
        /// <summary>
        /// Get the work list categories
        /// </summary>
        /// <param name="ws">All work</param>
        /// <param name="ss">All study</param>
        /// <param name="ps">All entertainment</param>
        public void WorkList(out List<Work> ws, out List<Work> ss, out List<Work> ps)
        {
            ws = new List<Work>();
            ss = new List<Work>();
            ps = new List<Work>();
            foreach (var w in Core.Graph.GraphConfig.Works)
            {
                switch (w.Type)
                {
                    case Work.WorkType.Study:
                        ss.Add(w);
                        break;
                    case Work.WorkType.Work:
                        ws.Add(w);
                        break;
                    case Work.WorkType.Play:
                        ps.Add(w);
                        break;
                }
            }
        }
        /// <summary>
        /// Work check
        /// </summary>
        public Func<Work, bool> WorkCheck;
        /// <summary>
        /// Start work
        /// </summary>
        /// <param name="work">Work content</param>
        public bool StartWork(Work work)
        {
            if (!Core.Controller.EnableFunction || Core.Save.Mode != IGameSave.ModeType.Ill)
                if (!Core.Controller.EnableFunction || Core.Save.Level >= work.LevelLimit)
                    if (State == Main.WorkingState.Work && NowWork.Name == work.Name)
                        WorkTimer.Stop(reason: FinishWorkInfo.StopReason.MenualStop);
                    else
                    {
                        if (WorkCheck != null && !WorkCheck.Invoke(work))
                            return false;
                        WorkTimer.Start(work);
                        return true;
                    }
                else
                    MessageBoxX.Show(LocalizeCore.Translate("您的桌宠等级不足{0}/{2}\n无法进行{1}", Core.Save.Level.ToString()
                        , work.NameTrans, work.LevelLimit), LocalizeCore.Translate("{0}取消", work.NameTrans));
            else
                MessageBoxX.Show(LocalizeCore.Translate("您的桌宠 {0} 生病啦,没法进行{1}", Core.Save.Name,
                  work.NameTrans), LocalizeCore.Translate("{0}取消", work.NameTrans));
            return false;
        }
        /// <summary>
        /// Called when the task starts
        /// </summary>
        public event Action<Work> Event_WorkStart;
        internal void Event_WorkStartInvoke(Work work)
        {
            Event_WorkStart?.Invoke(work);
        }
        /// <summary>
        /// Called when the task completes (redirected to WorkTimer.E_FinishWork)
        /// </summary>
        public event Action<FinishWorkInfo> Event_WorkEnd
        {
            add
            {
                WorkTimer.E_FinishWork += value;
            }
            remove
            {
                WorkTimer.E_FinishWork -= value;
            }
        }
        /// <summary>
        /// Called before movement starts (before the animation plays)
        /// </summary>
        public event Action<Move> Event_MoveStart;
        /// <summary>
        /// Called after movement ends (after the animation finishes)
        /// </summary>
        public event Action<Move> Event_MoveEnd;
        internal void Event_MoveStartInvoke(Move move)
        {
            Event_MoveStart?.Invoke(move);
        }
        internal void Event_MoveEndInvoke(Move move)
        {
            Event_MoveEnd?.Invoke(move);
        }
    }
}
