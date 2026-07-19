using LinePutScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static VPet_Simulator.Core.IGraph;

namespace VPet_Simulator.Core
{
    /// <summary>
    /// Food animation, supports a 3-layer sandwich animation (front/middle/back)
    /// Not necessarily only used for food, it is just named this way
    /// </summary>
    public class FoodAnimation : IRunImage
    {
        /// <summary>
        /// Create a food animation; the second (middle) sandwich layer is provided at runtime
        /// </summary>
        /// <param name="graphCore">Animation core</param>
        /// <param name="graphinfo">Animation info</param>
        /// <param name="front_Lay">Front layer animation name</param>
        /// <param name="back_Lay">Back layer animation name</param>
        /// <param name="animations">Middle layer motion path</param>
        /// <param name="isLoop">Whether to loop</param>
        public FoodAnimation(GraphCore graphCore, GraphInfo graphinfo, string front_Lay,
            string back_Lay, ILine animations, bool isLoop = false)
        {
            IsLoop = isLoop;
            GraphInfo = graphinfo;
            GraphCore = graphCore;
            Front_Lay = front_Lay;
            Back_Lay = back_Lay;
            Animations = new List<Animation>();
            int i = 0;
            ISub sub = animations.Find("a" + i);
            while (sub != null)
            {
                Animations.Add(new Animation(this, sub));
                sub = animations.Find("a" + ++i);
            }
            IsReady = true;
        }

        public static void LoadGraph(GraphCore graph, FileSystemInfo path, ILine info)
        {
            bool isLoop = info[(gbol)"loop"];
            FoodAnimation pa = new FoodAnimation(graph, new GraphInfo(path, info), info[(gstr)"front_lay"], info[(gstr)"back_lay"], info, isLoop);
            graph.AddGraph(pa);
        }
        /// <summary>
        /// Front layer name
        /// </summary>
        public string Front_Lay;
        /// <summary>
        /// Back layer name
        /// </summary>
        public string Back_Lay;
        /// <summary>
        /// All animation frames
        /// </summary>
        public List<Animation> Animations;

        /// <summary>
        /// Whether to loop playback
        /// </summary>
        public bool IsLoop { get; set; }

        /// <summary>
        /// Animation info
        /// </summary>
        public GraphInfo GraphInfo { get; private set; }
        /// <summary>
        /// Whether preparation is complete
        /// </summary>
        public bool IsReady { get; set; } = false;
        public bool IsFail => false;
        public string FailMessage => "";

        public TaskControl Control { get; set; }

