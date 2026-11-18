import dnfile
import json
import os

def dump_strings(dll_path, out_json):
    pe = dnfile.dnPE(dll_path)
    strings = []
    if hasattr(pe.net, 'user_strings'):
        for s in pe.net.user_strings:
            if s and s.value:
                strings.append(s.value)
    
    with open(out_json, 'w', encoding='utf-8') as f:
        json.dump(strings, f, ensure_ascii=False, indent=2)
    print(f"{os.path.basename(dll_path)}: {len(strings)} strings saved to {out_json}")

if __name__ == "__main__":
    os.makedirs("extracted_texts", exist_ok=True)
    dump_strings(r"D:\VibeCode\Probably Stolen\workspace\ExtraPerks_EN\ExtraPerks.dll", "extracted_texts/extra_perks_en.json")
    dump_strings(r"D:\VibeCode\Probably Stolen\workspace\ExtraPerks_CN\ExtraPerks.dll", "extracted_texts/extra_perks_cn.json")
    dump_strings(r"D:\VibeCode\Probably Stolen\workspace\MoreFeatures\release-v1.9.20-new-demo\XIAOWOTradePerks.dll", "extracted_texts/xiaowo_en_cn.json")
    dump_strings(r"D:\VibeCode\Probably Stolen\workspace\ClientDemandHighlight\ClientDemandHighlight.dll", "extracted_texts/client_demand.json")
    dump_strings(r"D:\VibeCode\Probably Stolen\workspace\DeepSpacePortraits\Bai She's All Original Characters Feminized 1.0.0-Beta\Bai She's All Original Characters Feminized 1.0.0-Beta.dll", "extracted_texts/baishe_portrait.json")
