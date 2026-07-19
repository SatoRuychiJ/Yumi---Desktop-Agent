<div align="center">

<img src="docs/assets/yumi-wave.png" width="200" alt="Yumi" />

# Yumi — Desktop Agent

An AI desktop companion built around interaction — she keeps you company while you play, watch, and trade, reacts in the moment, and remembers.

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

## Overview

Most AI on the desktop is built to take an instruction, run a task, and get out of the way. Yumi
is built for the opposite: to be present. She stays on screen through whatever you are doing — a
gaming session, a movie, a long night in front of the charts — as someone to talk to, react with,
and keep you company in the moment. The point is not throughput; it is company.

What makes that work is that her behavior comes from a language model rather than a script. There
is no dialogue tree and no fixed set of canned lines. On every turn the model decides what she
says, which animation to play, and what is worth remembering. You type in a terminal-style box and
the reply streams back token by token; while it writes, it calls tools to move her on screen, so
what she does stays tied to what she is saying.

She also acts without being addressed. She reacts when you pick her up or pat her head, and during
quiet stretches she speaks up on her own — closer to someone sharing the desk with you than a
window you open when you need an answer.

Everything runs on your own machine. You bring an API key for a provider of your choice, and that
is the only place your text is sent: no account, no backend, no telemetry.

<div align="center">
<table>
<tr>
<td align="center"><img src="docs/assets/yumi-happy.png" width="140"/><br/>happy</td>
<td align="center"><img src="docs/assets/yumi-think.png" width="140"/><br/>thinking</td>
<td align="center"><img src="docs/assets/yumi-typing.png" width="140"/><br/>working</td>
<td align="center"><img src="docs/assets/yumi-shy.png" width="140"/><br/>shy</td>
<td align="center"><img src="docs/assets/yumi-stand.png" width="140"/><br/>idle</td>
</tr>
</table>
</div>

## How it works

The application is split in two. The host is the desktop app: the window, the character, the
animation engine, and the settings. The brain is a plugin (`AIPet`) that owns the conversation,
the tools, the memory, and the usage counters. The host gives the model a body; the plugin decides
what to do with it.

A turn runs like this:

```
input ──▶ build request      persona + long-term memory + recent history + the new message
      ──▶ stream to provider  Anthropic Messages API, or any OpenAI-compatible endpoint
      ──▶ receive text        rendered token by token in the chat bubble
      └── receive tool calls  play an animation · sleep · save a memory
          then continue the turn until the model stops
```

There are three ways a turn can start:

- **You talk to her.** Text from the chat box goes straight into a turn.
- **Something happens to her.** Picking her up or patting her head raises an event. Events are
  rate-limited and fire on a probability, so she reacts sometimes rather than every single time.
- **Nobody has said anything for a while.** A timer checks whether it is time to speak up. Quiet
  hours (configurable) suppress this so she stays silent at night.

### Tools

When tools are enabled, the model is given three of them and calls them as part of its reply:

| Tool | Argument | Effect |
|------|----------|--------|
| `play_animation` | `name` | Plays one of the character's animations (the available names are passed to the model, drawn from the loaded animation set). |
| `sleep` | — | Puts her into the sleeping state. The prompt tells the model to use it only when it makes sense — late at night, or when tired. |
| `remember` | `content` | Appends a one-line note to long-term memory. |

### Memory

`remember` writes a short, dated line into a list that persists to disk. The list is capped at the
most recent 100 entries; older ones fall off. On every turn the stored memories are folded back
into the prompt, so she can bring up something you told her days earlier. Conversation history is
kept separately and trimmed to a configurable depth.

### Providers

Two request formats are supported and selected per configuration:

- **Anthropic** — the native Messages API, including streaming and tool use.
- **OpenAI-compatible** — any endpoint that speaks the OpenAI chat-completions format, which
  covers most self-hosted and third-party gateways. Point `BaseUrl` at it and set the model name.

Both paths stream, so text appears as it is generated instead of arriving all at once.

## Configuration

Right-click the character and open **System → Settings → chat API → settings**. The relevant
fields:

| Setting | Meaning |
|---------|---------|
| `Provider` | `anthropic` or an OpenAI-compatible endpoint. |
| `BaseUrl` | API base URL. Leave blank to use the provider default. |
| `ApiKey` | Your key. Stored locally, never sent anywhere but the provider. |
| `Model` | Model name to call. |
| `Persona` | The character's personality, injected into the system prompt. Rewrite it and she becomes someone else. |
| `UserNick` | What she calls you. |
| `EnableTools` | Whether the model may play animations, sleep, and save memory. |
| `EnableReactions` | Whether she reacts to being touched or picked up. |
| `EnableProactive` | Whether she speaks during idle periods. |
| `ProactiveInterval` | How long a quiet stretch must be before she speaks. |
| `QuietStart` / `QuietEnd` | Hours during which proactive speech is suppressed. |
| `MaxHistory` | How many past turns to keep in context. |

Configuration lives in the `AIPet` line of `Setting.lps`. Chat history, saved memories, and token
usage are stored under `mod\AIPet\data\`.

## Building from source

Requires the **.NET 8 SDK**. The application is **x64 only**.

```powershell
dotnet build VPet-Simulator.Windows\VPet-Simulator.Windows.csproj -c Release -p:Platform=x64
dotnet build VPet.Plugin.AIPet\VPet.Plugin.AIPet.csproj -c Release
```

Assemble the run directory (build output goes to
`VPet-Simulator.Windows\bin\x64\Release\net8.0-windows`):

1. Copy `VPet-Simulator.Windows\mod` into the output `mod\` folder.
2. Copy the built plugin DLL into `mod\AIPet\plugin\`.
3. Run `VPet-Simulator.Windows.exe`.

## Project layout

```
VPet-Simulator.Core                Rendering, animation, and interaction engine
VPet-Simulator.Windows             The desktop application (window, settings, mod loader)
VPet-Simulator.Windows.Interface   Contracts shared between the app and plugins
VPet.Plugin.AIPet                  The brain: conversation, tools, memory, usage stats
mod/0000_core                      Character, themes, text, and UI assets
tools/gen_pet.py                   Generates a character's animation frames from source poses
```

## Roadmap

- Spoken replies (EdgeTTS) and voice input (Vosk).
- A self-hosted backend for shared, multi-user presence.
- Screen and context awareness the model can act on.
- Reworking the remaining secondary windows to match the current theme.

## Notes

Yumi is Windows-only; it is a WPF application on .NET 8. A few of the older secondary windows
(the detailed stats panel, the shop) are inherited from the underlying engine and have not been
restyled yet. The character ships as a set of pre-generated animation frames; `tools/gen_pet.py`
is what turns a handful of source poses into the full set.

## Credits

Built on the VPet-Simulator engine (Apache-2.0).
