using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Dictionary;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using static VPet_Simulator.Core.GraphHelper;
using static VPet_Simulator.Core.GraphInfo;

namespace VPet_Simulator.Core
{
    /// <summary>
    /// Image display core
    /// </summary>
    public class GraphCore : IDisposable
    {
        /// <summary>
        /// Rendering resolution of the pet graphics; higher means sharper
        /// </summary>
        public int Resolution { get; set; } = 1000;
        /// <summary>
        /// Animation cache idle timeout; animations unused past this time are released, in Ticks
        /// </summary>
        public long IdleCacheTimeout = TimeSpan.FromMinutes(2).Ticks;

        public readonly Dispatcher Dispatcher;
        public readonly Timer CleanTimer;
        public GraphCore(int resolution, Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
            CommConfig["Cache"] = new List<string>();
            Resolution = resolution;
            CleanTimer = new Timer((_) =>
            {
                if (GraphsALL == null)
                    return;
                long cleanTicks = DateTime.Now.Ticks - IdleCacheTimeout;
                for (int i = 0; i < GraphsALL.Count; i++)
                {
                    GraphsALL[i].CleanupIdleCache(cleanTicks);
                }
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        public static string CachePath = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName + @"\cache";

        /// <summary>
        /// Image name dictionary: animation type -> animation name
        /// </summary>
        public Dictionary<GraphType, HashSet<string>> GraphsName = new Dictionary<GraphType, HashSet<string>>();
        /// <summary>
        /// Image dictionary: animation name -> mode+action -> animation
        /// </summary>
        public Dictionary<string, Dictionary<AnimatType, List<IGraph>>> GraphsList = new Dictionary<string, Dictionary<AnimatType, List<IGraph>>>();
        /// <summary>
        /// List of all images, used for releasing resources
        /// </summary>
        public List<IGraph> GraphsALL = new List<IGraph>();
        /// <summary>
        /// Common UI resources
        /// </summary>
        public Dictionary<string, UIElement> CommUIElements = new Dictionary<string, UIElement>();
        /// <summary>
        /// Common config properties/methods
        /// </summary>
        public Dictionary<string, object> CommConfig = new Dictionary<string, object>();
        /// <summary>
        /// Add an animation
        /// </summary>
        /// <param name="graph">animation</param>
        public void AddGraph(IGraph graph)
        {
            if (graph.GraphInfo.Type != GraphType.Common)
            {
                if (!GraphsName.TryGetValue(graph.GraphInfo.Type, out var d2))
                {
                    d2 = new HashSet<string>();
                    GraphsName.Add(graph.GraphInfo.Type, d2);
                }
                d2.Add(graph.GraphInfo.Name);
            }
            if (!GraphsList.TryGetValue(graph.GraphInfo.Name, out var d3))
            {
                d3 = new Dictionary<AnimatType, List<IGraph>>();
                GraphsList.Add(graph.GraphInfo.Name, d3);
            }
            if (!d3.TryGetValue(graph.GraphInfo.Animat, out var l3))
            {
                l3 = new List<IGraph>();
                d3.Add(graph.GraphInfo.Animat, l3);
            }
            l3.Add(graph);
            GraphsALL.Add(graph);
        }

        /// <summary>
        /// Get a random animation name
        /// </summary>
        /// <param name="type">animation type</param>
        /// <returns>animation name, or null if not found</returns>
        public string FindName(GraphType type)
        {
            if (GraphsName.TryGetValue(type, out var gl))
            {
                return gl.ElementAt(Function.Rnd.Next(gl.Count));
            }
            return null;
        }
        /// <summary>
        /// Find an animation
        /// </summary>
        /// <param name="GraphName">animation name</param>
        /// <param name="mode">mode type; if not found, falls back to the same animation type</param>
        /// <param name="animat">animation action: Start Loop End</param>
        public IGraph FindGraph(string GraphName, AnimatType animat, IGameSave.ModeType mode)
        {
            if (GraphName == null)
                return null;
            if (GraphsList.TryGetValue(GraphName, out var d3) && d3.TryGetValue(animat, out var gl))
            {
                var list = gl.FindAll(x => x.GraphInfo.ModeType == mode);
                if (list.Count > 0)
                {
                    if (list.Count == 1)
                        return list[0];
                    return list[Function.Rnd.Next(list.Count)];
                }
                if (mode == IGameSave.ModeType.Ill)
                {
                    return null;
                }
                int i = (int)mode + 1;
                if (i < 3)
                {
                    //downward-compatible animation
                    list = gl.FindAll(x => x.GraphInfo.ModeType == (IGameSave.ModeType)i);
                    if (list.Count > 0)
                        return list[Function.Rnd.Next(list.Count)];
                }
                i = (int)mode - 1;
                if (i >= 1)
                {
                    //upward-compatible animation
                    list = gl.FindAll(x => x.GraphInfo.ModeType == (IGameSave.ModeType)i);
                    if (list.Count > 0)
                        return list[Function.Rnd.Next(list.Count)];
                }
                //if still not found, fall back to a random one (excluding ill)
                list = gl.FindAll(x => x.GraphInfo.ModeType != IGameSave.ModeType.Ill);
                if (list.Count > 0)
                    return list[Function.Rnd.Next(list.Count)];
            }
            return null;// FindGraph(GraphType.Default, mode);
        }
        /// <summary>
        /// Find an animation list
        /// </summary>
        /// <param name="mode">mode type; if not found, falls back to the same animation type</param>
        /// <param name="animat">animation action: Start Loop End</param>
        public List<IGraph> FindGraphs(string GraphName, AnimatType animat, IGameSave.ModeType mode)
        {
            if (GraphName == null)
                return null;
            if (GraphsList.TryGetValue(GraphName, out var d3) && d3.TryGetValue(animat, out var gl))
            {
                var list = gl.FindAll(x => x.GraphInfo.ModeType == mode);
                if (list.Count > 0)
                {
                    return list;
                }
                int i = (int)mode + 1;
                if (i < 3)
                {
                    //downward-compatible animation
                    list = gl.FindAll(x => x.GraphInfo.ModeType == (IGameSave.ModeType)i);
                    if (list.Count > 0)
                        return list;
                }
                i = (int)mode - 1;
                if (i >= 0)
                {
                    //upward-compatible animation
                    list = gl.FindAll(x => x.GraphInfo.ModeType == (IGameSave.ModeType)i);
                    if (list.Count > 0)
                        return list;
                }
                //if still not found, fall back to a random one
                //if (mode != GameSave.ModeType.Ill)
                //{
                list = gl;
                if (list.Count > 0)
                    return list;
                //}                
            }
            return new List<IGraph>();// FindGraph(GraphType.Default, mode);
        }

        public void Dispose()
        {
            CleanTimer.Dispose();
            GraphConfig = null;
            if (GraphsALL != null)
                foreach (var graph in GraphsALL)
                {
                    graph.Dispose();
                }
            GraphsALL.Clear();
            GraphsList.Clear();
            GraphsName.Clear();
            CommUIElements.Clear();
            CommConfig.Clear();
            GraphsALL = null;
            CommConfig = null;
            CommUIElements = null;
            GraphsName = null;
            GraphsList = null;
        }

        public Config GraphConfig;
        /// <summary>
        /// Animation settings
        /// </summary>
        public class Config
        {
            /// <summary>
            /// Head-pat trigger position
            /// </summary>
            public Point TouchHeadLocate;
            /// <summary>
            /// Pick-up trigger position
            /// </summary>
            public Point[] TouchRaisedLocate;
            /// <summary>
            /// Head-pat trigger size
            /// </summary>
            public Size TouchHeadSize;
            /// <summary>
            /// Body-pat trigger position
            /// </summary>
            public Point TouchBodyLocate;
            /// <summary>
            /// Body-pat trigger size
            /// </summary>
            public Size TouchBodySize;
            /// <summary>
            /// Pick-up trigger size
            /// </summary>
            public Size[] TouchRaisedSize;

            /// <summary>
            /// Pick-up anchor point
            /// </summary>
            public Point[] RaisePoint;

            /// <summary>
            /// All movements
            /// </summary>
            public List<Move> Moves = new List<Move>();

            /// <summary>
            /// All work/study
            /// </summary>
            public List<Work> Works = new List<Work>();

            public Line_D Str;
            /// <summary>
            /// Duration
            /// </summary>
            public Line_D Duration;
            /// <summary>
            /// Get duration
            /// </summary>
            /// <param name="name">animation name</param>
            /// <returns>duration</returns>
            public int GetDuration(string name) => Duration.GetInt(name ?? "", 10);
            /// <summary>
            /// Get the text stored in Str (translated)
            /// </summary>
            /// <param name="name">key name</param>
            /// <returns>stored text (translated)</returns>
            public string StrGetString(string name) => LocalizeCore.Translate(Str.GetString(name));
            /// <summary>
            /// Remaining settings data
            /// </summary>
            public LPS_D Data;
            /// <summary>
            /// Initialize settings
            /// </summary>
            /// <param name="lps"></param>
            public Config(LpsDocument lps)
            {
                TouchHeadLocate = new Point(lps["touchhead"][(gdbe)"px"], lps["touchhead"][(gdbe)"py"]);
                TouchHeadSize = new Size(lps["touchhead"][(gdbe)"sw"], lps["touchhead"][(gdbe)"sh"]);
                TouchBodyLocate = new Point(lps["touchbody"][(gdbe)"px"], lps["touchbody"][(gdbe)"py"]);
                TouchBodySize = new Size(lps["touchbody"][(gdbe)"sw"], lps["touchbody"][(gdbe)"sh"]);
                TouchRaisedLocate = new Point[] {
                    new Point(lps["touchraised"][(gdbe)"happy_px"], lps["touchraised"][(gdbe)"happy_py"]),
                    new Point(lps["touchraised"][(gdbe)"nomal_px"], lps["touchraised"][(gdbe)"nomal_py"]),
                    new Point(lps["touchraised"][(gdbe)"poorcondition_px"], lps["touchraised"][(gdbe)"poorcondition_py"]),
                    new Point(lps["touchraised"][(gdbe)"ill_px"], lps["touchraised"][(gdbe)"ill_py"])
                };
                TouchRaisedSize = new Size[] {
                    new Size(lps["touchraised"][(gdbe)"happy_sw"], lps["touchraised"][(gdbe)"happy_sh"]),
                    new Size(lps["touchraised"][(gdbe)"nomal_sw"], lps["touchraised"][(gdbe)"nomal_sh"]),
                    new Size(lps["touchraised"][(gdbe)"poorcondition_sw"], lps["touchraised"][(gdbe)"poorcondition_sh"]),
                    new Size(lps["touchraised"][(gdbe)"ill_sw"], lps["touchraised"][(gdbe)"ill_sh"])
                };
                RaisePoint = new Point[] {
                    new Point(lps["raisepoint"][(gdbe)"happy_x"], lps["raisepoint"][(gdbe)"happy_y"]),
                    new Point(lps["raisepoint"][(gdbe)"nomal_x"], lps["raisepoint"][(gdbe)"nomal_y"]),
                    new Point(lps["raisepoint"][(gdbe)"poorcondition_x"], lps["raisepoint"][(gdbe)"poorcondition_y"]),
                    new Point(lps["raisepoint"][(gdbe)"ill_x"], lps["raisepoint"][(gdbe)"ill_y"])
                };

                foreach (var line in lps.FindAllLine("work"))
                {
                    Works.Add(LPSConvert.DeserializeObject<Work>(line));
                }
                foreach (var line in lps.FindAllLine("move"))
                {
                    Moves.Add(LPSConvert.DeserializeObject<Move>(line));
                }
                Str = new Line_D(lps["str"]);
                Duration = new Line_D(lps["duration"]);
                Data = new LPS_D(lps);
            }
            /// <summary>
            /// Load additional settings; new values replace later ones, empty content allowed
            /// </summary>
            public void Set(LpsDocument lps)
            {
                if (lps.FindLine("touchhead") != null && lps["touchhead"][(gdbe)"py"] != 0)
                {
                    TouchHeadLocate = new Point(lps["touchhead"][(gdbe)"px"], lps["touchhead"][(gdbe)"py"]);
                    TouchHeadSize = new Size(lps["touchhead"][(gdbe)"sw"], lps["touchhead"][(gdbe)"sh"]);
                }
                if (lps.FindLine("touchbody") != null && lps["touchbody"][(gdbe)"py"] != 0)
                {
                    TouchBodyLocate = new Point(lps["touchbody"][(gdbe)"px"], lps["touchbody"][(gdbe)"py"]);
                    TouchBodySize = new Size(lps["touchbody"][(gdbe)"sw"], lps["touchbody"][(gdbe)"sh"]);
                }

                if (lps.FindLine("touchraised") != null)
                {
                    if (lps["touchraised"][(gdbe)"happy_py"] != 0)
                        TouchRaisedLocate = new Point[] {
                        new Point(lps["touchraised"].GetDouble("happy_px", TouchRaisedLocate[0].X), lps["touchraised"].GetDouble("happy_py", TouchRaisedLocate[0].Y)),
                        new Point(lps["touchraised"].GetDouble("nomal_px", TouchRaisedLocate[1].X), lps["touchraised"].GetDouble("nomal_py", TouchRaisedLocate[1].Y)),
                        new Point(lps["touchraised"].GetDouble("poorcondition_px", TouchRaisedLocate[2].X), lps["touchraised"].GetDouble("poorcondition_py", TouchRaisedLocate[2].Y)),
                        new Point(lps["touchraised"].GetDouble("ill_px", TouchRaisedLocate[3].X), lps["touchraised"].GetDouble("ill_py", TouchRaisedLocate[3].Y))
                    };
                    if (lps["touchraised"][(gdbe)"happy_sh"] != 0)
                        TouchRaisedSize = new Size[] {
                        new Size(lps["touchraised"].GetDouble("happy_sw", TouchRaisedSize[0].Width), lps["touchraised"].GetDouble("happy_sh", TouchRaisedSize[0].Height)),
                        new Size(lps["touchraised"].GetDouble("nomal_sw", TouchRaisedSize[1].Width), lps["touchraised"].GetDouble("nomal_sh", TouchRaisedSize[1].Height)),
                        new Size(lps["touchraised"].GetDouble("poorcondition_sw", TouchRaisedSize[2].Width), lps["touchraised"].GetDouble("poorcondition_sh", TouchRaisedSize[2].Height)),
                        new Size(lps["touchraised"].GetDouble("ill_sw", TouchRaisedSize[3].Width), lps["touchraised"].GetDouble("ill_sh", TouchRaisedSize[3].Height))
                    };
                }
                if (lps.FindLine("raisepoint") != null && lps["raisepoint"][(gdbe)"happy_y"] != 0)
                {
                    RaisePoint = new Point[] {
                    new Point(lps["raisepoint"].GetDouble("happy_x",RaisePoint[0].X), lps["raisepoint"].GetDouble("happy_y",RaisePoint[0].Y)),
                    new Point(lps["raisepoint"].GetDouble ("nomal_x",RaisePoint[1].X), lps["raisepoint"].GetDouble("nomal_y",RaisePoint[1].Y)),
                    new Point(lps["raisepoint"].GetDouble("poorcondition_x",RaisePoint[2].X), lps["raisepoint"].GetDouble("poorcondition_y",RaisePoint[2].Y)),
                    new Point(lps["raisepoint"].GetDouble("ill_x",RaisePoint[3].X), lps["raisepoint"].GetDouble("ill_y",RaisePoint[3].Y))};
                }

                Str.AddRange(lps["str"]);
                Duration.AddRange(lps["duration"]);

                foreach (var line in lps.FindAllLine("work"))
                {
                    Works.Add(LPSConvert.DeserializeObject<Work>(line));
                }
                foreach (var line in lps.FindAllLine("move"))
                {
                    Moves.Add(LPSConvert.DeserializeObject<Move>(line));
                }
                foreach (var line in lps)
                {
                    if (!string.IsNullOrEmpty(line.info))
                        Data.Add(line);
                }
            }
        }

    }
}
