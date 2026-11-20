# Bàn Giao Tiến Độ Kỹ Thuật (Checkpoint) - Hoàn Tất 100%

- **Dự án**: Bộ Mod & Phân Loại Kho game Probably Stolen (Bản IMod Native)
- **Thư mục lưu trữ**: `D:\VibeCode\Probably Stolen`
- **Kho lưu trữ GitHub**: [https://github.com/lvh0210/Probably-Stolen-Mods](https://github.com/lvh0210/Probably-Stolen-Mods)
- **Trạng thái**: Hoàn tất 100% việc hỗ trợ hệ thống Giấy phép (Permits) cuối game, tách biệt hạng cấm RESTRICTED khỏi tag CONTRABAND (`SmartInventorySort` v1.6.4).

## Các Tối Ưu Hóa & Cải Tiến Cốt Lõi (`SmartInventorySort` v1.6.4):
1. **Tách Biệt Hạng Cấm (RESTRICTED Classes) Khỏi Tag Hàng Cấm (CONTRABAND) Khi Đã Hợp Pháp Hóa**:
   - **Bản chất cơ chế trong game engine**:
     - Khi người chơi mua Giấy phép (Permit) cho súng, vật tư y tế, hóa chất ở giai đoạn cuối game, phương thức `PermitHelper.GrantedFromPermit()` kích hoạt:
       + Gán cấp độ hạn chế kiểm định (`SetRestrictedClass`: `RESTRICTED_1`, `RESTRICTED_2`, `RESTRICTED_3` tương ứng Cấm cấp I, Cấm cấp II, Cấm cấp III).
       + Kích hoạt `RemoveContrabandStatus()`, xóa bỏ hoàn toàn `CONTRABAND` và tắt cờ `CONTRABAND_ITEM_TAG`. Trên giao diện game, nhãn cấm màu đỏ biến mất.
     - Trong phiên bản cũ của mod, hàm phân loại tự động gộp các tag `RESTRICTED_1/2/3` chung vào danh mục `CONTRABAND`. Dẫn đến việc súng đã có giấy phép (như `Sentinel Mk. III.`) vẫn bị mod ép nhận tag `CONTRABAND`, khiến lệnh gom đồ F3 kéo nhầm vào hòm hàng cấm thay vì hòm súng.
   - **Giải pháp xử lý triệt để**:
     - Loại bỏ việc tự động gán `CONTRABAND` cho `RESTRICTED_1`, `RESTRICTED_2`, `RESTRICTED_3` trong cả 2 hàm `GetItemTagIds` và `AddMatchingTagIds`. Tag `CONTRABAND` chỉ được gán khi vật phẩm thực sự là hàng cấm chưa có giấy phép (`CONTRABAND_ITEM_TAG`, `CONTRABAND`, `TRAFFICKING`, `POISON`).
     - Tích hợp lớp kiểm định Permit & Restricted Verification vào Master Isolation Shield: Nếu vật phẩm mang cấp độ cấm `RESTRICTED_1/2/3` nhưng không còn cờ `CONTRABAND_ITEM_TAG` hay `CONTRABAND` của game, tự động thanh lọc và giải phóng hoàn toàn tag `CONTRABAND`.
     - Giúp toàn bộ vũ khí, đạn dược, hóa chất và vật tư y tế đã có giấy phép đi thẳng 100% vào các hòm chuyên biệt tương ứng (`GUN`, `AMMO`, `CHEMICAL`, `MEDICAL`).

2. **Kế Thừa Toàn Bộ Tính Năng Ổn Định v1.6.3**:
   - Khắc phục lỗi gói hạt giống (`nutrifruit_seed_packet`...) nhảy nhầm vào hòm đang mở qua việc tách biệt `STORABLE_PACKAGE_TAG` khỏi `CONTAINER`.
   - Bảo toàn ID bền vững (`box_..._uniqueId`) cho hòm đang mở, quét container kho tiệm trước tiên, chống reset cấu hình hòm.
   - Master Isolation Shield ngăn ngừa chéo hóa chất với thực phẩm/y tế/nước uống.
   - Nhận diện 100% Tab nâng cấp của game (`MachineryTabs`), quét đệ quy hòm lồng trong hòm (`FindAllChildItems`).

3. **Đóng Gói & Triển Khai Thực Tế**:
   - Nâng cấp phiên bản lên `v1.6.4` trong `SmartInventorySortMod.cs` và `manifest.xml`.
   - Biên dịch thành công thư viện `SmartInventorySort.dll` (v1.6.4) với `ExitCode: 0`.
   - Triển khai tệp thư viện và manifest trực tiếp vào thư mục mod của game.

*Cập nhật tự động: v1.6.4 | 2026-11-20 21:40*
