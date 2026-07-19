<div align="center">

<img src="docs/assets/yumi-wave.png" width="260" alt="Yumi" />

# Yumi — Desktop Agent

**Your desktop isn't empty anymore. Yumi lives there.**

*An AI companion that talks, reacts, moves, and remembers — driven entirely by a large language model, running entirely on your machine.*

![Windows](https://img.shields.io/badge/platform-Windows-0078D6?logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNCAyNCI%2BPHBhdGggZmlsbD0iI2ZmZiIgZD0iTTAgMy40IDkuNiAyLjF2OS4zSDB6TTEwLjggMS45IDI0IDB2MTEuNEgxMC44ek0wIDEyLjZoOS42djkuM0wwIDIwLjZ6TTEwLjggMTIuNkgyNFYyNGwtMTMuMi0xLjh6Ii8%2BPC9zdmc%2B)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![Claude](https://img.shields.io/badge/Anthropic-Claude-D97757?logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNCAyNCI%2BPGcgc3Ryb2tlPSIjZmZmIiBzdHJva2Utd2lkdGg9IjIuNCIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIj48cGF0aCBkPSJNMTIgM3YxOE0zIDEyaDE4TTUuNiA1LjZsMTIuOCAxMi44TTE4LjQgNS42IDUuNiAxOC40Ii8%2BPC9nPjwvc3ZnPg%3D%3D)
![OpenAI](https://img.shields.io/badge/OpenAI-compatible-412991?logo=openai&logoColor=white)
![Local First](https://img.shields.io/badge/local--first-no%20telemetry-2ea44f?logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNCAyNCI%2BPHBhdGggZmlsbD0iI2ZmZiIgZD0iTTEyIDFhNSA1IDAgMCAwLTUgNXYzSDZhMiAyIDAgMCAwLTIgMnY5YTIgMiAwIDAgMCAyIDJoMTJhMiAyIDAgMCAwIDItMnYtOWEyIDIgMCAwIDAtMi0yaC0xVjZhNSA1IDAgMCAwLTUtNVptLTMgOFY2YTMgMyAwIDEgMSA2IDB2M1oiLz48L3N2Zz4%3D)

<p>
<a href="https://pump.fun/">
  <img src="docs/assets/pumpfun.png" alt="pump.fun" height="30" />
  <img src="https://img.shields.io/badge/Buy%20%24YUMI%20on-pump.fun-2FD57C?style=for-the-badge" alt="pump.fun" height="28" />
</a>
&nbsp;&nbsp;
<a href="https://x.com/">
  <img src="https://img.shields.io/badge/Follow%20on-X-000000?style=for-the-badge&logo=x&logoColor=white" alt="Follow on X" height="28" />
</a>
</p>

</div>

---

## 🌌 What is Yumi?

Most "desktop pets" are puppets — a fixed set of canned animations on a timer. **Yumi is not a puppet.**

Yumi is a small animated character who lives on your desktop and is driven **end-to-end by a
large language model**. There is no dialogue tree, no scripted personality, no hard-coded
responses. When you talk to her, the model decides what to say, *how* to feel, which animation
to play, and whether something is worth remembering — all in real time, all on your machine.

She streams her replies token-by-token in a terminal-style chat bubble as she "thinks."
She triggers her own expressions through tool calls, so her face matches her words. She keeps
a long-term memory that survives restarts. And when the room goes quiet, she'll speak up on
her own. Point her at any Claude or OpenAI-compatible endpoint, paste your key, and she wakes up.

> **The pitch in one line:** it's not a mascot with a chatbot bolted on — it's an LLM given a
> body, a face, a memory, and a place to live.

---

## ✨ Meet Yumi

<div align="center">

<table>
<tr>
<td align="center"><img src="docs/assets/yumi-happy.png" width="150"/><br/><b>happy</b></td>
<td align="center"><img src="docs/assets/yumi-think.png" width="150"/><br/><b>thinking</b></td>
<td align="center"><img src="docs/assets/yumi-typing.png" width="150"/><br/><b>working</b></td>
<td align="center"><img src="docs/assets/yumi-shy.png" width="150"/><br/><b>shy</b></td>
<td align="center"><img src="docs/assets/yumi-stand.png" width="150"/><br/><b>idle</b></td>
</tr>
</table>

*Every expression is chosen by the model, not a random timer.*

</div>

---

## ⚡ Why Yumi is different

| | Ordinary desktop pet | **Yumi** |
|---|---|---|
| Personality | Hard-coded lines | **Whatever the model is** — swap the persona, swap the vibe |
| Animations | Random / on a timer | **Chosen by the model** to fit the moment, via tool calls |
| Memory | None | **Persistent** long-term memory across sessions |
| Conversation | Keyword bot | **Real** streaming LLM chat, any Claude/OpenAI model |
| Initiative | Reactive only | **Proactive** — idle lines, reactions to events |
| Your data | Often phones home | **Nothing leaves your machine.** No account, no telemetry |

---

## 🧠 Features

- **🤖 LLM-driven, not scripted** — every reply, mood, and animation choice comes from the model at runtime. Change the model and Yumi genuinely changes.
- **🔌 Bring your own model** — native **Anthropic Messages API** *and* any **OpenAI-compatible** endpoint. Set base URL, key, and model name; that's it.
- **💬 Streaming speech** — replies render token-by-token in a terminal-style bubble, so she "types" as she thinks.
- **🎭 Tool-driven animation** — the model calls tools to play animations, keeping her expression in sync with what she's saying.
- **🧩 Long-term memory** — she remembers facts about you and recalls them in later sessions.
- **👀 Proactive & reactive** — idle lines when it's quiet, reactions to events — not just call-and-response.
- **📊 Usage panel** — a live readout of the active model, requests today, and input / output / total tokens.
- **🔒 Private by design** — everything runs locally. No cloud service, no account, no telemetry. Your key and chats stay on disk.
- **🎨 Themeable** — ships a **Dark Purple** terminal theme, plus **Blue / Purple / Yellow**.

---

## 🔧 How it works

```
You type  ─►  AIPet plugin builds the prompt (persona + memory + context)
          ─►  streams to your LLM (Anthropic / OpenAI-compatible)
          ─►  model streams back text  ──►  shown token-by-token in the bubble
          └─  model emits tool calls   ──►  play animation • save memory • react
                                        └─  loop until the turn is done
```

Yumi is a **host app** (the animated character, window, and rendering) plus a **brain plugin**
(`AIPet`) that owns the conversation, the tools, the memory, and the usage stats. The brain
talks to whatever model you point it at; the host gives it a body to express through.

---

## 🗂️ Architecture

```
VPet-Simulator.Core/               Rendering / animation / interaction core
VPet-Simulator.Windows/            Main application (settings, mod loader, saves)
VPet-Simulator.Windows.Interface/  Plugin contracts (chat box, base classes)
VPet.Plugin.AIPet/                 The brain: chat, tools, memory, usage stats
mod/0000_core/                     Core data: character, themes, language, text
tools/gen_pet.py                   Character animation-frame generator
```

**Core data layout (`mod/0000_core`):**

```mermaid
flowchart LR
    root["📦 0000_core"]
    root --> file["📁 file"]
    root --> image["📁 image"]
    root --> lang["📁 lang"]
    root --> pet["📁 pet"]
    root --> text["📁 text"]
    root --> theme["📁 theme"]

    file --> fileN["2025 · 2026 · expression<br/>gif · illustration · Thumbnail<br/><i>packaged core data</i>"]
    image --> imageN["<i>UI icons &amp; shared images</i>"]
    lang --> en["en"]
    en --> enN["Base* · CGPT* · Prog* · Text*<br/><i>English localization</i>"]
    pet --> aigirl["aigirl · Yumi"]
    aigirl --> aigirlN["Default · IDEL · Say · Think<br/>Touch_Head · Touch_Body<br/>Sleep · StartUP · Shutdown · Raise<br/><i>animation states</i>"]
    text --> textN["ClickText · SelectText · LowText<br/><i>preset idle / click lines</i>"]
    theme --> themeN["terminal = Dark Purple · default = Blue<br/>prupe = Purple · meme = Yellow<br/><i>UI themes</i>"]
```

---

## 🚀 Build & run

Requires the **.NET 8 SDK**. The app is **x64-only**.

```powershell
# Build the app (x64 required)
dotnet build VPet-Simulator.Windows\VPet-Simulator.Windows.csproj -c Release -p:Platform=x64

# Build the AI plugin
dotnet build VPet.Plugin.AIPet\VPet.Plugin.AIPet.csproj -c Release
```

Assemble the run directory (output lands in `VPet-Simulator.Windows\bin\x64\Release\net8.0-windows`):

1. Copy `VPet-Simulator.Windows\mod` into the output `mod\` folder.
2. Copy the built plugin DLL into `mod\AIPet\plugin\`.
3. Run `VPet-Simulator.Windows.exe`.

---

## ⚙️ Configuration

Right-click Yumi → **System → Settings → chat API "AIDeskPet" → open settings**, then set:

- **Protocol** — Anthropic or OpenAI-compatible
- **API base URL**, **API key**, **model name**

Storage:
- AI config → the `AIPet` line of `Setting.lps`
- Chat history & token usage → `mod\AIPet\data\`

---

## 🗺️ Roadmap

**Phase 1 — Alive** ✅
Core LLM loop, streaming speech, tool-driven animation, long-term memory, usage panel, Yumi character, terminal theme.

**Phase 2 — Voice** 🔜
- 🔊 Voice output (EdgeTTS) — Yumi speaks out loud
- 🎙️ Voice input (Vosk) — talk to her hands-free

**Phase 3 — Reach** 🧭
- 🌐 Self-hosted backend for multiplayer / shared presence
- 🖥️ Screen & context awareness she can act on

**Phase 4 — Polish** 💅
- Restyle the remaining secondary windows to match the theme
- More characters and themes

---

## ❓ FAQ

**Does it send my data anywhere?**
No. Your API key and conversations stay on your machine. The only network call is the one *you*
configure — straight to your chosen LLM provider.

**Which models work?**
Any Anthropic Claude model (native Messages API) and any OpenAI-compatible endpoint.

**Can I change her personality?**
Yes — the persona is part of the prompt. Rewrite it and she becomes someone else.

**Windows only?**
For now, yes — it's a WPF / .NET 8 desktop app.

---

<div align="center">
<sub>Built on the VPet-Simulator engine (Apache-2.0).</sub>
</div>
