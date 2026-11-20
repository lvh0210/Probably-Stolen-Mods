# Bàn Giao Tiến Độ Kỹ Thuật (Checkpoint) - Hoàn Tất 100%

- **Dự án**: Bộ Mod & Việt Hóa game Probably Stolen (Bản IMod Native)
- **Thư mục lưu trữ**: `D:\VibeCode\Probably Stolen`
- **Kho lưu trữ GitHub**: [https://github.com/lvh0210/Probably-Stolen-Mods](https://github.com/lvh0210/Probably-Stolen-Mods)
- **Trạng thái**: Hoàn tất 100% việc khắc phục lỗi Gói Hạt Giống nhảy nhầm vào Hòm Đang Mở và bảo toàn ID bền vững cho hòm đồ (`SmartInventorySort` v1.6.3).

## Các Tối Ưu Hóa & Cải Tiến Cốt Lõi (`SmartInventorySort` v1.6.3):
1. **Khắc Phục Lỗi Gói Hạt Giống Nhảy Nhầm Vào Hòm Đang Mở**:
   - **Loại bỏ `STORABLE_PACKAGE_TAG` khỏi danh mục Hòm Chứa (`CONTAINER`)**:
     - Trong engine game, các túi hạt giống (`nutrifruit_seed_packet`, `lifeweed_seed_packet`...) mang tag `STORABLE_PACKAGE_TAG`.
     - Ở các bản trước, tag này bị xếp nhầm vào nhóm `CONTAINER` ("📦 Hòm, Rương & Két"), khiến gói hạt giống bị gắn đồng thời 2 tag: `SEED` và `CONTAINER`.
     - Khi một hòm đang mở có tag `CONTAINER` (hoặc `GENERAL`), gói hạt giống bị hòm này tiếp nhận và kéo về thay vì vào hòm hạt giống.
     - Sau khi loại bỏ, gói hạt giống trở về đúng 100% bản chất là vật phẩm hạt giống nông nghiệp (`SEED`).
   - **Mở rộng nhận diện tag `SEED`**:
     - Bổ sung `SEED_TAG`, `SEED_ITEM_TAG`, `MODIFIED_SEED`, `FARMING_SUPPLIES`, `CROP`, `PLANT`, `FARMING`.
     - Thêm kiểm tra từ khóa định danh và tên gọi tiếng Việt (`hạt giống`, `gói hạt`, `túi hạt`, `seed`, `spore`) trong bộ fallback.
   - **Nâng cấp Master Isolation Shield**:
     - Bổ sung `tags.Remove("CONTAINER")` cho toàn bộ nhóm vật phẩm tiêu thụ: `SEED`, `FOOD`, `MEDICAL`, `DRINK`, `ALCOHOL`.
     - Đảm bảo tuyệt đối không có bất kỳ hạt giống, đồ ăn, thuốc hay nước uống nào bị gắn tag hòm chứa lưu trữ.

2. **Bảo Toàn ID Bền Vững Cho Hòm Đang Mở (Persistent ID Integrity)**:
   - **Nguyên nhân**: Khi người chơi mở một hòm đồ trên màn hình, cửa sổ `PixelWindow` trong `WindowsHandler.current.visibleWindows` không chứa tham chiếu `parentItems`, khiến hàm sinh ID tạo ra ID băm tạm thời (`box_hash_...`) và đè lên ID cố định của hòm trong kho tiệm (`box_..._uniqueId`). Hệ quả là cấu hình tag người chơi đã cài đặt (như tag Hạt giống) bị tạm thời mất hiệu lực trong lúc mở hòm và quay về tag mặc định.
   - **Giải pháp**: Tái cấu trúc hàm `GetAvailableInventories()` để quét toàn bộ container cố định trong kho tiệm trước tiên, giữ nguyên ID gốc kèm mọi cấu hình tag/mức ưu tiên đã lưu. Trạng thái mở của hòm được nhận diện trực tiếp qua `it.contentWindow.IsVisible()` để hiển thị huy hiệu `🔓 [Đang Mở]` mà không làm thay đổi ID hay cấu hình của hòm.

3. **Đóng Gói & Triển Khai Thực Tế**:
   - Nâng cấp phiên bản lên `v1.6.3` trong `SmartInventorySortMod.cs` và `manifest.xml`.
   - Biên dịch thành công thư viện `SmartInventorySort.dll` (v1.6.3) với `ExitCode: 0`.
   - Sao chép trực tiếp vào thư mục cài đặt game: `D:\GameCrack\Probably Stolen\Mods\SmartInventorySort\`.

*Cập nhật tự động: v1.6.3 | 2026-11-20 20:12*
