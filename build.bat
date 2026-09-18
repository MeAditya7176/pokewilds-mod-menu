@echo off
title Building PokeWilds Mod Menu...
echo ===================================================
echo     Compiling PokeWilds Mod Menu ^& Spawner v1.5
echo ===================================================
echo.

set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC%" (
    echo [ERROR] csc.exe not found at %CSC%
    echo Please make sure .NET Framework 4.5+ is installed.
    pause
    exit /b 1
)

echo Compiling PokeWildsModMenu.cs...
if exist app_logo.ico (
    "%CSC%" /target:winexe /optimize+ /win32icon:app_logo.ico /r:System.dll,System.Drawing.dll,System.Windows.Forms.dll,System.IO.Compression.dll,System.IO.Compression.FileSystem.dll,System.Web.Extensions.dll /out:PokeWilds-Mod-Menu.exe PokeWildsModMenu.cs
) else (
    "%CSC%" /target:winexe /optimize+ /r:System.dll,System.Drawing.dll,System.Windows.Forms.dll,System.IO.Compression.dll,System.IO.Compression.FileSystem.dll,System.Web.Extensions.dll /out:PokeWilds-Mod-Menu.exe PokeWildsModMenu.cs
)

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [SUCCESS] PokeWilds-Mod-Menu.exe compiled successfully!
    echo You can now run PokeWilds-Mod-Menu.exe directly.
) else (
    echo.
    echo [ERROR] Compilation failed with error code %ERRORLEVEL%.
)

echo.
pause
