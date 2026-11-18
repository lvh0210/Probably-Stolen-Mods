# Probably Stolen - Native IMod Collection (Bộ Mod Tối Ưu & Tiện Ích Thuần Việt)

Bộ sưu tập mã nguồn các bản mod Native (`IMod`) độc lập, không phụ thuộc MelonLoader hay BepInEx, dành riêng cho tựa game **Probably Stolen** (v0.46+). Tương thích hoàn toàn với hệ thống Mod mặc định của game và quản lý qua Harmony.

---

## 🌟 Danh Sách Các Mod Chính

### 1. 📦 Sắp Xếp Kho Thông Minh (`SmartInventorySort`)
* **Thư mục mã nguồn**: [`custom_mods/SmartInventorySort/`](custom_mods/SmartInventorySort/)
* **Tác giả**: Antigravity
* **Mục tiêu**: Khắc phục triệt để vấn đề thuật toán sắp xếp kho mặc định của game để lại quá nhiều khoảng trống chết (gaps), tối ưu hóa diện tích cho cả kho hàng, kệ trưng bày và túi ba lô (`TechnicianBackpack`).

#### 🔬 Phân tích nguyên nhân lỗi trong game gốc (Root Cause):
* Thuật toán gốc của game trong `InventorySortHelper.Sort` mặc định ưu tiên gọi `InventorySortHelper.PackShelves`.
* `PackShelves` phân kho thành các hàng "kệ" (shelves) ngang cố định. Chiều cao của mỗi kệ bị khóa cứng theo **món đồ cao nhất** trên hàng đó.
* Ví dụ: Trên cùng một hàng nếu có 1 món cao 3 ô và 1 món cao 1 ô, **2 ô trống phía trên món cao 1 ô sẽ bị bỏ phí vĩnh viễn**, game không bao giờ lấp đầy chúng, dẫn tới tình trạng người chơi bấm sắp xếp xong thì kho vẫn có nhiều lỗ hổng lớn lắt nhắt.

#### 💡 Giải pháp kỹ thuật (Technical Solution):
* **Harmony Prefix Hook**: Hook trực tiếp vào `InventorySortHelper.PackShelves` và thiết lập `__result = null; return false;`.
* **Kích hoạt tự động `PackFirstFit`**: Khi `PackShelves` trả về null, engine gốc của game sẽ ngay lập tức chuyển sang thuật toán quét từng ô (`PackFirstFit`).
* **Ưu điểm vượt trội**:
  - `PackFirstFit` quét từng tọa độ `(x, y)` theo thứ tự, tự động chèn các món đồ nhỏ (1x1, 1x2...) vào bất kỳ góc cạnh hoặc lỗ hổng còn trống nào.
  - Sử dụng 100% logic va chạm chuẩn (`FitsOccupancy` + `CheckShapePlacement`) của Unity engine, đảm bảo không bao giờ bị đè chồng item hoặc dồn cục vào góc.
  - Bảo toàn trọn vẹn các vật phẩm đã ghim (`ItemPinHelper.IsPinned`) và rác bẩn trong kho (`DebrisHelper.IsDebris`).
  - Hỗ trợ hoàn hảo tính năng Hoàn tác (`Undo`) gốc của game.

---

### 2. 👁️ Máy Dò Dị Nhân & Ngụy Nhân (`AugmentedDetector`)
* **Thư mục mã nguồn**: [`custom_mods/AugmentedDetector/`](custom_mods/AugmentedDetector/)
* **Tác giả**: Antigravity
* **Mục tiêu**: Nhận diện tức thì khách hàng là Dị Nhân Cải Tạo (Augmented / Anomaly) hoặc Kẻ Đang Bị Truy Nã (Wanted Poster Target), giúp người chơi không bao giờ bị bỏ lỡ hoặc chọn nhầm phương án đối thoại.

#### 💡 Các cơ chế hiển thị trực quan 3 tầng:
1. **Thẻ tên trực quan trên khung thoại (`DialogUIManager.DisplayClientText`)**:
   - Khách là Dị nhân: Hiển thị tiền tố rực rỡ màu đỏ neon kèm bậc cải tạo: `<color=#FF0055>[⚠️ DỊ NHÂN CẢI TẠO - BẬC X]</color> <Tên Khách>`.
   - Khách là người thường: `<color=#00FF88>[NGƯỜI THƯỜNG]</color> <Tên Khách>`.
   - Khách bị truy nã: Thêm tiền tố cảnh báo màu cam: `<color=#FFAA00>[🚨 TRUY NÃ: ID]</color>`.
