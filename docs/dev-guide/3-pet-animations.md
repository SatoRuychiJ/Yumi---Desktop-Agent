# 桌宠动画制作设置解读（未完善）

## vup.lps的解读
先分析第一行示范代码

- 
```
pet#默认虚拟桌宠:|intor#虚拟主播模拟器默认人物形象:|path#vup:|petname#萝莉斯:|
```

- 参数解读：

 
| 参数 | 参数名称 
| pet | 宠物动画选择名称 
| intro | 宠物动画选择介绍 
| path | 动画分帧图像存放位置 
| petname | 宠物默认名字 这是桌宠形象的基本属性，pet和intro将会在设置面板——>图形——>宠物动画 一栏中显示[图片] 
接下来分析第2-4行示范代码

- 
```
2.
touchhead:|px#159:|py#16:|sw#189:|sh#178:|
3. touchraised:|happy_px#0:|happy_py#50:|happy_sw#500:|happy_sh#200:|nomal_px#0:|nomal_py#50:|nomal_sw#500:|nomal_sh#200:|poorcondition_px#0:|poorcondition_py#50:|poorcondition_sw#500:|poorcondition_sh#200:|ill_px#0:|ill_py#200:|ill_sw#500:|ill_sh#300:|
4. raisepoint:|happy_x#290:|happy_y#128:|nomal_x#290:|nomal_y#128:|poorcondition_x#290:|poorcondition_y#128:|ill_x#225:|ill_y#115:|

```

- 参数解读：

| 参数 | 参数名称
| touchhead | 摸头的范围
| touchraised | 提起的范围
| raisepoint | 提起定位点这是鼠标与桌宠互动的范围，x坐标从左上角第一个像素点往右数，y坐标从左上角第一个像素点往下数。
您可以根据宠物动画的不同状态，来调整他们的位置。比如第二行，当高兴时触发桌宠摸头的起点坐标是x=159,y=16，sw(宽，您可以理解为x坐标向右延申的长度)=189,sh(高，您可以理解为y坐标向下延申的长度)=178[图片] 

接下来请看5-9行：

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
这些是互动动作的绑定。目前桌宠暂不支持自定义互动类别
目前的互动类别及子分类：

| 类别 | 子分类
| Work | 
- workone

- worktwo
| Study | 
| Play | 提起定位点

## vup文件夹解读
vup文件夹负责储存宠物的动作逐帧图片，其内应当包含以下子文件夹：

## 动画读取顺序及逻辑的解读
VPet的动画读取非常的自由随意，您可以自由排序动画文件夹中的子文件夹。
我们要首先介绍一下规则：
 
- 目前动画及注册名对照表以及建议：
 
| 必须添加的动画 | 建议添加的动画 
| Raised_Dynamic 被提起动态 | Touch_Head 摸头 (开始&循环&结束) 
| Raised_Static 被提起静态 (开始&循环&结束) | Touch_Body 摸身体 (开始&循环&结束) 
| Sleep 睡觉 (开始&循环&结束) | IDEL 空闲 (包括下蹲/无聊等通用空闲随机动画) (开始&循环&结束)
- squat 下蹲

- boring 无聊

- 您可以在pet目录下的vup.lps内注册IDEL动画 
| Say 说话 (开始&循环&结束) | StateONE 待机 模式1 (开始&循环&结束)

- 执行完StateONE之后有几率执行StateTwo 
| StartUP 入场 | StateTWO 待机 模式2 (开始&循环&结束)

- 执行完StateONE之后有几率执行StateTwo 
| Work 工作 (开始&循环&结束) | Shutdown 离场 
| Default 呼吸 | Switch 状态切换 
| | Gift 收到礼物 
| | Eat 吃东西 
| | Drink 喝东西 
| | Move 移动 
- 部分动画缺失ill状态可能会炸。
 
- 状态默认是 normal
 
- 除了Mode，IDEL类，请不要随意添加动作 >_<
