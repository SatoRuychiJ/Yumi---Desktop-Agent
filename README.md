# Yumi — Desktop Agent

An **AI-powered desktop assistant** for Windows. A small animated character (Yumi) lives on
your desktop and is driven end-to-end by a large language model — you talk to it, it talks
back, and it controls its own animations, reactions, and behavior.

Yumi is a standalone fork of the open-source
[VPet-Simulator](https://github.com/LorisYounger/VPet) (Apache-2.0). It **does not depend on
Steam** and is built from source. The original project is a virtual pet game; this fork
strips the game systems away and rebuilds it as an English, AI-first assistant.

## What's different from the upstream game

| Area | Yumi — Desktop Agent |
|---|---|
| Focus | An AI assistant, not a pet. Hunger / mood / stamina / thirst stats are removed — it can't "starve" or need care. |
| Panel | The old status bars are replaced by an **AI usage panel**: current model, requests today, input/output tokens, cumulative tokens. |
| Brain | Built-in **AIPet** plugin: Anthropic Messages API (native) **and** OpenAI-compatible endpoints, streaming replies, tool-use to drive animations, long-term memory, idle/proactive lines, and event reactions. |
| Character | Ships with **Yumi**, a program-generated character (10 source poses expanded into animation frames). |
| Language | **English only.** All single-player game text, Chinese language packs, and anime-flavored dialogue have been removed or rewritten. |
| Theme | **Dark Purple** terminal-style theme (also ships Blue / Purple / Yellow). |
| Removed | Feeding, study, sleep, the money/economy system, mod publishing/management, photo gallery, and the crash-feedback uploader. |
| Privacy | **All telemetry and external network calls to the original author's servers (`*.exlb.net`) have been removed.** No save data or IDs are sent anywhere. |
| Multiplayer | Cut from the UI; the code is kept as a minimal skeleton to rebuild on a self-hosted backend later. |

## Project layout

```
VPet-Simulator.Core/               Rendering / animation / interaction core
                                   (toolbar panel, message bubble, theme colors)
VPet-Simulator.Windows/            Main application (settings, mod loader, save system)
VPet-Simulator.Windows.Interface/  Plugin contracts (chat box, plugin base classes)
VPet.Plugin.AIPet/                 The AI brain: chat, tools, memory, usage stats
mod/0000_core/                     Core data mod: character (aigirl/Yumi), themes,
                                   language, text, UI images
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

Right-click the character → **System → Settings → chat API "AIDeskPet" → open settings**,
then set the protocol (Anthropic / OpenAI-compatible), API base URL, key, and model.

- AI configuration is stored in the `AIPet` line of `Setting.lps`.
- Chat history and token usage are stored in `mod\AIPet\data\`.

## Roadmap

- Voice output (EdgeTTS) and voice input (Vosk).
- A self-hosted backend to bring multiplayer back.
- Restyling the remaining upstream windows (detailed panel, shop) to match the theme.

## License & attribution

AIDeskPet is derived from [VPet-Simulator](https://github.com/LorisYounger/VPet), licensed
under **Apache-2.0**. That license and its attribution requirements continue to apply to the
upstream-derived code in this repository.
