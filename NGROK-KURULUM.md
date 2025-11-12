# Ngrok Kurulum Rehberi

## 🚀 Hızlı Kurulum (3 Yöntem)

### Yöntem 1: Manuel İndirme (Önerilen)

1. **Ngrok'u İndirin:**
   - https://ngrok.com/download adresine gidin
   - Windows için `ngrok.exe` dosyasını indirin

2. **Ngrok'u Proje Klasörüne Kopyalayın:**
   - İndirdiğiniz `ngrok.exe` dosyasını proje klasörüne (`AI-Duygu-Analizi-Chat-App`) kopyalayın

3. **Ngrok Token'ınızı Ayarlayın:**
   ```powershell
   .\ngrok.exe config add-authtoken YOUR_TOKEN
   ```
   - Token'ı almak için: https://dashboard.ngrok.com/get-started/your-authtoken

4. **Ngrok'u Çalıştırın:**
   ```powershell
   .\ngrok.exe http 5000
   ```
   Veya `start-ngrok.bat` dosyasını kullanın!

---

### Yöntem 2: Chocolatey ile (Eğer Chocolatey yüklüyse)

```powershell
choco install ngrok -y
```

Sonra:
```powershell
ngrok config add-authtoken YOUR_TOKEN
ngrok http 5000
```

---

### Yöntem 3: Scoop ile

```powershell
scoop install ngrok
```

---

## ⚠️ Windows Defender Uyarısı

Eğer Windows Defender ngrok'u engelliyorsa:

1. **Windows Defender'ı Geçici Olarak Kapatın:**
   - Ayarlar > Güvenlik > Windows Defender
   - Veya ngrok.exe'yi "İzin Ver" listesine ekleyin

2. **Veya Güvenlik Uyarısını Geçin:**
   - İndirme sonrası "Daha fazla bilgi" > "Yine de çalıştır" seçeneğini kullanın

---

## 📝 Adım Adım Kullanım

### 1. Backend'i Başlatın
```bash
cd backend
dotnet run
```
Backend `http://localhost:5000` adresinde çalışacak.

### 2. Ngrok'u Başlatın
```bash
.\ngrok.exe http 5000
```

### 3. Ngrok URL'sini Alın
Terminal'de şöyle bir çıktı göreceksiniz:
```
Forwarding  https://xxxx-xx-xx-xx-xx.ngrok-free.app -> http://localhost:5000
```

Bu URL'yi kopyalayın (örnek: `https://abc123.ngrok-free.app`)

### 4. Frontend'i Yapılandırın

`frontend` klasöründe `.env` dosyası oluşturun:
```env
VITE_API_URL=https://abc123.ngrok-free.app
```

### 5. Frontend'i Başlatın
```bash
cd frontend
npm run dev
```

---

## 🔧 Sorun Giderme

### "ngrok is not recognized" Hatası
- Ngrok'u PATH'e eklemediniz veya proje klasöründe değil
- Çözüm: `.\ngrok.exe` şeklinde çalıştırın (proje klasöründen)

### "authtoken required" Hatası
- Token'ınızı ayarlamadınız
- Çözüm: `.\ngrok.exe config add-authtoken YOUR_TOKEN`

### CORS Hatası
- Backend CORS ayarları zaten ngrok URL'lerini destekliyor
- Eğer hala sorun varsa, backend'i yeniden başlatın

---

## 📱 Test Etme

Ngrok URL'si ile:
- ✅ Backend API'ye her yerden erişebilirsiniz
- ✅ Mobil cihazlardan test edebilirsiniz
- ✅ Başkalarıyla paylaşabilirsiniz

**Not:** Ücretsiz planda her başlatmada farklı URL alırsınız.

