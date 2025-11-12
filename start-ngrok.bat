@echo off
echo ========================================
echo Ngrok Tunnel Baslatiliyor...
echo ========================================
echo.

REM Ngrok'un yüklü olup olmadığını kontrol et
where ngrok >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Ngrok bulundu!
    goto :start_ngrok
)

REM Proje klasöründe ngrok.exe var mı kontrol et
if exist "ngrok.exe" (
    echo Proje klasöründe ngrok.exe bulundu!
    goto :start_local_ngrok
)

echo.
echo [HATA] Ngrok bulunamadi!
echo.
echo Ngrok'u kurmak icin:
echo   1. install-ngrok.ps1 scriptini calistirin
echo   2. Veya https://ngrok.com/download adresinden indirin
echo   3. Veya: choco install ngrok
echo.
pause
exit /b 1

:start_local_ngrok
echo Ngrok tunnel aciliyor (yerel)...
echo.
echo Backend'in calistigindan emin olun (localhost:5000)
echo.
pause
ngrok.exe http 5000
goto :end

:start_ngrok
echo Ngrok tunnel aciliyor...
echo.
echo Backend'in calistigindan emin olun (localhost:5000)
echo.
pause
ngrok http 5000

:end
pause

