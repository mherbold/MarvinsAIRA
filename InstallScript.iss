[Setup]
AppName=Marvin's Awesome iRacing App
AppVersion=0.17
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
SetupIconFile="C:\Users\marvi\Documents\GitHub\MarvinsAIRA\icon.ico"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Files]
Source: "C:\Users\marvi\Documents\GitHub\MarvinsAIRA\bin\publish\*"; DestDir: "{app}"; Flags: ignoreversion

[Dirs]
Name: "{userdocs}\MarvinsAIRA"

[Icons]
Name: "{group}\MarvinsAIRA"; Filename: "{app}\MarvinsAIRA.exe"
Name: "{userdesktop}\MarvinsAIRA"; Filename: "{app}\MarvinsAIRA.exe"; Tasks: desktopicon
