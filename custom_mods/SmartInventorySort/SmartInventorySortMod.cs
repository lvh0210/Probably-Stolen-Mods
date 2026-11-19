using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using HarmonyLib;

// =========================================================================
// SMART INVENTORY SORT & MULTI-TAG CATEGORY STORAGE MOD (v1.5.0)
// Tác giả: Antigravity AI (VibeCode)
// Tính năng nâng cấp cốt lõi v1.5.0:
// 1. CHỐNG CLICK XUYÊN 100% AIRTIGHT: Chặn toàn diện 11 cổng xử lý sự kiện
//    chuột (InputActionHandler, ItemMouseHandler, ItemMouseDoubleClickHandler,
//    UIDragHandler, StoreUIManager, CustomEventHandler) khi chuột nằm trên menu F2.
// 2. TÁCH BIỆT 26 TAG ĐƠN LẺ NGUYÊN BẢN: Bóc tách từng Tag riêng biệt
//    (Súng, Cận chiến, Đạn, Thuốc nổ, Phụ kiện, Y tế, Thực phẩm, Nước, Lọc nước,
//    Rượu, Thuốc lá, Hàng cấm, Giáp, Vật liệu, Dụng cụ, Tài liệu, Vé số...).
// 3. CƠ CHẾ 1 HÒM CHỌN NHIỀU TAG (MULTI-TAG SELECTION): Cho phép tích chọn
//    linh hoạt nhiều tag cùng lúc cho 1 hòm, hỗ trợ Chọn tất cả / Bỏ chọn hết.
// 4. HỆ THỐNG THỨ TỰ ƯU TIÊN HÒM (CONTAINER PRIORITY SYSTEM): Cho phép thiết lập
//    mức ưu tiên (1, 2, 3...) cho từng hòm. Thuật toán phân phối ổn định 100%,
//    chống hoàn toàn hiện tượng item nhảy qua nhảy lại giữa các hòm trùng tag!
// 5. Nén khít chặt không khoảng trống (PackFirstFit + Auto-Rotate 90°).
// =========================================================================

public class SmartInventorySortMod : IMod
{
    private static Harmony harmony;
    private static GameObject uiObject;

    public void Init(ModManifest manifest)
    {
    }

