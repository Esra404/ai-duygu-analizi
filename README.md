# AI-Duygu-Analizi-Chat-App

AI ile duygu analizi yapan çok kullanıcılı chat uygulaması.

## 🚀 Özellikler

- **Çok Kullanıcılı Chat**: Birden fazla kullanıcı aynı anda chat yapabilir
- **AI Duygu Analizi**: Her mesaj için AI'dan duygu analizi cevabı alınır
- **Gerçek Zamanlı Güncelleme**: Mesajlar otomatik olarak güncellenir
- **Kullanıcı Yönetimi**: Aktif kullanıcıları görüntüleme
- **Mesaj Geçmişi**: Tüm mesajlar veritabanında saklanır

## 📁 Proje Yapısı

```
AI-Duygu-Analizi-Chat-App/
├── backend/          # ASP.NET Core Web API
├── frontend/         # React + Vite
├── ai-service/       # Python AI servisi (Hugging Face)
└── mobile/          # Mobile uygulama (gelecekte)
```
<img width="1101" height="918" alt="image" src="https://github.com/user-attachments/assets/c10c753e-af34-4af1-b812-56d6f1d3b55f" />
<img width="1185" height="937" alt="image" src="https://github.com/user-attachments/assets/d6952242-3991-428a-ac73-449585eafd74" />
<img width="673" height="859" alt="image" src="https://github.com/user-attachments/assets/dd81d3af-be31-4f16-b1a5-542c475876bf" />
<img width="910" height="753" alt="image" src="https://github.com/user-attachments/assets/bda7d411-cd69-435f-97a5-d3c65dae1fa7" />





## 🎯 Kullanım

1. Backend'i başlatın (`dotnet run` - backend klasöründe)
2. Frontend'i başlatın (`npm run dev` - frontend klasöründe)
3. Tarayıcıda `http://localhost:3000` adresine gidin
4. Bir kullanıcı adı girin ve chat'e katılın
5. Mesaj gönderin ve AI'dan duygu analizi cevabı alın!

## 🔧 Teknolojiler

- **Backend**: ASP.NET Core 9.0, SQLite
- **Frontend**: React 18, Vite
- **AI Service**: Python, Gradio Client, Hugging Face
- **Veritabanı**: SQLite

## 📝 API Endpoints

- `POST /api/chat` - Yeni mesaj gönder
- `GET /api/chat/history?username={username}` - Mesaj geçmişini getir
- `GET /api/users` - Aktif kullanıcıları listele

## 🎨 Özellikler

- Modern ve responsive UI
- Gerçek zamanlı mesaj güncelleme
- Kullanıcı bazlı mesaj filtreleme
- AI entegrasyonu ile duygu analizi

## 🎬 Demo

![AI Duygu Analizi Demo](docs/demo.gif)

