import re

path = r"C:\Users\LvH\AppData\LocalLow\Questing Goose Studio\Probably Stolen\SaveFile.es3"
with open(path, "r", encoding="utf-8") as f:
    s = f.read()

m = re.search(r'"modOrder"[^}]+', s)
if m:
    print(m.group(0))
else:
    print("Not found")
