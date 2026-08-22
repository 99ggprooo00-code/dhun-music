#define MyAppName "DHUN"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "DHUN contributors"
#define MyAppURL "https://github.com/99ggprooo00-code/dhun-music"
#define MyMsix "Dhun.WinUI_1.0.0.0_x64.msix"

[Setup]
AppId={{6E105052-3FC5-4F17-B46C-C168B6DF924E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
DefaultDirName={autopf}\DHUN
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\artifacts\installer
OutputBaseFilename=DHUN-Native-v1.0.0-Setup-x64
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\src\Dhun.WinUI\Assets\AppLogo.ico
UninstallDisplayIcon={app}\AppLogo.ico
LicenseFile=..\LICENSE
VersionInfoVersion=1.0.0.0
VersionInfoProductName=DHUN
VersionInfoDescription=DHUN Native Windows Installer
VersionInfoCompany={#MyAppPublisher}

[Files]
Source: "..\artifacts\msix\Dhun.WinUI_1.0.0.0_x64_Test\{#MyMsix}"; DestDir: "{tmp}"; Flags: deleteafterinstall
Source: "..\src\Dhun.WinUI\Assets\AppLogo.ico"; DestDir: "{app}"
Source: "..\LICENSE"; DestDir: "{app}"
Source: "..\THIRD_PARTY.md"; DestDir: "{app}"

[Run]
Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""Add-AppxPackage -Path '{tmp}\{#MyMsix}' -AllowUnsigned"""; StatusMsg: "Installing the DHUN Windows package..."; Flags: runhidden waituntilterminated

[UninstallRun]
Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""Get-AppxPackage -Name 'DhunMusic.Dhun' | Remove-AppxPackage"""; Flags: runhidden waituntilterminated; RunOnceId: "RemoveDhunMsix"