        int nowid;
        /// <summary>
        /// Image resource
        /// </summary>
        public string Path { get; set; }
        private GraphCore GraphCore;
        /// <summary>
        /// Single-frame animation
        /// </summary>
        public class Animation
        {
            private FoodAnimation parent;
            public Thickness MarginWI;
            public double Rotate = 0;
            public double Opacity = 1;
            public bool IsVisiable = true;
            public double Width;
            /// <summary>
            /// Frame time
            /// </summary>
            public int Time;
            /// <summary>
            /// Create a single-frame animation
            /// </summary>
            /// <param name="parent">FoodAnimation</param>
            /// <param name="time">Duration</param>
            /// <param name="wx"></param>
            public Animation(FoodAnimation parent, int time, Thickness wx, double width, double rotate = 0, bool isVisiable = true, double opacity = 1)
            {
                this.parent = parent;
                Time = time;
                MarginWI = wx;
                Rotate = rotate;
                IsVisiable = isVisiable;
                Width = width;
                Opacity = opacity;
            }
            /// <summary>
            /// Create a single-frame animation
            /// </summary>
            public Animation(FoodAnimation parent, ISub sub)
            {
                this.parent = parent;
                var strs = sub.GetInfos();
                Time = int.Parse(strs[0]);//0: Time
                if (strs.Length == 1)
                    IsVisiable = false;
                else
                {//1,2: Margin X,Y
                    Width = double.Parse(strs[3]);//3:Width
                    MarginWI = new Thickness(double.Parse(strs[1]), double.Parse(strs[2]), 0, 0);
                    if (strs.Length > 4)
                    {
                        Rotate = double.Parse(strs[4]);//Rotate
                        if (strs.Length > 5)
                            Opacity = double.Parse(strs[5]);//Opacity
                    }
                }
            }
            /// <summary>
            /// Run this layer
            /// </summary>
            public void Run(FrameworkElement This, TaskControl Control)
            {
                //First show this layer
                This.Dispatcher.Invoke(() =>
                {
                    if (IsVisiable)
                    {
                        This.Visibility = Visibility.Visible;
                        This.Margin = MarginWI;
                        This.LayoutTransform = new RotateTransform(Rotate);
                        This.Opacity = Opacity;
                        This.Width = Width;
                        This.Height = Width;
                    }
                    else
                    {
                        This.Visibility = Visibility.Collapsed;
                    }

                });
                //Then wait for the frame time in milliseconds
                Thread.Sleep(Time);
                //Decide whether to proceed to the next step
                switch (Control.Type)
                {
                    case TaskControl.ControlType.Stop:
                        Control.EndAction?.Invoke();
                        return;
                    case TaskControl.ControlType.Status_Stoped:
                        return;
                    case TaskControl.ControlType.Status_Quo:
                    case TaskControl.ControlType.Continue:
                        if (++parent.nowid >= parent.Animations.Count)
                            if (parent.IsLoop)
                            {
                                parent.nowid = 0;
                                //Restart the loop animation on a new thread to avoid stackoverflow
                                Task.Run(() => parent.Animations[0].Run(This, Control));
                                return;
                            }
                            else if (Control.Type == TaskControl.ControlType.Continue)
                            {
                                Control.Type = TaskControl.ControlType.Status_Quo;
                                parent.nowid = 0;
                            }
                            else
                            {
                                //parent.endwilldo = () => parent.Dispatcher.Invoke(Hidden);
                                //parent.Dispatcher.Invoke(Hidden);
                                Control.Type = TaskControl.ControlType.Status_Stoped;
                                //Wait for the other two animations to finish
                                Control.EndAction?.Invoke(); //Event fired when the end animation runs
                                ////Delayed hide
                                //Task.Run(() =>
                                //{
                                //    Thread.Sleep(25);
                                //    parent.Dispatcher.Invoke(Hidden);
                                //});
                                return;
                            }
                        //Proceeding to the next step, hide the layer now
                        //Hide this layer
                        //parent.Dispatcher.Invoke(Hidden);
                        parent.Animations[parent.nowid].Run(This, Control);
                        return;
                }
            }
        }
        public static FoodAnimatGrid FoodGrid = new FoodAnimatGrid();
        public class FoodAnimatGrid : Grid
        {
            public FoodAnimatGrid()
            {
                Width = 500;
                Height = 500;
                VerticalAlignment = VerticalAlignment.Top;
                HorizontalAlignment = HorizontalAlignment.Left;
                Front = new Image();
                Back = new Image();
                Food = new Image
                {
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Visibility = Visibility.Collapsed,
                };
                this.Children.Add(Back);
                this.Children.Add(Food);
                this.Children.Add(Front);
            }
            public Image Front;
            public Image Food;
            public Image Back;
        }

        public void Run(Decorator parant, Action EndAction = null) => Run(parant, null, EndAction);

        public void Run(Decorator parant, ImageSource image, Action EndAction = null)
        {
            if (Control?.PlayState == true)
            {//If currently running, reset the state
                Control.Stop(() => Run(parant, EndAction));
                return;
            }
            nowid = 0;
            var NEWControl = new TaskControl(EndAction);
            Control = NEWControl;
            parant.Dispatcher.Invoke(() =>
            {
                parant.Tag = this;
                if (parant.Child != FoodGrid)
                {
                    if (FoodGrid.Parent != null)
                    {
                        ((Decorator)FoodGrid.Parent).Child = null;
                    }
                    parant.Child = FoodGrid;
                }
                var FL = GraphCore.FindGraph(Front_Lay, GraphInfo.Animat, GraphInfo.ModeType);
                var BL = GraphCore.FindGraph(Back_Lay, GraphInfo.Animat, GraphInfo.ModeType);
                var t1 = FL?.Run(FoodGrid.Front);
                var t2 = BL?.Run(FoodGrid.Back);
                if (FoodGrid.Food.Source != image)
                {
                    if (FoodGrid.Food.Source is BitmapImage bitmapImage)
                    {//Memory reclamation
                        bitmapImage.StreamSource?.Dispose();
                    }
                    FoodGrid.Food.Source = image;
                }
                t1?.Start();
                t2?.Start();
                Task.Run(() => Animations[0].Run(FoodGrid.Food, NEWControl));
            });
        }
        public void Dispose()
        {
            Animations = null;
            GraphCore = null;
        }
    }
}
