# Third-party notices

Original CS MIC code is MIT licensed. Third-party material retains its own terms.

- **NAudio 3.3.0** and its NAudio component packages: Mark Heath and contributors, MIT. See `third_party/NAudio-LICENSE.txt`. Restored from NuGet. Source: https://github.com/naudio/NAudio .
- **RNNoise v0.1**: https://github.com/xiph/rnnoise/tree/v0.1 . Sources and the embedded `src/rnn_data.c` model are included in `native/rnnoise`. Keep `COPYING` and per-file notices (Mozilla, Jean-Marc Valin, Xiph.Org, Mark Borgerding, CSIRO and other authors). `third_party/RNNoise-NOTICES.txt` collects source notices for binary distribution. Windows changes in `pitch.c` and `celt_lpc.c` replace C99 variable-length arrays with equivalent `_alloca` storage only under MSVC; upstream license headers remain intact. The CMake build links the Microsoft C runtime statically.
- **.NET 10 / WPF**: Microsoft and contributors. The self-contained installer includes the runtime. Packaging copies its license, third-party notices and WPF license into `Licenses`. See https://github.com/dotnet/runtime and https://github.com/dotnet/wpf .
- **Inno Setup**: Jordan Russell and Martijn Laan. Used as an external build tool; its installer engine is covered by its own license. https://jrsoftware.org . Not part of the MIT source license.
- **VB-CABLE**: external VB-Audio software under its own terms. Download separately from https://vb-audio.com/Cable/ . No driver is bundled or installed by CS MIC.
- **GIF artwork and derived icon**: supplied by the repository owner, who reports finding it on Pinterest. Its original author and redistribution license have not been established. Widespread reposting does not establish public-domain status. The original GIF is preserved, and the dashboard copy and ICO derive from it. These image assets are excluded from the MIT grant for original code; no ownership or free-use claim is made. Contact the maintainer with attribution or rights information.
