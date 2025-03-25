[Setup]
AppName=Marvin's Awesome iRacing App
AppVersion=0.65
AppCopyright=Created by Marvin Herbold
AppPublisher=Marvin Herbold
AppPublisherURL=
WizardStyle=modern
DefaultDirName={autopf}\MarvinsAIRA
DefaultGroupName=MarvinsAIRA
UninstallDisplayIcon={app}\MarvinsAIRA.exe
Compression=lzma2
SolidCompression=yes
OutputBaseFilename=MarvinsAIRA-Setup
OutputDir=userdocs:MarvinsAIRA
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
SetupIconFile="C:\Users\marvi\Documents\GitHub\MarvinsAIRA\icon.ico"

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
