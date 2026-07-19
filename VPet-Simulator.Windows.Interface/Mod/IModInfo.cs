using LinePutScript.Converter;
using LinePutScript.Dictionary;
using LinePutScript.Localization.WPF;
using LinePutScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Core;

namespace VPet_Simulator.Windows.Interface;

/// <summary>
/// Mod info interface
/// </summary>
public interface IModInfo
{
    /// <summary>
    /// Mod name
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Mod author
    /// </summary>
    public string Author { get; }
    /// <summary>
    /// If uploaded to Steam, this is the SteamUserID
    /// </summary>
    public long AuthorID { get; }
    /// <summary>
    /// The ItemID uploaded to Steam
    /// </summary>
    public ulong ItemID { get; }
    /// <summary>
    /// Description
    /// </summary>
    public string Intro { get; }
    /// <summary>
    /// Mod path
    /// </summary>
    public DirectoryInfo Path { get; }
    /// <summary>
    /// Game version
    /// </summary>
    public int GameVer { get; }
    /// <summary>
    /// Mod version
    /// </summary>
    public int Ver { get; }
    /// <summary>
    /// Mod tags
    /// </summary>
    public HashSet<string> Tag { get; }
}
