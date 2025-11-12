using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// CORS ayarları - Frontend'den istekleri kabul et (localhost + ngrok)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        // Ngrok URL'leri, Render URL'leri ve localhost için esnek CORS
        policy.SetIsOriginAllowed(origin => 
            origin.Contains("ngrok-free.app") || 
            origin.Contains("ngrok.io") ||
            origin.Contains("onrender.com") ||
            origin.StartsWith("http://localhost") ||
            origin.StartsWith("https://localhost")
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

// CORS middleware'i ekle
app.UseCors("AllowReactApp");

// Veritabanı yolu - göreli yol kullan
var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "message.db");

// Veritabanını başlat
InitializeDatabase(dbPath);

// Root path için health check endpoint
app.MapGet("/", () => Results.Ok(new
{
    status = "ok",
    message = "AI Duygu Analizi Chat API",
    version = "1.0.0",
    endpoints = new
    {
        chat = "POST /api/chat",
        history = "GET /api/chat/history?username={username}",
        users = "GET /api/users",
        test = "GET /api/test-ai"
    }
}));

// AI servisi test endpoint'i
app.MapGet("/api/test-ai", async () =>
{
    try
    {
        var testMessage = "test";
        var currentDir = Directory.GetCurrentDirectory();
        var pythonPaths = new[]
        {
            Path.Combine("/app", "ai-service", "app.py"),
            Path.Combine(currentDir, "ai-service", "app.py"),
        };
        
        var pythonFile = pythonPaths.FirstOrDefault(p => File.Exists(p));
        var pythonExe = "/usr/bin/python3";
        var hfToken = Environment.GetEnvironmentVariable("HUGGINGFACE_TOKEN");
        
        // Python script'i manuel test et
        string? pythonTestResult = null;
        string? pythonTestError = null;
        int pythonExitCode = -1;
        
        if (pythonFile != null && File.Exists(pythonFile) && File.Exists(pythonExe))
        {
            try
            {
                var start = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = $"\"{pythonFile}\" \"test message\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    CreateNoWindow = true
                };
                start.Environment["PYTHONUTF8"] = "1";
                if (!string.IsNullOrEmpty(hfToken))
                {
                    start.Environment["HUGGINGFACE_TOKEN"] = hfToken;
                }
                
                using var process = Process.Start(start);
                if (process != null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    
                    pythonTestResult = output.Trim();
                    pythonTestError = error.Trim();
                    pythonExitCode = process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                pythonTestError = ex.Message;
            }
        }
        
        return Results.Ok(new
        {
            currentDirectory = currentDir,
            pythonFileExists = pythonFile != null && File.Exists(pythonFile),
            pythonFile = pythonFile,
            pythonExe = pythonExe,
            pythonExeExists = File.Exists(pythonExe),
            hfTokenExists = !string.IsNullOrEmpty(hfToken),
            hfTokenLength = hfToken?.Length ?? 0,
            aiServicePath = "/app/ai-service",
            aiServiceExists = Directory.Exists("/app/ai-service"),
            pythonTest = new
            {
                exitCode = pythonExitCode,
                output = pythonTestResult,
                error = pythonTestError,
                outputLength = pythonTestResult?.Length ?? 0,
                errorLength = pythonTestError?.Length ?? 0
            }
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { error = ex.Message, stackTrace = ex.StackTrace });
    }
});

// API Endpoints
app.MapPost("/api/chat", async (HttpContext context) =>
{
    try
    {
        var request = await context.Request.ReadFromJsonAsync<ChatRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.Message) || string.IsNullOrWhiteSpace(request.Username))
        {
            return Results.BadRequest(new { error = "Kullanıcı adı ve mesaj gereklidir." });
        }

        // Python AI servisini çağır
        Console.WriteLine($"AI servisi çağrılıyor: {request.Message}");
        var aiResponse = await CallAIService(request.Message);
        
        if (aiResponse != null && aiResponse.Length > 0)
        {
            Console.WriteLine($"AI servisi yanıtı (ilk 200 karakter): {aiResponse.Substring(0, Math.Min(200, aiResponse.Length))}...");
        }
        else
        {
            Console.WriteLine($"❌ AI servisi boş yanıt döndü!");
        }
        
        if (string.IsNullOrEmpty(aiResponse) || 
            aiResponse.StartsWith("AI servisi hatası") || 
            aiResponse.StartsWith("Hata:") || 
            aiResponse.Contains("bulunamadı") ||
            aiResponse.Contains("Python") ||
            aiResponse.Contains("timeout") ||
            aiResponse.Contains("zaman aşımı"))
        {
            Console.WriteLine($"❌ AI servisi hatası: {aiResponse}");
            // Detaylı hata mesajı döndür
            return Results.BadRequest(new { 
                error = $"AI servisi hatası: {aiResponse}",
                details = "Python servisi çalışmıyor olabilir. Render loglarını kontrol edin."
            });
        }

        // AI cevabını Türkçe'ye çevir
        aiResponse = TranslateAISponseToTurkish(aiResponse);

        // Veritabanına kaydet
        var messageId = Guid.NewGuid().ToString();
        var timestamp = DateTime.UtcNow;
        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO Messages (id, username, mesaj, cevap, timestamp)
                VALUES ($id, $username, $mesaj, $cevap, $timestamp);";
            insertCmd.Parameters.AddWithValue("$id", messageId);
            insertCmd.Parameters.AddWithValue("$username", request.Username);
            insertCmd.Parameters.AddWithValue("$mesaj", request.Message);
            insertCmd.Parameters.AddWithValue("$cevap", aiResponse);
            insertCmd.Parameters.AddWithValue("$timestamp", timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
            insertCmd.ExecuteNonQuery();
        }

        return Results.Ok(new ChatResponse
        {
            Id = messageId,
            Username = request.Username,
            Message = request.Message,
            Response = aiResponse,
            Timestamp = timestamp
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ HATA DETAYLARI:");
        Console.WriteLine($"Mesaj: {ex.Message}");
        Console.WriteLine($"Tip: {ex.GetType().Name}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }
        return Results.StatusCode(500);
    }
});

app.MapGet("/api/chat/history", (HttpContext context) =>
{
    try
    {
        var username = context.Request.Query["username"].ToString();
        var messages = new List<ChatMessage>();

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
            var selectCmd = connection.CreateCommand();
            
            if (!string.IsNullOrWhiteSpace(username))
            {
                selectCmd.CommandText = @"
                    SELECT id, username, mesaj, cevap, timestamp 
                    FROM Messages 
                    WHERE username = $username 
                    ORDER BY timestamp DESC 
                    LIMIT 50;";
                selectCmd.Parameters.AddWithValue("$username", username);
            }
            else
            {
                selectCmd.CommandText = @"
                    SELECT id, username, mesaj, cevap, timestamp 
                    FROM Messages 
                    ORDER BY timestamp DESC 
                    LIMIT 100;";
            }

            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        messages.Add(new ChatMessage
                        {
                            Id = reader.GetString(0),
                            Username = reader.FieldCount > 1 ? reader.GetString(1) : "Unknown",
                            Message = reader.FieldCount > 2 ? reader.GetString(2) : reader.GetString(1),
                            Response = reader.FieldCount > 3 && !reader.IsDBNull(3) ? reader.GetString(3) : "",
                            Timestamp = reader.FieldCount > 4 ? DateTime.Parse(reader.GetString(4)) : DateTime.UtcNow
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Mesaj okuma hatası: {ex.Message}");
                    }
                }
            }
        }

        return Results.Ok(messages.OrderBy(m => m.Timestamp).ToList());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Hata: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        return Results.StatusCode(500);
    }
});

