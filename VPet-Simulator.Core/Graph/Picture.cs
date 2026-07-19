using LinePutScript;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static VPet_Simulator.Core.IGraph;
using static VPet_Simulator.Core.Picture;


namespace VPet_Simulator.Core
{
    /// <summary>
    /// Interaction logic for Picture.xaml
    /// </summary>
    public class Picture : IImageRun
    {
        /// <summary>
        /// Create a new static image
        /// </summary>
        /// <param name="path">Image path</param>
        public Picture(GraphCore graphCore, string path, GraphInfo graphinfo, int length = 1000, bool isloop = false)
        {
            GraphInfo = graphinfo;
            IsLoop = isloop;
            Length = length;
            GraphCore = graphCore;
            Path = path;
            if (!GraphCore.CommConfig.ContainsKey("PIC_Setup"))
            {
                GraphCore.CommConfig["PIC_Setup"] = true;
                GraphCore.Dispatcher.Invoke(() =>
                {
                    GraphCore.CommUIElements["Image1.Picture"] = new Image() { Width = 500, Height = 500 };
                    GraphCore.CommUIElements["Image2.Picture"] = new Image() { Width = 500, Height = 500 };
                    GraphCore.CommUIElements["Image3.Picture"] = new Image() { Width = 500, Height = 500 };
                });
            }
            IsReady = true;
        }
        public static void LoadGraph(GraphCore graph, FileSystemInfo path, ILine info)
        {
            if (!(path is FileInfo))
            {
                PNGAnimation.LoadGraph(graph, path, info);
                return;
            }
            if (path.Extension != ".png")
                return;
            int length = info.GetInt("length");
            if (length == 0)
            {
                var nameParts = path.Name.Split('.');
                if (nameParts.Length > 1 && !int.TryParse(nameParts[nameParts.Length - 2].Split('_').Last(), out length))
                    length = 1000;
            }
            bool isLoop = info[(gbol)"loop"];
            Picture pa = new Picture(graph, path.FullName, new GraphInfo(path, info), length, isLoop);
            graph.AddGraph(pa);
        }
        /// <summary>
        /// Image resource
        /// </summary>
        public string Path { get; set; }
        private GraphCore GraphCore;
        public bool IsLoop { get; set; }
        /// <summary>
        /// Playback duration in milliseconds
        /// </summary>
        public int Length { get; set; }
        //public bool StoreMemory => true;//After testing, storing in memory has many benefits; not storing still uses a lot of memory, so just store it

        /// <summary>
        /// Animation info
        /// </summary>
        public GraphInfo GraphInfo { get; private set; }

        public bool IsReady { get; set; } = false;

        public TaskControl Control { get; set; }

        public bool IsFail => false;

        public string FailMessage => "";

        public void Run(Decorator parant, Action EndAction = null)
        {
            if (Control?.PlayState == true)
            {//If currently running, reset the state
                Control.SetContinue();
                Control.EndAction = EndAction;
                return;
            }
            var NEWControl = new TaskControl(EndAction);
            Control = NEWControl;

            parant.Dispatcher.Invoke(() =>
            {
                if (parant.Tag != this)
                {
                    Image img;
                    if (parant.Child == GraphCore.CommUIElements["Image1.Picture"])
                    {
                        img = (Image)GraphCore.CommUIElements["Image1.Picture"];
                    }
                    else if (parant.Child == GraphCore.CommUIElements["Image3.Picture"])
                    {
                        img = (Image)GraphCore.CommUIElements["Image3.Picture"];
                    }
                    else
                    {
                        img = (Image)GraphCore.CommUIElements["Image2.Picture"];
                        if (parant.Child != img)
                        {
                            if (img.Parent == null)
                            {
                                parant.Child = img;
                            }
                            else
                            {
                                img = (Image)GraphCore.CommUIElements["Image1.Picture"];
                                if (img.Parent != null)
                                    ((Decorator)img.Parent).Child = null;
                                parant.Child = img;
                            }
                        }
                    }
                    img.Width = 500;
                    img.Source = new BitmapImage(new Uri(Path));
                    parant.Tag = this;
                }
                Task.Run(() => Run(NEWControl));
            });
        }
        /// <summary>
        /// Run through the controller
        /// </summary>
        /// <param name="Control"></param>
        public void Run(TaskControl Control)
        {
            Thread.Sleep(Length);
            //Determine whether to proceed to the next step
            switch (Control.Type)
            {
                case TaskControl.ControlType.Stop:
                    Control.EndAction?.Invoke();
                    return;
                case TaskControl.ControlType.Status_Stoped:
                    return;
                case TaskControl.ControlType.Continue:
                    Control.Type = TaskControl.ControlType.Status_Quo;
                    Run(Control);
                    return;
                case TaskControl.ControlType.Status_Quo:
                    if (IsLoop)
                    {
                        Task.Run(() => Run(Control));
                    }
                    else
                    {
                        Control.Type = TaskControl.ControlType.Status_Stoped;
                        Control.EndAction?.Invoke(); //Event triggered when the animation finishes running
                    }
                    return;
            }
        }

        public Task Run(Image img, Action EndAction = null)
        {
            if (Control?.PlayState == true)
            {//If currently running, reset the state
                Control.EndAction = null;
                Control.Type = TaskControl.ControlType.Stop;
            }
            Control = new TaskControl(EndAction);
            return img.Dispatcher.Invoke(() =>
            {
                if (img.Tag == this)
                {
                    return new Task(() => Run(Control));
                }
                img.Tag = this;
                img.Source = new BitmapImage(new Uri(Path));
                img.Width = 500;
                return new Task(() => Run(Control));
            });
        }
        /// <summary>
        /// This animation can be run through the picture module
        /// </summary>
        public interface IImageRun : IGraph
        {
            /// <summary>
            /// Specify the image control to prepare running this animation
            /// </summary>
            /// <param name="img">Image used for display</param>
            /// <param name="EndAction">End animation</param>
            /// <returns>The prepared thread</returns>
            Task Run(System.Windows.Controls.Image img, Action EndAction = null);
        }
        public void Dispose()
        {
            GraphCore = null;
        }
    }


}
