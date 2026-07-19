# 食物参数解读

## food文件夹中的{name}.lps
为了便于管理，在示例模组中，food文件夹下面有两个.lps文件

```
drug.lps
food.lps
```
在实际情况中，您可以根据自己的需要，合理配置文件个数(一个或多个)。
我们来分析一下里面的参数吧。
首先给出两行代码：

```
food:|type#Drink:|name#ab钙奶:|price#7.0:|desc#健康美味，经济实惠:|Exp#5:|Strength#10:|StrengthDrink#40:|StrengthFood#5:|Health#1:|Feeling#2:|

food:|type#Meal:|name#香煎牛仔骨:|price#83.0:|desc#开瓶红酒吧，上流:|Exp#300:|Strength#120:|StrengthFood#160:|Health#10:|Feeling#125:|Likability#2:|

```
分析：

 
| 参数名 | 参数信息 
| food | 表头，说明这是食物类 
| type | 食物类型

- Drink 饮料

- Functional 功能性

- Snack 零食

- Meal 正餐

- Drug 药品

- Gift 礼物(未实装) 
| name | 食物名称，可以绑定{mod}\image\food文件夹内同一名称的食物图片 
| price | 食物价格 
| desc | 食物描述(请不要在里面写入 # , : |等符号，逗号可用/com代替) 
| Exp | 经验 
| Strength | 体力 
| StrengthDrink | 口渴度 
| StrengthFood | 饱腹度 
| Health | 健康 
| Feeling | 心情 
| Likability | 好感度 各项参数可以乱序，数值可以为负数，当参数没有定义时默认为0。
建议大家设立合理的数值，营造良好的创意工坊环境
您可以在模组名称中加上[平衡] 二字来表明您模组的身份
游戏开发者非常喜欢平衡的mod,看到了会给你点个赞

## image文件夹及目录下food.png和food子文件夹
food子文件夹负责储存已经在{mod}\food\{name}.lps文件中注册过食物的图片，food子文件夹中的图片名称应该与{mod}\food\{name}.lps中的name参数一致。
image文件夹目录下的food.png是默认食物图片，当food文件夹内的{name}.lps中注册的食物没有对应图片时，它就会跳出来，成为食物图片。 
 
 
 
 

 
 
 

 

 

 
 [图片]
 [图片]
 

 

 
 
 Share to your Steam activity feed
 
 
 [图片] <>
 [图片] <>
 [图片] <>
 
 
 
 
 
 
 Link: 
 
 
 
 
 
 You need to sign in or create an account to do that.
 
 Sign In 
 Create an Account 
 Cancel
 
 
 
 
 
 

 
 [图片]
 
 
 
 
 function UpdateKVTagsSingle()
 {
 $('PromptModalForm').submit();
 }
 
 
 
 

 
 
 
 
 
 

 Update 

 
 
 
 

 

 
 RecordAppImpression( 1920960, '2_100100_100101_100106' );