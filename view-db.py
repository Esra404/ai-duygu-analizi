import sqlite3
import os
from datetime import datetime

# Veritabanı yolu
db_path = os.path.join(os.path.dirname(__file__), "backend", "message.db")

print(f"Veritabanı yolu: {db_path}")
print("=" * 80)

if not os.path.exists(db_path):
    print("❌ Veritabanı dosyası bulunamadı!")
    exit(1)

try:
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    # Toplam mesaj sayısı
    cursor.execute("SELECT COUNT(*) FROM Messages")
    total_messages = cursor.fetchone()[0]
    print(f"📊 Toplam Mesaj Sayısı: {total_messages}")
    print("=" * 80)

    # Kullanıcılar
    cursor.execute("SELECT DISTINCT username FROM Messages ORDER BY username")
    users = cursor.fetchall()
    print(f"👥 Kullanıcılar ({len(users)}):")
    for user in users:
        # Her kullanıcının mesaj sayısı
        cursor.execute("SELECT COUNT(*) FROM Messages WHERE username = ?", (user[0],))
        msg_count = cursor.fetchone()[0]
        print(f"  - {user[0]} ({msg_count} mesaj)")
    print("=" * 80)

    # Tüm mesajlar
    cursor.execute("""
        SELECT id, username, mesaj, cevap, timestamp 
        FROM Messages 
        ORDER BY timestamp DESC 
        LIMIT 50
    """)

    rows = cursor.fetchall()
    print(f"💬 Son 50 Mesaj:\n")

    if not rows:
        print("Henüz mesaj yok.")
    else:
        for i, row in enumerate(rows, 1):
            msg_id, username, message, response, timestamp = row
            print(f"{i}. [{timestamp}] @{username}")
            print(f"   Mesaj: {message}")
            print(f"   AI Cevabı: {response}")
            print("-" * 80)

    conn.close()
    print("\n✅ Veritabanı okuma tamamlandı.")

except sqlite3.Error as e:
    print(f"❌ Veritabanı hatası: {e}")
except Exception as e:
    print(f"❌ Hata: {e}")

