# Render'de Deploy Rehberi

## 🚀 Render'de Docker ile Deploy

### 1. GitHub'a Push Yapın

Önce projenizi GitHub'a push edin:
```bash
git add .
git commit -m "Add Dockerfile for Render deployment"
git push
```

### 2. Render'de Yeni Web Service Oluşturun

1. [Render Dashboard](https://dashboard.render.com) → "New +" → "Web Service"
2. GitHub repository'nizi bağlayın
3. Ayarları yapın:

**Temel Ayarlar:**
- **Name:** `ai-duygu-analizi-backend` (veya istediğiniz isim)
- **Environment:** `Docker`
- **Region:** Size en yakın bölgeyi seçin
- **Branch:** `main` (veya hangi branch'i kullanıyorsanız)
- **Root Directory:** (boş bırakın, root'tan build edilecek)

**Dockerfile Ayarları:**
- **Dockerfile Path:** `Dockerfile` (root'ta olduğu için)
- **Docker Context:** `.` (root directory)

### 3. Environment Variables Ekleyin

Render Dashboard'da "Environment" sekmesine gidin ve şu değişkenleri ekleyin:

| Key | Value | Açıklama |
|-----|-------|----------|
| `HUGGINGFACE_TOKEN` | `your_token_here` | Hugging Face API token'ınız (zorunlu) |
| `PORT` | (otomatik) | Render otomatik sağlar, eklemenize gerek yok |

**HUGGINGFACE_TOKEN Nasıl Alınır:**
1. https://huggingface.co/settings/tokens adresine gidin
2. "New token" butonuna tıklayın
3. Token'ı kopyalayın ve Render'a ekleyin

### 4. Build & Deploy

Render otomatik olarak:
1. Dockerfile'ı bulacak
2. Docker image'ı build edecek
3. Container'ı başlatacak

**Build Command:** (otomatik, Dockerfile'dan alınır)
**Start Command:** (otomatik, Dockerfile'dan alınır)

### 5. Deploy Sonrası

Deploy tamamlandıktan sonra:
- Render size bir URL verecek (örnek: `https://ai-duygu-analizi-backend.onrender.com`)
- Bu URL backend API'nizin adresidir
- Frontend'de bu URL'yi kullanın

---

## 📝 Frontend'i Yapılandırma

Frontend'i de Render'de deploy edebilirsiniz veya localhost'ta çalıştırabilirsiniz.

### Frontend'i Render'de Deploy (Opsiyonel)

1. Yeni bir **Static Site** oluşturun
2. Frontend klasörünü seçin
3. Build Command: `cd frontend && npm install && npm run build`
4. Publish Directory: `frontend/dist`

**Frontend Environment Variable:**
- `VITE_API_URL`: Backend URL'nizi ekleyin (örnek: `https://ai-duygu-analizi-backend.onrender.com`)

### Localhost'ta Frontend Çalıştırma

`frontend` klasöründe `.env` dosyası oluşturun:
```env
VITE_API_URL=https://ai-duygu-analizi-backend.onrender.com
```

Sonra:
```bash
cd frontend
npm run dev
```

---

## 🔧 Sorun Giderme

### Build Hatası: "Dockerfile not found"
- Dockerfile'ın proje root'unda olduğundan emin olun
- Root Directory ayarını kontrol edin

### Runtime Hatası: "Python not found"
- Dockerfile'da Python kurulumu var, build loglarını kontrol edin
- AI servisi için Python gerekli

### CORS Hatası
- Backend CORS ayarları zaten Render URL'lerini destekliyor
- Eğer hala sorun varsa, frontend URL'sini backend CORS ayarlarına ekleyin

### Port Hatası
- Render otomatik PORT environment variable sağlar
- Dockerfile'da `ASPNETCORE_URLS=http://+:${PORT:-8080}` ayarı var
- Program.cs'de PORT kontrolü var

### SQLite Veritabanı
- SQLite dosyası container içinde geçici olabilir
- Kalıcı depolama için Render Disk kullanabilirsiniz (ücretli plan)
- Veya PostgreSQL kullanın (Render'de ücretsiz)

---

## 💡 Önemli Notlar

1. **Free Plan Limitleri:**
   - 15 dakika inaktiflikten sonra uyku moduna geçer
   - İlk istekte yavaş başlatma olabilir (cold start)
   - Aylık 750 saat ücretsiz

2. **Python Bağımlılıkları:**
   - AI servisi Python gerektiriyor
   - Dockerfile'da Python 3 ve pip otomatik yüklenir
   - `gradio_client` paketi otomatik yüklenir

3. **Environment Variables:**
   - `HUGGINGFACE_TOKEN` zorunlu
   - `PORT` otomatik (Render sağlar)

4. **Build Süresi:**
   - İlk build 5-10 dakika sürebilir
   - Sonraki build'ler daha hızlı (cache sayesinde)

---

## 🎯 Hızlı Başlangıç Checklist

- [ ] GitHub'a push yaptım
- [ ] Render'de yeni Web Service oluşturdum
- [ ] Dockerfile root'ta
- [ ] Environment Variable: `HUGGINGFACE_TOKEN` ekledim
- [ ] Build başarılı
- [ ] Backend URL'yi aldım
- [ ] Frontend'i yapılandırdım

---

## 📚 Ek Kaynaklar

- [Render Docker Docs](https://render.com/docs/docker)
- [Render Environment Variables](https://render.com/docs/environment-variables)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)

