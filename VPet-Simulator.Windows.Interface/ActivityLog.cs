using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Core;

namespace VPet_Simulator.Windows.Interface;

/// <summary>
/// Activity log: e.g. work, events, etc.
/// </summary>
public class ActivityLog
{
    /// <summary>
    /// Activity log: e.g. work, events, etc.
    /// </summary>
    public ActivityLog()
    {
    }
    /// <summary>
    /// Activity log: e.g. work, events, etc.
    /// </summary>
    /// <param name="type">Log type</param>
    /// <param name="isDebug">Whether this is a debug log (only shown to the player in debug mode)</param>
    /// <param name="description">Log details</param>
    public ActivityLog(string type, bool isDebug = false, params string[] description)
    {
        Time = DateTime.Now;
        Type = type;
        Description = string.Join('|', description);
        IsDebug = isDebug;
    }
    /// <summary>
    /// Activity log (non-debug): e.g. work, events, etc.
    /// </summary>
    /// <param name="type">Log type</param>
    /// <param name="description">Log details</param>
    public ActivityLog(string type, params string[] description)
    {
        Time = DateTime.Now;
        Type = type;
        Description = string.Join('|', description);
        IsDebug = false;
    }
    /// <summary>
    /// Log time
    /// </summary>
    [Line]
    public DateTime Time { get; set; }
    /// <summary>
    /// Log type
    /// </summary>
    [Line]
    public string Type { get; set; }
    /// <summary>
    /// Log details, separated by |
    /// </summary>
    [Line]
    public string Description { get; set; }

    /// <summary>
    /// Whether this is a debug log (only shown to the player in debug mode)
    /// </summary>
    [Line] public bool IsDebug { get; set; }

    /// <summary>
    /// Convert to a player-readable string
    /// </summary>
    public string ToString(Main m)
    {
        return $"[{Time.ToShortTimeString()}] {string.Format(IText.ConverText(("al_" + Type).Translate(), m), Description.Split('|'))}";
    }
}
