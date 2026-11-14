# 科学实验室 ScienceLab

自由开放的数学 · 物理 · 化学学习工具集（Windows 桌面应用）。

## 下载安装

从 [Releases](https://github.com/ml-308/ScienceLab/releases) 下载 `ScienceLab-Setup-1.2.0.exe`，双击安装即可（每用户安装，无需管理员权限）。

> 运行需要 [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)（Windows x64）。

## 功能模块

### 📐 数学 · 坐标系
- 可自由缩放（滚轮）、平移（拖拽）的平面直角坐标系
- **自定义函数绘图**：输入任意表达式即时绘制，支持四则运算、幂、括号、常用函数（sin/cos/tan/asin/acos/atan/sqrt/abs/ln/log/exp/sinh/cosh/tanh）、常量 pi/e、变量 x
- 预设函数快速选择、函数列表管理（增删、配色）
- 自动处理 tan 等函数的渐近线断开

### ⚙️ 物理实验室
- **斜面 + 小物块** 运动仿真（参考 bilibili 物理运动可视化）
- 可调参数：斜面倾角、质量、摩擦系数、初速度、起始位置
- 实时显示物块状态：位移、速度、加速度
- 受力箭头可视化：重力 G、支持力 N、摩擦力 f
- 动画播放/暂停/重置，到达端点自动停止

### ⚗️ 化学实验室
- 2D 自由实验室（参考 NB 实验室）
- 试剂架拖拽滴入：稀盐酸、NaOH、酚酞、石蕊、CuSO₄、Na₂CO₃ 等 10 种试剂
- 反应规则引擎：酸碱中和、酚酞/石蕊变色、蓝色沉淀 Cu(OH)₂、白色沉淀 AgCl、CO₂ 气泡
- 实验记录日志
- 元素周期表（常用元素，点击查看详情）

## 技术栈

- .NET 8 + WPF（MVVM 分层，DirectX 渲染）
- 现代化卡片式 UI，深/浅主题可切换
- 模块化架构：`Controls/`（绘图引擎）、`Views/`（视图）、`Chemistry/`（化学数据层）、`Utils/`（表达式解析器）

## 构建与发布

```bash
# 编译
dotnet build -c Debug

# 发布（框架依赖，体积小）
dotnet publish -c Release -r win-x64 --self-contained false -o publish/win-x64

# 或双击 publish.cmd（默认常规发布，传参"自包含"可打单文件）
```

发布产物在 `publish/win-x64/`，双击 `ScienceLab.exe` 即可运行。

## 项目结构

```
ScienceLab/
├── App.xaml / App.xaml.cs       # 全局主题资源、主题切换
├── MainWindow.xaml(.cs)         # 主窗口：导航 + 内容区
├── Controls/
│   ├── CoordinateCanvas.cs      # 坐标系绘图引擎
│   ├── PhysicsCanvas.cs         # 物理仿真画布
│   └── ChemistryCanvas.cs       # 化学实验台画布
├── Views/                       # 四大视图（Home/Math/Physics/Chemistry）
├── Chemistry/                   # 试剂库、反应引擎、元素周期表
├── Utils/
│   └── ExpressionParser.cs      # 数学表达式解析器
└── publish.cmd                  # 一键发布脚本
```
