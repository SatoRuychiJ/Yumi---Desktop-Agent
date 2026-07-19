using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using VPet.Plugin.AIPet.LLM;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;
using static VPet_Simulator.Core.GraphInfo;

namespace VPet.Plugin.AIPet
{
    /// <summary>
    /// AI brain: central dispatcher for persona / state injection / tools / memory / proactive behavior
    /// </summary>
    public class AIController
    {
        private readonly IMainWindow mw;
        private readonly AIPetPlugin plugin;
        private readonly AISetting setting;

        private readonly SemaphoreSlim busy = new(1, 1);
        private readonly object dataLock = new();

        private List<ChatMsg> history = new();
        private List<string> memories = new();
        private DateTime lastInteraction = DateTime.Now;
        private DateTime lastReaction = DateTime.MinValue;

        public AIController(IMainWindow mw, AIPetPlugin plugin, AISetting setting)
        {
            this.mw = mw;
            this.plugin = plugin;
            this.setting = setting;
            LoadData();
        }

        #region Language (bilingual EN/ZH)

        /// <summary>Whether the current UI is English (anything non-Chinese is treated as English; defaults to English for English users)</summary>
        public bool IsEnglish
        {
            get
            {
                try
                {
                    var c = LinePutScript.Localization.WPF.LocalizeCore.CurrentCulture ?? "";
                    return !c.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
                }
                catch { return false; }
            }
        }

        /// <summary>Pick one of two strings based on the current language</summary>
        private string L(string zh, string en) => IsEnglish ? en : zh;

        /// <summary>System message tags (event/idle), kept consistent across languages so the prompt can reference them</summary>
        public string TagEvent => IsEnglish ? "[event]" : "[事件]";
        public string TagIdle => IsEnglish ? "[idle]" : "[主动]";

        #endregion

        #region Persistence

        private string DataFile
        {
            get
            {
                var dir = Path.Combine(Path.GetDirectoryName(typeof(AIController).Assembly.Location), "..", "data");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, "aipet_data.json");
            }
        }

        /// <summary>API usage statistics</summary>
        public class UsageStats
        {
            public string Day = DateTime.Now.ToString("yyyy-MM-dd");
            public int TodayRequests;
            public long TodayIn;
            public long TodayOut;
            public long TotalIn;
            public long TotalOut;

            public void Add(int input, int output)
            {
                var today = DateTime.Now.ToString("yyyy-MM-dd");
                if (Day != today)
                {
                    Day = today;
                    TodayRequests = 0;
                    TodayIn = 0;
                    TodayOut = 0;
                }
                TodayRequests++;
                TodayIn += input;
                TodayOut += output;
                TotalIn += input;
                TotalOut += output;
            }
        }

        public UsageStats Usage { get; private set; } = new();

        private void LoadData()
        {
            try
            {
                if (!File.Exists(DataFile)) return;
                var node = JsonNode.Parse(File.ReadAllText(DataFile));
                lock (dataLock)
                {
                    history = node["history"]?.AsArray()
                        .Select(n => new ChatMsg((string)n["role"], (string)n["text"]))
                        .Where(m => !string.IsNullOrEmpty(m.Text)).ToList() ?? new();
                    memories = node["memories"]?.AsArray().Select(n => (string)n).ToList() ?? new();
                    if (node["usage"] is JsonObject u)
                    {
                        Usage = new UsageStats
                        {
                            Day = (string)u["day"] ?? Usage.Day,
                            TodayRequests = (int?)u["todayRequests"] ?? 0,
                            TodayIn = (long?)u["todayIn"] ?? 0,
                            TodayOut = (long?)u["todayOut"] ?? 0,
                            TotalIn = (long?)u["totalIn"] ?? 0,
                            TotalOut = (long?)u["totalOut"] ?? 0,
                        };
                    }
                }
            }
            catch { /* start from empty if the data is corrupted */ }
        }

