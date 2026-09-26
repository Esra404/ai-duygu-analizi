import sys
import json
import requests


SPACE_URL = "https://esra404-neco.hf.space"


if len(sys.argv) < 2:
    print(json.dumps({
        "error": "Lütfen bir prompt girin."
    }, ensure_ascii=False))
    sys.exit(1)


input_text = sys.argv[1]


try:
    # 1. Gradio API'ye isteği gönder
    response = requests.post(
        f"{SPACE_URL}/gradio_api/call/predict",
        json={"data": [input_text]},
        timeout=60
    )

    response.raise_for_status()

    event_id = response.json()["event_id"]

    # 2. Sonucu bekle
    result_response = requests.get(
        f"{SPACE_URL}/gradio_api/call/predict/{event_id}",
        timeout=60,
        stream=True
    )

    result_response.raise_for_status()

    result = None

    for line in result_response.iter_lines(decode_unicode=True):
        if line and line.startswith("data:"):
            data = line[5:].strip()

            try:
                parsed = json.loads(data)

                if parsed:
                    result = parsed[0]
                    break

            except json.JSONDecodeError:
                continue

    if result is None:
        raise Exception("AI servisinden sonuç alınamadı.")

    print(json.dumps({
        "result": result
    }, ensure_ascii=False))


except Exception as e:
    print(json.dumps({
        "error": str(e)
    }, ensure_ascii=False))
    sys.exit(1)