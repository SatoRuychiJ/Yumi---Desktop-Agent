using LinePutScript;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.AIPet
{
    /// <summary>
    /// Plugin settings, stored in the AIPet line of the game's Setting.lps
    /// </summary>
    public class AISetting
    {
        private readonly IMainWindow mw;
        public AISetting(IMainWindow mw) => this.mw = mw;

        private ILine Line => mw.Set["AIPet"];

        /// <summary>API protocol: anthropic / openai</summary>
        public string Provider
        {
            get => Line.GetString("provider", "anthropic");
            set => Line.SetString("provider", value);
        }
        public string BaseUrl
        {
            get => Line.GetString("baseurl", "");
            set => Line.SetString("baseurl", value);
        }
        public string ApiKey
        {
            get => Line.GetString("apikey", "");
            set => Line.SetString("apikey", value);
        }
        public string Model
        {
            get => Line.GetString("model", "claude-opus-4-8");
            set => Line.SetString("model", value);
        }
        /// <summary>Custom persona (character setup); uses the default when empty</summary>
        public string Persona
        {
            get => Line.GetString("persona", "");
            set => Line.SetString("persona", value);
        }
        /// <summary>How to address the user</summary>
        public string UserNick
        {
            get => Line.GetString("usernick", "you");
            set => Line.SetString("usernick", value);
        }
        /// <summary>Whether the AI can see the user's screen (screen vision; needs a vision-capable model)</summary>
        public bool EnableVision
        {
            get => Line.GetString("enablevision", "0") == "1";
            set => Line.SetString("enablevision", value ? "1" : "0");
        }
        /// <summary>Whether the AI is allowed to control the body (play animations, etc.)</summary>
        public bool EnableTools
        {
            get => Line.GetString("enabletools", "1") == "1";
            set => Line.SetString("enabletools", value ? "1" : "0");
        }
        /// <summary>Whether the AI reacts to interactions such as head pats / feeding</summary>
        public bool EnableReactions
        {
            get => Line.GetString("enablereact", "1") == "1";
            set => Line.SetString("enablereact", value ? "1" : "0");
        }
        /// <summary>Whether proactive speech is enabled</summary>
        public bool EnableProactive
        {
            get => Line.GetString("enableproactive", "1") == "1";
            set => Line.SetString("enableproactive", value ? "1" : "0");
        }
        /// <summary>Proactive speech interval (minutes)</summary>
        public int ProactiveInterval
        {
            get => Line.GetInt("proactiveinterval", 30);
            set => Line.SetInt("proactiveinterval", value);
        }
        /// <summary>Do-not-disturb start hour (inclusive)</summary>
        public int QuietStart
        {
            get => Line.GetInt("quietstart", 23);
            set => Line.SetInt("quietstart", value);
        }
        /// <summary>Do-not-disturb end hour (exclusive)</summary>
        public int QuietEnd
        {
            get => Line.GetInt("quietend", 8);
            set => Line.SetInt("quietend", value);
        }
        /// <summary>Max number of conversation messages to keep</summary>
        public int MaxHistory
        {
            get => Line.GetInt("maxhistory", 40);
            set => Line.SetInt("maxhistory", value);
        }

        /// <summary>Resolved API endpoint URL</summary>
        public string ResolvedBaseUrl
        {
            get
            {
                var url = BaseUrl.Trim().TrimEnd('/');
                if (string.IsNullOrEmpty(url))
                    return Provider == "anthropic" ? "https://api.anthropic.com" : "https://api.openai.com/v1";
                return url;
            }
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(Model);

        public void Save() => mw.Save();
    }
}
