# Preset Dialogue Parameters Explained

## The Parameters in {name}.lps Under the text Folder

In the example mod, the `text` folder contains two files:

```
ClickText.lps
LowText.lps
```

This is just for organization; you can create one or more `{name}.lps` configuration files as needed. Let's look at a few sample lines (the LPS keys, field names, and example values are kept verbatim):

```
clicktext:|LikeMin#0:|State#Work:|Working#直播:|Text#关注虚拟桌宠喵:|:|
clicktext:|LikeMin#0:|State#Sleep:|Mode#1:|Text#诶嘻嘻嘻:|
clicktext:|State#Work:|Working#学习:|dayTime#1:|Text#早读~:|
```

These are dialogues triggered when the pet is clicked.

Explanation:

| Parameter | Description |
| :-------- | :---------- |
| clicktext | The header, indicating this line is a dialogue triggered on click. |
| LikeMin | Minimum Affinity requirement. Default is 0. |
| LikeMax | Maximum Affinity requirement. Default is the maximum value. |
| State | The state when clicked. Different states can trigger different dialogues: `Nomal` (normal), `Work`, `Sleep`. Default is `Nomal`. |
| Working | The job currently in progress when clicked. Different jobs can trigger different dialogues, e.g. Study, Prepping, Live. |
| Text | The preset dialogue text. |
| mode | Pet state mode: `1` allowed when happy, `2` allowed when normal, `4` allowed when unhappy, `8` allowed when ill. You can add the values together to allow multiple trigger conditions. For example, `3` (1+2) allows triggering when both happy and normal (the content in parentheses explains the value 3). Default is `7` (1+2+4) (triggers when happy, normal, or unhappy). |
| daytime | Time requirement: `1` morning, `2` noon, `4` evening, `8` late night. You can also add the values together to allow multiple trigger times. For example, `3` (1+2) allows triggering in both morning and noon (the content in parentheses explains the value 3). Default is `15` (1+2+4+8) (all times can trigger). |

Here are two more lines:

```
LowFoodText:|Mode#H:|Strength#M:|Like#N:|Text#礼物？是好吃的吗！:|
LowDrinkText:|Mode#H:|Strength#L:|Like#N:|Text#喝！继续喝！还没尽兴呢！:|
```

These are dialogues triggered when the pet is thirsty or hungry.

Explanation:

| Parameter | Description |
| :-------- | :---------- |
| LowFoodText | Dialogue triggered when hungry. |
| LowDrinkText | Dialogue triggered when thirsty. |
| Mode | Pet state mode: `H` high state (happy / normal), `L` low state (poor condition / ill). |
| Strength | Thirst/hunger intensity: `L` slightly hungry/thirsty, `S` normally hungry/thirsty, `M` very hungry/thirsty. |
| Like | Affinity requirement: `N` no Affinity required, `S` low Affinity required, `M` medium Affinity required, `L` high Affinity required. |
| Text | The preset dialogue text. |

> Note: `daytime` refers to the time-of-day segment. For the additive `State`/`daytime`/`mode` values, you must supply the summed number (for example, use `3` for "happy + normal", i.e. 1+2), not a literal expression like `1+2`.
