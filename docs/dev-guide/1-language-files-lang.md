# Food Parameters Explained

## The {name}.lps Files in the food Folder

For easier management, the example mod has two `.lps` files under the `food` folder:

```
drug.lps
food.lps
```

In practice, you can configure the number of files (one or more) as needed. Let's analyze the parameters inside. Here are two sample lines:

```
food:|type#Drink:|name#ab钙奶:|price#7.0:|desc#健康美味，经济实惠:|Exp#5:|Strength#10:|StrengthDrink#40:|StrengthFood#5:|Health#1:|Feeling#2:|

food:|type#Meal:|name#香煎牛仔骨:|price#83.0:|desc#开瓶红酒吧，上流:|Exp#300:|Strength#120:|StrengthFood#160:|Health#10:|Feeling#125:|Likability#2:|
```

Analysis:

| Parameter | Description |
| :-------- | :---------- |
| food | The header, indicating that this is a food entry. |
| type | Food type: `Drink`, `Functional`, `Snack`, `Meal`, `Drug`, or `Gift` (Gift not yet implemented). |
| name | Food name. Can be bound to a food image with the same name inside `{mod}\image\food`. |
| price | Food price. |
| desc | Food description (do not use the symbols `#`, `:`, or `|` inside it; a comma can be replaced with `/com`). |
| Exp | Experience. |
| Strength | Stamina. |
| StrengthDrink | Thirst. |
| StrengthFood | Hunger (fullness). |
| Health | Health. |
| Feeling | Mood. |
| Likability | Affinity. |

The parameters can be in any order and values may be negative. When a parameter is not defined, it defaults to 0.

You are encouraged to set reasonable values to help create a healthy Workshop environment. You can add the word "[Balanced]" to your mod name to indicate that your mod is balanced. The developers are very fond of balanced mods and will give yours a thumbs-up when they see it.

## The image Folder, food.png, and the food Subfolder

The `food` subfolder stores the images of the foods that have been registered in the `{mod}\food\{name}.lps` files. The image names in the `food` subfolder should match the `name` parameter in `{mod}\food\{name}.lps`.

The `food.png` located directly in the `image` folder is the default food image. When a food registered in a `{name}.lps` file has no corresponding image, `food.png` steps in and becomes the food image.
