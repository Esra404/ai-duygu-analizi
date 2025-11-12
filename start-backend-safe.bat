@echo off
echo Port 5000'i kontrol ediliyor...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5000 ^| findstr LISTENING') do (
    echo Port 5000 kullaniliyor, process kapatiliyor...
    taskkill /F /PID %%a >nul 2>&1
    timeout /t 1 >nul
)
echo Backend baslatiliyor...
cd backend
dotnet run

