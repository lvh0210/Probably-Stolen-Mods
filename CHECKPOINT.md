# Bàn Giao Tiến Độ Kỹ Thuật (Checkpoint) - Hoàn Tất 100%

- **Dự án**: Bộ Mod & Việt Hóa game Probably Stolen (Bản IMod Native)
- **Thư mục lưu trữ**: `D:\VibeCode\Probably Stolen`
- **Kho lưu trữ GitHub**: [https://github.com/lvh0210/Probably-Stolen-Mods](https://github.com/lvh0210/Probably-Stolen-Mods)
- **Trạng thái**: Hoàn tất 100% cơ chế chuyển trang đơn cửa sổ (Single-Window Page Switching) không còn chồng đè giao diện và tích hợp 3 bộ lọc Harmony triệt tiêu hoàn toàn hiện tượng nhấp chuột xuyên menu vào bàn chơi (`SmartInventorySort` v1.4.1).

## Các Tối Ưu Hóa & Cải Tiến Cốt Lõi (`SmartInventorySort` v1.4.1):
1. **Loại bỏ triệt để hiện tượng cửa sổ chồng đè (Single-Window Page Switching)**:
   - Loại bỏ hoàn toàn việc tạo cửa sổ con (`GUI.Window`) thứ hai độc lập gây tình trạng 2 lớp cửa sổ trong suốt đè lên nhau khiến chữ dính vào nhau và phải kéo rê thủ công.
   - Chuyển sang cơ chế chuyển trang nội bộ (Inline Page View) ngay trong khung cửa sổ chính (`windowRect` 860x640):
     - Khi bấm **[🎯 Chọn Tag ▾]**, giao diện lập tức chuyển mượt sang trang chọn danh mục dạng lưới 2 cột.
     - Sau khi click chọn 1 danh mục, hệ thống tự động lưu cấu hình và quay trở lại danh sách hòm đồ ngay lập tức.
     - Trang cấu hình có sẵn nút điều hướng lớn `[⬅ QUAY LẠI DANH SÁCH HÒM (Hủy Bỏ / Giữ Nguyên)]` để quay về bất kỳ lúc nào mà không cần kéo chuột.
2. **Triệt tiêu hoàn toàn sự cố click xuyên chuột vào bàn chơi (Anti-Click-Through Patches)**:
   - Tích hợp hàm kiểm tra tọa độ con trỏ chuột chính xác `SmartCategorySortController.IsPointerOverUI()` dựa trên chuyển đổi hệ trục tọa độ IMGUI (`mouseGui.y = Screen.height - Input.mousePosition.y`).
   - Bổ sung 3 bộ lọc Harmony prefix:
     - `CustomEventHandler.UpdateRayCast`: Vô hiệu hóa và xóa sạch kết quả dò tia (`___hasRayCast = false`, `___rayCastResult = null`) khi trỏ chuột nằm trong phạm vi menu.
     - `StoreUIManager.CheckRaycast`: Chặn việc kích hoạt tương tác với vật phẩm hoặc bàn chơi của cửa hàng phía dưới menu.
     - `ItemMouseDragHandler.StartDrag`: Chặn sự kiện bắt đầu kéo thả vật phẩm trong game khi đang thao tác nhấp chuột trên bảng menu.
3. **Bao quát 100% 21 Danh Mục Tag Vật Phẩm**:
   - Duy trì hệ thống 21 nhóm danh mục trích xuất trực tiếp từ mã nguồn và bộ dữ liệu game (Súng, Đạn dược, Y tế, Thực phẩm, Rượu bia, Thuốc lá/Hàng cấm, Áo giáp, Vật liệu chế tạo, v.v.).
4. **Nén khít chặt tự động xoay 90° & bảo vệ kho khóa**:
   - Duy trì Harmony prefix `ForceSortAutoRotatePatch` tự động xoay item ngang/dọc nén khít chặt không còn khe hở.
   - Duy trì cơ chế khóa an toàn, bảo vệ hòm khóa và kho ẩn khỏi việc bị di chuyển vật phẩm ngoài ý muốn.
5. **Đồng bộ kê khai quyền hạn (Manifest Declaration)**:
   - Cập nhật phiên bản mod lên `1.4.1` trong cả mã nguồn `SmartInventorySortMod.cs` và tệp kê khai `manifest.xml`.

*Cập nhật tự động: v1.4.1 | 2026-11-19 15:56*
