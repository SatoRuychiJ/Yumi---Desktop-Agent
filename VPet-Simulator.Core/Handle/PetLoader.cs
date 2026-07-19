using LinePutScript;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;
using static VPet_Simulator.Core.GraphCore;


namespace VPet_Simulator.Core
{
    /// <summary>
    /// Pet loader
    /// </summary>
    public class PetLoader
    {
        /// <summary>
        /// Number of animations
        /// </summary>
        public int GraphCount { get; private set; }
        /// <summary>
        /// Pet graphics
        /// </summary>
        public GraphCore Graph(int Resolution, Dispatcher dispatcher)
        {
            GraphCount = 0;
            var g = new GraphCore(Resolution, dispatcher);
            foreach (var p in path)
                GraphCount += LoadGraph(g, new DirectoryInfo(p), p);
            g.GraphConfig = Config;
            return g;
        }
        /// <summary>
        /// Image location
        /// </summary>
        public List<string> path = new List<string>();
        /// <summary>
        /// Pet display name
        /// </summary>
        public string Name;
        /// <summary>
        /// Pet description
        /// </summary>
        public string Intor;
        /// <summary>
        /// Pet default name
        /// </summary>
        public string PetName;
        public GraphCore.Config Config;
        public PetLoader(LpsDocument lps, DirectoryInfo directory)
        {
            Name = lps.First().Info;
            Intor = lps.First()["intor"].Info;
            PetName = lps.First()["petname"].Info;
            path.Add(directory.FullName + "\\" + lps.First()["path"].Info);
            Config = new Config(lps);
        }
        public delegate void LoadGraphDelegate(GraphCore graph, FileSystemInfo path, ILine info);
        /// <summary>
        /// Custom image loading methods
        /// </summary>
        public static Dictionary<string, LoadGraphDelegate> IGraphConvert = new Dictionary<string, LoadGraphDelegate>()
        {
            { "pnganimation", PNGAnimation.LoadGraph},
            { "picture", Picture.LoadGraph },
            { "foodanimation", FoodAnimation.LoadGraph },
        };
        /// <summary>
        /// Load image animation
        /// </summary>
        /// <param name="graph">Animation core to load</param>
        /// <param name="di">Directory currently being traversed</param>
        /// <param name="startuppath">Starting directory</param>
        public static int LoadGraph(GraphCore graph, DirectoryInfo di, string startuppath)
        {
            if (!di.Exists)
                return 0;
            int GraphCount = 0;
            var list = di.EnumerateDirectories();
            if (File.Exists(di.FullName + @"\info.lps"))
            {
                //If it comes with description info, load it manually
                LpsDocument lps = new LpsDocument(File.ReadAllText(di.FullName + @"\info.lps"));
                foreach (ILine line in lps)
                {
                    if (IGraphConvert.TryGetValue(line.Name.ToLowerInvariant(), out var func))
                    {
                        line.Add(new Sub("startuppath", startuppath));
                        var str = line.GetString("path");
                        if (!string.IsNullOrEmpty(str))
                        {
                            var p = Path.Combine(di.FullName, str);
                            if (Directory.Exists(p))
                                func.Invoke(graph, new DirectoryInfo(p), line);
                            else if (File.Exists(p))
                                func.Invoke(graph, new FileInfo(p), line);
                            else
                                Console.WriteLine("Unknow Graph Type: " + p);
                        }
                        else
                            func.Invoke(graph, di, line);
                        GraphCount++;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(line.Name))
                            Console.WriteLine("Unknow Graph Type: " + line.Name.ToLowerInvariant());
                    }
                }
            }
            else if (list.Count() == 0)
            {//Start automatic generation
                var paths = di.GetFiles();
                if (paths.Length == 0)
                    return GraphCount;
                if (paths.Length == 1)
                    Picture.LoadGraph(graph, paths[0], new Line("picture", "", "", new Sub("startuppath", startuppath)));
                else
                    PNGAnimation.LoadGraph(graph, di, new Line("pnganimation", "", "", new Sub("startuppath", startuppath)));
                GraphCount++;
            }
            else
                foreach (var p in list)
                {
                    GraphCount += LoadGraph(graph, p, startuppath);
                }
            return GraphCount;
        }
    }
}
