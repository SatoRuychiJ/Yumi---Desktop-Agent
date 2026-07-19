using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Panuon.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VPet_Simulator.Core;

namespace VPet_Simulator.Windows.Interface;

/// <summary>
/// Item (an item that can be viewed and used in the inventory)
/// </summary>
/// Note: item usage must be hand-written by code plugins, implemented in imw.TakeItem
public class Item : NotifyPropertyChangedBase
{
    /// <summary>
    /// Item creation method
    /// </summary>
    /// <param name="data">Item data</param>
    /// <returns>Item</returns>
    public static Item CreateItem(IMainWindow imw, ILine data)
    {
        if (Creators.ContainsKey(data[(gstr)"itemtype"]))
        {
            return Creators[data[(gstr)"itemtype"]](imw, data);
        }
        else
        {
            return LPSConvert.DeserializeObject<Item>(data);
        }
    }
    /// <summary>
    /// Collection of item creation methods; add custom item type creators here, after LoadPlugin and before GameLoaded. Do not add blocking operations
    /// </summary>
    public static Dictionary<string, Func<IMainWindow, ILine, Item>> Creators = new()
    {
        { "Food", (_,line) => { return LPSConvert.DeserializeObject<Food>(line); } },
    };
    /// <summary>
    /// Usage method for the corresponding item type (item / whether use completed)
    /// </summary>
    public static Dictionary<string, List<Func<IMainWindow, Item, bool>>> UseAction = new();
    /// <summary>
    /// Item image (defaults to {itemtypes}/{Image or itemname}.png )
    /// </summary>
    [Line(ignoreCase: true)]
    public virtual string Image { get; set; } = null;
    /// <summary>
    /// Use this item
    /// </summary>
    public virtual void Use(IMainWindow imw)
    {
        if (UseAction.ContainsKey(ItemType))
        {
            foreach (var action in UseAction[ItemType])
            {
                if (action(imw, this))
                    return;
            }
            return;
        }
        MessageBoxX.Show("物品 {0} 使用失败".Translate(TranslateName), "该物品无法使用".Translate());
    }
    /// <summary>
    /// Consume this item; if the count drops to 0 or below, destroy it (remove from inventory) (not called automatically)
    /// </summary>
    /// <param name="count">Amount to consume</param>
    public virtual void Consume(IMainWindow imw, int count = 1)
    {
        Count -= count;
        if (Count <= 0)
        {
            //destroy the item
            imw.Items.Remove(this);
        }
    }

    /// <summary>
    /// Item name (ID)
    /// </summary>
    [Line(name: "name")]
    public string Name { get; set; }
    private string transname = null;
    private string transdesc = null;
    /// <summary>
    /// Item name (translated)
    /// </summary>
    public string TranslateName
    {
        get
        {
            if (transname == null)
            {
                transname = LocalizeCore.Translate(Name);
            }
            return transname;
        }
    }

    /// <summary>
    /// Item type
    /// </summary>
    [Line(name: "itemtype")]
    public virtual string ItemType { get; set; } = "Item";
    /// <summary>
    /// Description (translated)
    /// </summary>

    public virtual string Description
    {
        get
        {
            if (transdesc == null)
            {
                transdesc = LocalizeCore.Translate(Desc);
            }
            return transdesc;
        }
    }

    /// <summary>
    /// List of supported custom item types (remember to translate, e.g. Item_Item => Item)
    /// </summary>

    public static List<string> ItemTypes = new List<string>()
    {
        //Item - default category
        "Item",
        //Food - edible food (can also refer to items)
        "Food",
        //Prop - an item with special functionality
        "Tool",
        //Toy - an item that can play animations
        "Toy",
        //Mail - a letter that grants items when opened
        "Mail",
    };

    /// <summary>
    /// Item price
    /// </summary>
    [Line(ignoreCase: true)]
    public virtual double Price { get; set; }
    /// <summary>
    /// Description
    /// </summary>
    [Line(ignoreCase: true)]
    public string Desc { get; set; }

    /// <summary>
    /// Displayed image (defaults to {itemtypes}/{itemname}.png )
    /// </summary>
    public virtual BitmapImage ImageSource { get; set; }

    /// <summary>
    /// Item count
    /// </summary>
    [Line(ignoreCase: true)]
    public virtual int Count { get; set; } = 1;
    /// <summary>
    /// Other data, used by the program to store custom data
    /// </summary>
    [Line(ignoreCase: true)]
    public virtual string Data { get; set; } = "";
    /// <summary>
    /// Whether it can be used
    /// </summary>
    [Line(ignoreCase: true)]
    public virtual bool CanUse { get; set; } = true;
    /// <summary>
    /// Whether the item is favorited
    /// </summary>
    [Line(ignoreCase: true)]
    public virtual bool Star { get; set; } = false;
    /// <summary>
    /// Whether it is a single item (non-stackable) (not consumed when used together) (note: regardless of this flag, the final consumption logic must be implemented yourself in the Use method)
    /// </summary>
    [Line(ignoreCase: true)]
    public virtual bool IsSingle { get; set; } = false;

    /// <summary>
    /// Whether it can be shown in the inventory
    /// </summary>
    [Line(ignoreCase: true)]
    public virtual bool Visibility { get; set; } = true;
    /// <summary>
    /// Load the item image
    /// </summary>
    public virtual void LoadSource(IMainWindow imw)
    {
        ImageSource = imw.ImageSources.FindImage(ItemType + "_" + (Image ?? Name), "food");
    }
}
