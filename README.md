# CS MIC

[![Windows](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4)](https://github.com/lordvamp9/CS-MIC/releases)
[![.NET 10](https://img.shields.io/badge/.NET-10%20LTS-512BD4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Build](https://github.com/lordvamp9/CS-MIC/actions/workflows/build.yml/badge.svg)](https://github.com/lordvamp9/CS-MIC/actions/workflows/build.yml)
[![Release](https://img.shields.io/github/v/release/lordvamp9/CS-MIC?include_prereleases)](https://github.com/lordvamp9/CS-MIC/releases)
[![License](https://img.shields.io/badge/code-MIT-green)](LICENSE)

<img src="src/CSMic/Assets/brand.gif" width="110" alt="CS MIC animated artwork">

**Noise out. Chaos in.** Local microphone cleanup and Russian CS-style meme saturation for Windows. English by default; switch to Spanish instantly. No accounts, telemetry or game hooks.

## Install

1. Download **setup.exe** from [Releases](https://github.com/lordvamp9/CS-MIC/releases). Windows x64; Windows 10 22H2 or Windows 11 required. The .NET runtime is included. The preview installer is unsigned.
2. Install [VB-CABLE separately from VB-Audio](https://vb-audio.com/Cable/) and restart if requested. No driver is included in CS MIC.
3. Open CS MIC. Pick your physical microphone and **CABLE Input** as output. Press **Start**.
4. Select **CABLE Output** as the microphone in Discord, Valorant, CS2 or League voice settings. Keep your headphones as their playback device. If a game only uses the Windows communications default, set CABLE Output there and restart the client.

```text
Physical microphone → CS MIC → CABLE Input
                                  ↓ VB-CABLE
Discord / game      ←         CABLE Output
```

Install is per user, without administrator access or automatic startup. Uninstall from Windows Installed apps. Local settings are retained. Compare the download against `SHA256SUMS.txt` in the release.

## Controls

- **Clean:** RNNoise, soft gate, 85 Hz high-pass, compression and safety limiter.
- **Russian meme:** adds presence EQ, low-pass and adjustable saturation.
- **Bypass:** immediate dry voice, with the safety limiter retained.
- **Microphone test:** 8 seconds of headphone monitoring; also sends audio to the selected output. Mute voice chat first if needed.
- **Language / Idioma:** English or Spanish; saved immediately. **Save settings** stores devices and effect controls locally.

Use headphones to prevent feedback. Monitoring runs at reduced volume. Do not select a cable as the monitoring device or enable Windows “Listen to this device” simultaneously. Stop before changing devices. After a USB disconnect, stop, refresh and restart. If voice is cut off, compare with Discord/game noise suppression disabled to avoid double processing.

Settings: `%LOCALAPPDATA%\CSMic\ajustes.json`. No audio files are recorded. Processing is local. Device names come from Windows and are not translated.

## Build from source

Requires .NET 10 SDK, CMake 3.20+, and Visual Studio 2022/2026 C++ Build Tools with an x64 toolchain and Windows SDK.

```powershell
.\scripts\build.ps1
.\src\CSMic\bin\Release\net10.0-windows\CSMic.exe
```

Build the self-contained installer with [Inno Setup 6.7.4](https://jrsoftware.org/isdl.php):

```powershell
.\scripts\package.ps1 -Iscc 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'
```

Output: `dist/setup.exe` and `dist/SHA256SUMS.txt`. Build tools and generated binaries are ignored by Git. The installer is attached to releases, not committed to source. NuGet is needed during build; the app does not require network access.

## Validation and limits

See [validation results](docs/VALIDATION.md) for what was actually checked. `--diagnostics report.json` runs native DSP, limiter and device-enumeration checks without microphone capture. `--capture-check report.json` additionally processes three seconds of the physical microphone without saving or transmitting audio. Both exit nonzero on failure; reports contain local endpoint identifiers, so review before sharing.

48 kHz mono, 480-sample blocks. **30 ms added latency is a goal, not a guarantee.** DSP time and queue depth shown in the app exclude Windows, VB-CABLE and the receiving app. Measure end-to-end delay on your hardware; instructions are in the validation document. Queues are bounded; a growing drop counter indicates audio falling behind.

Uses NAudio 3.3.0 WASAPI compatibility APIs, which currently emit obsolescence warnings recommending the newer builders. No process inspection, memory access, injection, anti-cheat evasion or game integration. **Valorant/Vanguard compatibility is not guaranteed.**

## License

Original code: [MIT](LICENSE). RNNoise, model, NAudio and bundled runtime notices: [third-party notices](THIRD_PARTY_NOTICES.md). The supplied GIF and derived icon have unverified artwork rights and are excluded from the MIT grant for original code. VB-CABLE is separately licensed and distributed by VB-Audio.
