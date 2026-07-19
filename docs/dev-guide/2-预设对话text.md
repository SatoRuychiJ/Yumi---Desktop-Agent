# 预设对话参数解读

## text文件夹下{name}.lps各项参数解读
在示例模组中，text文件夹内有两个文件：

```
ClickText.lps
LowText.lps
```
此举是为了便于归类，您可以根据自己的需要，合理创建一个或多个{name}.lps配置文件。
让我们看两几行示范代码：

```
clicktext:|LikeMin#0:|State#Work:|Working#直播:|Text#关注虚拟桌宠喵:|:|
clicktext:|LikeMin#0:|State#Sleep:|Mode#1:|Text#诶嘻嘻嘻:|
clicktext:|State#Work:|Working#学习:|dayTime#1:|Text#早读~:|
```
这是当点击宠物时的对话

- 解读：
 
| 参数名 | 参数信息 
| clicktext | 标头,表明这一行是点击宠物时的对话 
| LikeMin | 最低好感度要求
默认是 0 
| LikeMax | 最高好感度要求
默认是 最大值 
| State | 点击时的状态，可配置不同的状态所触发不同的对话

- Nomal 正常

- Work 工作

- Sleep 睡觉默认是Nomal 
| Working | 点击时正在进行的工作，可配置不同的工作所触发不同的对话

- 学习

- 文案

- 直播 
| Text | 预设对话 
| mode | 宠物状态模式
mode:
- 1 允许高兴时触发

- 2 允许普通时触发

- 4 允许不高兴时触发

- 8 允许生病时触发
可以通过相加来同时允许触发条件

- 例如，3 (1+2) 同时允许高兴和普通时触发(mode#3,后面括号内容是对3的解释)默认是 7 (1+2+4)(高兴，普通，不高兴是皆可触发) 
| daytime | 时间要求
daytime:
- 1 早晨

- 2 中午

- 4 晚上

- 8 深夜
也可以通过相加同时允许触发时间

- 例如3 (1=2)同时允许早晨和中午触发(daytime#3,后面括号内容是对3的解释)默认是 15 (1+2+4+8)(所有时间皆可触发) 再看两行：

```
LowFoodText:|Mode#H:|Strength#M:|Like#N:|Text#礼物？是好吃的吗！:|
LowDrinkText:|Mode#H:|Strength#L:|Like#N:|Text#喝！继续喝！还没尽兴呢！:|
```
这是当宠物口渴或饥饿时的对话

- 解读：

 
| 参数名 | 参数信息 
| LowFoodText | 饥饿时触发的对话 
| LowDrinkText | 口渴时触发的对话 
| Mode | 宠物状态模式

- H 高状态: 开心/普通

- L 低状态: 低状态/生病 
| Strength | 口渴/饥饿强度

- L 一般饥饿/口渴

- S 普通饥饿/口渴

- M 非常饥饿/口渴 
| Like | 好感度要求

- N 不需要好感度

- S 低好感度需求

- M 中好感度需求

- L 高好感度 
| Text | 预设对话 
 
 
 
 

 
 
 $J( function() {
 InitializeCommentThread( "PublishedFile_Public", "PublishedFile_Public_76561199056697514_3020939669", {"feature":"3020939669","feature2":-1,"owner":"76561199056697514","total_count":5,"start":0,"pagesize":10,"has_upvoted":0,"upvotes":0,"votecountid":null,"voteupid":null,"commentcountid":null,"subscribed":false,"extended_data":"{\"contributors\":[\"76561199056697514\",{},{}],\"appid\":1920960,\"sharedfile\":{\"m_parentsDetails\":null,\"m_parentBundlesDetails\":null,\"m_bundledChildren\":[],\"m_ownedBundledItems\":[]},\"parent_item_reported\":false}"}, 'https://steamcommunity.com/comment/PublishedFile_Public/', 40 );
 } );

 
 
 
 5 Comments 
 
 
 
 
 
 < 
 
 
 > 
 
 
 
 
 
 
 
 
 

 
 
 
 
 
 
 
 [图片]
 
 
 
 [图片] 
 
 
 
 
 Вторник 

 
 
 
 
 
 
 
 Apr 29, 2024 @ 2:05am 
 
 

 
 
 
 
 
 RUS: Выберите что то одно из этого списка и напишите в моём профиле, отвечу тем же!
ENG: Choose the one that's on the list and write in my profile, I will answer the same!

💜+Rep Clutch King 👑
💜+Rep 300 iq 🧠
💜+Rep ak 47 god👻
💜+Rep SECOND S1MPLE😎
💜+Rep relax teammate🤤
💜+Rep Killing Machine 😈
💜+Rep AWP GOD 💢
💜+Rep kind person💯
💜+Rep ONE TAP MACHINE 💢
💜+Rep nice profile 💜
💜+Rep add me pls😇
💜+Rep very nice and non-toxic player😈
💜+Rep AYYYY LMAO
💜+Rep nice flicks👽
💜+Rep king deagle💥
💜+Rep best👹
💜+Rep killer👺
💜+Rep Good player 💜
💜+Rep Amazing Tactics 👌
💜+Rep Top Player 🔝 
 
 
 
 
 
 
 
 
 
 

 
 
 
 
 
 
 
 [图片]
 
 
 
 [图片] 
 
 
 
 
 Ender_DeerOVO 

 
  [author] 
 
 
 
 
 
 Sep 4, 2023 @ 8:17am 
 
 

 
 
 
 
 
 时间段是daytime鸭 
 
 
 
 
 
 
 
 
 
 

 
 
 
 
 
 
 
 [图片]
 
 
 
 
 
 [图片]
 
 
 
 
 
 
 小忧忧 

 
 
 
 
 
 
 
 Sep 4, 2023 @ 8:09am 
 
 

 
 
 
 
 
 大佬想问下，目前只支持「点击」及「口渴/饥饿」这两个对话触发吗
有那种可以实现在某个时间段随机让桌宠主动互动的对话触发参数不（类似晚上的时候会问候晚上好或者晚安） 
 
 
 
 
 
 
 
 
 
 

 
 
 
 
 
 
 
 [图片]
 
 
 
 [图片] 
 
 
 
 
 Ender_DeerOVO 

 
  [author] 
 
 
 
 
 
 Aug 19, 2023 @ 7:24pm 
 
 

 
 
 
 
 
 您好，这里是我写的不够详细，抱歉给您产生了困扰>_< 
 
 
 
 
 
 
 
 
 
 

 
 
 
 
 
 
 
 [图片]
 
 
 
 [图片] 
 
 
 
 
 ꧁꧂ 

 
 
 
 
 
 
 
 Aug 19, 2023 @ 12:05pm 
 
 

 
 
 
 
 
 点的部分状态和时间是要数字加起来的值比如高兴和普通的“1+2”要打“3”，直接1+2启用后会打不开，试了半天才发现原因[图片] 
 
 
 
 
 
 
 

 
 
 
 
 < 
 
 
 > 
 
 
 
 
 

 

 

 
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