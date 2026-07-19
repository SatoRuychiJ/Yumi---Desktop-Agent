# Yumi — Desktop Agent

**A living, AI-powered companion that sits on your desktop.**

Yumi is a small animated character driven end-to-end by a large language model. You chat with
her in a terminal-style box and she talks back — but she also reacts, plays her own
animations, remembers things about you, and speaks up on her own when things go quiet. She
feels less like a scripted mascot and more like someone actually there.

Everything Yumi does is decided by the model in real time. She streams her replies as she
"thinks," triggers her own animations through tool calls so her expressions match her words,
keeps a long-term memory across sessions, and reacts to what's happening on screen. Point her
at any Anthropic or OpenAI-compatible endpoint, drop in your API key, and she comes alive.

Yumi runs entirely on your machine — no account, no cloud service, no telemetry. Your API key
and conversations never leave your computer.

## Features

- **LLM-driven, not scripted** — every reply, reaction, and animation choice comes from the model at runtime.
- **Bring your own model** — native Anthropic Messages API and any OpenAI-compatible endpoint; just set the base URL, key, and model name.
- **Streaming speech** — replies appear token-by-token in a terminal-style chat bubble as she generates them.
- **Tool-driven animation** — the model calls tools to play animations, so her expressions match what she's saying.
- **Long-term memory** — she remembers facts about you across restarts.
- **Proactive & reactive** — idle lines when it's quiet, reactions to events, not just call-and-response.
- **Usage panel** — a live readout of the current model, requests today, and input / output / total tokens.
- **Private by design** — everything runs locally; nothing is uploaded anywhere.
- **Themeable** — ships a Dark Purple terminal theme, plus Blue / Purple / Yellow.

## Project layout

```
VPet-Simulator.Core/               Rendering / animation / interaction core
                                   (toolbar panel, message bubble, theme colors)
VPet-Simulator.Windows/            Main application (settings, mod loader, save system)
VPet-Simulator.Windows.Interface/  Plugin contracts (chat box, plugin base classes)
VPet.Plugin.AIPet/                 The AI brain: chat, tools, memory, usage stats
mod/0000_core/                     Core data: character (Yumi), themes, language,
                                   text, UI images
tools/gen_pet.py                   Character animation-frame generator
docs/dev-guide/                    Mod-development notes
```

## Build & run

Requires the .NET 8 SDK. The app is **x64-only**.

```powershell
# Build the app (x64 required)
dotnet build VPet-Simulator.Windows\VPet-Simulator.Windows.csproj -c Release -p:Platform=x64

# Build the AI plugin
dotnet build VPet.Plugin.AIPet\VPet.Plugin.AIPet.csproj -c Release
```

Then assemble the run directory (output lands in
`VPet-Simulator.Windows\bin\x64\Release\net8.0-windows`):

1. Copy `VPet-Simulator.Windows\mod` into the output `mod\` folder.
2. Copy the built plugin DLL into `mod\AIPet\plugin\`.
3. Run `VPet-Simulator.Windows.exe`.

## Configuring the AI

Right-click Yumi → **System → Settings → chat API "AIDeskPet" → open settings**, then set the
protocol (Anthropic / OpenAI-compatible), API base URL, key, and model.

- AI configuration is stored in the `AIPet` line of `Setting.lps`.
- Chat history and token usage are stored in `mod\AIPet\data\`.

## Roadmap

- Voice output (EdgeTTS) and voice input (Vosk).
- A self-hosted backend for multiplayer.
- Restyling the remaining secondary windows (detailed panel, shop) to match the theme.

---

<sub>Built on the VPet-Simulator engine (Apache-2.0).</sub>