app.MapGet("/api/users", () =>
{
    try
    {
        var users = new HashSet<string>();
        var messageCount = 0;

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
            
            // Kullanıcıları getir
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT DISTINCT username FROM Messages;";

            using (var reader = selectCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(reader.GetString(0));
                }
            }
            
            // Toplam mesaj sayısını getir
            var countCmd = connection.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(*) FROM Messages;";
            var countResult = countCmd.ExecuteScalar();
            messageCount = countResult != null ? Convert.ToInt32(countResult) : 0;
        }

        return Results.Ok(new
        {
            users = users.ToList(),
            totalMessages = messageCount,
            databasePath = dbPath,
            databaseExists = File.Exists(dbPath)
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Hata: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        return Results.StatusCode(500);
    }
});

// Port ayarı - ASPNETCORE_URLS env var varsa onu kullan, yoksa PORT env var'ından al
// Docker/Render için ASPNETCORE_URLS genelde ayarlanır
var aspnetcoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (string.IsNullOrEmpty(aspnetcoreUrls))
{
    // ASPNETCORE_URLS yoksa manuel port ayarla
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    var url = $"http://0.0.0.0:{port}";
    app.Urls.Add(url);
    Console.WriteLine($"Backend başlatılıyor: {url}");
}
else
{
    Console.WriteLine($"Backend başlatılıyor: {aspnetcoreUrls}");
}

