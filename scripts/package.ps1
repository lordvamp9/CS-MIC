param([string]$Iscc = "${env:ProgramFiles(x86)}/Inno Setup 6/ISCC.exe")
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
& "$PSScriptRoot/build.ps1"
$repoPath = (Get-Location).Path
$publishPath = [IO.Path]::GetFullPath((Join-Path $repoPath 'artifacts/publish'))
if (-not $publishPath.StartsWith($repoPath + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Publish directory must stay inside the repository.' }
if (Test-Path -LiteralPath $publishPath) { Remove-Item -LiteralPath $publishPath -Recurse -Force }
dotnet publish src/CSMic/CSMic.csproj -c Release -r win-x64 --self-contained true -o artifacts/publish
if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
# Include runtime and WPF license material from the packages used by publish.
$packages = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $env:USERPROFILE '.nuget/packages' }
[xml]$project = Get-Content src/CSMic/CSMic.csproj
$runtimeVersion = $project.Project.PropertyGroup.RuntimeFrameworkVersion
Copy-Item "$packages/microsoft.netcore.app.runtime.win-x64/$runtimeVersion/LICENSE.TXT" artifacts/publish/Licenses/DotNet-LICENSE.txt
Copy-Item "$packages/microsoft.netcore.app.runtime.win-x64/$runtimeVersion/THIRD-PARTY-NOTICES.TXT" artifacts/publish/Licenses/DotNet-THIRD-PARTY-NOTICES.txt
Copy-Item "$packages/microsoft.windowsdesktop.app.runtime.win-x64/$runtimeVersion/LICENSE" artifacts/publish/Licenses/WPF-LICENSE.txt
& $Iscc installer/CSMic.iss
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
Get-FileHash dist/setup.exe -Algorithm SHA256 | ForEach-Object { "$($_.Hash.ToLower())  setup.exe" } | Set-Content dist/SHA256SUMS.txt
