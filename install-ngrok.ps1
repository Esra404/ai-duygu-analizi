# Ngrok Kurulum Scripti
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Ngrok Kurulum Scripti" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Ngrok'un zaten yüklü olup olmadığını kontrol et
$ngrokPath = Get-Command ngrok -ErrorAction SilentlyContinue
if ($ngrokPath) {
    Write-Host "✓ Ngrok zaten yüklü: $($ngrokPath.Source)" -ForegroundColor Green
    Write-Host ""
    Write-Host "Ngrok'u çalıştırmak için:" -ForegroundColor Yellow
    Write-Host "  ngrok http 5000" -ForegroundColor White
    exit 0
}

Write-Host "Ngrok bulunamadı. Kurulum başlatılıyor..." -ForegroundColor Yellow
Write-Host ""

# Seçenek 1: Chocolatey ile kurulum (önerilen)
$chocoInstalled = Get-Command choco -ErrorAction SilentlyContinue
if ($chocoInstalled) {
    Write-Host "[1] Chocolatey ile kurulum (önerilen)" -ForegroundColor Green
    Write-Host "    Komut: choco install ngrok -y" -ForegroundColor Gray
    Write-Host ""
}

# Seçenek 2: Manuel indirme
Write-Host "[2] Manuel indirme" -ForegroundColor Green
Write-Host "    1. https://ngrok.com/download adresine gidin" -ForegroundColor Gray
Write-Host "    2. Windows için ngrok.exe'yi indirin" -ForegroundColor Gray
Write-Host "    3. İndirdiğiniz ngrok.exe'yi bu klasöre kopyalayın" -ForegroundColor Gray
Write-Host "    4. Veya PATH'e ekleyin" -ForegroundColor Gray
Write-Host ""

# Seçenek 3: Proje klasörüne indirme (otomatik)
Write-Host "[3] Otomatik indirme (proje klasörüne)" -ForegroundColor Green
Write-Host "    Ngrok'u proje klasörüne indirip oradan çalıştıracağız" -ForegroundColor Gray
Write-Host ""

$choice = Read-Host "Hangi yöntemi kullanmak istersiniz? (1/2/3)"

if ($choice -eq "1" -and $chocoInstalled) {
    Write-Host ""
    Write-Host "Chocolatey ile kurulum başlatılıyor..." -ForegroundColor Yellow
    choco install ngrok -y
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✓ Ngrok başarıyla kuruldu!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Şimdi ngrok'u çalıştırabilirsiniz:" -ForegroundColor Yellow
        Write-Host "  ngrok http 5000" -ForegroundColor White
    }
}
elseif ($choice -eq "3") {
    Write-Host ""
    Write-Host "Ngrok indiriliyor..." -ForegroundColor Yellow
    
    # Ngrok download URL (Windows 64-bit)
    $downloadUrl = "https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-windows-amd64.zip"
    $zipPath = "$PSScriptRoot\ngrok.zip"
    $extractPath = "$PSScriptRoot"
    
    try {
        # Ngrok'u indir
        Write-Host "İndirme başlatılıyor..." -ForegroundColor Yellow
        Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath -UseBasicParsing
        
        # Zip'i aç
        Write-Host "Açılıyor..." -ForegroundColor Yellow
        Expand-Archive -Path $zipPath -DestinationPath $extractPath -Force
        
        # Zip dosyasını sil
        Remove-Item $zipPath -Force
        
        Write-Host ""
        Write-Host "✓ Ngrok başarıyla indirildi!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Ngrok'u çalıştırmak için:" -ForegroundColor Yellow
        Write-Host "  .\ngrok.exe http 5000" -ForegroundColor White
        Write-Host ""
        Write-Host "Veya start-ngrok.bat dosyasını kullanın!" -ForegroundColor Yellow
    }
    catch {
        Write-Host ""
        Write-Host "✗ Hata: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        Write-Host "Manuel indirme yapın:" -ForegroundColor Yellow
        Write-Host "  https://ngrok.com/download" -ForegroundColor White
    }
}
else {
    Write-Host ""
    Write-Host "Manuel kurulum için:" -ForegroundColor Yellow
    Write-Host "  1. https://ngrok.com/download adresine gidin" -ForegroundColor White
    Write-Host "  2. Windows için ngrok.exe'yi indirin" -ForegroundColor White
    Write-Host "  3. İndirdiğiniz ngrok.exe'yi bu klasöre kopyalayın" -ForegroundColor White
    Write-Host "  4. Sonra .\ngrok.exe http 5000 komutunu kullanın" -ForegroundColor White
}

Write-Host ""
Write-Host "Ngrok token'ınızı ayarlamayı unutmayın:" -ForegroundColor Cyan
Write-Host "  ngrok config add-authtoken YOUR_TOKEN" -ForegroundColor White
Write-Host "  (Token'ı https://dashboard.ngrok.com/get-started/your-authtoken adresinden alın)" -ForegroundColor Gray

