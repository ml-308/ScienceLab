; 科学实验室 ScienceLab · 安装程序（Inno Setup 6）
; 页面结构与选项由 common.iss 统一提供；这里只声明应用身份与要安装的文件。
; 构建：build-setup.cmd   产物：dist\ScienceLab-Setup-1.1.1.exe

#define AppName "科学实验室"
#define AppSlug "ScienceLab"
#define AppVersion "1.1.1"
#define AppId "{{DD92FB8A-BFF3-4454-A3F5-FC7200CAFCAF}"
#define AppExe "ScienceLab.exe"
#define AppRepo "ScienceLab"
#define AppTagline "自由开放的数学 · 物理 · 化学学习工具集：自定义函数坐标系、物块运动仿真、2D 化学实验室。"

#include "common.iss"

[Files]
; 框架依赖发布产物（需 .NET 8 Desktop Runtime）
Source: "{#AppRoot}\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#AppRoot}\README.md"; DestDir: "{app}"
Source: "{#AppRoot}\LICENSE"; DestDir: "{app}"

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"; WorkingDir: "{app}"; Tasks: startmenuicon
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; WorkingDir: "{app}"; Tasks: desktopicon
