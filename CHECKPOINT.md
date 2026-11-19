# Bàn Giao Tiến Độ Kỹ Thuật (Checkpoint) - Hoàn Tất 100%

- **Dự án**: Bộ Mod & Việt Hóa game Probably Stolen (Bản IMod Native)
- **Thư mục lưu trữ**: `D:\VibeCode\Probably Stolen`
- **Kho lưu trữ GitHub**: [https://github.com/lvh0210/Probably-Stolen-Mods](https://github.com/lvh0210/Probably-Stolen-Mods)
- **Trạng thái**: Hoàn tất 100% việc loại bỏ các gói/hộp chứa chuyên biệt 1 vật phẩm (túi hạt giống, hộp viên lọc nước, hộp đạn, két bia...) khỏi danh sách kho Menu F2, chỉ giữ lại các hòm chứa thực thụ (`SmartInventorySort` v1.5.3).

## Các Tối Ưu Hóa & Cải Tiến Cốt Lõi (`SmartInventorySort` v1.5.3):
1. **Bộ Lọc Hòm Chứa Thực Thụ (`IsGeneralStorageContainer`)**:
   - *Nguyên nhân trước đây*: Một số vật phẩm dạng gói hoặc hộp đựng đơn mục đích trong game (như `nutrifruit_seed_packet`, `water_purification_tablet_container`, `ammo_box`, `water_test_kit`, `beer_case`) sở hữu một lưới ô chứa bên trong (`GameGridInventory`), nhưng engine game giới hạn ngặt nghèo hàm `mayInventoryAddItemFunc` chỉ cho phép nhét đúng 1 loại vật phẩm duy nhất tương ứng (chỉ đựng hạt giống, chỉ đựng viên lọc nước, chỉ đựng đạn...). Do đó, chúng bị liệt kê nhầm vào danh sách hòm cấu hình F2 khiến người dùng khó quản lý.
   - *Giải pháp v1.5.3*:
     - Nhận diện và loại trừ triệt để các tag gói chuyên biệt: `PRE_CONTAINER_TAG`, `STORABLE_PACKAGE_TAG`, `AMMO_BOX_TAG`, `WINE_CONTAINER_TAG`, `CASSETTE_PLAYER`, `CAGE_TAG`.
     - Loại trừ theo định danh vật phẩm: `seed_packet`, `seed_pack`, `gói hạt giống`, `túi hạt giống`, `water_tablet`, `tablet_container`, `nutrient_tablet`, `test_kit`, `viên lọc`, `que thử`, `ammo_box`, `ammo_case`, `beer_case`, `cigarette_pack`, `cigar_case`, `cassette_player`.
     - Xác thực các thùng chứa đa năng thực thụ qua tag: `CONTAINER_TAG`, `BACKPACK_TAG`, `POUCH_TAG`, `STORAGE`, `SAFE_TAG`, `SUPPLY_CRATE`, `TOOLBOX_TAG`, `SMUGGLER_BAY` hoặc từ khóa ba lô, két sắt, rương, hòm đồ, hốc giấu, kho nền tiệm.
2. **Tích Hợp Nhất Quán Tại 2 Điểm Then Chốt**:
   - *Menu Quản Lý Kho F2 (`GetAvailableInventories`)*: Toàn bộ gói hạt giống, hộp viên lọc, hộp que thử, hộp đạn... bị ẩn hoàn toàn khỏi danh sách hòm, trả lại giao diện gọn gàng chỉ gồm ba lô, rương, két, hòm tiếp tế và kho nền tiệm.
   - *Luồng Phân Loại F3 (`ExecuteCategorySort`)*: Các gói hạt giống và hộp viên lọc nước được đối xử chuẩn xác như vật phẩm hàng hóa thông thường (tự động gom về hòm Hạt Giống hoặc Nước/Lọc Nước), trong khi các hòm chứa thực thụ luôn được bảo vệ không bị chuyển nhầm thành vật phẩm lẻ.
3. **Đồng Bộ Phiên Bản & Đóng Gói**:
   - Cập nhật phiên bản lên `1.5.3` trong `SmartInventorySortMod.cs` và `manifest.xml`.
   - Biên dịch thành công `SmartInventorySort.dll` (v1.5.3) và triển khai vào `D:\GameCrack\Probably Stolen\Mods\SmartInventorySort\`.

*Cập nhật tự động: v1.5.3 | 2026-11-20 00:18*
