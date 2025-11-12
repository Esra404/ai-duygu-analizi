# Ngrok ile Projeyi Yayınlama

## 📋 Adımlar

### 1. Ngrok Kurulumu

1. [ngrok.com](https://ngrok.com) adresine gidin
2. Ücretsiz hesap oluşturun
3. Ngrok'u indirin: https://ngrok.com/download
4. İndirdiğiniz `ngrok.exe` dosyasını PATH'e ekleyin veya proje klasörüne koyun

**Alternatif (Chocolatey ile):**
```bash
choco install ngrok
```

### 2. Ngrok Authentication Token

1. [ngrok dashboard](https://dashboard.ngrok.com/get-started/your-authtoken) adresine gidin
2. Authtoken'ınızı kopyalayın
3. Terminal'de çalıştırın:
```bash
ngrok config add-authtoken YOUR_AUTH_TOKEN
```

### 3. Backend'i Başlatın

Backend'i çalıştırın (port 5000):
```bash
cd backend
dotnet run
```

Veya batch dosyası ile:
```bash
start-backend.bat
```

### 4. Ngrok Tunnel'ı Başlatın

**Yöntem 1: Otomatik Script (Önerilen)**
```bash
start-backend-with-ngrok.bat
```
Bu script hem backend'i hem ngrok'u başlatır.

**Yöntem 2: Manuel**
```bash
ngrok http 5000
```

### 5. Ngrok URL'sini Alın

Ngrok başladığında terminal'de şöyle bir çıktı göreceksiniz:
```
Forwarding  https://xxxx-xx-xx-xx-xx.ngrok-free.app -> http://localhost:5000
```

Bu URL'yi kopyalayın (örnek: `https://abc123.ngrok-free.app`)

### 6. Frontend'i Güncelleyin

**Yöntem 1: Environment Variable (Önerilen)**

Frontend klasöründe `.env` dosyası oluşturun:
```env
VITE_API_URL=https://abc123.ngrok-free.app
```

Sonra frontend'i yeniden başlatın:
```bash
cd frontend
npm run dev
```

**Yöntem 2: Manuel Değiştirme**

`frontend/src/App.jsx` dosyasında:
```javascript
const API_BASE_URL = 'https://abc123.ngrok-free.app'
```

### 7. CORS Ayarları

Backend'de CORS ayarları zaten var, ama ngrok URL'sini de eklemeniz gerekebilir:

`backend/Program.cs` dosyasında:
```csharp
policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "https://abc123.ngrok-free.app")
```

## 🔧 Önemli Notlar

1. **Ngrok Free Plan:**
   - Her başlatmada farklı URL alırsınız
   - 8 saat sonra otomatik kapanır
   - Aynı URL'yi korumak için ücretli plan gerekir

2. **Ngrok Warning Sayfası:**
   - İlk ziyarette ngrok bir uyarı sayfası gösterir
   - "Visit Site" butonuna tıklayın

3. **HTTPS:**
   - Ngrok otomatik HTTPS sağlar
   - Frontend'de HTTP yerine HTTPS kullanın

4. **CORS:**
   - Backend CORS ayarlarında ngrok URL'sini eklemeyi unutmayın

## 🚀 Hızlı Başlangıç

```bash
# 1. Backend'i başlat
start-backend.bat

# 2. Yeni terminal'de ngrok'u başlat
ngrok http 5000

# 3. Ngrok URL'sini kopyala ve frontend/.env dosyasına ekle
# VITE_API_URL=https://xxxx.ngrok-free.app

# 4. Frontend'i başlat
cd frontend
npm run dev
```

## 📱 Mobil Cihazlardan Test

Ngrok URL'si ile projenizi mobil cihazlardan da test edebilirsiniz:
- Frontend: `http://localhost:3000` (sadece bilgisayarınızda)
- Backend: `https://xxxx.ngrok-free.app` (her yerden erişilebilir)

Mobil test için frontend'i de deploy etmeniz gerekir (Vercel, Netlify vb.)

