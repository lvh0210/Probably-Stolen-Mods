import json

with open("extracted_texts/extra_perks_en_cecil.json", "r", encoding="utf-8-sig") as f:
    en_list = json.load(f)

with open("extracted_texts/extra_perks_cn_cecil.json", "r", encoding="utf-8-sig") as f:
    cn_list = json.load(f)

pairs = []
for e, c in zip(en_list, cn_list):
    if e["String"] != c["String"]:
        pairs.append({
            "Type": e["Type"],
            "Method": e["Method"],
            "Index": e["Index"],
            "EN": e["String"],
            "CN": c["String"]
        })

print(f"Total differing strings: {len(pairs)}")
with open("extracted_texts/extra_perks_diff.json", "w", encoding="utf-8") as f:
    json.dump(pairs, f, ensure_ascii=False, indent=2)

for p in pairs[:10]:
    print(f"[{p['Type']}::{p['Method']}]")
    print(f"  EN: {p['EN']}")
    print(f"  CN: {p['CN']}")
