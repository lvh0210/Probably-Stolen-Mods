# Bàn Giao Tiến Độ Kỹ Thuật (Checkpoint) - Hoàn Tất 100%

- **Dự án**: Bộ Mod & Việt Hóa game Probably Stolen (Bản IMod Native)
- **Thư mục lưu trữ**: `D:\VibeCode\Probably Stolen`
- **Kho lưu trữ GitHub**: [https://github.com/lvh0210/Probably-Stolen-Mods](https://github.com/lvh0210/Probably-Stolen-Mods)
- **Trạng thái**: Hoàn tất 100% tối ưu hóa nén khít chặt nhờ tự động xoay item 90° và thu gọn menu F2 chỉ hiển thị hòm có ID (`SmartInventorySort` v1.3.1).

## Các Tối Ưu Hóa & Cải Tiến Cốt Lõi (`SmartInventorySort` v1.3.1):
1. **Tự động xoay ngang/dọc (Auto-Rotate 90°) để nén khít tuyệt đối**:
   - Thêm Harmony prefix `ForceSortAutoRotatePatch` trên `EmporiumOption.get_SortAutoRotate` trả về `true`.
   - Cơ chế này kích hoạt bộ phát sinh hướng đặt đồ `<Orientations>d__18` của game, tự động thử cả hướng ngang (0) và dọc (1) cho mọi item trong quá trình nén `PackFirstFit`. Nhờ đó, các món đồ hình chữ nhật dài sẽ tự động xoay để vừa khít vào các khoảng trống hẹp còn thừa.
2. **Thu gọn Menu F2 siêu tinh giản (Chỉ hiển thị hòm có ID)**:
   - Thêm thuộc tính `HasId` vào `NamedInventory`. Các kho nền cố định của tiệm (`main_inv`, `showcase`, `front_box`, `back_box`, `counter_box`, `bazar_left`, `afterhour`, `hidden_stash`) được đánh dấu `HasId = false`.
   - Menu cấu hình F2 tự động lọc chỉ hiển thị các hòm đồ di động/thùng đồ thực sự của người chơi (`uniqueId > 0`), loại bỏ hoàn toàn các dòng kho hệ thống tĩnh gây rối mắt.
3. **Định danh hòm bền vững vĩnh viễn (Persistent Container Identification)**:
   - Sử dụng ID chuẩn của game: `item.uniqueId` kết hợp `item.identifier` (ví dụ `box_sec_box_1042`).
   - Cấu hình gán danh mục của người chơi lưu bền vững qua mọi phiên chơi, save/load hay sang ngày mới.
4. **Kiểm tra tính tương thích an toàn trước khi chuyển đồ (`CanContainerAcceptItem`)**:
   - Tích hợp kiểm tra `destInv.mayInventoryAddItemFunc(item, destInv)` và `IsInsertLocked()`, tránh nhét sai loại đồ vào hòm chuyên dụng.
5. **Hiển thị trực quan vị trí hòm trong Menu F2**:
   - Hiển thị rõ nguồn gốc vị trí hòm: `[Kho Chính]`, `[Hòm Sau]`, `[Kệ Kính]`, `[Đang Mở]` kèm mã số `#UID` để phân biệt các hòm cùng loại.

*Cập nhật tự động: v1.3.1 | 2026-11-19 11:18*
