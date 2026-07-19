using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace VPet.Plugin.AIPet.LLM
{
    /// <summary>
    /// Simple chat message (role: user / assistant)
    /// </summary>
    public class ChatMsg
    {
        public string Role { get; set; }
        public string Text { get; set; }
        public ChatMsg() { }
        public ChatMsg(string role, string text) { Role = role; Text = text; }
    }

    /// <summary>
    /// Tool definition; Schema is the JSON Schema object definition
    /// </summary>
    public class ToolDef
    {
        public string Name;
        public string Description;
        /// <summary>Build the JSON Schema (a new instance each call, to avoid JsonNode parent-node conflicts)</summary>
        public Func<JsonObject> Schema;
        /// <summary>Execute the tool; input is the argument object, returns the result text</summary>
        public Func<JsonObject, string> Execute;
    }

    /// <summary>
    /// A single chat request
    /// </summary>
    public class LLMRequest
    {
        public string System;
        public List<ChatMsg> Messages = new();
        public List<ToolDef> Tools;
        /// <summary>Streaming text callback</summary>
        public Action<string> OnTextDelta;
        /// <summary>Usage callback (input tokens, output tokens), invoked once per API round</summary>
        public Action<int, int> OnUsage;
        public int MaxTokens = 2048;
        /// <summary>Optional base64 image attached to the current (last) user message — used for screen vision</summary>
        public string ScreenImageBase64;
        public string ScreenImageMediaType = "image/jpeg";
    }
}
