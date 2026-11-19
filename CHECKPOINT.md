# Bàn Giao Tiến Độ Kỹ Thuật (Checkpoint) - Hoàn Tất 100%

- **Dự án**: Bộ Mod & Việt Hóa game Probably Stolen (Bản IMod Native)
- **Thư mục lưu trữ**: `D:\VibeCode\Probably Stolen`
- **Kho lưu trữ GitHub**: [https://github.com/lvh0210/Probably-Stolen-Mods](https://github.com/lvh0210/Probably-Stolen-Mods)
- **Trạng thái**: Hoàn tất 100% cơ chế phân loại đa Tag theo mức ưu tiên (Priority-Ranked Multi-Tag Sorting), khớp 1-Tag linh hoạt (không đòi hỏi chọn đủ mọi tag của item) và gom đồ từ bàn thao tác vào hòm riêng (`SmartInventorySort` v1.5.1).

## Các Tối Ưu Hóa & Cải Tiến Cốt Lõi (`SmartInventorySort` v1.5.1):
1. **Khớp 1-Tag Linh Hoạt (Flexible Single-Tag Match)**:
   - Sửa lỗi item không được sắp xếp khi hòm không chọn đủ toàn bộ tag của item.
   - Giờ đây, hòm chỉ cần khớp **ít nhất 1 Tag** với món đồ là đủ điều kiện nhận đồ (`itemTags.Overlaps(SelectedTags)`). Người chơi không cần phải tìm và tích đủ tất cả các tag của món đồ.
2. **Quy Tắc Ưu Tiên Tuyệt Đối Khi Item Có Nhiều Tag (Multi-Tag Priority Resolution)**:
   - Khi một món đồ mang nhiều Tag (ví dụ: vừa có tag Súng vừa có tag Hàng Cấm):
     - Hệ thống quét tất cả các hòm có chứa ít nhất 1 Tag của món đồ đó.
     - Sắp xếp các hòm khớp theo **mức ưu tiên (Priority)**: Hòm có Priority 1 được xét trước, tiếp theo là 2, 3...
     - Món đồ sẽ luôn được **ưu tiên xếp vào hòm có mức ưu tiên cao nhất**.
     - Nếu hòm ưu tiên cao nhất đã đầy, món đồ sẽ tự động lùi xuống hòm ưu tiên tiếp theo có khớp tag.
     - Nếu món đồ đã ở trong hòm ưu tiên cao nhất, hệ thống giữ nguyên vị trí ổn định, hoàn toàn không bị nhảy qua lại.
3. **Phân Định Mức Ưu Tiên Mặc Định Thông Minh (Auto Hierarchy)**:
   - Các hòm đồ riêng (`HasId == true`: Hòm an ninh, két sắt, hộp sơ cứu, hộp cơm...): Mặc định `Priority = 1` để người chơi tùy chỉnh.
   - Bàn thao tác và kho nền tiệm (`HasId == false`: `main_inv`, kệ kính, hòm sau...): Mặc định `Priority = 99` (Kho đệm/xếp sau).
   - Nhờ vậy, khi nhấn F3, tất cả đồ đạc bày trên bàn thao tác sẽ được tự động gom gọn và phân phối chính xác vào các hòm riêng theo mức ưu tiên.
4. **Mở Rộng Nhận Diện Tag Toàn Diện**:
   - Quét sâu cả `itemTypes`, `itemFeatures`, `state.dict`, `IsTag()` và so khớp từ khóa không phân biệt hoa thường.
   - Bao quát 26 Tag đơn lẻ nguyên bản không gộp nhóm.
5. **Chống Click Xuyên Chuột & Nén Khít Chặt 90°**:
   - Duy trì toàn vẹn 11 cổng bảo vệ Harmony Prefix chống click chuột xuyên qua menu vào game.
   - Duy trì tự động xoay ngang/dọc 90° lấp khít toàn bộ khoảng trống trong kho.
6. **Đồng Bộ Phiên Bản**:
   - Cập nhật phiên bản lên `1.5.1` trong `SmartInventorySortMod.cs` và `manifest.xml`.

*Cập nhật tự động: v1.5.1 | 2026-11-19 23:11*
