$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
cmake -S native -B native/build -A x64
if ($LASTEXITCODE -ne 0) { throw 'RNNoise configuration failed.' }
cmake --build native/build --config Release
if ($LASTEXITCODE -ne 0) { throw 'RNNoise build failed.' }
dotnet build src/CSMic/CSMic.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'CS MIC build failed.' }