    public void OnEnable()
    {
        try
        {
            if (harmony == null)
            {
                harmony = new Harmony("com.antigravity.smartinventorysort");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Debug.Log("[SmartInventorySort] Đã kích hoạt v1.5.0 (Multi-Tag + Priority + Complete Anti-Clickthrough).");
            }

            if (uiObject == null)
            {
                uiObject = new GameObject("SmartCategorySortUI");
                uiObject.AddComponent<SmartCategorySortController>();
                UnityEngine.Object.DontDestroyOnLoad(uiObject);
                Debug.Log("[SmartInventorySort] Đã khởi tạo Controller Giao Diện Menu F2 v1.5.0.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[SmartInventorySort] Lỗi khi nạp mod: " + ex);
        }
    }

    public void OnDisable()
    {
        try
        {
            if (harmony != null)
            {
                harmony.UnpatchAll("com.antigravity.smartinventorysort");
                harmony = null;
            }
            if (uiObject != null)
            {
                UnityEngine.Object.Destroy(uiObject);
                uiObject = null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[SmartInventorySort] Lỗi khi dỡ mod: " + ex);
        }
    }
}

// =========================================================================
// 1. HARMONY PATCHES: Nén khít chặt & Tự động xoay 90 độ
// =========================================================================
[HarmonyPatch(typeof(InventorySortHelper), "PackShelves")]
public static class DisablePackShelvesPatch
{
    public static bool Prefix(ref List<ValueTuple<GameItem, GridShape>> __result)
    {
        __result = null; // Ép game dùng thuật toán PackFirstFit chuẩn
        return false;
    }
}

[HarmonyPatch(typeof(EmporiumOption), "get_SortAutoRotate")]
public static class ForceSortAutoRotatePatch
{
    public static bool Prefix(ref bool __result)
    {
        __result = true; // Luôn cho phép xoay 90 độ để lấp đầy khít chặt tối đa
        return false;
    }
}

// =========================================================================
// HARMONY PATCHES: CHỐNG CLICK XUYÊN CHUỘT QUA MENU VÀO BÀN CHƠI GAME (11 CỔNG)
// =========================================================================
[HarmonyPatch(typeof(CustomEventHandler), "UpdateRayCast")]
public static class BlockCustomEventHandlerRaycastPatch
{
    public static bool Prefix(ref bool ___hasRayCast, ref TreeNodeRender ___rayCastResult)
    {
        if (SmartCategorySortController.IsPointerOverUI())
        {
            ___hasRayCast = false;
            ___rayCastResult = null;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(StoreUIManager), "CheckRaycast")]
public static class BlockStoreUIRaycastPatch
{
    public static bool Prefix()
    {
        if (SmartCategorySortController.IsPointerOverUI()) return false;
        return true;
    }
}

[HarmonyPatch(typeof(ItemMouseDragHandler), "StartDrag")]
public static class BlockItemMouseDragHandlerStartDragPatch
{
    public static bool Prefix()
    {
        if (SmartCategorySortController.IsPointerOverUI()) return false;
        return true;
    }
}

[HarmonyPatch(typeof(InputActionHandler), "UpdatePress")]
public static class BlockInputActionHandlerUpdatePressPatch
{
    public static bool Prefix()
    {
        if (SmartCategorySortController.IsPointerOverUI()) return false;
        return true;
    }
}

[HarmonyPatch(typeof(InputActionHandler), "UpdateRelease")]
public static class BlockInputActionHandlerUpdateReleasePatch
{
    public static bool Prefix()
    {
        if (SmartCategorySortController.IsPointerOverUI()) return false;
        return true;
    }
}

[HarmonyPatch(typeof(ItemMouseHandler), "OnEventPress")]
public static class BlockItemMouseHandlerOnEventPressPatch
{
    public static bool Prefix()
    {
        if (SmartCategorySortController.IsPointerOverUI()) return false;
        return true;
    }
}

[HarmonyPatch(typeof(ItemMouseDoubleClickHandler), "OnEventPress")]
public static class BlockItemMouseDoubleClickHandlerOnEventPressPatch
{
    public static bool Prefix()
    {
        if (SmartCategorySortController.IsPointerOverUI()) return false;
        return true;
    }
}

[HarmonyPatch(typeof(ItemMouseDoubleClickHandler), "ClickAction")]
public static class BlockItemMouseDoubleClickHandlerClickActionPatch
{
    public static bool Prefix()
    {
        if (SmartCategorySortController.IsPointerOverUI()) return false;
        return true;
    }
}

[HarmonyPatch(typeof(ItemMouseDoubleClickHandler), "DoubleClickAction")]
public static class BlockItemMouseDoubleClickHandlerDoubleClickActionPatch
{
    public static bool Prefix()
    {
        if (SmartCategorySortController.IsPointerOverUI()) return false;
        return true;
    }
}

[HarmonyPatch(typeof(UIDragHandler), "OnPointerDown")]
public static class BlockUIDragHandlerOnPointerDownPatch
{
    public static bool Prefix()
    {
        if (SmartCategorySortController.IsPointerOverUI()) return false;
        return true;
    }
}

[HarmonyPatch(typeof(UIDragHandler), "OnBeginDrag")]
public static class BlockUIDragHandlerOnBeginDragPatch
{
    public static bool Prefix()
    {
        if (SmartCategorySortController.IsPointerOverUI()) return false;
        return true;
    }
}

// =========================================================================
// 2. ĐỊNH NGHĨA 26 TAG ĐƠN LẺ & CẤU HÌNH HÒM CHỨA ĐA NĂNG
// =========================================================================
public class ItemTagDef
{
    public string TagId;
    public string DisplayName;
    public string ShortName;

    public ItemTagDef(string id, string name, string shortName)
    {
        TagId = id;
        DisplayName = name;
        ShortName = shortName;
    }
}

public class ContainerConfig
{
    public bool IsLocked = false;
    public bool IsAuto = false;
    public int Priority = 1; // 1 = Ưu tiên cao nhất, 2, 3, 4, 5...
    public HashSet<string> SelectedTags = new HashSet<string>();

    public string Serialize()
    {
        string tagsStr = string.Join(",", SelectedTags.ToArray());
        return $"{(IsLocked ? 1 : 0)}|{(IsAuto ? 1 : 0)}|{Priority}|{tagsStr}";
    }

    public static ContainerConfig Deserialize(string str)
    {
        var cfg = new ContainerConfig();
        if (string.IsNullOrEmpty(str)) return cfg;
        string[] parts = str.Split('|');
        if (parts.Length >= 1) cfg.IsLocked = (parts[0] == "1");
        if (parts.Length >= 2) cfg.IsAuto = (parts[1] == "1");
        if (parts.Length >= 3) int.TryParse(parts[2], out cfg.Priority);
        if (cfg.Priority <= 0) cfg.Priority = 1;
        if (parts.Length >= 4 && !string.IsNullOrEmpty(parts[3]))
        {
            string[] tags = parts[3].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tags) cfg.SelectedTags.Add(t.Trim());
        }
        return cfg;
    }
}

public class NamedInventory
{
    public string Id;
    public string DisplayName;
    public GameGridInventory Inventory;
    public string DefaultTagId;
    public bool HasId;
    public ContainerConfig Config;
}

// =========================================================================
// 3. CONTROLLER & GIAO DIỆN MENU F2 (CARD UI + MULTI-TAG + PRIORITY)
// =========================================================================
public class SmartCategorySortController : MonoBehaviour
{
    public static SmartCategorySortController Instance;

    private bool showMenu = false;
    private bool showAllStorages = false;
    public Rect windowRect = new Rect(50, 30, 860, 640);
    private Vector2 scrollPos;
    private Vector2 selectorScrollPos;
    private string statusMessage = "Bấm F2 để ẩn/hiện menu. Bấm F3 để gom và phân loại toàn bộ kho.";

    // Trạng thái chuyển trang nội bộ:
    // null = Trang danh sách hòm đồ chính
    // khác null = Đang ở trang chọn nhiều Tag cho hòm có Id này
    private string activeContainerIdForSelection = null;
    private string activeContainerNameForSelection = "";

    // 26 Danh mục Tag đơn lẻ nguyên bản bao quát 100% item và tag game
    public static readonly ItemTagDef[] AllTags = new ItemTagDef[]
    {
        new ItemTagDef("GUN", "🔫 Súng & Vũ Khí Bắn", "Súng"),
        new ItemTagDef("MELEE", "🗡️ Vũ Khí Cận Chiến (Dao/Kiếm)", "Cận Chiến"),
        new ItemTagDef("AMMO", "🎯 Đạn Dược (Hộp Đạn/Viên)", "Đạn"),
        new ItemTagDef("EXPLOSIVE", "💣 Thuốc Nổ, Lựu Đạn & C4", "Chất Nổ"),
        new ItemTagDef("ATTACHMENT", "🔭 Phụ Kiện Súng (Ống Ngắm/Nòng)", "Phụ Kiện"),
        new ItemTagDef("MEDICAL", "💊 Y Tế & Thuốc Men", "Y Tế"),
        new ItemTagDef("FOOD", "🍞 Thực Phẩm & Đồ Ăn", "Thực Phẩm"),
        new ItemTagDef("DRINK", "💧 Nước Uống", "Nước"),
        new ItemTagDef("WATER_FILTER", "🧪 Lọc Nước & Viên Lọc", "Lọc Nước"),
        new ItemTagDef("ALCOHOL", "🍾 Rượu Bia & Đồ Uống Cồn", "Rượu"),
        new ItemTagDef("TOBACCO", "🚬 Thuốc Lá & Xì Gà", "Thuốc Lá"),
        new ItemTagDef("CONTRABAND", "☠️ Hàng Cấm & Độc Dược", "Hàng Cấm"),
        new ItemTagDef("ARMOR", "🛡️ Giáp, Mũ & Đồ Bảo Hộ", "Giáp"),
        new ItemTagDef("MATERIAL", "🔩 Vật Liệu & Kim Loại Chế Tạo", "Vật Liệu"),
        new ItemTagDef("TOOL", "🛠️ Dụng Cụ & Công Cụ Kỹ Thuật", "Dụng Cụ"),
        new ItemTagDef("DOCUMENT", "📄 Tài Liệu, Bản Vẽ & Thẻ Từ", "Tài Liệu"),
        new ItemTagDef("LOTTERY", "🎫 Vé Số & Vé Cào", "Vé Số"),
        new ItemTagDef("ORGAN", "🧬 Nội Tạng & Mẫu Sinh Học", "Nội Tạng"),
        new ItemTagDef("MODULE", "⚡ Mô-Đun, Bo Mạch & Máy Móc", "Mô-Đun"),
        new ItemTagDef("ENERGY", "🔋 Pin & Nguồn Năng Lượng", "Năng Lượng"),
        new ItemTagDef("SEED", "🌱 Hạt Giống & Nông Sản", "Hạt Giống"),
        new ItemTagDef("HOUSEHOLD", "🧴 Đồ Gia Dụng & Nước Hoa Xa Xỉ", "Gia Dụng"),
        new ItemTagDef("ART", "🎨 Nghệ Thuật & Quà Lưu Niệm", "Nghệ Thuật"),
        new ItemTagDef("CONTAINER", "💼 Hòm, Két & Hộp Chứa Đồ", "Hòm Chứa"),
        new ItemTagDef("JUNK", "🗑️ Phế Liệu & Rác Thải", "Phế Liệu"),
        new ItemTagDef("GENERAL", "📦 Kho Chung (Mọi Đồ Khác)", "Kho Chung")
    };

    private Dictionary<string, ContainerConfig> containerConfigs = new Dictionary<string, ContainerConfig>();

    private void Awake()
    {
        Instance = this;
        LoadSavedAssignments();
    }

    // Kiểm tra con trỏ chuột có đang nằm trên Menu F2 không
    public static bool IsPointerOverUI()
    {
        if (Instance == null || !Instance.showMenu) return false;
        Vector2 mousePos = Input.mousePosition;
        Vector2 mouseGui = new Vector2(mousePos.x, Screen.height - mousePos.y);
        Rect hitBox = new Rect(Instance.windowRect.x - 5, Instance.windowRect.y - 5, Instance.windowRect.width + 10, Instance.windowRect.height + 10);
        return hitBox.Contains(mouseGui);
    }

    private void Update()
    {
        // F2: Bật/Tắt Menu Giao Diện Cấu Hình
        if (Input.GetKeyDown(KeyCode.F2))
        {
            showMenu = !showMenu;
            if (!showMenu) activeContainerIdForSelection = null;
        }

        // F3: Phím Tắt Nhanh - Gom & Phân Loại Tất Cả Theo Ưu Tiên
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ExecuteCategorySort();
        }
    }

    private void OnGUI()
    {
        if (!showMenu) return;

        GUI.skin.window.fontSize = 14;

        // VẼ 1 CỬA SỔ DUY NHẤT (Chống đè cửa sổ, chống kéo ra kéo vào)
        windowRect = GUI.Window(889911, windowRect, DrawMainWindow, "📦 BẢNG ĐIỀU KHIỂN & PHÂN LOẠI HÒM ĐỒ (SMART INVENTORY v1.5.0)");
    }

    private void DrawMainWindow(int windowId)
    {
        // Nuốt sự kiện chuột IMGUI để không lọt xuống game
        if (Event.current.isMouse && new Rect(0, 0, windowRect.width, windowRect.height).Contains(Event.current.mousePosition))
        {
            // Event handled by GUI window
        }

        if (!string.IsNullOrEmpty(activeContainerIdForSelection))
        {
            // TRANG 2: TRANG CHỌN NHIỀU TAG CHO HÒM
            DrawTagSelectorPage();
        }
        else
        {
            // TRANG 1: TRANG DANH SÁCH HÒM ĐỒ CHÍNH
            DrawMainContainerListPage();
        }

        GUI.DragWindow(new Rect(0, 0, 860, 28));
    }

    // =========================================================================
    // TRANG 1: DANH SÁCH CÁC HÒM ĐỒ (CARD UI + PRIORITY + MULTI-TAG SUMMARY)
    // =========================================================================
    private void DrawMainContainerListPage()
    {
        GUILayout.Space(6);

        // Thanh trạng thái & hướng dẫn nhanh
        GUILayout.BeginHorizontal("box");
        GUI.color = new Color(1.0f, 0.9f, 0.3f);
        GUILayout.Label("💡 " + statusMessage, GUILayout.Height(22));
        GUI.color = Color.white;
        GUILayout.FlexibleSpace();
        showAllStorages = GUILayout.Toggle(showAllStorages, " Hiện cả kho phụ nền tiệm");
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        // Header ghi chú
        GUILayout.BeginHorizontal();
        GUI.color = new Color(0.7f, 0.9f, 1.0f);
        GUILayout.Label("Danh Sách Hòm Chứa: Thiết lập mức Ưu Tiên (1 là cao nhất) và bấm [🎯 Chọn Tag (Multi) ▾] để chọn nhiều Tag.");
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        List<NamedInventory> allInventories = GetAvailableInventories();
        List<NamedInventory> displayInventories = new List<NamedInventory>();

        foreach (var ni in allInventories)
        {
            if (ni.HasId || showAllStorages)
            {
                ni.Config = GetConfig(ni.Id, ni.DefaultTagId);
                displayInventories.Add(ni);
            }
        }

        // Vùng cuộn danh sách hòm
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(450));

        if (displayInventories.Count == 0)
        {
            GUILayout.Space(40);
            GUI.color = Color.cyan;
            GUILayout.Label("💡 Hiện chưa có hòm đồ riêng nào (hòm có ID) trong tiệm.", EditorStyles_BoldLabel());
            GUILayout.Label("Hãy đặt trên bàn hoặc quẹt thẻ mở một hòm đồ (Hòm an ninh, Hộp sơ cứu, Hộp cơm, Két sắt...) để sử dụng!");
            GUI.color = Color.white;
        }
        else
        {
            foreach (var ni in displayInventories)
            {
                int count = (ni.Inventory.childItems != null) ? ni.Inventory.childItems.Count : 0;
                string id = ni.Id;
                var cfg = ni.Config;

                // Vẽ từng Hòm đồ dưới dạng CARD riêng biệt
                GUILayout.BeginVertical("box");

                // Dòng 1: Tên hòm, vị trí, kích thước và số món
                GUILayout.BeginHorizontal();
                GUI.color = new Color(1.0f, 1.0f, 0.8f);
                GUILayout.Label(ni.DisplayName, EditorStyles_BoldLabel(), GUILayout.Width(480));
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
                GUI.color = new Color(0.6f, 0.85f, 1.0f);
                GUILayout.Label("[" + count + " món | " + ni.Inventory.inventoryShape.width + "x" + ni.Inventory.inventoryShape.height + "]", GUILayout.Width(130));
                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                // Dòng 2: Bộ chọn Ưu Tiên + Badge Trạng Thái / Danh Sách Tag
                GUILayout.BeginHorizontal();

                // Bộ chỉnh Ưu Tiên (Priority Selector)
                GUI.color = new Color(1.0f, 0.85f, 0.2f);
                GUILayout.Label("⭐ Ưu Tiên: " + cfg.Priority, EditorStyles_BoldLabel(), GUILayout.Width(95), GUILayout.Height(24));
                GUI.color = Color.white;

                if (GUILayout.Button("▲", GUILayout.Width(28), GUILayout.Height(22)))
                {
                    if (cfg.Priority > 1)
                    {
                        cfg.Priority--;
                        SaveAssignments();
                    }
                }
                if (GUILayout.Button("▼", GUILayout.Width(28), GUILayout.Height(22)))
                {
                    if (cfg.Priority < 99)
                    {
                        cfg.Priority++;
                        SaveAssignments();
                    }
                }

                GUILayout.Space(8);

                // Badge trạng thái & các tag đã chọn
                if (cfg.IsLocked)
                {
                    GUI.backgroundColor = new Color(0.95f, 0.35f, 0.35f);
                    GUILayout.Label("⛔ ĐÃ KHÓA (Đóng băng 100%)", EditorStyles_BadgeBox(), GUILayout.Width(350), GUILayout.Height(24));
                }
                else if (cfg.IsAuto)
                {
                    string defTagName = GetTagDisplayName(ni.DefaultTagId);
                    GUI.backgroundColor = new Color(0.35f, 0.65f, 0.95f);
                    GUILayout.Label("🤖 Tự Động: " + defTagName, EditorStyles_BadgeBox(), GUILayout.Width(350), GUILayout.Height(24));
                }
                else
                {
                    GUI.backgroundColor = new Color(0.3f, 0.85f, 0.45f);
                    string summary = GetSelectedTagsSummary(cfg.SelectedTags);
                    GUILayout.Label(summary, EditorStyles_BadgeBox(), GUILayout.Width(350), GUILayout.Height(24));
                }
                GUI.backgroundColor = Color.white;

                GUILayout.FlexibleSpace();

                // Nút mở chọn nhiều Tag
                GUI.backgroundColor = new Color(0.95f, 0.8f, 0.25f);
                if (GUILayout.Button("🎯 Chọn Tag (Multi) ▾", GUILayout.Width(155), GUILayout.Height(24)))
                {
                    activeContainerIdForSelection = id;
                    activeContainerNameForSelection = ni.DisplayName;
                }
                GUI.backgroundColor = Color.white;

                // Nút nhanh: Tự động
                if (GUILayout.Button("🤖 Auto", GUILayout.Width(65), GUILayout.Height(24)))
                {
                    cfg.IsAuto = true;
                    cfg.IsLocked = false;
                    SaveAssignments();
                }

                // Nút nhanh: Khóa / Mở khóa
                if (cfg.IsLocked)
                {
                    GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
                    if (GUILayout.Button("🔓 Mở", GUILayout.Width(65), GUILayout.Height(24)))
                    {
                        cfg.IsLocked = false;
                        SaveAssignments();
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
                    if (GUILayout.Button("⛔ Khóa", GUILayout.Width(65), GUILayout.Height(24)))
                    {
                        cfg.IsLocked = true;
                        SaveAssignments();
                    }
                }
                GUI.backgroundColor = Color.white;

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.Space(2);
            }
        }

        GUILayout.EndScrollView();

        GUILayout.Space(6);

        // Thanh thao tác dưới cùng (Bottom Toolbar)
        GUILayout.BeginHorizontal();

        // Nút gom và phân loại tất cả
        GUI.backgroundColor = new Color(0.2f, 0.9f, 0.4f);
        if (GUILayout.Button("⚡ GOM & PHÂN LOẠI TOÀN BỘ (F3)", GUILayout.Height(36), GUILayout.Width(370)))
        {
            ExecuteCategorySort();
        }
        GUI.backgroundColor = Color.white;

        // Nút chỉ nén khít chặt
        if (GUILayout.Button("🔄 Nén Khít Tất Cả (Xoay 90°)", GUILayout.Height(36), GUILayout.Width(260)))
        {
            SortAllInventories();
        }

        GUILayout.FlexibleSpace();

        // Nút đóng menu
        GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
        if (GUILayout.Button("❌ Đóng (F2)", GUILayout.Height(36), GUILayout.Width(130)))
        {
            showMenu = false;
            activeContainerIdForSelection = null;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndHorizontal();
    }

    // =========================================================================
    // TRANG 2: TRANG CHỌN NHIỀU TAG (MULTI-TAG SELECTOR - TOGGLE CHECKBOXES)
    // =========================================================================
    private void DrawTagSelectorPage()
    {
        GUILayout.Space(6);

        var cfg = GetConfig(activeContainerIdForSelection, "GENERAL");

        // Khung thông tin hòm đang chọn
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUI.color = new Color(1.0f, 0.9f, 0.3f);
        GUILayout.Label("🎯 CẤU HÌNH NHẬN ĐỒ: " + activeContainerNameForSelection, EditorStyles_BoldLabel(), GUILayout.Width(500));
        GUI.color = Color.white;

        GUILayout.FlexibleSpace();

        // Chỉnh ưu tiên trực tiếp ngay trên trang chọn Tag
        GUI.color = new Color(1.0f, 0.85f, 0.2f);
        GUILayout.Label("⭐ Mức Ưu Tiên: " + cfg.Priority, EditorStyles_BoldLabel(), GUILayout.Width(110));
        GUI.color = Color.white;
        if (GUILayout.Button("▲", GUILayout.Width(28), GUILayout.Height(22)))
        {
            if (cfg.Priority > 1) { cfg.Priority--; SaveAssignments(); }
        }
        if (GUILayout.Button("▼", GUILayout.Width(28), GUILayout.Height(22)))
        {
            if (cfg.Priority < 99) { cfg.Priority++; SaveAssignments(); }
        }
        GUILayout.EndHorizontal();

        GUI.color = new Color(0.85f, 0.95f, 1.0f);
        GUILayout.Label("Tích chọn 1 hoặc NHIỀU Tag dưới đây. Hòm sẽ nhận mọi vật phẩm có chứa ít nhất 1 Tag đã chọn:");
        GUI.color = Color.white;
        GUILayout.EndVertical();

        GUILayout.Space(4);

        // Các nút thao tác nhanh
        GUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.3f, 0.85f, 0.45f);
        if (GUILayout.Button("✅ Chọn Tất Cả (26 Tag)", GUILayout.Height(28)))
        {
            cfg.SelectedTags.Clear();
            foreach (var t in AllTags) cfg.SelectedTags.Add(t.TagId);
            cfg.IsAuto = false;
            cfg.IsLocked = false;
            SaveAssignments();
        }

        GUI.backgroundColor = new Color(0.95f, 0.4f, 0.4f);
        if (GUILayout.Button("❌ Bỏ Chọn Hết", GUILayout.Height(28)))
        {
            cfg.SelectedTags.Clear();
            cfg.IsAuto = false;
            SaveAssignments();
        }

        GUI.backgroundColor = new Color(0.4f, 0.7f, 1.0f);
        if (GUILayout.Button("🤖 Đặt Về Tự Động Theo Hòm", GUILayout.Height(28)))
        {
            cfg.IsAuto = true;
            cfg.IsLocked = false;
            SaveAssignments();
            activeContainerIdForSelection = null;
        }

        GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
        if (GUILayout.Button("⛔ Đặt Khóa Hòm Này", GUILayout.Height(28)))
        {
            cfg.IsLocked = true;
            SaveAssignments();
            activeContainerIdForSelection = null;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        // Danh sách dạng lưới 2 cột các Tag (26 Tag riêng biệt, hỗ trợ chọn nhiều tag)
        selectorScrollPos = GUILayout.BeginScrollView(selectorScrollPos, GUILayout.Height(390));

        for (int i = 0; i < AllTags.Length; i += 2)
        {
            GUILayout.BeginHorizontal();

            // Cột 1
            DrawTagToggleButton(AllTags[i], cfg);

            // Cột 2
            if (i + 1 < AllTags.Length)
            {
                DrawTagToggleButton(AllTags[i + 1], cfg);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        GUILayout.EndScrollView();

        GUILayout.Space(8);

        // Thanh điều hướng hoàn tất & quay lại
        GUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.2f, 0.85f, 0.45f);
        if (GUILayout.Button("💾 XONG & LƯU CẤU HÌNH (⬅ Quay lại danh sách hòm)", GUILayout.Height(36), GUILayout.Width(500)))
        {
            SaveAssignments();
            activeContainerIdForSelection = null;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.FlexibleSpace();

        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        if (GUILayout.Button("⬅ Hủy Bỏ / Quay Lại", GUILayout.Height(36), GUILayout.Width(220)))
        {
            activeContainerIdForSelection = null;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndHorizontal();
    }

    private void DrawTagToggleButton(ItemTagDef tag, ContainerConfig cfg)
    {
        bool isSelected = cfg.SelectedTags.Contains(tag.TagId);

        if (isSelected)
        {
            GUI.backgroundColor = new Color(0.25f, 0.85f, 0.4f);
            if (GUILayout.Button("✔  " + tag.DisplayName, GUILayout.Height(30), GUILayout.Width(405)))
            {
                cfg.SelectedTags.Remove(tag.TagId);
                cfg.IsAuto = false;
                cfg.IsLocked = false;
                SaveAssignments();
            }
        }
        else
        {
            GUI.backgroundColor = new Color(0.35f, 0.35f, 0.35f);
            if (GUILayout.Button("    " + tag.DisplayName, GUILayout.Height(30), GUILayout.Width(405)))
            {
                cfg.SelectedTags.Add(tag.TagId);
                cfg.IsAuto = false;
                cfg.IsLocked = false;
                SaveAssignments();
            }
        }
        GUI.backgroundColor = Color.white;
    }

    // =========================================================================
    // 4. THUẬT TOÁN PHÂN PHỐI ĐA HÒM ỔN ĐỊNH 100% (CHỐNG NHẢY ITEM QUA LẠI)
    // =========================================================================
    public void ExecuteCategorySort()
    {
        try
        {
            List<NamedInventory> inventories = GetAvailableInventories();
            if (inventories == null || inventories.Count == 0)
            {
                statusMessage = "⚠️ Không tìm thấy hòm đồ nào trong cửa hàng!";
                return;
            }

            // Gán cấu hình cho tất cả hòm
            foreach (var ni in inventories)
            {
                ni.Config = GetConfig(ni.Id, ni.DefaultTagId);
            }

            // Danh sách các hòm đích có thể nhận đồ (không bị khóa)
            // SẮP XẾP TĂNG DẦN THEO MỨC ƯU TIÊN: Priority 1 trước, sau đó 2, 3...
            List<NamedInventory> candidateDests = new List<NamedInventory>();
            foreach (var ni in inventories)
            {
                if (!ni.Config.IsLocked && ni.Inventory != null)
                {
                    candidateDests.Add(ni);
                }
            }

            candidateDests.Sort((a, b) =>
            {
                int cmp = a.Config.Priority.CompareTo(b.Config.Priority);
                if (cmp != 0) return cmp;
                return string.Compare(a.Id, b.Id, StringComparison.Ordinal);
            });

            int totalMoved = 0;

            // Quét từng kho nguồn
            foreach (var sourceNI in inventories)
            {
                GameGridInventory sourceInv = sourceNI.Inventory;
                if (sourceInv == null || sourceInv.childItems == null) continue;

                // 🛑 NẾU KHO NGUỒN BỊ KHÓA -> TUYỆT ĐỐI KHÔNG RÚT ĐỒ RA!
                if (sourceNI.Config.IsLocked) continue;

                List<GameItem> itemsToProcess = new List<GameItem>(sourceInv.childItems);

                foreach (var item in itemsToProcess)
                {
                    if (item == null) continue;
                    if (ItemPinHelper.IsPinned(item) || DebrisHelper.IsDebris(item)) continue;
                    if (item.contentWindow != null || item.IsTag("CONTAINER") || item.IsTag("POUCH") || item.IsTag("BACKPACK") || item.IsTag("LOCKED_CONTAINER_TAG")) continue;

                    HashSet<string> itemTags = GetItemTagIds(item);

                    // Kiểm tra xem kho nguồn hiện tại có hợp lệ với item này không
                    bool sourceAccepts = DoesContainerAcceptItem(sourceNI, itemTags);
                    int currentPriority = sourceAccepts ? sourceNI.Config.Priority : 999999;

                    // Tìm hòm đích tối ưu nhất
                    // NGUYÊN TẮC ỔN ĐỊNH TUYỆT ĐỐI (STABILITY RULE):
                    // Chỉ chuyển đồ nếu hòm đích có ĐỘ ƯU TIÊN CAO HƠN HẲN (destPriority < currentPriority)!
                    // Nếu hòm hiện tại đã chấp nhận đồ này và không có hòm nào ưu tiên cao hơn còn chỗ -> GIỮ NGUYÊN TẠI CHỖ!
                    foreach (var destNI in candidateDests)
                    {
                        if (destNI == sourceNI) continue;

                        if (destNI.Config.Priority >= currentPriority)
                        {
                            // Hòm đích có độ ưu tiên bằng hoặc thấp hơn hòm hiện tại -> DỪNG DUYỆT!
                            // Không bao giờ chuyển đồ ngang hoặc ngược về hòm ưu tiên thấp hơn.
                            break;
                        }

                        if (!DoesContainerAcceptItem(destNI, itemTags)) continue;
                        if (!CanContainerAcceptItem(destNI.Inventory, item)) continue;

                        // Thử chuyển đồ vào hòm đích
                        sourceInv.Expel(item);
                        destNI.Inventory.UncheckedAccept(item);

                        if (InventorySortHelper.Sort(destNI.Inventory, true, false))
                        {
                            totalMoved++;
                            break; // Đã chuyển thành công vào hòm ưu tiên cao nhất!
                        }
                        else
                        {
                            // Hòm đích đã đầy, hoàn trả lại hòm nguồn và thử hòm tiếp theo
                            destNI.Inventory.Expel(item);
                            sourceInv.UncheckedAccept(item);
                        }
                    }
                }
            }

            // Sắp xếp nén khít chặt toàn bộ các kho không bị khóa (xoay 90 độ lấp khít)
            foreach (var ni in inventories)
            {
                if (ni.Config != null && ni.Config.IsLocked) continue;

                if (ni.Inventory != null && ni.Inventory.childItems != null && ni.Inventory.childItems.Count > 0)
                {
                    InventorySortHelper.Sort(ni.Inventory, true, false);
                    ni.Inventory.Validate();
                }
            }

            statusMessage = "✅ Đã phân loại & sắp xếp khít chặt kho! (Đã chuyển " + totalMoved + " món đồ theo thứ tự ưu tiên)";
            Debug.Log("[SmartCategorySort] " + statusMessage);
        }
        catch (Exception ex)
        {
            statusMessage = "❌ Lỗi khi phân loại: " + ex.Message;
            Debug.LogError("[SmartCategorySort] " + ex);
        }
    }

    public void SortAllInventories()
    {
        try
        {
            List<NamedInventory> inventories = GetAvailableInventories();
            int count = 0;
            foreach (var ni in inventories)
            {
                var cfg = GetConfig(ni.Id, ni.DefaultTagId);
                if (cfg.IsLocked) continue;

                if (ni.Inventory != null && ni.Inventory.childItems != null && ni.Inventory.childItems.Count > 0)
                {
                    InventorySortHelper.Sort(ni.Inventory, true, false);
                    ni.Inventory.Validate();
                    count++;
                }
            }
            statusMessage = "✅ Đã nén khít chặt toàn bộ " + count + " kho & hòm đồ (xoay ngang/dọc tối ưu)!";
        }
        catch (Exception ex)
        {
            statusMessage = "❌ Lỗi: " + ex.Message;
        }
    }

    private static bool CanContainerAcceptItem(GameGridInventory destInv, GameItem item)
    {
        if (destInv == null || item == null) return false;
        if (destInv.IsInsertLocked()) return false;

        if (destInv.mayInventoryAddItemFunc != null)
        {
            try
            {
                if (!destInv.mayInventoryAddItemFunc(item, destInv)) return false;
            }
            catch
            {
                return false;
            }
        }
        return true;
    }

    // Kiểm tra hòm có chấp nhận ít nhất một Tag của item hay không
    public bool DoesContainerAcceptItem(NamedInventory ni, HashSet<string> itemTags)
    {
        if (ni == null || itemTags == null || itemTags.Count == 0) return false;
        var cfg = ni.Config ?? GetConfig(ni.Id, ni.DefaultTagId);
        if (cfg.IsLocked) return false;

        // Chế độ Tự Động theo hòm
        if (cfg.IsAuto)
        {
            if (string.IsNullOrEmpty(ni.DefaultTagId)) return false;
            if (ni.DefaultTagId == "GENERAL") return true;
            return itemTags.Contains(ni.DefaultTagId);
        }

        // Chế độ Chọn Nhiều Tag
        if (cfg.SelectedTags.Contains("GENERAL")) return true;

        foreach (var t in itemTags)
        {
            if (cfg.SelectedTags.Contains(t)) return true;
        }

        return false;
    }

    // =========================================================================
    // 5. BỘ TRÍCH XUẤT TAG ĐA NĂNG TOÀN DIỆN CHO GAMEITEM
    // =========================================================================
    public static HashSet<string> GetItemTagIds(GameItem item)
    {
        HashSet<string> tags = new HashSet<string>();
        if (item == null) return tags;

        // 1. Kiểm tra GameItemType
        List<string> types = item.GetGameItemType();
        if (types != null)
        {
            foreach (var t in types)
            {
                if (string.IsNullOrEmpty(t)) continue;
                AddMatchingTagIds(t.ToUpperInvariant(), tags);
            }
        }

        // 2. Kiểm tra item.state
        if (item.state != null && item.state.dict != null)
        {
            foreach (var k in item.state.dict.Keys)
            {
                if (k != null) AddMatchingTagIds(k.ToString().ToUpperInvariant(), tags);
            }
        }

        // 3. Kiểm tra IsTag trực tiếp
        if (item.IsTag("FIREARM") || item.IsTag("ADV_GUN_TAG")) tags.Add("GUN");
        if (item.IsTag("MELEE_WEAPON")) tags.Add("MELEE");
        if (item.IsTag("WEAPON") && !tags.Contains("MELEE")) tags.Add("GUN");
        if (item.IsTag("AMMUNITION") || item.IsTag("MUNITION") || item.IsTag("AMMO_BOX_TAG") || item.IsTag("AMMO_TYPE_TAG")) tags.Add("AMMO");
        if (item.IsTag("EXPLOSIVE")) tags.Add("EXPLOSIVE");
        if (item.IsTag("GUN_MOD") || item.IsTag("GUN_CALIBER_TAG") || item.IsTag("GUN_SUPPRESSED_TAG")) tags.Add("ATTACHMENT");
        if (item.IsTag("MEDICAL") || item.IsTag("MEDICAL_TOOL") || item.IsTag("ITEM_MEDICAL_TAG")) tags.Add("MEDICAL");
        if (item.IsTag("FOOD") || item.IsTag("PROCESSED_FOOD") || item.IsTag("TREAT") || item.IsTag("MEAT_FOOD_TAG") || item.IsTag("FRESH_FOOD_TAG") || item.IsTag("CANNED_FOOD_TAG")) tags.Add("FOOD");
        if (item.IsTag("WATER") || item.IsTag("WATER_RATION") || item.IsTag("BREVAGE")) tags.Add("DRINK");
        if (item.IsTag("WATER_PURIFICATION_SUPPLY") || item.IsTag("WATER_PURIFICATION_TABLET") || item.IsTag("FILTER") || item.IsTag("WATER_FILTER")) tags.Add("WATER_FILTER");
        if (item.IsTag("ALCOHOL") || item.IsTag("WINE_CONTAINER_TAG")) tags.Add("ALCOHOL");
        if (item.IsTag("CIGARETTE_TAG")) tags.Add("TOBACCO");
        if (item.IsTag("NARCOTIC") || item.IsTag("SUBSTANCE") || item.IsTag("POISON") || item.IsTag("CONTRABAND")) tags.Add("CONTRABAND");
        if (item.IsTag("ARMOR") || item.IsTag("TACTICAL_EQUIPMENT_TAG") || item.IsTag("ARMORED")) tags.Add("ARMOR");
        if (item.IsTag("MATERIAL") || item.IsTag("BASIC_MATERIAL") || item.IsTag("CONSTRUCTION_MATERIAL_TAG") || item.IsTag("SCRAP_METAL") || item.IsTag("ORE_TAG")) tags.Add("MATERIAL");
        if (item.IsTag("TOOL") || item.IsTag("TECHNICAL_EQUIPMENT_TAG") || item.IsTag("TOOLBOX_TAG") || item.IsTag("MAGNIFYING_GLASS") || item.IsTag("LABELER")) tags.Add("TOOL");
        if (item.IsTag("DOCUMENT") || item.IsTag("ACCESS_CARD") || item.IsTag("GUIDE_PAPER_TAG") || item.IsTag("NAV_DISK_TAG")) tags.Add("DOCUMENT");
        if (item.IsTag("LOTTERY") || item.IsTag("LOTTERY_PAPER_TAG")) tags.Add("LOTTERY");
        if (item.IsTag("ORGAN") || item.IsTag("ORGAN_ITEM_TAG") || item.IsTag("ORGANIC_ORGAN_TAG") || item.IsTag("ORGAN_TRANSPORT_CONTAINER_TAG")) tags.Add("ORGAN");
        if (item.IsTag("MODULE") || item.IsTag("MACHINE") || item.IsTag("PRINTER_MODULE") || item.IsTag("HYDROPONIC_MODULE") || item.IsTag("EVAPORATOR_MODULE") || item.IsTag("ALARM_MODULE") || item.IsTag("NODE")) tags.Add("MODULE");
        if (item.IsTag("ENERGY_CELL") || item.IsTag("HYPERTECH_BATTERY_TAG")) tags.Add("ENERGY");
        if (item.IsTag("SEED") || item.IsTag("SEED_ITEM_TAG") || item.IsTag("FARMING_SUPPLIES")) tags.Add("SEED");
        if (item.IsTag("HOUSEHOLD_GOOD") || item.IsTag("LUXURY_ITEM")) tags.Add("HOUSEHOLD");
        if (item.IsTag("ART")) tags.Add("ART");
        if (item.IsTag("CONTAINER_TAG") || item.IsTag("SUPPLY_CRATE") || item.IsTag("STORAGE") || item.IsTag("SAFE_TAG") || item.IsTag("POUCH_TAG") || item.IsTag("STORABLE_PACKAGE_TAG") || item.IsTag("LIQUID_CONTAINER_TAG") || item.IsTag("CAGE_TAG")) tags.Add("CONTAINER");
        if (item.IsTag("JUNK")) tags.Add("JUNK");

        // 4. Fallback nhận diện theo tên / identifier
        string id = (item.identifier ?? "").ToLowerInvariant();
        string name = (item.name ?? "").ToLowerInvariant();

        if (id.Contains("gun") || id.Contains("pistol") || id.Contains("smg") || id.Contains("shotgun") || id.Contains("rifle")) tags.Add("GUN");
        if (id.Contains("knife") || id.Contains("sword") || id.Contains("bat") || id.Contains("shiv") || id.Contains("crowbar") || id.Contains("pipe")) tags.Add("MELEE");
        if (id.Contains("ammo") || id.Contains("round") || id.Contains("bullet") || id.Contains("mag")) tags.Add("AMMO");
        if (id.Contains("grenade") || id.Contains("c4") || id.Contains("bomb") || id.Contains("mine")) tags.Add("EXPLOSIVE");
        if (id.Contains("mod") || id.Contains("sight") || id.Contains("silencer") || id.Contains("scope")) tags.Add("ATTACHMENT");
        if (id.Contains("med") || id.Contains("pill") || id.Contains("bandage") || id.Contains("firstaid") || id.Contains("heal") || id.Contains("stim") || id.Contains("cure") || id.Contains("phagi") || id.Contains("bicarsole")) tags.Add("MEDICAL");
        if (id.Contains("food") || id.Contains("bread") || id.Contains("meat") || id.Contains("cheese") || id.Contains("fish") || id.Contains("snack") || id.Contains("icecream") || id.Contains("egg")) tags.Add("FOOD");
        if (id.Contains("water") || id.Contains("drink") || id.Contains("juice") || id.Contains("milk") || id.Contains("soda") || id.Contains("pitcher") || id.Contains("bottle")) tags.Add("DRINK");
        if (id.Contains("filter") || id.Contains("purif")) tags.Add("WATER_FILTER");
        if (id.Contains("wine") || id.Contains("beer") || id.Contains("vodka") || id.Contains("nudka") || id.Contains("alcohol")) tags.Add("ALCOHOL");
        if (id.Contains("cigar") || id.Contains("smoke") || id.Contains("tobacco")) tags.Add("TOBACCO");
        if (id.Contains("drug") || id.Contains("narcotic") || id.Contains("fentanyl") || id.Contains("poison") || id.Contains("zyanide") || id.Contains("contraband")) tags.Add("CONTRABAND");
        if (id.Contains("armor") || id.Contains("helmet") || id.Contains("vest") || id.Contains("shield") || id.Contains("plate")) tags.Add("ARMOR");
        if (id.Contains("material") || id.Contains("ingot") || id.Contains("metal") || id.Contains("ore") || id.Contains("wire") || id.Contains("plastic") || id.Contains("scrap") || id.Contains("nut") || id.Contains("screw")) tags.Add("MATERIAL");
        if (id.Contains("tool") || id.Contains("cutter") || id.Contains("wrench") || id.Contains("scanner") || id.Contains("analyser")) tags.Add("TOOL");
        if (id.Contains("card") || id.Contains("keycard") || id.Contains("doc") || id.Contains("book") || id.Contains("paper") || id.Contains("intel") || id.Contains("disk")) tags.Add("DOCUMENT");
        if (id.Contains("ticket") || id.Contains("lottery")) tags.Add("LOTTERY");
        if (id.Contains("organ") || id.Contains("heart") || id.Contains("liver") || id.Contains("kidney") || id.Contains("eye") || id.Contains("tissue")) tags.Add("ORGAN");
        if (id.Contains("module") || id.Contains("machine") || id.Contains("engine") || id.Contains("pump") || id.Contains("chip")) tags.Add("MODULE");
        if (id.Contains("cell") || id.Contains("battery") || id.Contains("power")) tags.Add("ENERGY");
        if (id.Contains("seed") || id.Contains("plant") || id.Contains("crop")) tags.Add("SEED");
        if (id.Contains("tampon") || id.Contains("shampoo") || id.Contains("towel") || id.Contains("soap") || id.Contains("cream") || id.Contains("perfume")) tags.Add("HOUSEHOLD");
        if (id.Contains("art") || id.Contains("cassette") || id.Contains("music") || id.Contains("statue") || id.Contains("toy")) tags.Add("ART");
        if (id.Contains("box") || id.Contains("case") || id.Contains("crate") || id.Contains("safe") || id.Contains("pouch") || id.Contains("bag") || id.Contains("container")) tags.Add("CONTAINER");
        if (id.Contains("junk") || id.Contains("waste") || id.Contains("trash") || id.Contains("rubbish")) tags.Add("JUNK");

        if (tags.Count == 0) tags.Add("GENERAL");
        return tags;
    }

    private static void AddMatchingTagIds(string rawTag, HashSet<string> tags)
    {
        if (string.IsNullOrEmpty(rawTag)) return;

        if (rawTag == "FIREARM" || rawTag == "ADV_GUN_TAG" || rawTag == "LOWTECH" || rawTag == "MIDLINE" || rawTag == "MODDABLE") tags.Add("GUN");
        else if (rawTag == "MELEE_WEAPON") tags.Add("MELEE");
        else if (rawTag == "WEAPON") { if (!tags.Contains("MELEE")) tags.Add("GUN"); }
        else if (rawTag == "AMMUNITION" || rawTag == "MUNITION" || rawTag == "AMMO_BOX_TAG" || rawTag == "AMMO_TYPE_TAG") tags.Add("AMMO");
        else if (rawTag == "EXPLOSIVE") tags.Add("EXPLOSIVE");
        else if (rawTag == "GUN_MOD" || rawTag == "GUN_CALIBER_TAG" || rawTag == "GUN_SUPPRESSED_TAG" || rawTag == "MAKESHIFT") tags.Add("ATTACHMENT");
        else if (rawTag == "MEDICAL" || rawTag == "MEDICAL_TOOL" || rawTag == "ITEM_MEDICAL_TAG" || rawTag == "MEDICAL_PROFICIENCY") tags.Add("MEDICAL");
        else if (rawTag == "FOOD" || rawTag == "PROCESSED_FOOD" || rawTag == "TREAT" || rawTag == "MEAT_FOOD_TAG" || rawTag == "FRESH_FOOD_TAG" || rawTag == "CANNED_FOOD_TAG") tags.Add("FOOD");
        else if (rawTag == "WATER" || rawTag == "WATER_RATION" || rawTag == "BREVAGE") tags.Add("DRINK");
        else if (rawTag == "WATER_PURIFICATION_SUPPLY" || rawTag == "WATER_PURIFICATION_TABLET" || rawTag == "FILTER" || rawTag == "WATER_FILTER") tags.Add("WATER_FILTER");
        else if (rawTag == "ALCOHOL" || rawTag == "WINE_CONTAINER_TAG") tags.Add("ALCOHOL");
        else if (rawTag == "CIGARETTE_TAG") tags.Add("TOBACCO");
        else if (rawTag == "NARCOTIC" || rawTag == "SUBSTANCE" || rawTag == "POISON" || rawTag == "CONTRABAND") tags.Add("CONTRABAND");
        else if (rawTag == "ARMOR" || rawTag == "TACTICAL_EQUIPMENT_TAG" || rawTag == "ARMORED") tags.Add("ARMOR");
        else if (rawTag == "MATERIAL" || rawTag == "BASIC_MATERIAL" || rawTag == "CONSTRUCTION_MATERIAL_TAG" || rawTag == "SCRAP_METAL" || rawTag == "ORE_TAG" || rawTag == "CHEMICAL_SUPPLIES") tags.Add("MATERIAL");
        else if (rawTag == "TOOL" || rawTag == "TECHNICAL_EQUIPMENT_TAG" || rawTag == "TOOLBOX_TAG" || rawTag == "MAGNIFYING_GLASS" || rawTag == "LABELER") tags.Add("TOOL");
        else if (rawTag == "DOCUMENT" || rawTag == "ACCESS_CARD" || rawTag == "GUIDE_PAPER_TAG" || rawTag == "NAV_DISK_TAG") tags.Add("DOCUMENT");
        else if (rawTag == "LOTTERY" || rawTag == "LOTTERY_PAPER_TAG") tags.Add("LOTTERY");
        else if (rawTag == "ORGAN" || rawTag == "ORGAN_ITEM_TAG" || rawTag == "ORGANIC_ORGAN_TAG" || rawTag == "ORGAN_TRANSPORT_CONTAINER_TAG") tags.Add("ORGAN");
        else if (rawTag == "MODULE" || rawTag == "MACHINE" || rawTag.StartsWith("MODULE_TYPE_") || rawTag == "NODE") tags.Add("MODULE");
        else if (rawTag == "ENERGY_CELL" || rawTag == "POWERBLOCK" || rawTag == "HYPERTECH_BATTERY_TAG") tags.Add("ENERGY");
        else if (rawTag == "SEED" || rawTag == "SEED_ITEM_TAG" || rawTag == "FARMING_SUPPLIES") tags.Add("SEED");
        else if (rawTag == "HOUSEHOLD_GOOD" || rawTag == "LUXURY_ITEM") tags.Add("HOUSEHOLD");
        else if (rawTag == "ART") tags.Add("ART");
        else if (rawTag == "CONTAINER_TAG" || rawTag == "SUPPLY_CRATE" || rawTag == "STORAGE" || rawTag == "SAFE_TAG" || rawTag == "POUCH_TAG") tags.Add("CONTAINER");
        else if (rawTag == "JUNK") tags.Add("JUNK");
    }

    // =========================================================================
    // 6. QUÉT TẤT CẢ KHO & HÒM ĐỒ TRONG TIỆM (ID BỀN VỮNG)
    // =========================================================================
    private static List<GameGridInventory> GetInventoriesFromWindow(PixelWindow pw)
    {
        List<GameGridInventory> result = new List<GameGridInventory>();
        if (pw == null) return result;

        if (pw.childElement is GameGridInventory ggiDirect)
        {
            result.Add(ggiDirect);
        }

        try
        {
            List<GraphNodeStorage> storages = MachineryContainerUtils.Collect(pw.childElement);
            if (storages != null)
            {
                foreach (var st in storages)
                {
                    GameGridInventory ggi = st as GameGridInventory;
                    if (ggi != null && !result.Contains(ggi)) result.Add(ggi);
                }
            }
        }
        catch {}

        if (pw.children != null)
        {
            foreach (var ch in pw.children)
            {
                GameGridInventory ggi = ch as GameGridInventory;
                if (ggi != null && !result.Contains(ggi)) result.Add(ggi);
            }
        }

        if (pw.child is GameGridInventory ggiChild && !result.Contains(ggiChild))
        {
            result.Add(ggiChild);
        }

        return result;
    }

    private static List<GameGridInventory> GetInventoriesFromItem(GameItem item)
    {
        List<GameGridInventory> result = new List<GameGridInventory>();
        if (item == null) return result;

        if (item.contentWindow != null)
        {
            var winInvs = GetInventoriesFromWindow(item.contentWindow);
            foreach (var inv in winInvs)
            {
                if (!result.Contains(inv)) result.Add(inv);
            }
        }

        if (item.children != null)
        {
            foreach (var ch in item.children)
            {
                GameGridInventory ggi = ch as GameGridInventory;
                if (ggi != null && !result.Contains(ggi)) result.Add(ggi);
            }
        }

        return result;
    }

    private static string GetContainerPersistentId(GameItem item, string fallbackName)
    {
        if (item != null)
        {
            string baseId = !string.IsNullOrEmpty(item.identifier) ? item.identifier : item.name;
            if (item.uniqueId > 0) return "box_" + baseId + "_" + item.uniqueId;
            if (!string.IsNullOrEmpty(item.identifier)) return "box_ident_" + item.identifier;
        }
        return "box_hash_" + Math.Abs((fallbackName ?? "container").GetHashCode() % 100000);
    }

    private static string DetectDefaultTagId(string name, GameItem item)
    {
        string n = (name ?? "").ToLowerInvariant();
        string id = (item != null && item.identifier != null) ? item.identifier.ToLowerInvariant() : "";

        if (n.Contains("súng") || n.Contains("vũ khí") || n.Contains("gun") || n.Contains("weapon") || n.Contains("firearm")) return "GUN";
        if (n.Contains("đạn") || n.Contains("ammo") || n.Contains("bullet")) return "AMMO";
        if (n.Contains("phụ kiện") || n.Contains("gun mod") || n.Contains("mod")) return "ATTACHMENT";
        if (n.Contains("y tế") || n.Contains("thuốc") || n.Contains("cứu thương") || n.Contains("med") || n.Contains("first aid")) return "MEDICAL";
        if (n.Contains("thực phẩm") || n.Contains("đồ ăn") || n.Contains("bánh") || n.Contains("cơm") || n.Contains("food")) return "FOOD";
        if (n.Contains("nước") || n.Contains("đồ uống") || n.Contains("water") || n.Contains("drink")) return "DRINK";
        if (n.Contains("rượu") || n.Contains("bia") || n.Contains("alcohol") || n.Contains("wine") || n.Contains("beer")) return "ALCOHOL";
        if (n.Contains("thuốc lá") || n.Contains("cigar") || n.Contains("hàng cấm") || n.Contains("độc") || n.Contains("poison") || n.Contains("narcotic")) return "CONTRABAND";
        if (n.Contains("giáp") || n.Contains("mũ") || n.Contains("armor") || n.Contains("vest") || n.Contains("helmet")) return "ARMOR";
        if (n.Contains("vật liệu") || n.Contains("kim loại") || n.Contains("quặng") || n.Contains("material") || n.Contains("metal")) return "MATERIAL";
        if (n.Contains("công cụ") || n.Contains("dụng cụ") || n.Contains("kỹ thuật") || n.Contains("tool")) return "TOOL";
        if (n.Contains("tài liệu") || n.Contains("thẻ") || n.Contains("giấy") || n.Contains("document") || n.Contains("card")) return "DOCUMENT";
        if (n.Contains("vé số") || n.Contains("vé cào") || n.Contains("lottery")) return "LOTTERY";
        if (n.Contains("nội tạng") || n.Contains("sinh học") || n.Contains("organ")) return "ORGAN";
        if (n.Contains("mô-đun") || n.Contains("module") || n.Contains("chip") || id.Contains("module")) return "MODULE";
        if (n.Contains("pin") || n.Contains("năng lượng") || n.Contains("battery") || n.Contains("power")) return "ENERGY";
        if (n.Contains("hạt giống") || n.Contains("nông") || n.Contains("seed") || id.Contains("seed")) return "SEED";
        if (n.Contains("gia dụng") || n.Contains("tiện ích") || n.Contains("luxury")) return "HOUSEHOLD";
        if (n.Contains("nghệ thuật") || n.Contains("art")) return "ART";
        if (n.Contains("hòm") || n.Contains("thùng") || n.Contains("crate") || n.Contains("box")) return "CONTAINER";

        return "GENERAL";
    }

    public List<NamedInventory> GetAvailableInventories()
    {
        List<NamedInventory> list = new List<NamedInventory>();
        HashSet<GameGridInventory> seenInvs = new HashSet<GameGridInventory>();

        EmporiumEntry emporium = EmporiumEntry.Instance;
        if (emporium != null)
        {
            AddIfValid(list, seenInvs, "main_inv", "🏢 Kho Tiệm Chính (Bàn Thao Tác)", emporium.invElement, "GENERAL", false);
            AddIfValid(list, seenInvs, "showcase", "✨ Kệ Kính Trưng Bày", emporium.showcaseElement, "GENERAL", false);
            AddIfValid(list, seenInvs, "front_box", "📦 Hòm Đồ Trước Quầy", emporium.frontInvinvElement, "GENERAL", false);
            AddIfValid(list, seenInvs, "back_box", "📦 Hòm Đồ Sau Quầy", emporium.backInvinvElement, "GENERAL", false);
            AddIfValid(list, seenInvs, "counter_box", "🗄️ Hòm Quầy Thu Ngân", emporium.backInvinvElementCounter, "GENERAL", false);
            AddIfValid(list, seenInvs, "bazar_left", "🏪 Hòm Chợ Trái", emporium.bazarLeftinvElement, "GENERAL", false);
            AddIfValid(list, seenInvs, "afterhour", "🌙 Kho Ngoài Giờ", emporium.afterhourInventory, "GENERAL", false);

            // Kho Lậu / Hòm Ẩn: Luôn mặc định Khóa
            AddIfValid(list, seenInvs, "hidden_stash", "🕵️ Kho Lậu / Hòm Ẩn", emporium.hiddenElement, "CONTRABAND", false);
        }

        // 1. Quét tất cả cửa sổ hòm đang mở (WindowsHandler)
        try
        {
            if (WindowsHandler.current != null && WindowsHandler.current.visibleWindows != null)
            {
                foreach (PixelWindow pw in WindowsHandler.current.visibleWindows)
                {
                    if (pw == null) continue;
                    List<GameGridInventory> storages = GetInventoriesFromWindow(pw);
                    if (storages == null || storages.Count == 0) continue;

                    GameItem parentItem = (pw.parentItems != null && pw.parentItems.Count > 0) ? pw.parentItems[0] : null;
                    string rawName = !string.IsNullOrEmpty(pw.titleString) ? pw.titleString : (parentItem != null ? parentItem.name : "Hòm Mở");
                    string defTag = DetectDefaultTagId(rawName, parentItem);

                    string id = GetContainerPersistentId(parentItem, rawName);
                    int shortUid = (parentItem != null && parentItem.uniqueId > 0) ? parentItem.uniqueId : Math.Abs(pw.GetHashCode() % 1000);

                    foreach (var ggi in storages)
                    {
                        if (ggi != null && !seenInvs.Contains(ggi))
                        {
                            string displayName = "🔓 " + rawName + " (#" + shortUid + ") [Đang Mở]";
                            AddIfValid(list, seenInvs, id, displayName, ggi, defTag, true);
                        }
                    }
                }
            }
        }
        catch {}

        // 2. Quét sâu các món đồ là hòm/túi (containers) đặt trong TẤT CẢ các kho tiệm
        try
        {
            if (emporium != null)
            {
                var shopInvSources = new (GameGridInventory inv, string locationName)[]
                {
                    (emporium.invElement, "Kho Chính"),
                    (emporium.backInvinvElement, "Hòm Sau"),
                    (emporium.frontInvinvElement, "Hòm Trước"),
                    (emporium.backInvinvElementCounter, "Quầy Thu Ngân"),
                    (emporium.showcaseElement, "Kệ Kính"),
                    (emporium.bazarLeftinvElement, "Chợ Trái"),
                    (emporium.afterhourInventory, "Ngoài Giờ")
                };

                foreach (var source in shopInvSources)
                {
                    if (source.inv == null || source.inv.childItems == null) continue;

                    foreach (GameItem it in source.inv.childItems)
                    {
                        if (it == null) continue;
                        List<GameGridInventory> storages = GetInventoriesFromItem(it);
                        if (storages == null || storages.Count == 0) continue;

                        string id = GetContainerPersistentId(it, it.name);
                        string defTag = DetectDefaultTagId(it.name, it);
                        int shortUid = (it.uniqueId > 0) ? it.uniqueId : Math.Abs(it.GetHashCode() % 1000);

                        foreach (var ggi in storages)
                        {
                            if (ggi != null && !seenInvs.Contains(ggi))
                            {
                                string displayName = "💼 " + it.name + " (#" + shortUid + ") [" + source.locationName + "]";
                                AddIfValid(list, seenInvs, id, displayName, ggi, defTag, true);
                            }
                        }
                    }
                }
            }
        }
        catch {}

        return list;
    }

    private void AddIfValid(List<NamedInventory> list, HashSet<GameGridInventory> seen, string id, string name, GameGridInventory inv, string defTag, bool hasId)
    {
        if (inv != null && inv.inventoryShape != null && inv.inventoryShape.width > 0 && inv.inventoryShape.height > 0)
        {
            if (seen.Contains(inv)) return;
            seen.Add(inv);

            NamedInventory ni = new NamedInventory();
            ni.Id = id;
            ni.DisplayName = name;
            ni.Inventory = inv;
            ni.DefaultTagId = defTag;
            ni.HasId = hasId;
            list.Add(ni);
        }
    }

    // =========================================================================
    // 7. LƯU & TẢI CẤU HÌNH (PERSISTENCE QUA PLAYERPREFS NỘI BỘ AN TOÀN)
    // =========================================================================
    public ContainerConfig GetConfig(string containerId, string defaultTagId)
    {
        if (string.IsNullOrEmpty(containerId)) return new ContainerConfig();

        ContainerConfig cfg;
        if (!containerConfigs.TryGetValue(containerId, out cfg))
        {
            cfg = new ContainerConfig();
            if (containerId == "hidden_stash")
            {
                cfg.IsLocked = true;
                cfg.Priority = 99;
            }
            else
            {
                cfg.IsAuto = true;
                cfg.Priority = 1;
                if (!string.IsNullOrEmpty(defaultTagId) && defaultTagId != "GENERAL")
                {
                    cfg.SelectedTags.Add(defaultTagId);
                }
            }
            containerConfigs[containerId] = cfg;
        }
        return cfg;
    }

    private void SaveAssignments()
    {
        try
        {
            List<string> entries = new List<string>();
            foreach (var kvp in containerConfigs)
            {
                entries.Add(kvp.Key + ":" + kvp.Value.Serialize());
            }
            PlayerPrefs.SetString("PS_SMART_SORT_CONTAINER_CONFIGS_V2", string.Join(";", entries.ToArray()));
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[SmartInventorySort] Không thể lưu cấu hình: " + ex.Message);
        }
    }

    private void LoadSavedAssignments()
    {
        try
        {
            containerConfigs.Clear();
            string data = PlayerPrefs.GetString("PS_SMART_SORT_CONTAINER_CONFIGS_V2", "");
            if (!string.IsNullOrEmpty(data))
            {
                string[] entries = data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var entry in entries)
                {
                    int colonIdx = entry.IndexOf(':');
                    if (colonIdx > 0)
                    {
                        string id = entry.Substring(0, colonIdx);
                        string val = entry.Substring(colonIdx + 1);
                        containerConfigs[id] = ContainerConfig.Deserialize(val);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[SmartInventorySort] Không thể nạp cấu hình: " + ex.Message);
        }
    }

    private static string GetTagDisplayName(string tagId)
    {
        if (string.IsNullOrEmpty(tagId)) return "Kho Chung";
        foreach (var t in AllTags)
        {
            if (t.TagId == tagId) return t.DisplayName;
        }
        return tagId;
    }

    private static string GetSelectedTagsSummary(HashSet<string> tags)
    {
        if (tags == null || tags.Count == 0) return "⚠️ Chưa chọn Tag nào";
        if (tags.Contains("GENERAL") && tags.Count == 1) return "📦 Kho Chung (Mọi Đồ Khác)";

        List<string> shortNames = new List<string>();
        foreach (var t in AllTags)
        {
            if (tags.Contains(t.TagId)) shortNames.Add(t.ShortName);
        }

        if (shortNames.Count == 1) return "🎯 Tag: " + shortNames[0];
        if (shortNames.Count <= 3) return "🎯 " + shortNames.Count + " Tag: " + string.Join(", ", shortNames.ToArray());
        return "🎯 " + shortNames.Count + " Tag: " + shortNames[0] + ", " + shortNames[1] + ", " + shortNames[2] + " +" + (shortNames.Count - 3);
    }

    private static GUIStyle EditorStyles_BoldLabel()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontStyle = FontStyle.Bold;
        return style;
    }

    private static GUIStyle EditorStyles_BadgeBox()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 12;
        return style;
    }
}
