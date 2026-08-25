@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1"
if errorlevel 1 (
  echo.
  echo Build failed. Press any key to close.
  pause >nul
  exit /b 1
)
start "" "%~dp0GAISWatcher.html"
