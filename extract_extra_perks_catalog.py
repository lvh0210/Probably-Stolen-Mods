import json
import re

with open("extracted_texts/extra_perks_en_cecil.json", "r", encoding="utf-8-sig") as f:
    en_items = json.load(f)

with open("extracted_texts/extra_perks_cn_cecil.json", "r", encoding="utf-8-sig") as f:
    cn_items = json.load(f)

# Clean trailing nulls from strings
def clean_str(s):
    if not s:
        return ""
    return s.strip("\x00").strip()

catalog = {}
for en, cn in zip(en_items, cn_items):
    t = en["Type"]
    m = en["Method"]
    idx = en["Index"]
    en_str = clean_str(en["String"])
    cn_str = clean_str(cn["String"])
    
    # We only care about user-facing strings
    # get_DisplayName, get_Description, messages, dialogs
    if m in ["get_DisplayName", "get_Description", "TryMissCustomer", ".cctor"] or (en_str != cn_str and m != "get_Id" and m != "IsActive" and m != "IsSelectedIn" and m != "EnsureVanillaIncompatibility"):
        key = f"{t}::{m}::{idx}"
        catalog[key] = {
            "type": t,
            "method": m,
            "index": idx,
            "en": en_str,
            "cn": cn_str
        }

print(f"Extracted {len(catalog)} user-facing strings for ExtraPerks.")
with open("extracted_texts/extra_perks_catalog.json", "w", encoding="utf-8") as f:
    json.dump(catalog, f, ensure_ascii=False, indent=2)