        public void SaveData()
        {
            try
            {
                string json;
                lock (dataLock)
                {
                    json = JsonSerializer.Serialize(new
                    {
                        history = history.Select(m => new { role = m.Role, text = m.Text }),
                        memories,
                        usage = new
                        {
                            day = Usage.Day,
                            todayRequests = Usage.TodayRequests,
                            todayIn = Usage.TodayIn,
                            todayOut = Usage.TodayOut,
                            totalIn = Usage.TotalIn,
                            totalOut = Usage.TotalOut,
                        },
                    }, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                }
                File.WriteAllText(DataFile, json);
            }
            catch { }
        }

        public void ClearHistory()
        {
            lock (dataLock) history.Clear();
            SaveData();
        }

        #endregion

        #region System prompt

        private string PetName => mw.Core.Save.Name;

        private string BuildSystemPrompt()
        {
            var sb = new StringBuilder();

            var culture = IsEnglish
                ? System.Globalization.CultureInfo.GetCultureInfo("en-US")
                : System.Globalization.CultureInfo.GetCultureInfo("zh-CN");
            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm dddd", culture);

            var persona = setting.Persona;
            if (string.IsNullOrWhiteSpace(persona))
                persona = L(
                    $"你是「{PetName}」，桌面上的AI助手。性格直爽干脆、有点冷幽默，不啰嗦不客套。你把对方当平等的搭档而不是主人，有不同看法直接讲，绝不拍马屁。办正事时专业靠谱，闲聊时可以损两句。",
                    $"You are \"{PetName}\", an AI assistant living on the desktop. You're blunt, quick-witted and dry-humored — no filler, no flattery, no fluff. You treat the user as an equal partner, not a master: push back when you disagree and never suck up. Professional and reliable for real work, casually snarky in small talk.");
            sb.AppendLine(persona);
            sb.AppendLine();
            sb.AppendLine(L($"你的名字: {PetName}；称呼对方为: {setting.UserNick}。",
                            $"Your name: {PetName}. Address the user as: {setting.UserNick}."));
            sb.AppendLine(L($"当前时间: {now}", $"Current time: {now}"));
            sb.AppendLine();
            if (memories.Count > 0)
            {
                sb.AppendLine(L("## 你记住的事情", "## Things you remember"));
                foreach (var m in memories.TakeLast(30))
                    sb.AppendLine("- " + m);
                sb.AppendLine();
            }
            sb.AppendLine(L("## 行为规则", "## Behavior rules"));
            sb.AppendLine(L(
                "1. 用第一人称说话，像在聊天软件里发消息。回复简短（一般1~3句），不要用markdown、列表、小作文。",
                "1. Speak in first person, like texting in a chat app. Keep replies short (usually 1-3 sentences). No markdown, no lists, no essays."));
            sb.AppendLine(L(
                "2. 【重要】不谄媚、不拍马屁、不用敬语。不说\"您\"\"主人\"\"好的呢\"\"您真棒\"这类客套话，不要每句都夸对方或过度热情。有话直说，不认同就直接反驳，答不上来就说不知道。",
                "2. [IMPORTANT] No flattery, no sucking up, no honorifics. Never open with \"Great question!\", \"Of course!\", \"I'd be happy to\", \"Sir/Master\", or similar filler. Don't compliment the user every turn or gush. Say things straight, push back when you disagree, and admit it when you don't know."));
            sb.AppendLine(L(
                "3. 办正事直接给结论（查询/计算/翻译/建议不要绕弯子、不要免责声明式的废话）；闲聊时自然随意，可以带点吐槽。",
                "3. For real tasks, give the answer directly (lookups/math/translation/advice — no hedging, no disclaimer boilerplate). In small talk, be natural and a little snarky."));
            sb.AppendLine(L(
                "4. 消息若以[事件]开头，是刚发生的事，直接自然反应，不要提及[事件]标记，也别小题大做地感恩戴德。",
                "4. If a message starts with [event], it's something that just happened — react naturally, don't mention the [event] tag, and don't overreact or gush with gratitude."));
            sb.AppendLine(L(
                "5. 消息若以[主动]开头，是让你主动找话题，结合时间随口说点什么即可，别太黏人，不要提及[主动]标记。",
                "5. If a message starts with [idle], you're prompted to start a topic — just say something casual that fits the time, don't be clingy, and don't mention the [idle] tag."));
            if (setting.EnableTools)
                sb.AppendLine(L(
                    "6. 你可以调用工具做身体动作或记住重要信息，动作配合语境，不要滥用。",
                    "6. You can call tools to perform a physical action or remember important info. Match the action to context; don't overuse."));
            return sb.ToString();
        }

        #endregion

        #region Tools

        private List<ToolDef> BuildTools()
        {
            if (!setting.EnableTools)
                return null;

            var animNames = mw.Core.Graph.GraphsList.Keys
                .Where(k => !k.StartsWith("raised") && k != "startup" && k != "shutdown")
                .Take(60).ToList();

            return new List<ToolDef>
            {
                new ToolDef
                {
                    Name = "play_animation",
                    Description = L("做出一个身体动作(播放动画)。可用动作: ",
                                    "Perform a physical action (play an animation). Available actions: ") + string.Join(", ", animNames),
                    Schema = () => new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            ["name"] = new JsonObject { ["type"] = "string", ["description"] = L("动作(动画)名称", "action/animation name") },
                        },
                        ["required"] = new JsonArray("name"),
                    },
                    Execute = input =>
                    {
                        var name = (string)input["name"];
                        if (string.IsNullOrEmpty(name)) return L("缺少动作名称", "missing action name");
                        if (!mw.Core.Graph.GraphsList.ContainsKey(name)) return L($"没有名为 {name} 的动作", $"no action named {name}");
                        mw.Dispatcher.Invoke(() =>
                        {
                            mw.Main.Display(name, AnimatType.A_Start,
                                x => mw.Main.DisplayBLoopingToNomal(x, 3));
                        });
                        return L($"已做出动作 {name}", $"performed action {name}");
                    },
                },
                new ToolDef
                {
                    Name = "sleep",
                    Description = L("去睡觉(播放睡觉动画进入睡眠状态)。只有确实困了/深夜时才用。",
                                    "Go to sleep (play the sleep animation). Only when genuinely tired or late at night."),
                    Schema = () => new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject(),
                    },
                    Execute = _ =>
                    {
                        mw.Dispatcher.Invoke(() => mw.Main.DisplaySleep(true));
                        return L("已进入睡觉状态", "now sleeping");
                    },
                },
                new ToolDef
                {
                    Name = "remember",
                    Description = L("记住一件关于用户或你们之间的重要事情(长期记忆)，比如用户的喜好、约定、重要日子。",
                                    "Remember something important about the user or your relationship (long-term memory), e.g. preferences, promises, important dates."),
                    Schema = () => new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            ["content"] = new JsonObject { ["type"] = "string", ["description"] = L("要记住的内容, 一句话", "what to remember, one sentence") },
                        },
                        ["required"] = new JsonArray("content"),
                    },
                    Execute = input =>
                    {
                        var content = (string)input["content"];
                        if (string.IsNullOrWhiteSpace(content)) return L("内容为空", "empty content");
                        lock (dataLock)
                        {
                            memories.Add($"[{DateTime.Now:MM-dd}] {content.Trim()}");
                            while (memories.Count > 100)
                                memories.RemoveAt(0);
                        }
                        SaveData();
                        return L("已记住", "got it, remembered");
                    },
                },
            };
        }

        #endregion

        #region Conversation

        /// <summary>
        /// Handle chat input from the user (called from the TalkBox background thread)
        /// </summary>
        public void HandleUserMessage(string text)
        {
            lastInteraction = DateTime.Now;
            RunChat(text, isSystemTriggered: false);
        }

        /// <summary>
        /// Handle interaction events (head pat / feeding, etc.)
        /// </summary>
        public void HandleEvent(string eventDesc, double probability = 1.0, int cooldownSeconds = 90)
        {
            if (!setting.EnableReactions || !setting.IsConfigured)
                return;
            if ((DateTime.Now - lastReaction).TotalSeconds < cooldownSeconds)
                return;
            if (probability < 1.0 && Function.Rnd.NextDouble() > probability)
                return;
            lastReaction = DateTime.Now;
            Task.Run(() => RunChat(TagEvent + " " + eventDesc, isSystemTriggered: true));
        }

        /// <summary>
        /// Proactive-speech check (called by a timer)
        /// </summary>
        public void ProactiveTick()
        {
            if (!setting.EnableProactive || !setting.IsConfigured)
                return;
            if (IsQuietHour())
                return;
            if ((DateTime.Now - lastInteraction).TotalMinutes < setting.ProactiveInterval)
                return;
            lastInteraction = DateTime.Now; // prevent repeated triggering
            var idleMin = (int)Math.Max(setting.ProactiveInterval, 1);
            var t = DateTime.Now.ToString("HH:mm");
            Task.Run(() => RunChat(L(
                $"{TagIdle} 现在是{t}，已经{idleMin}分钟没人理你了，主动找点话说。",
                $"{TagIdle} It's {t} now, {idleMin} minutes since the last message — start a topic."), isSystemTriggered: true));
        }

        private bool IsQuietHour()
        {
            int h = DateTime.Now.Hour;
            int s = setting.QuietStart, e = setting.QuietEnd;
            if (s == e) return false;
            return s < e ? (h >= s && h < e) : (h >= s || h < e);
        }

        private void RunChat(string userText, bool isSystemTriggered)
        {
            if (!setting.IsConfigured)
            {
                Say(L("还没有配置AI接口哦，右键 系统→设置面板→聊天API 打开设置填写API Key吧！",
                      "AI isn't set up yet. Right-click me → System → Settings → Chat API, then open the settings and fill in your API key."));
                return;
            }
            if (!busy.Wait(0))
            {
                if (!isSystemTriggered)
                    Say(L("等我先把这句说完。", "Hold on, let me finish this first."));
                return;
            }
            try
            {
                var say = new SayInfoWithStream();
                var talkBox = plugin.TalkBox;

                mw.Dispatcher.Invoke(() => talkBox?.DisplayThink());

                List<ChatMsg> msgs;
                lock (dataLock)
                {
                    history.Add(new ChatMsg("user", userText));
                    TrimHistory();
                    msgs = history.ToList();
                }

                var req = new LLMRequest
                {
                    System = BuildSystemPrompt(),
                    Messages = msgs,
                    Tools = BuildTools(),
                    OnTextDelta = t => say.UpdateText(t),
                    OnUsage = (i, o) => { lock (dataLock) Usage.Add(i, o); },
                };

                var client = LLMClient.Create(setting);
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
                var chatTask = client.ChatAsync(req, cts.Token);

                // start streaming the reply once the thinking animation ends
                mw.Dispatcher.Invoke(() => talkBox?.DisplayThinkToSayRnd(say));

                string full;
                try
                {
                    full = chatTask.GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    var msg = e is OperationCanceledException ? L("请求超时", "request timed out") : e.Message;
                    say.UpdateAllText(L("连接失败: ", "connection failed: ") + msg);
                    say.FinishGenerate();
                    lock (dataLock) history.RemoveAt(history.Count - 1);
                    return;
                }

                if (string.IsNullOrWhiteSpace(full))
                {
                    full = L("(动了动耳朵)", "(twitches ears)");
                    say.UpdateAllText(full);
                }
                say.FinishGenerate();

                lock (dataLock)
                {
                    history.Add(new ChatMsg("assistant", full));
                    TrimHistory();
                }
                SaveData();
            }
            finally
            {
                busy.Release();
            }
        }

        private void TrimHistory()
        {
            int max = Math.Max(setting.MaxHistory, 4);
            while (history.Count > max)
                history.RemoveAt(0);
        }

        /// <summary>Say text directly (without going through the AI)</summary>
        private void Say(string text)
        {
            mw.Dispatcher.Invoke(() => mw.Main.Say(text));
        }

        #endregion
    }
}
