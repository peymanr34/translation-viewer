#define MyAppName "Translation Viewer"
#define MyAppPublisher "Peyman Mohammadi"
#define MyAppExeName "TranslationViewer.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{6A1D2E50-42FE-4D4B-9E22-F071CD24E3AA}
AppName={#MyAppName}
AppPublisher={#MyAppPublisher}
AppCopyright=© 2023 {#MyAppPublisher}

; Versions
AppVersion={#MyAppVersion}
VersionInfoVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}

; Output
WizardStyle=modern
SolidCompression=yes
Compression=lzma

DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Assume all files in the source folder are application files.
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "Files\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

