# Probably Stolen - Native IMod Collection (Bộ Mod Tối Ưu & Tiện Ích Thuần Việt)

Bộ sưu tập mã nguồn các bản mod Native (`IMod`) độc lập, không phụ thuộc MelonLoader hay BepInEx, dành riêng cho tựa game **Probably Stolen** (v0.46+). Tương thích hoàn toàn với hệ thống Mod mặc định của game và quản lý qua Harmony.

---

## 🌟 Danh Sách Các Mod Chính

### 1. 📦 Sắp Xếp & Phân Loại Kho Đa Hòm Thông Minh (`SmartInventorySort` v1.2.0)
* **Thư mục mã nguồn**: [`custom_mods/SmartInventorySort/`](custom_mods/SmartInventorySort/)
* **Tác giả**: Antigravity
* **Mục tiêu**: Kết hợp hoàn hảo giữa thuật toán sắp xếp khít chặt không khoảng trống và hệ thống phân loại đồ tự động vào các hòm chứa khác nhau (Vũ khí, Y tế, Thực phẩm...) với cơ chế tự nhận diện hòm và ưu tiên lấp đầy 2 tầng.

#### 💡 Các tính năng nổi bật:
1. **Tự động nhận diện hòm đồ thông minh (Auto-Detection Defaults)**:
   - Tự động quét toàn bộ kho cố định trong tiệm, các cửa sổ hòm đồ đang mở (`PixelWindow` sau khi quẹt thẻ mở khóa như Hòm an ninh `SecLootBox`, Hòm y tế `MedLootBox`, Hòm kỹ thuật `EngLootBox`, Hộp cơm `LunchBox`, Két sắt `Safe`...), và các hộp chứa đồ đặt trong kho.
   - Tự động nhận diện loại đồ phù hợp với từng hòm theo tên và tag:
     * 🛡️/⚔️ **Hòm An Ninh / Crate Vũ Khí / Két Sắt**: Tự động nhận diện thành `[🤖 Tự Động: ⚔️ Vũ Khí & Đạn]`
     * 🏥 **Hòm Y Tế / Hộp Sơ Cứu / First Aid**: Tự động nhận diện thành `[🤖 Tự Động: 💊 Y Tế & Dược Phẩm]`
     * 🍱 **Hộp Cơm / Tủ Lạnh / Thực Phẩm**: Tự động nhận diện thành `[🤖 Tự Động: 🍞 Thực Phẩm & Nước]`
     * ⚙️ **Hòm Kỹ Thuật / Engineering / Tech**: Tự động nhận diện thành `[🤖 Tự Động: ⚡ Linh Kiện & Module]`
     * 🔧 **Hộp Đồ Nghề / Tool Box / Vật Liệu**: Tự động nhận diện thành `[🤖 Tự Động: 🔧 Vật Liệu & Công Cụ]`
     * 📁 **Cặp Tài Liệu / Evidence Box**: Tự động nhận diện thành `[🤖 Tự Động: 📄 Tài Liệu & Quý Giá]`
     * 🏢 **Kho Tiệm Chính / Bàn Thao Tác**: Mặc định là `[📦 Kho Chung / Bỏ Qua]`.

2. **Cơ chế lấp đầy ưu tiên 2 tầng (Two-Tier Priority Filling)**:
   - **Tùy biến chỉ định linh hoạt**: Người chơi có thể bấm `F2` để gán riêng 1 hòm bất kỳ nhận loại đồ mong muốn (chuyển sang trạng thái `[⭐ Ưu Tiên]`). Các hòm khác vẫn giữ nguyên cấu hình cũ hoặc giữ nguyên chế độ `[🤖 Tự Động]`.
   - **Thứ tự lấp đầy**:
     * Khi bấm `F3` (hoặc nút Gom đồ), các món đồ sẽ được chuyển và **ưu tiên lấp đầy vào Hòm Chỉ Định (`⭐ Ưu Tiên`) TRƯỚC** cho đến khi hòm đó đầy ních không còn ô trống.
     * Khi Hòm Chỉ Định đã đầy, các món đồ tiếp theo mới tự động chuyển sang **Hòm Mặc Định (`🤖 Tự Động`)** cho đến khi đầy.
     * Nếu tất cả đều đầy, đồ giữ an toàn tuyệt đối tại kho nguồn.

3. **Phân loại theo Tag đầu tiên (First Tag Category Mapping)**:
   - Quét danh sách chủng loại `item.GetGameItemType()` của từng món đồ.
   - **Lấy tag đầu tiên (`itemTypes[0]`)** để gán nhóm danh mục chính:
     * ⚔️ **Vũ Khí & Đạn**: `WEAPON`, `FIREARM`, `MELEE_WEAPON`, `AMMUNITION`, `GUN_MOD`...
     * 💊 **Y Tế & Dược Phẩm**: `MEDICAL`, `MEDICAL_TOOL`, `NARCOTIC`, `POISON`, `ORGAN`...
     * 🍞 **Thực Phẩm & Nước**: `FOOD`, `PROCESSED_FOOD`, `WATER`, `ALCOHOL`, `TREAT`...
     * 🛡️ **Giáp & Trang Bị**: `ARMOR`...
     * ⚡ **Linh Kiện & Module**: `MODULE`, `ENERGY_CELL`, `MACHINE`...
     * 🔧 **Vật Liệu & Công Cụ**: `MATERIAL`, `TOOL`, `CHEMICAL_SUPPLIES`, `FILTER`...
     * 📄 **Tài Liệu & Quý Giá**: `DOCUMENT`, `LUXURY_ITEM`, `ACCESS_CARD`, `ART`...

4. **Kết hợp 2 phương thức điều khiển tiện lợi**:
   - **Cách 1 - Bảng Menu Trực Quan (Phím `F2`)**: 
     * Bấm `F2` để mở cửa sổ nổi hiển thị tất cả các hòm đồ trong tiệm (kèm số món hiện có và kích thước hòm).
     * Click vào từng hòm để đổi loại đồ nhận hoặc để Tự Động. Có nút "🔄 Đặt lại Tự Động" để reset mọi hòm về mặc định.
     * Cấu hình lưu tự động vào `PlayerPrefs`, chơi lại không cần cấu hình lại!
   - **Cách 2 - Phím Tắt Nhanh (Phím `F3`)**: 
     * Bấm `F3` bất cứ lúc nào để mod tự động gom đồ từ các kho và phân phối vào đúng các hòm theo thứ tự ưu tiên, không cần mở menu!

5. **Cơ chế bảo toàn an toàn & Nén khít chặt (PackFirstFit Hook)**:
   - Tự động bỏ qua các món đồ người chơi đã ghim (`IsPinned`), rác bẩn (`IsDebris`), và không nhét các hòm đồ vào trong hòm đồ khác.
   - Hook Harmony Prefix tại `InventorySortHelper.PackShelves` để kích hoạt thuật toán lấp đầy từng ô (`PackFirstFit`) gốc của Unity, tự động nén khít chặt tất cả các hòm sau khi phân loại.

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
