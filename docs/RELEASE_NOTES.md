# CS MIC v0.1.0

Local microphone cleanup and meme voice effects for Windows x64.

- English by default, with instant Spanish switching and saved language choice.
- Clean / Russian meme profiles, stronger adjustable saturation, RNNoise, bypass, percentage meters, headphone monitoring, and reduced speech clipping.
- Closing the window keeps CS MIC in the notification area; use its right-click menu to reopen or exit.
- Per-user `setup.exe` with the .NET 10 runtime included. No drivers bundled.
- Native DSP and physical microphone capture checks passed. See the validation report for scope and installer results.

Install VB-CABLE separately, send CS MIC to **CABLE Input**, and select **CABLE Output** in your voice app.

**Anti-cheat compatibility warning:** software and games with kernel-level anti-cheat, including Valorant with Vanguard, may block, interfere with, or flag virtual-audio routing or audio software. Active Valorant/Vanguard gameplay has not been tested; compatibility is not guaranteed. Follow game rules and do not bypass anti-cheat.

30 ms is a latency target, not a guarantee. The installer is unsigned. Artwork rights are unverified and separate from the MIT code license.

Download `setup.exe` and verify it against `SHA256SUMS.txt`. Source, build instructions and third-party notices are in the repository.
