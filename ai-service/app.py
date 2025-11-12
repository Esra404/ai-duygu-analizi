from gradio_client import Client
import sys
import json
import io
import os
from pathlib import Path

# Encoding sorunlarını önlemek için stdout'u UTF-8'e ayarla
if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

# .env dosyasını yükle (varsa)
def load_env_file():
    """ai-service klasöründeki .env dosyasını yükler"""
    script_dir = Path(__file__).parent
    env_file = script_dir / '.env'
    
    if env_file.exists():
        try:
            with open(env_file, 'r', encoding='utf-8') as f:
                for line in f:
                    line = line.strip()
                    if line and not line.startswith('#') and '=' in line:
                        key, value = line.split('=', 1)
                        os.environ[key.strip()] = value.strip()
        except Exception as e:
            pass  # .env dosyası okunamazsa sessizce devam et

# .env dosyasını yükle
load_env_file()

# Argüman kontrolü
if len(sys.argv) < 2:
    print(json.dumps({"error": "Lütfen bir prompt girin."}, ensure_ascii=False))
    sys.exit(1)

input_text = sys.argv[1]

SPACE_URL = "https://esra404-neco.hf.space/"
# Token önce environment variable'dan, sonra .env dosyasından alınır
token = os.getenv("HUGGINGFACE_TOKEN", "")

# Eğer gradio_client başlatırken konsola "✓" vb. unicode yazdırıp hata veriyorsa,
# PYTHONUTF8=1 ile çalıştırıldığında encoding problemi çözülür (C# tarafında ayar var).

try:
    # Gradio client'ın stdout'a yazmasını engelle
    import warnings
    warnings.filterwarnings('ignore')
    
    client = Client(SPACE_URL, hf_token=token, verbose=False)
    result = client.predict(
        input_text,
        api_name="/predict"
    )
    # JSON olarak yazdır (Türkçe karakterler için ensure_ascii=False)
    print(json.dumps({"result": result}, ensure_ascii=False))
except Exception as e:
    # Hata durumunda da JSON yaz
    error_msg = str(e).encode('ascii', 'replace').decode('ascii')  # ASCII'ye çevir
    print(json.dumps({"error": error_msg}, ensure_ascii=False))
    sys.exit(1)
