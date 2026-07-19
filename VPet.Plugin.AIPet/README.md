# VPet.Plugin.AIPet — AI 桌宠插件

接入大语言模型，让 VPet 桌宠真正"活起来"。

## 功能

- **AI 聊天**：聊天框输入 → 思考动画 → 流式说话气泡（边生成边说）
- **双协议支持**：Anthropic Claude 原生 API / OpenAI 兼容接口（DeepSeek、Kimi、GLM、OpenRouter 等）
- **有状态的人格**：每次对话自动注入桌宠当前状态（等级/金钱/体力/饱腹/口渴/心情/健康/好感度/模式），饿了会馋、病了会蔫
- **AI 控制身体**（tool use）：AI 可以自己决定播放动画（`play_animation`）、睡觉（`sleep`）
- **长期记忆**（`remember` 工具）：记住你的喜好、约定、重要日子，跨会话保留
- **交互感知**：摸头、摸身体、喂食、完成工作、新的一天 → AI 第一人称实时反应（带概率与冷却，不刷屏）
- **主动行为**：太久没理它会主动找你说话，支持免打扰时段
- **可自定义人设**、对用户的称呼、各功能开关

## 安装

1. 编译（或直接使用已编译好的 `mod/AIPet`）：
   ```
   dotnet build VPet.Plugin.AIPet.csproj -c Release
   ```
   把 `bin/Release/VPet.Plugin.AIPet.dll` 复制到 `mod/AIPet/plugin/`
2. 把整个 `mod/AIPet` 文件夹复制到游戏目录的 `mod` 文件夹下，例如：
   `E:\SteamLibrary\steamapps\common\VPet\mod\AIPet`
3. 启动游戏。首次加载时游戏会提示该 MOD 未认证 —— 在 MOD 管理里**启用并信任**该模组后重启。
4. 游戏内：系统设置 → 系统 → 聊天API 选择「AI桌宠」；在其设置窗口填入 API Key、模型名并保存（可点"测试连接"验证）。

## 配置说明

| 项 | 说明 |
|---|---|
| 协议类型 | Anthropic 原生（推荐 Claude 模型，默认 `claude-opus-4-8`）或 OpenAI 兼容 |
| API 地址 | 留空用官方地址；中转/第三方服务填对应 base URL（OpenAI 兼容需含 `/v1`） |
| 人设 | 留空使用内置默认人设；桌宠名字取自游戏内设置 |
| 主动说话间隔 | 距上次互动超过该分钟数后，桌宠可能主动找你 |
| 免打扰 | 例如 23 点 ~ 8 点之间不会主动说话 |

数据存储：聊天历史与长期记忆保存在 `mod/AIPet/data/aipet_data.json`；API 配置保存在游戏 `Setting.lps` 的 `AIPet` 行。

## 项目结构

```
AIPetPlugin.cs      插件入口: 注册聊天框、挂接游戏事件、主动行为定时器
AITalkBox.cs        聊天框 (继承游戏 TalkBox, 自带流式说话 UI)
AIController.cs     AI 大脑: 系统提示词/状态注入/工具/记忆/事件/主动行为
AISetting.cs        配置读写 (存于游戏 Setting.lps)
winAISetting.cs     设置窗口 (纯代码 WPF)
LLM/LLMClient.cs    流式 LLM 客户端 (Anthropic 原生 + OpenAI 兼容, 含工具循环)
LLM/LLMTypes.cs     消息/工具类型定义
mod/AIPet/          可直接安装的 MOD 文件夹
```
