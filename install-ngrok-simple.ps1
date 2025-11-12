# Basit Ngrok Kurulum Scripti
Write-Host "Ngrok Kurulum Baslatiliyor..." -ForegroundColor Cyan
Write-Host ""

# Ngrok download URL (Windows 64-bit)
$downloadUrl = "https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-windows-amd64.zip"
$zipPath = Join-Path $PSScriptRoot "ngrok.zip"
$extractPath = $PSScriptRoot

try {
    Write-Host "Ngrok indiriliyor..." -ForegroundColor Yellow
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath -UseBasicParsing
    
    Write-Host "Aciliyor..." -ForegroundColor Yellow
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipPath, $extractPath)
    
    Remove-Item $zipPath -Force
    
    Write-Host ""
    Write-Host "Ngrok basariyla indirildi!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Ngrok'u calistirmak icin:" -ForegroundColor Yellow
    Write-Host "  .\ngrok.exe http 5000" -ForegroundColor White
    Write-Host ""
    Write-Host "Veya start-ngrok.bat dosyasini kullanin!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "NOT: Ngrok token'inizi ayarlamayi unutmayin:" -ForegroundColor Cyan
    Write-Host "  .\ngrok.exe config add-authtoken YOUR_TOKEN" -ForegroundColor White
}
catch {
    Write-Host ""
    Write-Host "Hata: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Manuel indirme yapin:" -ForegroundColor Yellow
    Write-Host "  https://ngrok.com/download" -ForegroundColor White
}

Write-Host ""
pause