app.Run();

// Helper Methods
void InitializeDatabase(string dbPath)
{
    using (var connection = new SqliteConnection($"Data Source={dbPath}"))
    {
        connection.Open();
        
        // Önce tabloyu oluştur
        var createTableCmd = connection.CreateCommand();
        createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Messages (
                id TEXT PRIMARY KEY,
                username TEXT NOT NULL,
                mesaj TEXT NOT NULL,
                cevap TEXT NOT NULL,
                timestamp TEXT NOT NULL
            );";
        createTableCmd.ExecuteNonQuery();
        
        // Eski tabloda username kolonu var mı kontrol et
        try
        {
            var checkColumnCmd = connection.CreateCommand();
            checkColumnCmd.CommandText = "SELECT username FROM Messages LIMIT 1;";
            checkColumnCmd.ExecuteNonQuery();
        }
        catch
        {
            // username kolonu yoksa, tabloyu yeniden oluştur
            Console.WriteLine("Eski veritabanı şeması tespit edildi. Tablo güncelleniyor...");
            
            // Eski verileri yedekle (opsiyonel)
            var backupCmd = connection.CreateCommand();
            backupCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Messages_backup AS 
                SELECT * FROM Messages;
            ";
            try { backupCmd.ExecuteNonQuery(); } catch { }
            
            // Eski tabloyu sil
            var dropCmd = connection.CreateCommand();
            dropCmd.CommandText = "DROP TABLE IF EXISTS Messages;";
            dropCmd.ExecuteNonQuery();
            
            // Yeni tabloyu oluştur
            var newTableCmd = connection.CreateCommand();
            newTableCmd.CommandText = @"
                CREATE TABLE Messages (
                    id TEXT PRIMARY KEY,
                    username TEXT NOT NULL,
                    mesaj TEXT NOT NULL,
                    cevap TEXT NOT NULL,
                    timestamp TEXT NOT NULL
                );";
            newTableCmd.ExecuteNonQuery();
            
            Console.WriteLine("Veritabanı şeması güncellendi.");
        }
    }
}

