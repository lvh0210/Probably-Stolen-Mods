# Bàn Giao Tiến Độ Kỹ Thuật (Checkpoint) - Hoàn Tất 100%

- **Dự án**: Bộ Mod & Việt Hóa game Probably Stolen (Bản IMod Native)
- **Thư mục lưu trữ**: `D:\VibeCode\Probably Stolen`
- **Kho lưu trữ GitHub**: [https://github.com/lvh0210/Probably-Stolen-Mods](https://github.com/lvh0210/Probably-Stolen-Mods)
- **Trạng thái**: Hoàn tất 100% cơ chế 1 hòm chọn nhiều Tag (Multi-Tag Selection), tách biệt 26 Tag độc lập nguyên bản, hệ thống thứ tự ưu tiên hòm (Container Priority) chống nhảy đồ qua lại và 11 cổng Harmony triệt tiêu hoàn toàn sự cố click xuyên chuột (`SmartInventorySort` v1.5.0).

## Các Tối Ưu Hóa & Cải Tiến Cốt Lõi (`SmartInventorySort` v1.5.0):
1. **Cơ chế 1 Hòm chọn Nhiều Tag (Multi-Tag Selection)**:
   - Thay thế việc chỉ gán 1 danh mục duy nhất bằng cơ chế cho phép tích chọn linh hoạt 1 hoặc nhiều Tag cùng lúc cho từng hòm đồ.
   - Giao diện chọn Tag hiển thị nút toggle có dấu tick trực quan (`[✔] Tag Name` vs `[  ] Tag Name`).
   - Bổ sung các công cụ thao tác 1 chạm: `[✅ Chọn Tất Cả (26 Tag)]`, `[❌ Bỏ Chọn Hết]`, `[🤖 Đặt Về Tự Động Theo Hòm]`, `[⛔ Đặt Khóa Hòm]`.
   - Trên danh sách hòm đồ ở trang chính, hiển thị tóm tắt thông minh: `🎯 3 Tag: Súng, Đạn, Giáp`.
2. **Tách biệt 26 Tag Đơn Lẻ Nguyên Bản (Không gộp nhóm)**:
   - Bóc tách toàn diện các Tag độc lập: Súng, Cận chiến, Đạn dược, Thuốc nổ & C4, Phụ kiện súng, Y tế & Thuốc, Thực phẩm, Nước uống, Lọc nước, Rượu bia, Thuốc lá, Hàng cấm/Độc dược, Giáp & Đồ bảo hộ, Vật liệu chế tạo, Dụng cụ kỹ thuật, Tài liệu & Thẻ từ, Vé số & Vé cào, Nội tạng sinh học, Mô-đun & Chip, Pin & Năng lượng, Hạt giống & Nông sản, Đồ gia dụng, Nghệ thuật, Hòm chứa, Phế liệu, Kho chung.
3. **Hệ thống Thứ Tự Ưu Tiên Hòm (Container Priority System)**:
   - Cung cấp trường mức ưu tiên riêng cho từng hòm (`⭐ Ưu Tiên: 1`, 2, 3...) kèm nút tăng giảm nhanh `[▲]` / `[▼]` (Mức 1 là ưu tiên cao nhất).
   - Thuật toán phân phối F3 hoạt động theo nguyên tắc đơn điệu ổn định tuyệt đối (Stability Rule):
     - Sắp xếp các hòm đích theo mức ưu tiên (1 trước, rồi 2, 3...).
     - Nếu vật phẩm mang nhiều Tag và đã nằm trong một hòm hợp lệ: Hệ thống **GIỮ NGUYÊN VẬT PHẨM TẠI CHỖ**, chỉ chuyển đi nếu tồn tại một hòm có độ ưu tiên cao hơn hẳn (`destPriority < currentPriority`) và còn chỗ trống.
     - **Triệt tiêu 100% hiện tượng vật phẩm bị nhảy qua nhảy lại giữa các hòm trùng Tag** sau mỗi lần nhấn F3.
4. **Bảo vệ toàn diện 11 Cổng chống Click Xuyên Chuột (Anti-Click-Through Patches)**:
   - Thiết lập vùng nhận diện con trỏ chuột chính xác `IsPointerOverUI()` kèm viền đệm bảo vệ 5px.
   - Tích hợp đồng thời 11 bộ lọc Harmony Prefix chặn toàn bộ sự kiện click, kéo thả và raycast trong game khi con trỏ chuột nằm trên menu F2:
     - `InputActionHandler.UpdatePress` & `UpdateRelease`
     - `ItemMouseHandler.OnEventPress`
     - `ItemMouseDoubleClickHandler.OnEventPress`, `ClickAction`, `DoubleClickAction`
     - `UIDragHandler.OnPointerDown`, `OnBeginDrag`
     - `ItemMouseDragHandler.StartDrag`
     - `StoreUIManager.CheckRaycast`
     - `CustomEventHandler.UpdateRayCast`
5. **Nén khít chặt tự động xoay 90° & Khóa an toàn**:
   - Duy trì Harmony prefix `ForceSortAutoRotatePatch` tự động xoay item ngang/dọc nén khít chặt không còn khe hở.
   - Duy trì cơ chế khóa an toàn, bảo vệ hòm khóa và kho ẩn khỏi việc bị di chuyển vật phẩm ngoài ý muốn.
6. **Đồng bộ kê khai quyền hạn (Manifest Declaration)**:
   - Cập nhật phiên bản mod lên `1.5.0` trong cả mã nguồn `SmartInventorySortMod.cs` và tệp kê khai `manifest.xml`.

*Cập nhật tự động: v1.5.0 | 2026-11-19 22:51*
