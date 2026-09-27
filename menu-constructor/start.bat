@echo off
cd /d "%~dp0"
title MenuManager constructor
set "URL=http://127.0.0.1:8765/index.html"

netstat -ano | findstr ":8765" | findstr "LISTENING" >nul
if not errorlevel 1 (
  start "" "%URL%"
  exit /b 0
)

where python >nul 2>&1
if errorlevel 1 (
  where py >nul 2>&1
  if errorlevel 1 (
    echo Python was not found. Open index.html directly.
    pause
    exit /b 1
  )
  set "PY=py -3"
) else (
  set "PY=python"
)

echo.
echo  %URL%
echo  Close this window to stop the server.
echo.

start "open-browser" /min cmd /c "timeout /t 2 /nobreak >nul & start %URL%"
%PY% -m http.server 8765
echo.
echo Server stopped.
pause
