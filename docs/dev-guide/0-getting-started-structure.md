# 0. info.lps and icon.png

First, let's get familiar with LinePutScript (LPS) [github.com].

> Originally posted by the LinePutScript author: LinePutScript is a data-exchange format, a standard language that defines a line-reading structure and describes its contents.

It can be used in many scenarios such as saves, settings, resources, and template files. It is similar to XML or JSON, but easier to use than XML and JSON (in theory). This library makes it easier to create, modify, and save LinePutScript. The source code is open, so you can modify it yourself to support more features.

Let's start by analyzing what its symbols mean:

| Symbol | Meaning |
| :----- | :------ |
| (see the LinePutScript documentation for the full symbol reference) | |

## 1. Getting to Know the Mod Directory: Folders, Subfolders, and Files

Under the mod directory you will find the folders `food`, `image`, `lang`, `pet`, and `text`, plus the files `icon.png` and `info.lps`. Let's analyze what each of them does:

- **food folder**: The food folder. It should contain one or more `{name}.lps` files, which are responsible for the food attributes. For organization, the example mod creates two files. You can change their number freely to suit your needs (as long as the data inside is correct); the pet should read the food attributes correctly.
- **image folder**: The food-icon folder. Here, `food.png` is the default food image. In other words, when a food defined in one of the `{name}.lps` files inside the `food` folder does not define an icon, `food.png` steps up and serves as its icon. The `food` subfolder inside `image` should contain the icons for each food, and they should be bound to the foods defined in the `{name}.lps` files mentioned above.
- **lang folder**: The language and translation folder. Its subfolder names correspond to the English abbreviations of the respective languages.
- **pet folder**: The pet-animation folder. It stores the per-frame PNG images of the pet's animations.
- **text folder**: The preset-dialogue folder. The `{name}.lps` files it should contain are similar to those in the `food` folder; you can create one or more files depending on your situation without affecting how the pet reads them. `{name}.lps` is responsible for storing the pet's speaking behavior in different states.
- **plugin folder**: The plugin folder.

## Uploading a Mod

In theory, once your mod folder is in the correct location (`Steam\steamapps\common\VPet\mod\`) and the `info.lps` and `icon.png` files are configured correctly, you can already upload the mod.

But please note: it is best NOT to add `itemid` and `authorid`. Do not add them, do not add them, do not add them! (They are filled in automatically when the mod is uploaded.)

Right-click the pet ---> System ---> Settings panel ---> MOD Management ---> {your mod} ---> Operations (bottom right) ---> Upload to Steam.

Please make sure you have a good network connection.

## Troubleshooting Mod Uploads

Suppose you carefully filled in every value, racked your brain to write a good description, and sorted out the categories, but when you wake up your pet to check the result, a "Mod / Plugin corrupted" message suddenly pops up. You would certainly feel like cursing, and your world would go dark.