async Task<string> CallAIService(string message)
{
    // Python script dosyasının yolu - proje içindeki ai-service/app.py
    // Backend klasöründen çalıştığında bir üst dizine çık
    var currentDir = Directory.GetCurrentDirectory();
    Console.WriteLine($"[AI SERVICE] Mevcut dizin: {currentDir}");
    Console.WriteLine($"[AI SERVICE] AI servisi çağrılıyor, mesaj: {message.Substring(0, Math.Min(50, message.Length))}");
    
    // Farklı yol kombinasyonlarını dene (Windows ve Linux uyumlu)
    var possiblePaths = new[]
    {
        // Docker/Linux yolu
        Path.Combine("/app", "ai-service", "app.py"),
        // Göreli yollar
        Path.Combine(currentDir, "ai-service", "app.py"),
        Path.Combine(currentDir, "..", "ai-service", "app.py"),
        Path.Combine(currentDir, "..", "..", "ai-service", "app.py"),
        Path.GetFullPath(Path.Combine(currentDir, "ai-service", "app.py")),
        Path.GetFullPath(Path.Combine(currentDir, "..", "ai-service", "app.py")),
    };
    
    string? pythonFile = null;
    foreach (var path in possiblePaths)
    {
        var fullPath = Path.GetFullPath(path);
        if (File.Exists(fullPath))
        {
            pythonFile = fullPath;
            break;
        }
        Console.WriteLine($"Denenen yol (bulunamadı): {fullPath}");
    }
    
    if (pythonFile == null || !File.Exists(pythonFile))
    {
        var errorMsg = $"Python script bulunamadı. Denenen yollar:\n{string.Join("\n", possiblePaths.Select(p => Path.GetFullPath(p)))}";
        Console.WriteLine($"[AI SERVICE] ❌ {errorMsg}");
        Console.WriteLine($"[AI SERVICE] Tüm yollar kontrol edildi:");
        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            var exists = File.Exists(fullPath);
            Console.WriteLine($"[AI SERVICE]   - {fullPath} => {(exists ? "VAR" : "YOK")}");
        }
        return $"Python script dosyası bulunamadı. Mevcut dizin: {currentDir}";
    }
    
    Console.WriteLine($"[AI SERVICE] ✅ Python script bulundu: {pythonFile}");
    
    // Python interpreter yolu - Environment variable'dan veya otomatik bul
    var pythonExe = Environment.GetEnvironmentVariable("PYTHON_EXE");
    
    if (string.IsNullOrEmpty(pythonExe) || !File.Exists(pythonExe))
    {
        // Alternatif Python yollarını dene (Windows ve Linux uyumlu)
        var possiblePythonPaths = new[]
        {
            // Environment variable
            Environment.GetEnvironmentVariable("PYTHON_EXE"),
            // Linux/Docker yolları
            "/usr/bin/python3",
            "/usr/local/bin/python3",
            "python3",
            // Windows yolları
            @"C:\Users\codee\AppData\Local\Programs\Python\Python313\python.exe",
            @"C:\Users\codee\AppData\Local\Programs\Python\Python312\python.exe",
            @"C:\Python313\python.exe",
            @"C:\Python312\python.exe",
            @"python.exe",
            "python"
        }.Where(p => !string.IsNullOrEmpty(p)).ToArray();

        foreach (var path in possiblePythonPaths)
        {
            try
            {
                var testProcess = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(testProcess))
                {
                    if (p != null)
                    {
                        p.WaitForExit(2000); // 2 saniye bekle
                        if (p.ExitCode == 0 || p.HasExited)
                        {
                            pythonExe = path;
                            Console.WriteLine($"✅ Python bulundu: {path}");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Python test edilirken hata ({path}): {ex.Message}");
            }
        }
    }
    
    if (string.IsNullOrEmpty(pythonExe))
    {
        Console.WriteLine("❌ Python interpreter bulunamadı!");
        return "Python interpreter bulunamadı. Python'un yüklü olduğundan emin olun.";
    }
    
    Console.WriteLine($"Python interpreter: {pythonExe}");

    var escapedMessage = message.Replace("\"", "\\\"");

    var start = new ProcessStartInfo
    {
        FileName = pythonExe,
        Arguments = $"\"{pythonFile}\" \"{escapedMessage}\"",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        StandardOutputEncoding = Encoding.UTF8,
        StandardErrorEncoding = Encoding.UTF8,
        CreateNoWindow = true
    };
    start.Environment["PYTHONUTF8"] = "1";
    
    // Hugging Face token'ı environment variable'dan al
    var hfToken = Environment.GetEnvironmentVariable("HUGGINGFACE_TOKEN");
    if (!string.IsNullOrEmpty(hfToken))
    {
        start.Environment["HUGGINGFACE_TOKEN"] = hfToken;
    }

    try
    {
        Console.WriteLine($"Python process başlatılıyor: {pythonExe} \"{pythonFile}\" \"{message.Substring(0, Math.Min(50, message.Length))}...\"");
        
        using var process = Process.Start(start);
        if (process == null)
        {
            Console.WriteLine("❌ Process başlatılamadı!");
            return "AI servisi başlatılamadı.";
        }

        // Timeout ekle - 30 saniye
        var timeoutTask = Task.Delay(30000);
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        var exitTask = process.WaitForExitAsync();

        var completedTask = await Task.WhenAny(exitTask, timeoutTask);
        
        if (completedTask == timeoutTask)
        {
            Console.WriteLine("❌ Python process timeout (30 saniye)!");
            try { process.Kill(); } catch { }
            return "AI servisi zaman aşımına uğradı (30 saniye).";
        }

        var output = await outputTask;
        var error = await errorTask;
        await exitTask;

        // Python çıktılarını detaylı logla
        Console.WriteLine($"[AI SERVICE] Python Exit Code: {process.ExitCode}");
        Console.WriteLine($"[AI SERVICE] Python Output Length: {output?.Length ?? 0}");
        Console.WriteLine($"[AI SERVICE] Python Error Length: {error?.Length ?? 0}");
        
        // Python hatalarını logla
        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"[AI SERVICE] ⚠️ Python Error Output (tam): {error}");
            // Gradio client uyarılarını filtrele ama diğer hataları göster
            if (!error.Contains("gradio_client") && !error.Contains("WARNING") && !error.Contains("UserWarning"))
            {
                Console.WriteLine($"[AI SERVICE] ❌ Python Error: {error}");
            }
        }
        else
        {
            Console.WriteLine($"[AI SERVICE] Python Error Output: (boş)");
        }

        var response = output?.Trim() ?? "";
        
        if (!string.IsNullOrEmpty(response))
        {
            Console.WriteLine($"[AI SERVICE] ✅ Python çıktısı (ilk 500 karakter): {response.Substring(0, Math.Min(500, response.Length))}");
        }
        else
        {
            Console.WriteLine($"[AI SERVICE] ❌ Python'dan boş yanıt alındı!");
            Console.WriteLine($"[AI SERVICE] Python Exit Code: {process.ExitCode}");
            if (!string.IsNullOrEmpty(error))
            {
                // Error varsa onu döndür
                var errorMsg = error.Length > 500 ? error.Substring(0, 500) : error;
                Console.WriteLine($"[AI SERVICE] Python Error (döndürülüyor): {errorMsg}");
                return $"AI servisi hatası: {errorMsg}";
            }
            else if (process.ExitCode != 0)
            {
                return $"AI servisi hatası: Python process hata kodu {process.ExitCode} ile çıktı. Çıktı yok.";
            }
            return "AI servisinden yanıt alınamadı. Python process boş yanıt döndü.";
        }
        
        // JSON response'u parse et
        try
        {
            var jsonDoc = JsonDocument.Parse(response);
            if (jsonDoc.RootElement.TryGetProperty("result", out var resultElement))
            {
                var result = resultElement.GetString() ?? response;
                Console.WriteLine($"✅ AI cevabı alındı: {result.Substring(0, Math.Min(100, result.Length))}...");
                return result;
            }
            if (jsonDoc.RootElement.TryGetProperty("error", out var errorElement))
            {
                var errorMsg = errorElement.GetString() ?? "Bilinmeyen hata";
                Console.WriteLine($"❌ AI servisi hatası: {errorMsg}");
                return $"Hata: {errorMsg}";
            }
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"JSON parse hatası: {jsonEx.Message}");
            Console.WriteLine($"Ham yanıt: {response}");
            // JSON değilse direkt döndür
        }

        return response;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Python process exception: {ex.GetType().Name}");
        Console.WriteLine($"Mesaj: {ex.Message}");
        Console.WriteLine($"Stack: {ex.StackTrace}");
        return $"AI servisi hatası: {ex.Message}";
    }
}