2. **Đổi màu bong bóng thoại (`DialogUIManager.SetClientBubbleColor`)**:
   - Khi phát hiện khách là Dị nhân, bong bóng hội thoại và bong bóng tên khách tự động chuyển sang màu đỏ hồng neon cảnh báo (`RGB: 1.0, 0.15, 0.45`).
3. **Thẻ cảnh báo 3D trên đỉnh đầu (`StoreClientMono.PlayIdleForCurrentClient`)**:
   - Tạo một đối tượng 3D `TextMesh` lơ lửng ngay phía trên đỉnh đầu nhân vật: `⚠️ [DỊ NHÂN - BẬC X]`.

---

## 🛠️ Các Mod Bổ Trợ Khác Trong Bộ Sưu Tập

* **`LotteryForesight`** ([`custom_mods/LotteryForesight/`](custom_mods/LotteryForesight/)): Soi trước giải thưởng vé cào trong kho/kệ hàng, khôi phục vé quay số đêm tự nhiên để giữ cân bằng kinh tế game, hỗ trợ phím `Space`/`Enter` cào nhanh.
* **`PreciseTemperatureControl`** ([`custom_mods/PreciseTemperatureControl/`](custom_mods/PreciseTemperatureControl/)): Mở khóa nút điều chỉnh nhiệt độ chính xác từng độ C trên bếp nung/đun hóa chất (Click thường: ±1°C, Shift+Click: ±5°C, Ctrl+Click: ±10°C).
* **`AlwaysHighlightCustomerDemand`** ([`custom_mods/AlwaysHighlightCustomerDemand/`](custom_mods/AlwaysHighlightCustomerDemand/)): Tự động làm nổi bật các món hàng khách yêu cầu trong kho với hiệu ứng viền sáng và thông tin thưởng giá trị.

---

## 🚀 Hướng Dẫn Cài Đặt (Installation)

1. Tải về thư mục mod mong muốn (hoặc build từ mã nguồn theo hướng dẫn bên dưới).
2. Sao chép thư mục mod vào đường dẫn `Mods` của game:
   ```text
   <Thư_Mục_Game>\Mods\<Tên_Mod>\
   ├── <Tên_Mod>.dll
   └── manifest.xml
   ```
3. Khởi động game. Vào menu **Modding** trong game để bật kích hoạt mod (hoặc game sẽ tự động nhận diện thông qua file cấu hình `SaveFile.es3`).

---

## 🔨 Hướng Dẫn Biên Dịch Mã Nguồn (Build from Source)

Game sử dụng kiến trúc .NET Framework / .NET Standard 2.1 với Unity Mono. Bạn có thể sử dụng trình biên dịch Roslyn C# Compiler có sẵn trong Unity hoặc .NET SDK để biên dịch:

### Biên dịch `SmartInventorySort`:
```powershell
$managed = "<Thư_Mục_Game>\Probably Stolen_Data\Managed"
$mono = "D:\Unity\Editor\2022.3.62f1\Editor\Data\MonoBleedingEdge\bin\mono.exe"
$csc = "D:\Unity\Editor\2022.3.62f1\Editor\Data\MonoBleedingEdge\lib\mono\4.5\csc.exe"

$refs = @(
    "$managed\mscorlib.dll",
    "$managed\netstandard.dll",
    "$managed\Assembly-CSharp.dll",
    "$managed\0Harmony.dll",
    "$managed\UnityEngine.dll",
    "$managed\UnityEngine.CoreModule.dll"
)
$refArgs = $refs | ForEach-Object { "/reference:`"$_`"" }

& $mono $csc /target:library /nostdlib+ /langversion:latest "/out:SmartInventorySort.dll" $refArgs "SmartInventorySortMod.cs"
```

Tương tự, các script build sẵn `build_smart_sort.ps1` và `build_augmented_detector.ps1` đã được thiết lập sẵn trong thư mục gốc.

---

## 📜 Giấy Phép & Tuyên Bố Miễn Trừ Trách Nhiệm
- Dự án được phát triển phục vụ mục đích nghiên cứu, học tập kỹ thuật dịch ngược (Reverse Engineering) và tối ưu hóa trải nghiệm chơi game cho cộng đồng.
- Mọi bản quyền trò chơi gốc thuộc về **Questing Goose Studio**.
