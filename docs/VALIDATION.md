# Validation

Date: 2026-11-04. Windows x64, .NET 10 SDK 10.0.100, MSVC 19.44 and NAudio 3.3.0. Self-contained packaging targets .NET runtime 10.0.14. This is a preview release.

## Completed checks

- Native RNNoise and managed Release builds succeed. NAudio compatibility capture/render APIs produce four obsolescence warnings; no build errors.
- Native DSP integration check: 1,000 synthetic 10 ms blocks across Clean and Russian meme profiles. Non-silent finite output; peak about 0.650; limiter and bypass safety checks pass.
- Build diagnostic timing: median 0.32 ms, p95 0.37 ms, maximum 1.40 ms per block in the recorded run. These are DSP timings, not end-to-end latency, and vary with load and JIT warm-up.
- Physical microphone capture check: 300 blocks in three seconds at 48 kHz mono through WASAPI and RNNoise. Nonzero processed audio. Nothing was recorded or transmitted.
- Desktop inspection: English initial UI, live switch to Spanish, translated title/controls/messages and GIF animation observed. Small icon sizes were visually inspected.
- Original root GIF and dashboard copy have matching SHA-256: `8b93131a2fbd8b795b95fd05e9c822feb87ab3e7c89ee02c8f75973f980bbef0`.

## Installed build

- `setup.exe` built with Inno Setup 6.7.4, self-contained .NET 10.0.14 runtime, native DLL and license notices. Size: 48,690,125 bytes.
- Silent per-user installation completed with exit code 0, with no reboot requested.
- Diagnostics from the installed executable completed with exit code 0. DSP: 1,000 blocks, median 0.3202 ms, p95 0.3791 ms, max 1.7965 ms; limiter and bypass passed.
- Installed physical microphone check: 299 blocks during the three-second capture interval at 48 kHz mono, processed peak 0.0440. No audio saved or transmitted.
- SHA-256 for the release installer: `edfa757e3497fa523edc2dba939f769022882e48fc02652d7be70634a30ec184`.


## Not established

- VB-CABLE was not present among active audio endpoints. Sending audio through CABLE Input → CABLE Output into a voice application remains untested here.
- Valorant was not running. The `vgk` driver was running, while `vgc` was stopped. Local capture was checked on that machine; this does not demonstrate gameplay or full Vanguard compatibility. No game was launched and no anti-cheat service was changed.
- No claim is made about perceived voice quality, long-session stability, CPU use across hardware, or achieving 30 ms end-to-end latency.

## Reproduce local diagnostics

```powershell
$p = Start-Process .\src\CSMic\bin\Release\net10.0-windows\CSMic.exe -ArgumentList '--diagnostics',"$PWD/report.json" -Wait -PassThru
$p.ExitCode
```

Use `--capture-check` instead of `--diagnostics` to explicitly capture three seconds from a physical microphone without playback, recording or transmission. JSON reports include endpoint names and identifiers; keep them local or redact before publishing.

## Finish the audio path check

1. Install VB-CABLE from its official site, restart if needed, choose the physical microphone and CABLE Input in CS MIC, then select CABLE Output in the receiving application.
2. Compare quiet speech, syllable endings, keyboard/room noise, Clean/Meme/Bypass and monitoring with headphones. Confirm the stop button and 8-second test stop audio.
3. Repeat with Discord and with Valorant/Vanguard fully active in a permitted voice-test environment. Record actual versions and results. No guaranteed compatibility.
4. Run 15 minutes while watching DSP time, queue depth, drops and Task Manager CPU/memory. Disconnect/reconnect a USB microphone and confirm manual recovery.

## Measure latency manually

Record the physical microphone and CABLE Output simultaneously on separate tracks in an audio tool. Produce several transients and consonants, compare their sample offsets, and divide by 48 for milliseconds at 48 kHz. RNNoise can suppress isolated impulses, so use voice correlation as well. Repeat at least ten times, compare bypass and processed audio over the same route, and account for baseline recording-path offset. Report median, maximum, formats, hardware and measurement uncertainty. Headphone monitoring adds another output buffer and is not a cable-latency measurement.
