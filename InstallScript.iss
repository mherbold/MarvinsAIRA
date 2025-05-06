#define ApplicationName 'C:\Users\marvi\Documents\GitHub\MarvinsAIRA\bin\publish\MarvinsAIRA.exe'
#define ApplicationVersion GetFileVersion(ApplicationName)

[Setup]
AppName=Marvin's Awesome iRacing App
AppVersion={#ApplicationVersion}
AppCopyright=Created by Marvin Herbold
AppPublisher=Marvin Herbold
AppPublisherURL=
WizardStyle=modern
DefaultDirName={autopf}\MarvinsAIRA
DefaultGroupName=MarvinsAIRA
UninstallDisplayIcon={app}\MarvinsAIRA.exe
Compression=lzma2
SolidCompression=yes
OutputBaseFilename=MarvinsAIRA-Setup-{#ApplicationVersion}
OutputDir=userdocs:MarvinsAIRA
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
SetupIconFile="C:\Users\marvi\Documents\GitHub\MarvinsAIRA\white-icon.ico"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Dirs]
Name: "{userdocs}\MarvinsAIRA"

[Files]
Source: "C:\Users\marvi\Documents\GitHub\MarvinsAIRA\bin\publish\*"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\marvi\Documents\GitHub\MarvinsAIRASimHub\bin\release\MarvinsAIRASimHub.dll"; DestDir: "{userdocs}\MarvinsAIRA"; Flags: ignoreversion

[Icons]
Name: "{group}\MarvinsAIRA"; Filename: "{app}\MarvinsAIRA.exe"
Name: "{userdesktop}\MarvinsAIRA"; Filename: "{app}\MarvinsAIRA.exe"; Tasks: desktopicon

[Run]
Filename: {app}\MarvinsAIRA.exe; Description: {cm:LaunchProgram,{cm:AppName}}; Flags: nowait postinstall skipifsilent

[CustomMessages]
AppName=Marvin's Awesome iRacing App
LaunchProgram=Start Marvin's Awesome iRacing App
