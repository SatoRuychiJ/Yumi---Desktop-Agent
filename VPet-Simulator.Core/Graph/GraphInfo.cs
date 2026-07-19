using LinePutScript;
using System;
using System.IO;
using System.Linq;
using static VPet_Simulator.Core.GraphHelper;

namespace VPet_Simulator.Core
{
    /// <summary>
    /// Animation info
    /// </summary>
    /// New-version animation type is defined by overall type + name
    /// Animation type -> animation name
    /// Animation name -> state + action -> animation
    /// Type: main action category
    /// Name: user-defined; same-name animations share randomness, no longer uses StoreRand
    /// Action: animation action Start Loop End
    /// State: animation state Save.GameSave.ModeType
    public class GraphInfo
    {
        /// <summary>
        /// Empty animation info used for Convert
        /// </summary>
        public GraphInfo()
        {

        }
        /// <summary>
        /// Create animation info
        /// </summary>
        /// <param name="name">Name: user-defined; same-name animations share randomness, no longer uses StoreRand</param>
        /// <param name="animat">Action: animation action Start Loop End</param>
        /// <param name="type">Type: main action category</param>
        /// <param name="modeType">State: 4 states</param>
        public GraphInfo(string name, GraphType type = GraphType.Common, AnimatType animat = AnimatType.Single, IGameSave.ModeType modeType = IGameSave.ModeType.Nomal)
        {
            Name = name;
            Animat = animat;
            Type = type;
            ModeType = modeType;
        }
        /// <summary>
        /// Get animation info from file location and info
        /// </summary>
        /// <param name="path">Folder location</param>
        /// <param name="info">Info</param>
        public GraphInfo(FileSystemInfo path, ILine info)
        {
            string pn;
            if (path is DirectoryInfo)
                pn = Sub.Split(path.FullName.ToLowerInvariant(), info[(gstr)"startuppath"].ToLowerInvariant()).Last();
            else
                pn = Sub.Split(path.FullName.Substring(0, path.FullName.Length - path.Extension.Length).ToLowerInvariant(), info[(gstr)"startuppath"].ToLowerInvariant()).Last();

            var path_name = pn.Replace('\\', '_').Split('_').ToList();
            path_name.RemoveAll(string.IsNullOrWhiteSpace);
            if (!Enum.TryParse(info[(gstr)"mode"], true, out IGameSave.ModeType modetype))
            {
                if (path_name.Remove("happy"))
                {
                    modetype = IGameSave.ModeType.Happy;
                }
                else if (path_name.Remove("nomal"))
                {
                    modetype = IGameSave.ModeType.Nomal;
                }
                else if (path_name.Remove("poorcondition"))
                {
                    modetype = IGameSave.ModeType.PoorCondition;
                }
                else if (path_name.Remove("ill"))
                {
                    modetype = IGameSave.ModeType.Ill;
                }
                else
                {
                    modetype = IGameSave.ModeType.Nomal;
                }
            }

            if (!Enum.TryParse(info[(gstr)"graph"], true, out GraphType graphtype))
            {
                graphtype = GraphInfo.GraphType.Common;
                for (int i = 0; i < GraphTypeValue.Length; i++)
                {//find the first match one by one
                    if (path_name.Contains(GraphTypeValue[i][0]))
                    {
                        int index = path_name.IndexOf(GraphTypeValue[i][0]);
                        bool ismatch = true;
                        for (int b = 1; b < GraphTypeValue[i].Length && b + index < path_name.Count; b++)
                        {
                            if (path_name[index + b] != GraphTypeValue[i][b])
                            {
                                ismatch = false;
                                break;
                            }
                        }
                        if (ismatch)
                        {
                            graphtype = (GraphType)i;
                            path_name.RemoveRange(index, GraphTypeValue[i].Length);
                            break;
                        }
                    }
                }
            }

            if (!Enum.TryParse(info[(gstr)"animat"], true, out AnimatType animatType))
            {
                if (path_name.Remove("a") || path_name.Remove("start"))
                {
                    animatType = AnimatType.A_Start;
                }
                else if (path_name.Remove("b") || path_name.Remove("loop"))
                {
                    animatType = AnimatType.B_Loop;
                }
                else if (path_name.Remove("c") || path_name.Remove("end"))
                {
                    animatType = AnimatType.C_End;
                }
                else if (path_name.Remove("single"))
                {
                    animatType = AnimatType.Single;
                }
                else
                {
                    animatType = AnimatType.Single;
                }
            }
            Name = info.Info;
            if (string.IsNullOrWhiteSpace(Name))
            {
                while (path_name.Count > 0 && (double.TryParse(path_name.Last(), out _) || path_name.Last().StartsWith("~")))
                {
                    path_name.RemoveAt(path_name.Count - 1);
                }
                if (path_name.Count > 0)
                    Name = path_name.Last();
            }
            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = graphtype.ToString().ToLowerInvariant();
            }
            Type = graphtype;
            Animat = animatType;
            ModeType = modetype;
        }
        /// <summary>
        /// Type: main action category
        /// </summary>
        /// * marks required animations
        public enum GraphType
        {
            /// <summary>
            /// Common animation, used by other animations or mods etc.
            /// </summary>
            /// Not enabled/used by default, not included in GrapType
            Common,
            /// <summary>
            /// Raised dynamic *
            /// </summary>
            Raised_Dynamic,
            /// <summary>
            /// Raised static (Start & Loop & End) *
            /// </summary>
            Raised_Static,
            /// <summary>
            /// Everything that moves is now MOVE
            /// </summary>
            Move,
            /// <summary>
            /// Breathing *
            /// </summary>
            Default,
            /// <summary>
            /// Touch head (Start & Loop & End)
            /// </summary>
            Touch_Head,
            /// <summary>
            /// Touch body (Start & Loop & End)
            /// </summary>
            Touch_Body,
            /// <summary>
            /// Idle (includes crouch/bored and other common idle random animations) (Start & Loop & End)
            /// </summary>
            Idel,
            /// <summary>
            /// Sleep (Start & Loop & End) *
            /// </summary>
            Sleep,
            /// <summary>
            /// Talk (Start & Loop & End) *
            /// </summary>
            Say,
            /// <summary>
            /// Standby mode 1 (Start & Loop & End)
            /// </summary>
            StateONE,
            /// <summary>
            /// Standby mode 2 (Start & Loop & End)
            /// </summary>
            StateTWO,
            /// <summary>
            /// Startup *
            /// </summary>
            StartUP,
            /// <summary>
            /// Shutdown
            /// </summary>
            Shutdown,
            /// <summary>
            /// Work (Start & Loop & End) *
            /// </summary>
            Work,
            /// <summary>
            /// Switch state up
            /// </summary>
            Switch_Up,
            /// <summary>
            /// Switch state down
            /// </summary>
            Switch_Down,
            /// <summary>
            /// Thirsty
            /// </summary>
            Switch_Thirsty,
            /// <summary>
            /// Hungry
            /// </summary>
            Switch_Hunger,
            /// <summary>
            /// Hide (side)
            /// </summary>
            SideHide_Left_Main,
            /// <summary>
            /// Hide (side) show
            /// </summary>
            SideHide_Left_Rise,
            /// <summary>
            /// Hide (side)
            /// </summary>
            SideHide_Right_Main,
            /// <summary>
            /// Hide (side) show
            /// </summary>
            SideHide_Right_Rise,
        }
        /// <summary>
        /// Action: animation action Start Loop End
        /// </summary>
        public enum AnimatType
        {
            /// <summary>
            /// Animation has only one action
            /// </summary>
            Single,
            /// <summary>
            /// Start action
            /// </summary>
            A_Start,
            /// <summary>
            /// Loop action
            /// </summary>
            B_Loop,
            /// <summary>
            /// End action
            /// </summary>
            C_End,
        }
        /// <summary>
        /// Name: user-defined; same-name animations share randomness, no longer uses StoreRand
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Action: animation action Start Loop End
        /// </summary>
        public AnimatType Animat { get; set; }
        /// <summary>
        /// Type: main action category
        /// </summary>
        public GraphType Type { get; set; }
        /// <summary>
        /// State: 4 states
        /// </summary>
        public IGameSave.ModeType ModeType { get; set; }
        ///// <summary>
        ///// Other attached stored info
        ///// </summary>
        //public ILine Info { get; set; }
        public override string ToString()
        {
            return $"[{Name}]{Type}_{ModeType.ToString()[0]}{Animat.ToString()[0]}]";
        }
    }
}
