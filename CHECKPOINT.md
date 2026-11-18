# Bàn Giao Tiến Độ Kỹ Thuật (Checkpoint) - Hoàn Tất 100%

- **Dự án**: Bộ Mod & Việt Hóa game Probably Stolen (Bản IMod Native)
- **Thư mục lưu trữ**: `D:\VibeCode\Probably Stolen`
- **Kho lưu trữ GitHub**: [https://github.com/lvh0210/Probably-Stolen-Mods](https://github.com/lvh0210/Probably-Stolen-Mods)
- **Trạng thái**: Hoàn tất 100% nâng cấp hệ thống Tự Động Nhận Diện Hòm Đồ Mặc Định & Cơ chế Ưu Tiên Lấp Đầy 2 Tầng (`SmartInventorySort` v1.2.0).

## Tính Năng Mới Đã Triển Khai (`SmartInventorySort` v1.2.0):
1. **Tự động nhận diện hòm đồ (Auto-Detection Defaults)**:
   - Tự động nhận diện loại đồ phù hợp cho từng hòm dựa vào tên và tag của hòm (Hòm an ninh `SecLootBox` -> Vũ khí & Đạn, Hòm y tế `MedLootBox` -> Y tế & Dược phẩm, Hộp cơm `LunchBox` -> Thực phẩm, Hòm kỹ thuật `EngLootBox` -> Linh kiện, v.v.).
   - Mặc định khi bấm phím `F3`, tất cả đồ sẽ tự động xếp vào đúng các hòm theo loại mà không cần cấu hình thủ công.
2. **Tùy biến chọn hòm linh hoạt & Giữ nguyên các hòm khác**:
   - Menu `F2` cho phép người chơi click chọn 1 hòm bất kỳ để nhận loại đồ mong muốn (chuyển sang trạng thái `[⭐ Ưu Tiên]`). Các hòm khác vẫn giữ nguyên cấu hình cũ hoặc chế độ `[🤖 Tự Động]`.
3. **Cơ chế lấp đầy ưu tiên 2 tầng (Two-Tier Priority Filling)**:
   - Các món đồ thuộc danh mục sẽ ưu tiên lấp đầy vào **Hòm được chọn (`⭐ Ưu Tiên`)** trước cho đến khi hòm đó đầy ních không còn ô trống.
   - Khi hòm được chọn đã đầy, các món đồ tiếp theo sẽ tự động tràn sang **Hòm mặc định (`🤖 Tự Động`)** cho đến khi đầy.
   - Nếu đầy tất cả các hòm đích, món đồ được giữ an toàn tuyệt đối tại kho nguồn.
4. **Quy tắc phân loại theo Tag đầu tiên**:
   - Lấy tag đầu tiên (`itemTypes[0]`) của món đồ để phân nhóm chính.
5. **Nén khít chặt & Chống tràn hòm**:
   - Tự động bỏ qua đồ ghim (`IsPinned`), rác bẩn (`IsDebris`), và không nhét hòm vào trong hòm.
   - Kích hoạt thuật toán nén gọn `PackFirstFit` trên mọi hòm sau khi phân phối.

*Cập nhật mốc tự động: Step 1948 | 2026-11-18 17:30:00*
