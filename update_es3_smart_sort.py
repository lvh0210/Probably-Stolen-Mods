save_path = r"C:\Users\LvH\AppData\LocalLow\Questing Goose Studio\Probably Stolen\SaveFile.es3"

with open(save_path, "r", encoding="utf-8") as f:
    content = f.read()

target = '"value":["AutoAppraisal","FraudDetection","baishe_all_characters_feminized","baishe_r18_pack","AlwaysHighlightCustomerDemand","LotteryForesight","PreciseTemperatureControl","AugmentedDetector"]'
replacement = '"value":["AutoAppraisal","FraudDetection","baishe_all_characters_feminized","baishe_r18_pack","AlwaysHighlightCustomerDemand","LotteryForesight","PreciseTemperatureControl","AugmentedDetector","SmartInventorySort"]'

if target in content:
    new_content = content.replace(target, replacement)
    with open(save_path, "w", encoding="utf-8") as f:
        f.write(new_content)
    print("SUCCESS: SmartInventorySort added to SaveFile.es3!")
elif '"SmartInventorySort"' in content:
    print("ALREADY INCLUDED: SmartInventorySort already in SaveFile.es3!")
else:
    print("WARNING: Target string not found in SaveFile.es3!")
