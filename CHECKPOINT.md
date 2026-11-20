# Bàn Giao Tiến Độ Kỹ Thuật (Checkpoint) - Hoàn Tất 100%

- **Dự án**: Bộ Mod & Việt Hóa game Probably Stolen (Bản IMod Native)
- **Thư mục lưu trữ**: `D:\VibeCode\Probably Stolen`
- **Kho lưu trữ GitHub**: [https://github.com/lvh0210/Probably-Stolen-Mods](https://github.com/lvh0210/Probably-Stolen-Mods)
- **Trạng thái**: Hoàn tất 100% việc hỗ trợ hòm nâng cấp nhiều Tab (`MachineryTabs`), quét đồ trong hòm lồng nhau (`FindAllChildItems`), và chuẩn hóa 100% theo hệ thống Tag gốc của game (`SmartInventorySort` v1.6.0).

## Các Tối Ưu Hóa & Cải Tiến Cốt Lõi (`SmartInventorySort` v1.6.0):
1. **Hỗ Trợ Toàn Diện Hòm Nâng Cấp Nhiều Tab (`MachineryTabs`)**:
   - *Nguyên nhân trước đây*: Khi hòm chứa đồ được nâng cấp từ 1 tab lên 2 tab (hoặc nhiều tab), cửa sổ `PixelWindow` chuyển sang sử dụng cấu trúc container phân trang `MachineryTabs`. Hàm đọc kho cũ chỉ lấy phần tử đơn lẻ đầu tiên nên không phát hiện ra các tab mới, khiến hòm biến mất khỏi menu F2 và F3 không thể thao tác.
   - *Giải pháp v1.6.0*:
     - Tận dụng trực tiếp API gốc của game: Gọi `GameItem.FindContentTabs()` và `MachineryTabs.CollectStorage()`.
     - Phân giải đầy đủ từng tab inventory (`GameGridInventory`) con của hòm đồ.
     - Hiển thị độc lập từng Tab trên Menu F2 với định danh rõ ràng: `[Tên hòm - Tab 1/2]`, `[Tên hòm - Tab 2/2]`, giúp người dùng cấu hình mức ưu tiên và tag nhận đồ riêng cho từng tab mượt mà.

2. **Gom Đồ Trong Hòm Lồng Nhau (Nested Containers Extraction)**:
   - *Nguyên nhân trước đây*: Khi người chơi cất các hòm/ba lô nhỏ vào trong một kho hòm mở rộng lớn, luồng phân loại F3 trước đây chỉ duyệt các món đồ cấp 1, bỏ qua hoàn toàn các món đồ nằm sâu bên trong hòm con.
   - *Giải pháp v1.6.0*:
     - Tận dụng phương thức duyệt đệ quy có sẵn của game: `GameItem.FindAllChildItems()`.
     - Khi bấm F3, hệ thống tự động quét và lôi toàn bộ các vật phẩm hàng hóa hợp lệ nằm sâu trong các tầng hòm con ra để phân phối vào các hòm đích theo thứ tự ưu tiên.
     - Giữ nguyên vị trí của bản thân chiếc hòm con (`IsGeneralStorageContainer`), chỉ rút các vật phẩm hàng hóa bên trong bằng `(item.parentInventory as GameGridInventory).Expel(item)`.

3. **Chuẩn Hóa 100% Theo Hệ Thống Tag Gốc Của Game (Không Dùng Cô Lập Thủ Công)**:
   - *Nguyên nhân trước đây*: Các chuỗi nhận diện mở rộng và cơ chế lọc ép kiểu trước đây vô tình làm dính từ khóa liên quan đến nước/viên lọc vào nhóm thực phẩm.
   - *Giải pháp v1.6.0*:
     - Loại bỏ hoàn toàn các khối quy tắc cô lập thủ công (Safety Shield) và các chuỗi đoán mò.
     - Dựa 100% vào nguồn dữ liệu chuẩn mực từ engine: `item.IsTag(...)`, `item.itemTypes`, `item.GetGameItemType()`.
     - Nhóm đồ Lọc Nước (`WATER_PURIFICATION_SUPPLY`, `WATER_PURIFICATION_TABLET`, `FILTER`, `WATER_FILTER`) được phân định độc lập tuyệt đối với nhóm Thực Phẩm (`FOOD`, `PROCESSED_FOOD`), triệt tiêu 100% hiện tượng lọc nước bị rơi vào hòm thực phẩm.

4. **Đồng Bộ Phiên Bản & Đóng Gói**:
   - Cập nhật phiên bản lên `1.6.0` trong `SmartInventorySortMod.cs` và `manifest.xml`.
   - Biên dịch thành công `SmartInventorySort.dll` (v1.6.0) không lỗi và triển khai vào `D:\GameCrack\Probably Stolen\Mods\SmartInventorySort\`.

*Cập nhật tự động: v1.6.0 | 2026-11-20 14:20*
