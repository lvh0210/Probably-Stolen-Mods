# Probably Stolen - Native IMod Collection (Bộ Mod Tối Ưu & Tiện Ích Thuần Việt)

Bộ sưu tập mã nguồn các bản mod Native (`IMod`) độc lập, không phụ thuộc MelonLoader hay BepInEx, dành riêng cho tựa game **Probably Stolen** (v0.46+). Tương thích hoàn toàn với hệ thống Mod mặc định của game và quản lý qua Harmony.

---

## 🌟 Danh Sách Các Mod Chính

### 1. 📦 Sắp Xếp & Phân Loại Kho Đa Hòm Thông Minh (`SmartInventorySort` v1.1.0)
* **Thư mục mã nguồn**: [`custom_mods/SmartInventorySort/`](custom_mods/SmartInventorySort/)
* **Tác giả**: Antigravity
* **Mục tiêu**: Kết hợp hoàn hảo giữa thuật toán sắp xếp khít chặt không khoảng trống và hệ thống phân loại đồ tự động vào các hòm chứa khác nhau (Vũ khí, Y tế, Thực phẩm...).

#### 💡 Các tính năng nổi bật:
1. **Phân loại theo Tag đầu tiên (First Tag Category Mapping)**:
   - Quét danh sách chủng loại `item.GetGameItemType()` của từng món đồ.
   - **Lấy tag đầu tiên (`itemTypes[0]`)** để gán nhóm danh mục chính:
     * ⚔️ **Vũ Khí & Đạn**: `WEAPON`, `FIREARM`, `MELEE_WEAPON`, `AMMUNITION`, `GUN_MOD`...
     * 💊 **Y Tế & Dược Phẩm**: `MEDICAL`, `MEDICAL_TOOL`, `NARCOTIC`, `POISON`, `ORGAN`...
     * 🍞 **Thực Phẩm & Nước**: `FOOD`, `PROCESSED_FOOD`, `WATER`, `ALCOHOL`, `TREAT`...
     * 🛡️ **Giáp & Trang Bị**: `ARMOR`...
     * ⚡ **Linh Kiện & Module**: `MODULE`, `ENERGY_CELL`, `MACHINE`...
     * 🔧 **Vật Liệu & Công Cụ**: `MATERIAL`, `TOOL`, `CHEMICAL_SUPPLIES`, `FILTER`...
     * 📄 **Tài Liệu & Quý Giá**: `DOCUMENT`, `LUXURY_ITEM`, `ACCESS_CARD`, `ART`...
2. **Kết hợp 2 phương thức điều khiển tiện lợi**:
   - **Cách 1 - Bảng Menu Trực Quan (Phím `F2`)**: 
     * Bấm `F2` để mở cửa sổ nổi hiển thị tất cả các hòm đồ/kho trong tiệm (Kho Bàn Chính, Kệ Trưng Bày, Hòm Trước Quầy, Hòm Sau Quầy, Hòm Thu Ngân, Hòm Chợ Trái, Hòm Đang Mở...).
     * Click vào từng hòm để chọn danh mục đồ muốn chuyển vào.
     * Cấu hình được lưu tự động (`PlayerPrefs`), những lần chơi sau không cần chọn lại!
   - **Cách 2 - Phím Tắt Nhanh (Phím `F3`)**: 
     * Bấm `F3` bất cứ lúc nào để mod tự động gom đồ từ các kho và phân phối vào đúng các hòm đã gán từ trước, không cần mở menu!
3. **Cơ chế chống mất đồ & chống tràn hòm an toàn**:
   - Tự động bỏ qua các món đồ người chơi đã ghim (`IsPinned`) và rác bẩn (`IsDebris`).
   - Nếu hòm đích đã đầy không thể chứa thêm món đồ, hệ thống sẽ tự động hoàn trả món đồ về lại hòm ban đầu, đảm bảo 100% không bao giờ bị mất đồ.
4. **Sắp xếp khít chặt (PackFirstFit Hook)**:
   - Hook Harmony Prefix trả về null tại `InventorySortHelper.PackShelves` để kích hoạt thuật toán lấp đầy từng ô (`PackFirstFit`) gốc của Unity.
   - Sau khi phân loại, mod tự động chạy nén khít chặt toàn bộ các hòm, loại bỏ sạch khoảng trống chết.

---

### 2. 👁️ Máy Dò Dị Nhân & Ngụy Nhân (`AugmentedDetector`)
* **Thư mục mã nguồn**: [`custom_mods/AugmentedDetector/`](custom_mods/AugmentedDetector/)
* **Tác giả**: Antigravity
* **Mục tiêu**: Nhận diện tức thì khách hàng là Dị Nhân Cải Tạo (Augmented / Anomaly) hoặc Kẻ Đang Bị Truy Nã (Wanted Poster Target).

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

1. Tải về thư mục mod mong muốn.
2. Sao chép thư mục mod vào đường dẫn `Mods` của game:
   ```text
   <Thư_Mục_Game>\Mods\<Tên_Mod>\
   ├── <Tên_Mod>.dll
   └── manifest.xml
   ```
3. Khởi động game. Mod sẽ tự động kích hoạt (thông qua `SaveFile.es3` hoặc menu Modding trong game).
4. Phím tắt trong game:
   - `F2`: Bật/Tắt Menu Phân Loại Hòm Đồ.
   - `F3`: Gom & Phân loại nhanh tất cả vào các hòm theo cấu hình.

---

## 🔨 Hướng Dẫn Biên Dịch Mã Nguồn (Build from Source)

Game sử dụng kiến trúc .NET Framework / .NET Standard 2.1 với Unity Mono. Chạy script `build_smart_sort.ps1` để biên dịch:

```powershell
powershell -File build_smart_sort.ps1
```

---

## 📜 Giấy Phép & Tuyên Bố Miễn Trừ Trách Nhiệm
- Dự án được phát triển phục vụ mục đích nghiên cứu, học tập kỹ thuật dịch ngược (Reverse Engineering) và tối ưu hóa trải nghiệm chơi game cho cộng đồng.
- Mọi bản quyền trò chơi gốc thuộc về **Questing Goose Studio**.