string TranslateAISponseToTurkish(string response)
{
    if (string.IsNullOrEmpty(response))
        return response;

    // Sentiment çevirileri (tüm varyasyonlar)
    response = response.Replace("Sentiment: Positive", "Duygu: Pozitif");
    response = response.Replace("Sentiment: Negative", "Duygu: Negatif");
    response = response.Replace("Sentiment: Neutral", "Duygu: Nötr");
    response = response.Replace("Sentiment: positive", "Duygu: Pozitif");
    response = response.Replace("Sentiment: negative", "Duygu: Negatif");
    response = response.Replace("Sentiment: neutral", "Duygu: Nötr");
    response = response.Replace("Sentiment:", "Duygu:");
    response = response.Replace("sentiment:", "Duygu:");
    response = response.Replace("Sentiment", "Duygu");
    response = response.Replace("sentiment", "Duygu");
    
    // Confidence çevirisi (tüm varyasyonlar)
    response = response.Replace("Confidence:", "Güven:");
    response = response.Replace("confidence:", "Güven:");
    response = response.Replace("Confidence", "Güven");
    response = response.Replace("confidence", "Güven");
    
    // Positive, Negative, Neutral kelimelerini çevir
    response = response.Replace("Positive", "Pozitif");
    response = response.Replace("positive", "Pozitif");
    response = response.Replace("Negative", "Negatif");
    response = response.Replace("negative", "Negatif");
    response = response.Replace("Neutral", "Nötr");
    response = response.Replace("neutral", "Nötr");
    
    // Derece belirten kelimeler
    response = response.Replace("Very ", "Çok ");
    response = response.Replace("very ", "çok ");
    response = response.Replace("Extremely ", "Aşırı ");
    response = response.Replace("extremely ", "aşırı ");
    response = response.Replace("Slightly ", "Biraz ");
    response = response.Replace("slightly ", "biraz ");
    
    return response;
}

// Data Models
public class ChatRequest
{
    public string Username { get; set; } = "";
    public string Message { get; set; } = "";
}

public class ChatResponse
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string Message { get; set; } = "";
    public string Response { get; set; } = "";
    public DateTime Timestamp { get; set; }
}

public class ChatMessage
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string Message { get; set; } = "";
    public string Response { get; set; } = "";
    public DateTime Timestamp { get; set; }
}
