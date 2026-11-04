#define AppVersion "0.1.0"
[Setup]
AppId={{8C77C7E8-7535-4BC4-823F-C893CB29129D}
AppName=CS MIC
AppVersion={#AppVersion}
AppPublisher=CS MIC contributors
AppPublisherURL=https://github.com/lordvamp9/CS-MIC
AppSupportURL=https://github.com/lordvamp9/CS-MIC/issues
DefaultDirName={localappdata}\Programs\CS MIC
DefaultGroupName=CS MIC
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19045
LicenseFile=..\LICENSE
OutputDir=..\dist
OutputBaseFilename=setup
SetupIconFile=..\src\CSMic\Assets\app.ico
UninstallDisplayIcon={app}\CSMic.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
LanguageDetectionMethod=none
UsePreviousLanguage=yes
DisableProgramGroupPage=yes
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
[Files]
Source: "..\artifacts\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\CS MIC"; Filename: "{app}\CSMic.exe"
[Run]
Filename: "{app}\CSMic.exe"; Description: "{cm:LaunchProgram,CS MIC}"; Flags: nowait postinstall skipifsilent
