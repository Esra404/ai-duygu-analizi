@echo off
echo ========================================
echo Backend + Ngrok Baslatiliyor...
echo ========================================
echo.

REM Backend'i arka planda başlat
echo [1/2] Backend baslatiliyor...
start "Backend Server" cmd /k "cd backend && dotnet run"

REM Backend'in başlaması için 5 saniye bekle
echo Backend'in baslamasi icin 5 saniye bekleniyor...
timeout /t 5 /nobreak >nul

REM Ngrok'un yüklü olup olmadığını kontrol et
where ngrok >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [2/2] Ngrok tunnel aciliyor...
    echo.
    echo ========================================
    echo Ngrok URL'niz asagida gorunecektir!
    echo Bu URL'yi frontend'de kullanin.
    echo ========================================
    echo.
    ngrok http 5000
    goto :end
)

REM Proje klasöründe ngrok.exe var mı kontrol et
if exist "ngrok.exe" (
    echo [2/2] Ngrok tunnel aciliyor (yerel)...
    echo.
    echo ========================================
    echo Ngrok URL'niz asagida gorunecektir!
    echo Bu URL'yi frontend'de kullanin.
    echo ========================================
    echo.
    ngrok.exe http 5000
    goto :end
)

echo.
echo [HATA] Ngrok bulunamadi!
echo.
echo Ngrok'u kurmak icin install-ngrok.ps1 scriptini calistirin.
echo.
pause

:end

pause

