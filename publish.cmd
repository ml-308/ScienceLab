@echo off
REM =====================================================
REM  科学实验室 ScienceLab - 一键发布脚本
REM  用法：双击运行，或在命令行执行 publish.cmd [自包含]
REM    默认  ：框架依赖常规发布（体积小，需目标机器装 .NET 8 桌面运行时）
REM    自包含：单文件 exe，无需装 .NET，体积较大
REM =====================================================
setlocal
cd /d "%~dp0"

set MODE=%1
if "%MODE%"=="" set MODE=常规

echo ============================================
echo   正在发布 ScienceLab（模式：%MODE%）
echo ============================================

if /i "%MODE%"=="自包含" (
    dotnet publish -c Release -r win-x64 --self-contained true ^
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true ^
        -p:EnableCompressionInSingleFile=true -o publish\win-x64
) else (
    dotnet publish -c Release -r win-x64 --self-contained false -o publish\win-x64
)

if errorlevel 1 (
    echo.
    echo [失败] 发布出错，请检查上方错误信息。
    pause
    exit /b 1
)

echo.
echo ============================================
echo   发布完成！输出目录：publish\win-x64
echo   可执行文件：ScienceLab.exe
echo ============================================
pause
