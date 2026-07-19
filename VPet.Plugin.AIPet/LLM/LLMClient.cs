using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace VPet.Plugin.AIPet.LLM
{
    /// <summary>
    /// Base class for the streaming LLM client
    /// </summary>
    public abstract class LLMClient
    {
        protected static readonly HttpClient Http = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan };

        protected readonly string BaseUrl;
        protected readonly string ApiKey;
        protected readonly string Model;

        protected LLMClient(string baseUrl, string apiKey, string model)
        {
            BaseUrl = baseUrl;
            ApiKey = apiKey;
            Model = model;
        }

        public static LLMClient Create(AISetting setting)
        {
            return setting.Provider == "openai"
                ? new OpenAIClient(setting.ResolvedBaseUrl, setting.ApiKey, setting.Model)
                : new AnthropicClient(setting.ResolvedBaseUrl, setting.ApiKey, setting.Model);
        }

        /// <summary>
        /// Run one conversation (handles the tool loop internally), returns the full reply text
        /// </summary>
        public abstract Task<string> ChatAsync(LLMRequest req, CancellationToken ct = default);

        /// <summary>Read SSE data content line by line</summary>
        protected static async IAsyncEnumerable<JsonNode> ReadSseAsync(HttpResponseMessage resp, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);
            string line;
            while ((line = await reader.ReadLineAsync(ct)) != null)
            {
                if (!line.StartsWith("data:"))
                    continue;
                var data = line.Substring(5).Trim();
                if (data.Length == 0 || data == "[DONE]")
                    continue;
                JsonNode node = null;
                try { node = JsonNode.Parse(data); }
                catch { /* ignore lines that cannot be parsed */ }
                if (node != null)
                    yield return node;
            }
        }

        protected static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, CancellationToken ct)
        {
            var resp = await Http.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync(ct);
                resp.Dispose();
                if (err.Length > 400) err = err.Substring(0, 400);
                throw new Exception($"API request failed {(int)resp.StatusCode}: {err}");
            }
            return resp;
        }

        /// <summary>
        /// If a screen image is present, attach it to the last user message, formatted for the given
        /// provider (Anthropic image block, or OpenAI image_url). No-op when there is no image.
        /// </summary>
        protected static void AttachScreenImage(List<object> messages, LLMRequest req, bool anthropic)
        {
            if (string.IsNullOrEmpty(req.ScreenImageBase64) || messages.Count == 0)
                return;
            if (messages[messages.Count - 1] is not Dictionary<string, object> last)
                return;
            if (!last.TryGetValue("role", out var role) || (string)role != "user")
                return;
            if (!last.TryGetValue("content", out var content) || content is not string text)
                return;

            object imageBlock = anthropic
                ? new Dictionary<string, object>
                {
                    ["type"] = "image",
                    ["source"] = new Dictionary<string, object>
                    {
                        ["type"] = "base64",
                        ["media_type"] = req.ScreenImageMediaType,
                        ["data"] = req.ScreenImageBase64,
                    },
                }
                : new Dictionary<string, object>
                {
                    ["type"] = "image_url",
                    ["image_url"] = new Dictionary<string, object>
                    {
                        ["url"] = $"data:{req.ScreenImageMediaType};base64,{req.ScreenImageBase64}",
                    },
                };

            last["content"] = new List<object>
            {
                new Dictionary<string, object> { ["type"] = "text", ["text"] = text },
                imageBlock,
            };
        }
    }

    /// <summary>
    /// Anthropic Messages API (native protocol, streaming + tools)
    /// </summary>
    public class AnthropicClient : LLMClient
    {
        public AnthropicClient(string baseUrl, string apiKey, string model) : base(baseUrl, apiKey, model) { }

        public override async Task<string> ChatAsync(LLMRequest req, CancellationToken ct = default)
        {
            // Internal message list (the object tree is rebuilt each round during serialization)
            var messages = req.Messages.Select(m => (object)new Dictionary<string, object>
            {
                ["role"] = m.Role,
                ["content"] = m.Text,
            }).ToList();
            AttachScreenImage(messages, req, anthropic: true);

            var fullText = new StringBuilder();

            for (int round = 0; round < 5; round++)
            {
                var body = new Dictionary<string, object>
                {
                    ["model"] = Model,
                    ["max_tokens"] = req.MaxTokens,
                    ["stream"] = true,
                    ["messages"] = messages,
                };
                if (!string.IsNullOrEmpty(req.System))
                    body["system"] = req.System;
                if (req.Tools != null && req.Tools.Count > 0)
                    body["tools"] = req.Tools.Select(t => (object)new Dictionary<string, object>
                    {
                        ["name"] = t.Name,
                        ["description"] = t.Description,
                        ["input_schema"] = JsonSerializer.Deserialize<object>(t.Schema().ToJsonString()),
                    }).ToList();

                using var msg = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/v1/messages");
                msg.Headers.Add("x-api-key", ApiKey);
                msg.Headers.Add("anthropic-version", "2023-06-01");
                msg.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                using var resp = await SendAsync(msg, ct);

                // Accumulate content blocks
                var blocks = new SortedDictionary<int, (string type, StringBuilder text, string toolId, string toolName, StringBuilder json)>();
                string stopReason = null;
                int inTokens = 0, outTokens = 0;

                await foreach (var node in ReadSseAsync(resp, ct))
                {
                    var type = (string)node["type"];
                    switch (type)
                    {
                        case "message_start":
                            inTokens = (int?)node["message"]?["usage"]?["input_tokens"] ?? 0;
                            break;
                        case "content_block_start":
                        {
                            int idx = (int)node["index"];
                            var cb = node["content_block"];
                            var cbType = (string)cb["type"];
                            blocks[idx] = (cbType, new StringBuilder(),
                                (string)cb["id"], (string)cb["name"], new StringBuilder());
                            break;
                        }
                        case "content_block_delta":
                        {
                            int idx = (int)node["index"];
                            if (!blocks.TryGetValue(idx, out var b)) break;
                            var delta = node["delta"];
                            var dType = (string)delta["type"];
                            if (dType == "text_delta")
                            {
                                var t = (string)delta["text"];
                                b.text.Append(t);
                                fullText.Append(t);
                                req.OnTextDelta?.Invoke(t);
                            }
                            else if (dType == "input_json_delta")
                            {
                                b.json.Append((string)delta["partial_json"]);
                            }
                            break;
                        }
                        case "message_delta":
                            stopReason = (string)node["delta"]?["stop_reason"] ?? stopReason;
                            outTokens = (int?)node["usage"]?["output_tokens"] ?? outTokens;
                            break;
                        case "error":
                            throw new Exception("API error: " + node["error"]?["message"]);
                    }
                }

                if (inTokens > 0 || outTokens > 0)
                    req.OnUsage?.Invoke(inTokens, outTokens);

                if (stopReason != "tool_use")
                    return fullText.ToString();

                // Tool call: replay the assistant content + submit tool results
                var assistantContent = new List<object>();
                var toolResults = new List<object>();
                foreach (var b in blocks.Values)
                {
                    if (b.type == "text")
                    {
                        if (b.text.Length > 0)
                            assistantContent.Add(new Dictionary<string, object> { ["type"] = "text", ["text"] = b.text.ToString() });
                    }
                    else if (b.type == "tool_use")
                    {
                        object input;
                        try { input = JsonSerializer.Deserialize<object>(b.json.Length > 0 ? b.json.ToString() : "{}"); }
                        catch { input = new Dictionary<string, object>(); }
                        assistantContent.Add(new Dictionary<string, object>
                        {
                            ["type"] = "tool_use",
                            ["id"] = b.toolId,
                            ["name"] = b.toolName,
                            ["input"] = input,
                        });
                        toolResults.Add(new Dictionary<string, object>
                        {
                            ["type"] = "tool_result",
                            ["tool_use_id"] = b.toolId,
                            ["content"] = ExecuteTool(req, b.toolName, b.json.ToString()),
                        });
                    }
                }
                messages.Add(new Dictionary<string, object> { ["role"] = "assistant", ["content"] = assistantContent });
                messages.Add(new Dictionary<string, object> { ["role"] = "user", ["content"] = toolResults });
            }
            return fullText.ToString();
        }

        private static string ExecuteTool(LLMRequest req, string name, string inputJson)
        {
            var tool = req.Tools?.FirstOrDefault(t => t.Name == name);
            if (tool == null)
                return "Unknown tool";
            try
            {
                var input = string.IsNullOrWhiteSpace(inputJson) ? new JsonObject() : JsonNode.Parse(inputJson) as JsonObject;
                return tool.Execute(input ?? new JsonObject()) ?? "Done";
            }
            catch (Exception e)
            {
                return "Tool execution failed: " + e.Message;
            }
        }
    }

    /// <summary>
    /// OpenAI-compatible protocol (chat/completions; supports DeepSeek/Kimi/GLM/OpenRouter, etc.)
    /// </summary>
    public class OpenAIClient : LLMClient
    {
        public OpenAIClient(string baseUrl, string apiKey, string model) : base(baseUrl, apiKey, model) { }

        public override async Task<string> ChatAsync(LLMRequest req, CancellationToken ct = default)
        {
            var messages = new List<object>();
            if (!string.IsNullOrEmpty(req.System))
                messages.Add(new Dictionary<string, object> { ["role"] = "system", ["content"] = req.System });
            messages.AddRange(req.Messages.Select(m => (object)new Dictionary<string, object>
            {
                ["role"] = m.Role,
                ["content"] = m.Text,
            }));
            AttachScreenImage(messages, req, anthropic: false);

            var fullText = new StringBuilder();

            for (int round = 0; round < 5; round++)
            {
                var body = new Dictionary<string, object>
                {
                    ["model"] = Model,
                    ["max_tokens"] = req.MaxTokens,
                    ["stream"] = true,
                    ["stream_options"] = new Dictionary<string, object> { ["include_usage"] = true },
                    ["messages"] = messages,
                };
                if (req.Tools != null && req.Tools.Count > 0)
                    body["tools"] = req.Tools.Select(t => (object)new Dictionary<string, object>
                    {
                        ["type"] = "function",
                        ["function"] = new Dictionary<string, object>
                        {
                            ["name"] = t.Name,
                            ["description"] = t.Description,
                            ["parameters"] = JsonSerializer.Deserialize<object>(t.Schema().ToJsonString()),
                        },
                    }).ToList();

                using var msg = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/chat/completions");
                msg.Headers.Add("Authorization", "Bearer " + ApiKey);
                msg.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                using var resp = await SendAsync(msg, ct);

                // index -> (id, name, args)
                var toolCalls = new SortedDictionary<int, (string id, string name, StringBuilder args)>();
                string finishReason = null;
                var roundText = new StringBuilder();

                await foreach (var node in ReadSseAsync(resp, ct))
                {
                    if (node["usage"] is JsonObject u)
                    {
                        int pin = (int?)u["prompt_tokens"] ?? 0;
                        int pout = (int?)u["completion_tokens"] ?? 0;
                        if (pin > 0 || pout > 0)
                            req.OnUsage?.Invoke(pin, pout);
                    }
                    var choice = node["choices"]?[0];
                    if (choice == null) continue;
                    finishReason = (string)choice["finish_reason"] ?? finishReason;
                    var delta = choice["delta"];
                    if (delta == null) continue;

                    var text = (string)delta["content"];
                    if (!string.IsNullOrEmpty(text))
                    {
                        roundText.Append(text);
                        fullText.Append(text);
                        req.OnTextDelta?.Invoke(text);
                    }
                    if (delta["tool_calls"] is JsonArray tcs)
                    {
                        foreach (var tc in tcs)
                        {
                            int idx = (int?)tc["index"] ?? 0;
                            if (!toolCalls.TryGetValue(idx, out var cur))
                                cur = (null, null, new StringBuilder());
                            cur.id = (string)tc["id"] ?? cur.id;
                            cur.name = (string)tc["function"]?["name"] ?? cur.name;
                            var arg = (string)tc["function"]?["arguments"];
                            if (arg != null) cur.args.Append(arg);
                            toolCalls[idx] = cur;
                        }
                    }
                }

                if (toolCalls.Count == 0 || finishReason != "tool_calls")
                    return fullText.ToString();

                // Replay the assistant message + tool results
                var assistantMsg = new Dictionary<string, object>
                {
                    ["role"] = "assistant",
                    ["content"] = roundText.Length > 0 ? roundText.ToString() : null,
                    ["tool_calls"] = toolCalls.Values.Select(tc => (object)new Dictionary<string, object>
                    {
                        ["id"] = tc.id,
                        ["type"] = "function",
                        ["function"] = new Dictionary<string, object>
                        {
                            ["name"] = tc.name,
                            ["arguments"] = tc.args.ToString(),
                        },
                    }).ToList(),
                };
                messages.Add(assistantMsg);
                foreach (var tc in toolCalls.Values)
                {
                    string result;
                    var tool = req.Tools?.FirstOrDefault(t => t.Name == tc.name);
                    if (tool == null)
                        result = "Unknown tool";
                    else
                    {
                        try
                        {
                            var input = string.IsNullOrWhiteSpace(tc.args.ToString()) ? new JsonObject() : JsonNode.Parse(tc.args.ToString()) as JsonObject;
                            result = tool.Execute(input ?? new JsonObject()) ?? "Done";
                        }
                        catch (Exception e) { result = "Tool execution failed: " + e.Message; }
                    }
                    messages.Add(new Dictionary<string, object>
                    {
                        ["role"] = "tool",
                        ["tool_call_id"] = tc.id,
                        ["content"] = result,
                    });
                }
            }
            return fullText.ToString();
        }
    }
}
