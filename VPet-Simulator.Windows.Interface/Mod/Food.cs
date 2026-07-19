using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Panuon.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using VPet_Simulator.Core;
using static LinePutScript.Converter.LPSConvert;

namespace VPet_Simulator.Windows.Interface
{
    public class Food : Item, IFood
    {
        public override string ItemType => "Food";
        /// <summary>
        /// Food type
        /// </summary>
        public enum FoodType
        {
            /// <summary>
            /// Food (default)
            /// </summary>
            Food,
            /// <summary>
            /// Favorite (custom)
            /// </summary>
            Star,
            /// <summary>
            /// Meal
            /// </summary>
            Meal,
            /// <summary>
            /// Snack
            /// </summary>
            Snack,
            /// <summary>
            /// Drink
            /// </summary>
            Drink,
            /// <summary>
            /// Functional
            /// </summary>
            Functional,
            /// <summary>
            /// Medicine
            /// </summary>
            Drug,
            /// <summary>
            /// Gift
            /// </summary>
            Gift,
        }
        /// <summary>
        /// Food type
        /// </summary>
        [Line(type: ConvertType.ToEnum, ignoreCase: true)]
        public FoodType Type { get; set; } = FoodType.Food;
      
        [Line(ignoreCase: true)]
        public int Exp { get; set; }
        [Line(ignoreCase: true)]
        public double Strength { get; set; }
        [Line(ignoreCase: true)]
        public double StrengthFood { get; set; }
        [Line(ignoreCase: true)]
        public double StrengthDrink { get; set; }
        [Line(ignoreCase: true)]
        public double Feeling { get; set; }
        [Line(ignoreCase: true)]
        public double Health { get; set; }
        [Line(ignoreCase: true)]
        public double Likability { get; set; }

        /// <summary>
        /// Description (ToBetterBuy)
        /// </summary>

        public override string Description
        {
            get
            {
                return Data + '\n' + Desc.Translate();
            }
        }

        public IDictionary<string, string> DescriptionValues
        {
            get
            {
                var dic = new Dictionary<string, double>()
                {
                    { LocalizeCore.Translate("经验值"), (double)Exp },
                    { LocalizeCore.Translate("饱腹度"), StrengthFood },
                    { LocalizeCore.Translate("口渴度"), StrengthDrink },
                    { LocalizeCore.Translate("体力"), Strength },
                    { LocalizeCore.Translate("心情"), Feeling },
                    { LocalizeCore.Translate("健康"), Health },
                    { LocalizeCore.Translate("好感度"), Likability },
                };
                return dic.Where(kv => kv.Value != 0)
                    .ToDictionary(kv => kv.Key, kv => $"{(kv.Value > 0 ? "+" : "")}{kv.Value.ToString("f2")}");
            }
        }

       
        /// <summary>
        /// Whether favorited
        /// </summary>
        public override bool Star { get; set; }
       
        public bool? isoverload = null;
        /// <summary>
        /// Current recommended price for the item
        /// </summary>
        public double RealPrice => ((Exp / 3 + Strength / 5 + StrengthDrink / 3 + StrengthFood / 2 + Feeling / 6) / 3 + Health + Likability * 10);
        /// <summary>
        /// Whether this food is overpowered
        /// </summary>
        public bool IsOverLoad()
        {
            if (isoverload == null)
            {
                double relp = RealPrice;
                isoverload = Price < (relp - 10) * 0.7;// Price > (relp + 10) * 1.3;// || Price < (relp - 10) * 0.7;//30% tolerance
            }
            return isoverload.Value;
        }
        /// <summary>
        /// Load the item image
        /// </summary>
        public void LoadImageSource(IMainWindow imw)
        {
            ImageSource = imw.ImageSources.FindImage("food_" + (Image ?? Name), "food");
            Star = imw.Set["betterbuy"]["star"].GetInfos().Contains(Name);
            LoadEatTimeSource(imw);
        }
        public void LoadEatTimeSource(IMainWindow imw)
        {
            DateTime now = DateTime.Now;
            DateTime eattime = imw.GameSavesData["buytime"].GetDateTime(Name, now);
            if (eattime <= now)
            {
                if (Type == FoodType.Meal || Type == FoodType.Snack || Type == FoodType.Drink || Type == FoodType.Gift)// || Type == FoodType.Limit)
                    Data = "喜好度".Translate();
                else
                    Data = "有效度".Translate();
                Data += ":\t100%";
            }
            else
            {
                if (Type == FoodType.Meal || Type == FoodType.Snack || Type == FoodType.Drink || Type == FoodType.Gift)// || Type == FoodType.Limit)
                    Data = "喜好度".Translate();
                else
                    Data = "有效度".Translate();
                if (Type == FoodType.Gift)
                    Data += ":\t" + Math.Max(0.5, 1 - Math.Pow((eattime - now).TotalHours, 2) * 0.01).ToString("p0");
                else
                    Data += ":\t" + Math.Max(0.5, 1 - Math.Pow((eattime - now).TotalHours, 2) * 0.02).ToString("p0");
                Data += "\t\t" + "恢复".Translate() + ":\t" + (eattime).ToString("MM/dd HH");
            }
        }
        /// <summary>
        /// Animation shown when eating
        /// </summary>
        [Line(ignoreCase: true)]
        public string Graph { get; set; } = null;
        /// <summary>
        /// Get the animation shown when eating
        /// </summary>
        public string GetGraph()
        {
            if (string.IsNullOrEmpty(Graph))
                switch (Type)
                {
                    default:
                        return "eat";
                    case Food.FoodType.Drink:
                        return "drink";
                    case Food.FoodType.Gift:
                        return "gift";
                }
            else
                return Graph;
        }
        /// <summary>
        /// Clone the food object
        /// </summary>
        public Food Clone()
        {
            return (Food)MemberwiseClone();
        }
    }
}
