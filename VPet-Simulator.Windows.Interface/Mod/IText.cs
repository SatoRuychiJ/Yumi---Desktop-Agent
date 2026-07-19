using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using System;
using System.Linq;
using VPet_Simulator.Core;

namespace VPet_Simulator.Windows.Interface;

public class IText
{
    /// <summary>
    /// Spoken content
    /// </summary>
    [Line(IgnoreCase = true)] public string Text { get; set; }

    private string transText = null;
    /// <summary>
    /// Spoken content (translated)
    /// </summary>
    public string TranslateText
    {
        get
        {
            if (transText == null)
            {
                transText = LocalizeCore.Translate(Text);
            }
            return transText;
        }
        set
        {
            transText = value;
        }
    }
    /// <summary>
    /// Text content tag
    /// </summary>
    [Line(IgnoreCase = true)]
    public string Tag
    {
        get => string.Join(",", tags);
        set => tags = value.Split(',');
    }

    private string[] tags = new string[] { "all" };
    /// <summary>
    /// Check whether it matches the content tag
    /// </summary>
    public bool FindTag(string[] tags) => tags.Any(tag => this.tags.Contains(tag));


    /// <summary>
    /// Convert the text into its actual value
    /// </summary>
    public string TranslateTextConvert(Main m) => ConverText(TranslateText, m);
    /// <summary>
    /// Convert the text into its actual value (note: conflicts with Trainslate({0})); first Trainslate, then Convert, and finally Format
    /// </summary>
    public static string ConverText(string text, Main m)
    {
        if (text.Contains('{') && text.Contains('}'))
        {
            return text.Replace("{name}", m.Core.Save.Name).Replace("{food}", m.Core.Save.StrengthFood.ToString("f0"))
                .Replace("{drink}", m.Core.Save.StrengthDrink.ToString("f0")).Replace("{feel}", m.Core.Save.Feeling.ToString("f0")).
                Replace("{strength}", m.Core.Save.Strength.ToString("f0")).Replace("{money}", m.Core.Save.Money.ToString("f0"))
                .Replace("{level}", m.Core.Save.Level.ToString("f0")).Replace("{health}", m.Core.Save.Health.ToString("f0"))
                .Replace("{hostname}", m.Core.Save.HostName);
        }
        else
            return text;
    }
}
