# Build stage - .NET SDK ile derleme
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasını kopyala ve restore et
COPY backend/backend.csproj backend/
RUN dotnet restore backend/backend.csproj

# Tüm kaynak kodları kopyala ve build et
COPY backend/ backend/
WORKDIR /src/backend
RUN dotnet publish -c Release -o /app/publish

# Runtime stage - .NET Runtime + Python
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Python 3 ve pip yükle
RUN apt-get update && \
    apt-get install -y python3 python3-pip && \
    rm -rf /var/lib/apt/lists/*

# Python paketlerini yükle (ai-service için)
# --break-system-packages flag'i Python 3.11+ için gerekli (externally-managed-environment hatası için)
COPY backend/ai-service/requirements.txt /tmp/requirements.txt
RUN if [ -s /tmp/requirements.txt ]; then \
        pip3 install --no-cache-dir --break-system-packages -r /tmp/requirements.txt; \
    else \
        pip3 install --no-cache-dir --break-system-packages gradio_client; \
    fi

# AI servis dosyalarını kopyala
COPY backend/ai-service /app/ai-service

# .NET uygulamasını kopyala
WORKDIR /app
COPY --from=build /app/publish .

# Render PORT environment variable'ını kullan
# Render otomatik PORT env var sağlar
# Program.cs PORT env var'ını kontrol eder
ENV PORT=8080
EXPOSE 8080

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "backend.dll"]

