import json

with open("extracted_texts/client_demand_cecil.json", "r", encoding="utf-8-sig") as f:
    items = json.load(f)

user_strings = []
for it in items:
    s = it["String"].strip()
    # Filter out typical internal symbols, formats, shaders, etc.
    if len(s) > 2 and not s.startswith("__") and not s.startswith("m_") and not s.startswith("System."):
        user_strings.append(it)

print(f"Total potential strings: {len(user_strings)}")
with open("extracted_texts/client_demand_filtered.json", "w", encoding="utf-8") as f:
    json.dump(user_strings, f, ensure_ascii=False, indent=2)
