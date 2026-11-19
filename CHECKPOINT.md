# Bàn Giao Tiến Độ Kỹ Thuật (Checkpoint) - Hoàn Tất 100%

- **Dự án**: Bộ Mod & Việt Hóa game Probably Stolen (Bản IMod Native)
- **Thư mục lưu trữ**: `D:\VibeCode\Probably Stolen`
- **Kho lưu trữ GitHub**: [https://github.com/lvh0210/Probably-Stolen-Mods](https://github.com/lvh0210/Probably-Stolen-Mods)
- **Trạng thái**: Hoàn tất 100% việc khắc phục triệt để lỗi phân loại sai danh mục (False Positives) cho Hàng Cấm và Vật Liệu, thiết lập cơ chế bảo vệ chéo Master Safety Shield cho Thực Phẩm & Y Tế (`SmartInventorySort` v1.5.2).

## Các Tối Ưu Hóa & Cải Tiến Cốt Lõi (`SmartInventorySort` v1.5.2):
1. **Khắc Phục Triệt Để Lỗi Phân Loại Nhầm Hàng Cấm (Contraband False Positives Fix)**:
   - *Nguyên nhân trước đây*: Hệ thống quét từ khóa `SUBSTANCE` và `DRUG`. Trong game engine, toàn bộ đồ ăn, nước uống và thuốc điều trị y tế đều mang tag nội bộ `SUBSTANCE` (vật chất tiêu thụ) hoặc `DRUG` (dược phẩm). Điều này khiến món ăn (như củ Nutrifruit) và các viên thuốc y tế thông thường bị gán nhầm tag `CONTRABAND` và chuyển nhầm vào hòm Hàng Cấm.
   - *Khắc phục v1.5.2*: Loại bỏ hoàn toàn việc nhận diện `SUBSTANCE` và chuỗi `DRUG` làm hàng cấm. Nhận diện Hàng Cấm giờ đây chỉ kích hoạt với các danh mục chính thức từ engine: `CONTRABAND`, `RESTRICTED_1/2/3`, `NARCOTIC`, `POISON`, hoặc các vật phẩm đặc định (`fanta_pill`, `zyanide_pill`, `fentanyl`, `dream_dust`).
2. **Khắc Phục Triệt Để Lỗi Phân Loại Nhầm Vật Liệu (Material False Positives Fix)**:
   - *Nguyên nhân trước đây*: So khớp chuỗi `nut` (làm dính `nutrifruit`), chuỗi `plastic` (làm dính vỏ lọ thuốc / ống tiêm y tế), và quét đệ quy các khóa runtime trong `item.state.dict` (chứa các thuộc tính vật lý của món đồ).
   - *Khắc phục v1.5.2*:
     - Loại bỏ việc duyệt `item.state.dict.Keys`.
     - Thay đổi nhận diện ốc vít `nut` thành khớp chính xác (`nuts`, `screws`, `nuts_*`), tránh hoàn toàn `nutrifruit`.
     - Loại bỏ `plastic` để không làm dính các dụng cụ y tế bằng nhựa.
3. **Lá Chắn Bảo Vệ Chéo & Giải Trừ Xung Đột (Master Safety Shield)**:
   - *Quy tắc 1 (Thực Phẩm & Nước Uống)*: Nếu món đồ thuộc nhóm `FOOD` hoặc `DRINK`, hệ thống tự động loại trừ tuyệt đối khỏi `CONTRABAND` (trừ khi game chính thức gán nhãn hàng cấm) và loại trừ tuyệt đối khỏi `MATERIAL`.
   - *Quy tắc 2 (Y Tế)*: Nếu món đồ thuộc nhóm `MEDICAL`, hệ thống tự động loại trừ khỏi `MATERIAL` (không bao giờ xem thuốc là vật liệu) và loại trừ khỏi `CONTRABAND` (trừ các loại độc dược/thuốc phiện lậu thực sự).
   - *Quy tắc 3 (Vật Liệu)*: Nếu món đồ bị xung đột với Thực Phẩm hoặc Y Tế, hệ thống luôn ưu tiên Thực Phẩm / Y Tế và xóa bỏ tag Vật Liệu.
4. **Chuẩn Hóa Danh Mục Gốc (GameItemType Canonical Mapping)**:
   - Tận dụng chính xác danh mục hiển thị trên tooltip của game (`[Thực phẩm]`, `[Y tế]`, `[Vật liệu]`, `[Vũ khí]`, `[Hàng cấm]`) làm nguồn xác thực gốc trước khi xét các tag bổ trợ.
5. **Đồng Bộ Phiên Bản & Đóng Gói**:
   - Cập nhật phiên bản lên `1.5.2` trong `SmartInventorySortMod.cs` và `manifest.xml`.
   - Biên dịch thành công `SmartInventorySort.dll` (v1.5.2) và triển khai vào `D:\GameCrack\Probably Stolen\Mods\SmartInventorySort\`.

*Cập nhật tự động: v1.5.2 | 2026-11-19 23:50*
