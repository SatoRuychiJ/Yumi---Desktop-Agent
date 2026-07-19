# Pet Animation Setup Explained (work in progress)

## Understanding vup.lps

Let's start with the first sample line (the LPS keys, field names, and example values are kept verbatim):

```
pet#默认虚拟桌宠:|intor#虚拟主播模拟器默认人物形象:|path#vup:|petname#萝莉斯:|
```

Parameter explanation:

| Parameter | Description |
| :-------- | :---------- |
| pet | The display name of the pet animation set. |
| intro | The description of the pet animation set. |
| path | The folder where the animation frame images are stored. |
| petname | The pet's default name. |

These are the basic attributes of the pet appearance. `pet` and `intro` are shown under Settings panel ---> Graphics ---> Pet Animation.

Next, let's analyze sample lines 2-4:

```
2.
touchhead:|px#159:|py#16:|sw#189:|sh#178:|
3. touchraised:|happy_px#0:|happy_py#50:|happy_sw#500:|happy_sh#200:|nomal_px#0:|nomal_py#50:|nomal_sw#500:|nomal_sh#200:|poorcondition_px#0:|poorcondition_py#50:|poorcondition_sw#500:|poorcondition_sh#200:|ill_px#0:|ill_py#200:|ill_sw#500:|ill_sh#300:|
4. raisepoint:|happy_x#290:|happy_y#128:|nomal_x#290:|nomal_y#128:|poorcondition_x#290:|poorcondition_y#128:|ill_x#225:|ill_y#115:|
```

Parameter explanation:

| Parameter | Description |
| :-------- | :---------- |
| touchhead | The head-patting area. |
| touchraised | The pick-up (raised) area. |
| raisepoint | The pick-up anchor point. |

These define the areas where the mouse interacts with the pet. The x coordinate is counted rightward from the top-left pixel, and the y coordinate is counted downward from the top-left pixel. You can adjust their positions for the pet's different states. For example, in line 2, when happy, the head-patting start coordinate is x=159, y=16, `sw` (width, i.e. how far the x range extends to the right) = 189, and `sh` (height, i.e. how far the y range extends downward) = 178.

Now look at lines 5-9:

```
5.
work:|Type#Work:|Name#文案:|MoneyBase#8:|MoneyLevel#0.5:|Graph#workone:|StrengthFood#3.5:|StrengthDrink#2.5:|Feeling#1.5:|Time#60:|FinishBonus#0.1:|BorderBrush#000000:|Background#413d39:|ButtonBackground#322e2b:|ButtonForeground#FFFFFF:|Foreground#ccbdad:|Left#113:|Top#315:|Width#280:|
6.
work:|Type#Work:|Name#直播:|MoneyBase#16:|MoneyLevel#1:|Graph#worktwo:|StrengthFood#4.5:|StrengthDrink#7.5:|Feeling#2.5:|Time#180:|FinishBonus#0.25:|LevelLimit#10:|BorderBrush#000000:|Background#413d39:|ButtonBackground#322e2b:|ButtonForeground#FFFFFF:|Foreground#ccbdad:|Left#113:|Top#315:|Width#280:|
7.
work:|Type#Study:|Name#学习:|MoneyBase#16:|MoneyLevel#2:|Graph#study:|StrengthFood#1.5:|StrengthDrink#2:|Feeling#3:|Time#45:|FinishBonus#0.2:|BorderBrush#000000:|Background#413d39:|ButtonBackground#322e2b:|ButtonForeground#FFFFFF:|Foreground#ccbdad:|Left#113:|Top#315:|Width#280:|
8.
work:|Type#Study:|Name#研究:|MoneyBase#30:|MoneyLevel#3:|Graph#studytwo:|StrengthFood#2.5:|StrengthDrink#3.5:|Feeling#4.5:|Time#75:|FinishBonus#0.4:|BorderBrush#000000:|Background#413d39:|ButtonBackground#322e2b:|LevelLimit#15:|ButtonForeground#FFFFFF:|Foreground#ccbdad:|Left#113:|Top#315:|Width#280:|
9.
work:|Type#Play:|Name#玩游戏:|MoneyBase#10:|MoneyLevel#2:|Graph#playone:|StrengthFood#1.5:|StrengthDrink#1.5:|Feeling#4:|Time#30:|FinishBonus#0.2:|BorderBrush#000000:|Background#413d39:|ButtonBackground#322e2b:|ButtonForeground#FFFFFF:|Foreground#ccbdad:|Left#113:|Top#315:|Width#280:|
```

These bind the interactive actions. The pet does not currently support custom interaction categories. The current interaction categories and subcategories are:

| Category | Subcategory |
| :------- | :---------- |
| Work | workone, worktwo |
| Study | (none) |
| Play | (uses the pick-up anchor point) |

## Understanding the vup Folder

The `vup` folder stores the pet's per-frame action images. It should contain the appropriate subfolders.

## Understanding the Animation Loading Order and Logic

VPet loads animations very freely; you can order the subfolders inside the animation folder however you like. First, let's introduce the rules.

Current animation-to-registration-name reference and recommendations:

| Required Animations | Recommended Animations |
| :------------------ | :--------------------- |
| Raised_Dynamic (dynamic pick-up) | Touch_Head (head pat) (start & loop & end) |
| Raised_Static (static pick-up) (start & loop & end) | Touch_Body (body touch) (start & loop & end) |
| Sleep (start & loop & end) | IDEL (idle) (includes general random idle animations such as squat/boring) (start & loop & end): `squat`, `boring`. You can register IDEL animations inside vup.lps under the pet directory. |
| Say (speak) (start & loop & end) | StateONE (standby mode 1) (start & loop & end). After StateONE finishes, there is a chance to play StateTwo. |
| StartUP (entrance) | StateTWO (standby mode 2) (start & loop & end). After StateONE finishes, there is a chance to play StateTwo. |
| Work (start & loop & end) | Shutdown (exit) |
| Default (breathing) | Switch (state switch) |
| | Gift (receiving a gift) |
| | Eat (eating) |
| | Drink (drinking) |
| | Move (moving) |

Notes:

- Some animations missing the `ill` state may crash. The state defaults to `normal`.
- Aside from Mode and the IDEL category, please do not add actions arbitrarily.
