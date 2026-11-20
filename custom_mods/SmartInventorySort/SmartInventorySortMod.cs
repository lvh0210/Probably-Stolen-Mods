using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using HarmonyLib;

// =========================================================================
// SMART INVENTORY SORT & MULTI-TAG CATEGORY STORAGE MOD (v1.6.2)
// Tác giả: Antigravity AI (VibeCode)
// Tính năng nâng cấp cốt lõi v1.6.2:
// 1. KHẮC PHỤC TRIỆT ĐỂ LỖI HÒM HÓA CHẤT GOM NHẦM THỰC PHẨM & Y TẾ:
//    - Thanh lọc triệt để các game tag quá rộng khỏi danh mục Hóa Chất (CHEMICAL).
//    - Loại bỏ LIQUID_CONTAINER_TAG, LIQUID_CONTAINER_DROPPER_TAG, LIQUID_LUCK_TAG, CHEM_ITEM_TAG.
//    - Bổ sung toàn diện các game tag chuẩn cho Thực phẩm (FOOD_ITEM_TAG, SNACK_TAG, FISH_ITEM_TAG...) và Y tế (MEDICAL_ITEM_TAG, MEDICAL_SUPPLY_TAG...).
//    - Thiết lập MASTER ISOLATION SHIELD: Bảo vệ tuyệt đối Thực phẩm, Y tế, Nước uống, Hạt giống không bao giờ dính nhầm tag Hóa Chất.
// 2. TỐI ƯU HÓA TOÀN DIỆN GIAO DIỆN (UI REFACTOR):
//    - Lưới chọn nhiều Tag 2 cột cân xứng (418px) + Tag Kho Chung toàn hàng (842px).
//    - Nút bấm to rõ cao 32px, màu sắc trực quan (xanh ngọc khi chọn, xám tối khi chưa chọn).
//    - Badge hiển thị tag và mức ưu tiên trong thẻ hòm đồ gọn gàng, thoáng đãng, chống tràn mép.
// 3. TINH GỌN & HỢP NHẤT TAG THÔNG MINH (21 TAG TỔNG CỘNG):
//    - Gộp Súng + Phụ kiện -> Súng & Phụ Kiện Súng (GUN).
//    - Gộp Đạn + Thuốc nổ -> Đạn Dược & Thuốc Nổ (AMMO).
//    - Gộp Nước + Bộ lọc -> Nước Uống & Bộ Lọc Nước (DRINK).
//    - Gộp Rượu + Thuốc lá -> Rượu Bia & Thuốc Lá (ALCOHOL).
//    - Gộp Mô-đun + Năng lượng -> Mô-Đun & Pin Năng Lượng (MODULE).
//    - Gộp Tài liệu + Vé số -> Tài Liệu, Thẻ Từ & Vé Số (DOCUMENT).
//    - Gộp Gia dụng + Nghệ thuật -> Đồ Gia Dụng & Nghệ Thuật (HOUSEHOLD).
//    - BẢO TOÀN ĐỘC LẬP: Hàng cấm (CONTRABAND) và Chất gây nghiện (ADDICTIVE) luôn giữ riêng!
// 4. TƯƠNG THÍCH NGƯỢC 100%: Tự động chuyển đổi cấu hình cũ sang danh mục mới mà không mất dữ liệu.
// 5. TẬN DỤNG 100% NATIVE API: Quét Tab hòm nâng cấp (MachineryTabs), quét đệ quy hòm lồng hòm.
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
                Debug.Log("[SmartInventorySort] Đã kích hoạt v1.6.2 (Khắc phục triệt để phân loại tag Hóa chất vs Thực phẩm/Y tế).");
            }

            if (uiObject == null)
            {
                uiObject = new GameObject("SmartCategorySortUI");
                uiObject.AddComponent<SmartCategorySortController>();
                UnityEngine.Object.DontDestroyOnLoad(uiObject);
                Debug.Log("[SmartInventorySort] Đã khởi tạo Controller Giao Diện Menu F2 v1.6.2.");
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
            foreach (var rawT in tags)
            {
                string t = rawT.Trim();
                // Tương thích ngược: tự động gom các tag cũ vào tag gộp mới
                if (t == "ATTACHMENT") cfg.SelectedTags.Add("GUN");
                else if (t == "EXPLOSIVE") cfg.SelectedTags.Add("AMMO");
                else if (t == "WATER_FILTER") cfg.SelectedTags.Add("DRINK");
                else if (t == "TOBACCO") cfg.SelectedTags.Add("ALCOHOL");
                else if (t == "LOTTERY") cfg.SelectedTags.Add("DOCUMENT");
                else if (t == "ENERGY") cfg.SelectedTags.Add("MODULE");
                else if (t == "ART") cfg.SelectedTags.Add("HOUSEHOLD");
                else cfg.SelectedTags.Add(t);
            }
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
    public Rect windowRect = new Rect(50, 30, 880, 640);
    private Vector2 scrollPos;
    private Vector2 selectorScrollPos;
    private string statusMessage = "Bấm F2 để ẩn/hiện menu. Bấm F3 để gom và phân loại toàn bộ kho.";

    // Trạng thái chuyển trang nội bộ:
    // null = Trang danh sách hòm đồ chính
    // khác null = Đang ở trang chọn nhiều Tag cho hòm có Id này
    private string activeContainerIdForSelection = null;
    private string activeContainerNameForSelection = "";

    // 21 Danh mục Tag tinh gọn, chuẩn mực bao quát 100% item và tag game
    public static readonly ItemTagDef[] AllTags = new ItemTagDef[]
    {
        new ItemTagDef("GUN", "🔫 Súng & Phụ Kiện Súng", "Súng & PK"),
        new ItemTagDef("MELEE", "🗡️ Vũ Khí Cận Chiến (Dao/Kiếm)", "Cận Chiến"),
        new ItemTagDef("AMMO", "💣 Đạn Dược & Thuốc Nổ", "Đạn & Nổ"),
        new ItemTagDef("ARMOR", "🛡️ Giáp, Mũ & Đồ Bảo Hộ", "Giáp & Mũ"),
        new ItemTagDef("MEDICAL", "💊 Y Tế & Thuốc Men", "Y Tế"),
        new ItemTagDef("CHEMICAL", "🧪 Hóa Chất & Điều Chế", "Hóa Chất"),
        new ItemTagDef("FOOD", "🍞 Thực Phẩm & Đồ Ăn", "Thực Phẩm"),
        new ItemTagDef("DRINK", "💧 Nước Uống & Bộ Lọc Nước", "Nước & Lọc"),
        new ItemTagDef("ALCOHOL", "🍾 Rượu Bia & Thuốc Lá", "Rượu & Thuốc"),
        new ItemTagDef("ADDICTIVE", "💉 Chất Gây Nghiện (Ma Túy)", "Gây Nghiện"),
        new ItemTagDef("CONTRABAND", "☠️ Hàng Cấm & Độc Dược", "Hàng Cấm"),
        new ItemTagDef("MATERIAL", "🔩 Vật Liệu & Kim Loại Chế Tạo", "Vật Liệu"),
        new ItemTagDef("TOOL", "🛠️ Dụng Cụ & Sửa Chữa Kỹ Thuật", "Dụng Cụ"),
        new ItemTagDef("MODULE", "⚡ Mô-Đun Máy Móc & Pin Năng Lượng", "Mô-Đun & Pin"),
        new ItemTagDef("DOCUMENT", "📄 Tài Liệu, Thẻ Từ & Vé Số", "Tài Liệu & Vé"),
        new ItemTagDef("ORGAN", "🧬 Nội Tạng & Mẫu Sinh Học", "Nội Tạng"),
        new ItemTagDef("SEED", "🌱 Hạt Giống & Nông Sản", "Hạt Giống"),
        new ItemTagDef("HOUSEHOLD", "🎨 Đồ Gia Dụng & Nghệ Thuật Xa Xỉ", "Gia Dụng & NT"),
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
        windowRect = GUI.Window(889911, windowRect, DrawMainWindow, "📦 BẢNG ĐIỀU KHIỂN & PHÂN LOẠI HÒM ĐỒ (SMART INVENTORY v1.6.2)");
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

        GUI.DragWindow(new Rect(0, 0, 880, 28));
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
        GUILayout.Label("Danh Sách Hòm Chứa: Thiết lập mức Ưu Tiên (1 là cao nhất) và bấm [🎯 Chọn Tag ▾] để chọn loại vật phẩm hòm nhận.");
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        List<NamedInventory> allInventories = GetAvailableInventories();
        List<NamedInventory> displayInventories = new List<NamedInventory>();

        foreach (var ni in allInventories)
        {
            if (ni.HasId || showAllStorages)
            {
                ni.Config = GetConfig(ni.Id, ni.DefaultTagId, ni.HasId);
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
                GUI.color = new Color(1.0f, 0.95f, 0.7f);
                GUILayout.Label(ni.DisplayName, EditorStyles_BoldLabel(), GUILayout.Width(500));
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
                GUI.color = new Color(0.6f, 0.85f, 1.0f);
                GUILayout.Label("[" + count + " món | " + ni.Inventory.inventoryShape.width + "x" + ni.Inventory.inventoryShape.height + "]", GUILayout.Width(130));
                GUI.color = Color.white;
                GUILayout.EndHorizontal();

                // Dòng 2: Bộ chọn Ưu Tiên + Badge Trạng Thái / Danh Sách Tag + Nút Điều Khiển
                GUILayout.BeginHorizontal();

                // Bộ chỉnh Ưu Tiên (Priority Selector)
                GUI.color = new Color(1.0f, 0.85f, 0.2f);
                GUILayout.Label("⭐ Ưu Tiên: " + cfg.Priority, EditorStyles_BoldLabel(), GUILayout.Width(95), GUILayout.Height(24));
                GUI.color = Color.white;

                if (GUILayout.Button("▲", GUILayout.Width(26), GUILayout.Height(22)))
                {
                    if (cfg.Priority > 1)
                    {
                        cfg.Priority--;
                        SaveAssignments();
                    }
                }
                if (GUILayout.Button("▼", GUILayout.Width(26), GUILayout.Height(22)))
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
                    GUILayout.Label("⛔ ĐÃ KHÓA (Không Nhận Đồ)", EditorStyles_BadgeBox(), GUILayout.Width(350), GUILayout.Height(24));
                }
                else if (cfg.IsAuto)
                {
                    string defTagName = GetTagDisplayName(ni.DefaultTagId);
                    GUI.backgroundColor = new Color(0.35f, 0.65f, 0.95f);
                    GUILayout.Label("🤖 Tự Động: " + defTagName, EditorStyles_BadgeBox(), GUILayout.Width(350), GUILayout.Height(24));
                }
                else
                {
                    GUI.backgroundColor = new Color(0.22f, 0.75f, 0.42f);
                    string summary = GetSelectedTagsSummary(cfg.SelectedTags);
                    GUILayout.Label(summary, EditorStyles_BadgeBox(), GUILayout.Width(350), GUILayout.Height(24));
                }
                GUI.backgroundColor = Color.white;

                GUILayout.FlexibleSpace();

                // Nút mở chọn nhiều Tag
                GUI.backgroundColor = new Color(0.95f, 0.8f, 0.25f);
                if (GUILayout.Button("🎯 Chọn Tag ▾", GUILayout.Width(135), GUILayout.Height(24)))
                {
                    activeContainerIdForSelection = id;
                    activeContainerNameForSelection = ni.DisplayName;
                }
                GUI.backgroundColor = Color.white;

                // Nút nhanh: Tự động
                if (GUILayout.Button("🤖 Auto", GUILayout.Width(60), GUILayout.Height(24)))
                {
                    cfg.IsAuto = true;
                    cfg.IsLocked = false;
                    SaveAssignments();
                }

                // Nút nhanh: Khóa / Mở khóa
                if (cfg.IsLocked)
                {
                    GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
                    if (GUILayout.Button("🔓 Mở", GUILayout.Width(60), GUILayout.Height(24)))
                    {
                        cfg.IsLocked = false;
                        SaveAssignments();
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
                    if (GUILayout.Button("⛔ Khóa", GUILayout.Width(60), GUILayout.Height(24)))
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
        GUI.backgroundColor = new Color(0.2f, 0.85f, 0.4f);
        if (GUILayout.Button("⚡ GOM & PHÂN LOẠI TOÀN BỘ (F3)", GUILayout.Height(36), GUILayout.Width(380)))
        {
            ExecuteCategorySort();
        }
        GUI.backgroundColor = Color.white;

        // Nút chỉ nén khít chặt
        if (GUILayout.Button("🔄 Nén Khít Tất Cả (Xoay 90°)", GUILayout.Height(36), GUILayout.Width(270)))
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
        GUILayout.Label("🎯 CẤU HÌNH NHẬN ĐỒ: " + activeContainerNameForSelection, EditorStyles_BoldLabel(), GUILayout.Width(460));
        GUI.color = Color.white;

        // Badge đếm số tag đã chọn
        int selCount = cfg.SelectedTags.Count;
        GUI.color = (selCount > 0) ? new Color(0.4f, 1.0f, 0.5f) : new Color(0.85f, 0.85f, 0.85f);
        GUILayout.Label($"[Đã chọn: {selCount} / {AllTags.Length} Tag]", EditorStyles_BoldLabel(), GUILayout.Width(170));
        GUI.color = Color.white;

        GUILayout.FlexibleSpace();

        // Chỉnh ưu tiên trực tiếp ngay trên trang chọn Tag
        GUI.color = new Color(1.0f, 0.85f, 0.2f);
        GUILayout.Label("⭐ Mức Ưu Tiên: " + cfg.Priority, EditorStyles_BoldLabel(), GUILayout.Width(110));
        GUI.color = Color.white;
        if (GUILayout.Button("▲", GUILayout.Width(26), GUILayout.Height(22)))
        {
            if (cfg.Priority > 1) { cfg.Priority--; SaveAssignments(); }
        }
        if (GUILayout.Button("▼", GUILayout.Width(26), GUILayout.Height(22)))
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

        GUI.backgroundColor = new Color(0.28f, 0.8f, 0.45f);
        if (GUILayout.Button($"✅ Chọn Tất Cả ({AllTags.Length} Tag)", GUILayout.Height(28)))
        {
            cfg.SelectedTags.Clear();
            foreach (var t in AllTags) cfg.SelectedTags.Add(t.TagId);
            cfg.IsAuto = false;
            cfg.IsLocked = false;
            SaveAssignments();
        }

        GUI.backgroundColor = new Color(0.92f, 0.4f, 0.4f);
        if (GUILayout.Button("❌ Bỏ Chọn Hết", GUILayout.Height(28)))
        {
            cfg.SelectedTags.Clear();
            cfg.IsAuto = false;
            SaveAssignments();
        }

        GUI.backgroundColor = new Color(0.35f, 0.65f, 0.95f);
        if (GUILayout.Button("🤖 Đặt Về Tự Động Theo Hòm", GUILayout.Height(28)))
        {
            cfg.IsAuto = true;
            cfg.IsLocked = false;
            SaveAssignments();
            activeContainerIdForSelection = null;
        }

        GUI.backgroundColor = new Color(0.85f, 0.3f, 0.3f);
        if (GUILayout.Button("⛔ Đặt Khóa Hòm Này", GUILayout.Height(28)))
        {
            cfg.IsLocked = true;
            SaveAssignments();
            activeContainerIdForSelection = null;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        // Danh sách dạng lưới 2 cột các Tag (20 Tag chuyên biệt xếp 2 cột + 1 Tag Kho Chung hàng ngang)
        selectorScrollPos = GUILayout.BeginScrollView(selectorScrollPos, GUILayout.Height(400));

        // 20 Tag đầu tiên (indexes 0..19): hiển thị 10 cặp
        for (int i = 0; i < 20; i += 2)
        {
            GUILayout.BeginHorizontal();
            DrawTagToggleButton(AllTags[i], cfg, 418f);
            if (i + 1 < 20)
            {
                DrawTagToggleButton(AllTags[i + 1], cfg, 418f);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        // Tag thứ 21 (GENERAL - Kho Chung): Hiển thị nổi bật rộng toàn hàng
        if (AllTags.Length > 20)
        {
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            DrawTagToggleButton(AllTags[20], cfg, 842f);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.Space(8);

        // Thanh điều hướng hoàn tất & quay lại
        GUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.2f, 0.85f, 0.45f);
        if (GUILayout.Button("💾 XONG & LƯU CẤU HÌNH (⬅ Quay lại danh sách hòm)", GUILayout.Height(36), GUILayout.Width(540)))
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

    private void DrawTagToggleButton(ItemTagDef tag, ContainerConfig cfg, float width = 418f)
    {
        bool isSelected = cfg.SelectedTags.Contains(tag.TagId);

        if (isSelected)
        {
            GUI.backgroundColor = new Color(0.2f, 0.78f, 0.42f);
            if (GUILayout.Button("✔  " + tag.DisplayName, GUILayout.Height(32), GUILayout.Width(width)))
            {
                cfg.SelectedTags.Remove(tag.TagId);
                cfg.IsAuto = false;
                cfg.IsLocked = false;
                SaveAssignments();
            }
        }
        else
        {
            GUI.backgroundColor = new Color(0.26f, 0.28f, 0.32f);
            if (GUILayout.Button("    " + tag.DisplayName, GUILayout.Height(32), GUILayout.Width(width)))
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
                ni.Config = GetConfig(ni.Id, ni.DefaultTagId, ni.HasId);
            }

            int totalMoved = 0;

            // Quét từng kho nguồn
            foreach (var sourceNI in inventories)
            {
                GameGridInventory sourceInv = sourceNI.Inventory;
                if (sourceInv == null || sourceInv.childItems == null) continue;

                // 🛑 NẾU KHO NGUỒN BỊ KHÓA -> TUYỆT ĐỐI KHÔNG RÚT ĐỒ RA!
                if (sourceNI.Config.IsLocked) continue;

                List<GameItem> itemsToProcess = new List<GameItem>();
                foreach (var rootItem in sourceInv.childItems)
                {
                    if (rootItem == null) continue;
                    if (ItemPinHelper.IsPinned(rootItem) || DebrisHelper.IsDebris(rootItem)) continue;

                    // Nếu món đồ là hòm chứa đồ -> KHÔNG di chuyển chiếc hòm, nhưng gom đồ bên trong nó!
                    if (IsGeneralStorageContainer(rootItem, rootItem.name) || rootItem.IsTag("LOCKED_CONTAINER_TAG"))
                    {
                        if (!rootItem.IsTag("LOCKED_CONTAINER_TAG"))
                        {
                            try
                            {
                                List<GameItem> nestedItems = rootItem.FindAllChildItems();
                                if (nestedItems != null)
                                {
                                    foreach (var nItem in nestedItems)
                                    {
                                        if (nItem != null && !IsGeneralStorageContainer(nItem, nItem.name) &&
                                            !nItem.IsTag("LOCKED_CONTAINER_TAG") &&
                                            !ItemPinHelper.IsPinned(nItem) && !DebrisHelper.IsDebris(nItem))
                                        {
                                            itemsToProcess.Add(nItem);
                                        }
                                    }
                                }
                            }
                            catch {}
                        }
                    }
                    else
                    {
                        itemsToProcess.Add(rootItem);
                    }
                }

                foreach (var item in itemsToProcess)
                {
                    if (item == null) continue;

                    HashSet<string> itemTags = GetItemTagIds(item);

                    // 1. Tìm tất cả các hòm có thể nhận món đồ này (khớp ít nhất 1 Tag)
                    List<NamedInventory> matchingDests = new List<NamedInventory>();
                    foreach (var destNI in inventories)
                    {
                        if (destNI.Config.IsLocked || destNI.Inventory == null) continue;
                        if (DoesContainerAcceptItem(destNI, itemTags))
                        {
                            matchingDests.Add(destNI);
                        }
                    }

                    if (matchingDests.Count == 0) continue;

                    // 2. Sắp xếp danh sách hòm đích theo thứ tự ưu tiên:
                    // - Hòm khớp Tag chuyên biệt xếp trước Hòm Kho Chung (GENERAL)
                    // - Priority nhỏ hơn đứng trước (1 > 2 > 3...)
                    // - Hòm riêng (HasId == true) đứng trước Kho nền tiệm
                    // - Id ổn định
                    matchingDests.Sort((a, b) =>
                    {
                        bool aGeneral = (a.Config.IsAuto && a.DefaultTagId == "GENERAL") || (!a.Config.IsAuto && a.Config.SelectedTags.Contains("GENERAL") && a.Config.SelectedTags.Count == 1);
                        bool bGeneral = (b.Config.IsAuto && b.DefaultTagId == "GENERAL") || (!b.Config.IsAuto && b.Config.SelectedTags.Contains("GENERAL") && b.Config.SelectedTags.Count == 1);
                        if (aGeneral != bGeneral) return aGeneral ? 1 : -1;

                        int cmp = a.Config.Priority.CompareTo(b.Config.Priority);
                        if (cmp != 0) return cmp;

                        if (a.HasId != b.HasId) return a.HasId ? -1 : 1;

                        return string.Compare(a.Id, b.Id, StringComparison.Ordinal);
                    });

                    // 3. Vị trí hiện tại của món đồ
                    int currentRank = matchingDests.IndexOf(sourceNI);

                    // Nếu món đồ ĐÃ ở hòm số 1 (hòm ưu tiên cao nhất) -> GIỮ NGUYÊN!
                    if (currentRank == 0) continue;

                    // Thử chuyển đồ vào các hòm có thứ tự ưu tiên cao hơn vị trí hiện tại
                    int limit = (currentRank > 0) ? currentRank : matchingDests.Count;

                    for (int r = 0; r < limit; r++)
                    {
                        NamedInventory targetNI = matchingDests[r];
                        if (targetNI == sourceNI) continue;
                        if (!CanContainerAcceptItem(targetNI.Inventory, item)) continue;

                        GameGridInventory actualParent = (item.parentInventory as GameGridInventory) ?? sourceInv;
                        actualParent.Expel(item);
                        targetNI.Inventory.UncheckedAccept(item);

                        if (InventorySortHelper.Sort(targetNI.Inventory, true, false))
                        {
                            totalMoved++;
                            break; // Thành công đưa vào hòm ưu tiên cao nhất có chỗ!
                        }
                        else
                        {
                            // Hòm này đã đầy, hoàn trả lại hòm nguồn và thử hòm ưu tiên tiếp theo
                            targetNI.Inventory.Expel(item);
                            actualParent.UncheckedAccept(item);
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
                var cfg = GetConfig(ni.Id, ni.DefaultTagId, ni.HasId);
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
        if (ni == null) return false;
        var cfg = ni.Config ?? GetConfig(ni.Id, ni.DefaultTagId, ni.HasId);
        if (cfg.IsLocked) return false;

        // Chế độ Tự Động theo hòm
        if (cfg.IsAuto)
        {
            if (string.IsNullOrEmpty(ni.DefaultTagId)) return false;
            if (ni.DefaultTagId == "GENERAL") return true;
            return itemTags != null && itemTags.Contains(ni.DefaultTagId);
        }

        // Chế độ Chọn Nhiều Tag: Nếu có chọn Kho Chung thì nhận tất cả
        if (cfg.SelectedTags.Contains("GENERAL")) return true;

        if (itemTags != null && itemTags.Count > 0)
        {
            foreach (var t in itemTags)
            {
                if (cfg.SelectedTags.Contains(t)) return true;
            }
        }

        return false;
    }

    // =========================================================================
    // =========================================================================
    // 5. BỘ TRÍCH XUẤT TAG ĐA NĂNG TOÀN DIỆN CHO GAMEITEM (CHÍNH XÁC 100%)
    // =========================================================================
        public static HashSet<string> GetItemTagIds(GameItem item)
    {
        HashSet<string> tags = new HashSet<string>();
        if (item == null) return tags;

        // 1. Nguồn chuẩn 1: GameItemType của game ([Thực phẩm], [Y tế], [Vũ khí]...)
        List<string> types = item.GetGameItemType();
        if (types != null)
        {
            foreach (var t in types)
            {
                if (!string.IsNullOrEmpty(t)) AddMatchingTagIds(t, tags);
            }
        }

        // 2. Nguồn chuẩn 2: itemTypes của GameItem
        if (item.itemTypes != null)
        {
            foreach (var it in item.itemTypes)
            {
                if (!string.IsNullOrEmpty(it)) AddMatchingTagIds(it, tags);
            }
        }

        // 3. Nguồn chuẩn 3: Kiểm tra Tag chuẩn từ TagSystem qua item.IsTag(...)
        // Súng & Phụ kiện
        if (item.IsTag("FIREARM") || item.IsTag("ADV_GUN_TAG") || item.IsTag("GUN_FRAME_TAG") || item.IsTag("SERVICE_GUN_TAG") || item.IsTag("GUN_MOD") || item.IsTag("WEAPON_MOD_TAG") || item.IsTag("GUN_CALIBER_TAG") || item.IsTag("GUN_SUPPRESSED_TAG") || (item.IsTag("WEAPON") && !item.IsTag("MELEE_WEAPON"))) tags.Add("GUN");

        // Cận chiến
        if (item.IsTag("MELEE_WEAPON") || item.IsTag("MELEE")) tags.Add("MELEE");

        // Đạn dược & Thuốc nổ
        if (item.IsTag("AMMUNITION") || item.IsTag("MUNITION") || item.IsTag("AMMO_BOX_TAG") || item.IsTag("AMMO_TYPE_TAG") || item.IsTag("NORMAL_AMMO") || item.IsTag("ARMOR_PIERCING_AMMO") || item.IsTag("NON_LETHAL_AMMO") || item.IsTag("EXPLOSIVE")) tags.Add("AMMO");

        // Giáp & Bảo hộ
        if (item.IsTag("ARMOR") || item.IsTag("TACTICAL_EQUIPMENT_TAG") || item.IsTag("ARMORED")) tags.Add("ARMOR");

        // Y tế
        if (item.IsTag("MEDICAL") || item.IsTag("ITEM_MEDICAL_TAG") || item.IsTag("MEDICAL_ITEM_TAG") || item.IsTag("MEDICAL_SUPPLY_TAG") || item.IsTag("SUPPLIES_MEDICAL") || item.IsTag("MEDICAL_TOOL") || item.IsTag("AUTONOMOUS_MEDICAL_SYSTEM_TAG") || item.IsTag("AMENITIES_MEDICAL_TAG")) tags.Add("MEDICAL");

        // Hóa chất & Điều chế (Đã thanh lọc, loại bỏ hoàn toàn các tag dùng chung với Nước, Thực phẩm, Thuốc nhỏ mắt)
        if (item.IsTag("CHEMICAL_PRODUCT_TAG") || item.IsTag("CHEMICAL_PRODUCT_PURITY_TAG") || item.IsTag("CHEMICAL_SUPPLIES") || item.IsTag("LIQUID_ACID") || item.IsTag("LIQUID_CHLORINE") || item.IsTag("CATEGORY_CHEMICAL_PURITY") || item.IsTag("CHEMICAL_INTENT_TAG") || item.IsTag("CHEMICAL") || item.IsTag("CHEMIST") || item.IsTag("REAGENT") || item.IsTag("MODULE_TYPE_CHEM")) tags.Add("CHEMICAL");

        // Thực phẩm
        if (item.IsTag("FOOD") || item.IsTag("FOOD_ITEM_TAG") || item.IsTag("PROCESSED_FOOD") || item.IsTag("TREAT") || item.IsTag("MEAT_FOOD_TAG") || item.IsTag("FRESH_FOOD_TAG") || item.IsTag("CANNED_FOOD_TAG") || item.IsTag("XENO_MEAT_TAG") || item.IsTag("SNACK_TAG") || item.IsTag("FISH_ITEM_TAG") || item.IsTag("EDIBLE_CALORIES") || item.IsTag("FISH_FOOD")) tags.Add("FOOD");

        // Nước uống & Lọc nước
        if (item.IsTag("WATER") || item.IsTag("WATER_RATION") || item.IsTag("BREVAGE") || item.IsTag("WATER_TANK_TAG") || item.IsTag("WATER_PURIFICATION_SUPPLY") || item.IsTag("WATER_PURIFICATION_TABLET") || item.IsTag("FILTER") || item.IsTag("WATER_FILTER") || item.IsTag("WATER_TESTING_STRIP_TAG") || item.IsTag("WATER_BOTTLE_ITEM_TAG")) tags.Add("DRINK");

        // Rượu bia & Thuốc lá
        if (item.IsTag("ALCOHOL") || item.IsTag("WINE_CONTAINER_TAG") || item.IsTag("RED_IMP_BEER_TAG") || item.IsTag("WINEMAKER") || item.IsTag("CIGARETTE_TAG") || item.IsTag("ZHONGHUA")) tags.Add("ALCOHOL");

        // Hàng cấm & Độc dược
        if (item.IsTag("CONTRABAND") || item.IsTag("CONTRABAND_ITEM_TAG") || item.IsTag("RESTRICTED_1") || item.IsTag("RESTRICTED_2") || item.IsTag("RESTRICTED_3") || item.IsTag("TRAFFICKING") || item.IsTag("POISON")) tags.Add("CONTRABAND");

        // Chất gây nghiện (Ma túy)
        if (item.IsTag("NARCOTIC")) tags.Add("ADDICTIVE");

        // Vật liệu chế tạo
        if (item.IsTag("MATERIAL") || item.IsTag("BASIC_MATERIAL") || item.IsTag("REFINED_MATERIAL") || item.IsTag("MATERIAL_TAG") || item.IsTag("CONSTRUCTION_MATERIAL_TAG") || item.IsTag("PRINTER_MATERIAL_TAG") || item.IsTag("SCRAP_METAL") || item.IsTag("ORE_TAG") || item.IsTag("INGOT_PURITY_TAG")) tags.Add("MATERIAL");

        // Dụng cụ
        if (item.IsTag("TOOL") || item.IsTag("TOOLBOX_ITEM_TAG") || item.IsTag("TOOLBOX_TAG") || item.IsTag("SALVAGE_TOOL_TAG") || item.IsTag("TECHNICAL_EQUIPMENT_TAG") || item.IsTag("MAGNIFYING_GLASS") || item.IsTag("LABELER")) tags.Add("TOOL");

        // Mô-đun & Pin năng lượng
        if (item.IsTag("MODULE") || item.IsTag("MACHINE") || item.IsTag("SHIP_MODULE_TAG") || item.IsTag("PRINTER_MODULE") || item.IsTag("HYDROPONIC_MODULE") || item.IsTag("EVAPORATOR_MODULE") || item.IsTag("ALARM_MODULE") || item.IsTag("NODE") || item.IsTag("STANDARD_MACHINE_TAG") || item.IsTag("ENERGY_CELL") || item.IsTag("HYPERTECH_BATTERY_TAG")) tags.Add("MODULE");

        // Tài liệu, Thẻ từ & Vé số
        if (item.IsTag("DOCUMENT") || item.IsTag("ACCESS_CARD") || item.IsTag("GUIDE_PAPER_TAG") || item.IsTag("NAV_DISK_TAG") || item.IsTag("TUTORIAL_PAPER_TAG") || item.IsTag("SLIP_DATA") || item.IsTag("LOTTERY") || item.IsTag("LOTTERY_PAPER_TAG") || item.IsTag("SCRATCHER_TAG") || item.IsTag("SCRATCHER_NEW_TAG")) tags.Add("DOCUMENT");

        // Nội tạng & Mẫu sinh học
        if (item.IsTag("ORGAN") || item.IsTag("ORGAN_ITEM_TAG") || item.IsTag("ORGANIC_ORGAN_TAG") || item.IsTag("HUMAN_ORGAN_TAG") || item.IsTag("HARVESTED_ORGAN_TAG") || item.IsTag("ORGAN_TRANSPORT_CONTAINER_TAG")) tags.Add("ORGAN");

        // Hạt giống & Cây trồng
        if (item.IsTag("SEED") || item.IsTag("SEED_ITEM_TAG") || item.IsTag("MODIFIED_SEED") || item.IsTag("FARMING_SUPPLIES")) tags.Add("SEED");

        // Đồ gia dụng & Nghệ thuật
        if (item.IsTag("HOUSEHOLD_GOOD") || item.IsTag("LUXURY_ITEM") || item.IsTag("ART")) tags.Add("HOUSEHOLD");

        // Hòm, két chứa đồ
        if (item.IsTag("CONTAINER_TAG") || item.IsTag("SUPPLY_CRATE") || item.IsTag("STORAGE") || item.IsTag("SAFE_TAG") || item.IsTag("POUCH_TAG") || item.IsTag("STORABLE_PACKAGE_TAG") || item.IsTag("CAGE_TAG")) tags.Add("CONTAINER");

        // Phế liệu
        if (item.IsTag("JUNK")) tags.Add("JUNK");

        // 4. Fallback chỉ kích hoạt khi item hoàn toàn không có tag hệ thống nào
        if (tags.Count == 0)
        {
            string id = (item.identifier ?? "").ToLowerInvariant();
            if (id.Contains("pill") || id.Contains("bandage") || id.Contains("firstaid") || id.Contains("injector") || id.Contains("stim") || id.Contains("medical") || id.Contains("medicine") || id.StartsWith("med_")) tags.Add("MEDICAL");
            else if (id.Contains("food") || id.Contains("bread") || id.Contains("meat") || id.Contains("cheese") || id.Contains("snack") || id.Contains("nutrifruit") || id.Contains("potato") || id.Contains("crop") || id.Contains("mushroom") || id.Contains("canned") || id.Contains("ration")) tags.Add("FOOD");
            else if (id.Contains("water") || id.Contains("drink") || id.Contains("juice") || id.Contains("water_filter") || id.Contains("purif") || id.Contains("water_tablet") || id.Contains("filter") || id.Contains("soda") || id.Contains("cola")) tags.Add("DRINK");
            else if (id.Contains("wine") || id.Contains("beer") || id.Contains("vodka") || id.Contains("alcohol") || id.Contains("cigar") || id.Contains("smoke") || id.Contains("tobacco") || id.Contains("cigarette")) tags.Add("ALCOHOL");
            else if (id.Contains("chem") || id.Contains("acid_bottle") || id.Contains("reagent") || id.Contains("chlorine") || id.Contains("solvent") || id.Contains("purity_tube") || id.Contains("chemical")) tags.Add("CHEMICAL");
            else if (id.Contains("contraband") || id.Contains("fentanyl") || id.Contains("zyanide")) tags.Add("CONTRABAND");
            else if (id.Contains("narcotic") || id.Contains("morphine")) tags.Add("ADDICTIVE");
            else if (id.Contains("ammo") || id.Contains("bullet") || id.EndsWith("_mag") || id.Contains("explosive") || id.Contains("grenade") || id.Contains("c4") || id.Contains("mine")) tags.Add("AMMO");
            else if (id.Contains("pistol") || id.Contains("rifle") || id.Contains("shotgun") || id.Contains("smg") || id.Contains("scope") || id.Contains("silencer")) tags.Add("GUN");
            else if (id.Contains("knife") || id.Contains("sword") || id.Contains("crowbar")) tags.Add("MELEE");
            else if (id.Contains("module") || id.Contains("battery") || id.Contains("energy_cell")) tags.Add("MODULE");
            else if (id.Contains("paper") || id.Contains("card") || id.Contains("document") || id.Contains("lottery") || id.Contains("ticket")) tags.Add("DOCUMENT");
            else if (id.Contains("seed")) tags.Add("SEED");
        }

        // 5. MASTER ISOLATION SHIELD: Bảo vệ tuyệt đối Thực phẩm, Y tế, Nước uống, Hạt giống không bao giờ bị dính nhầm tag Hóa Chất
        if (tags.Contains("FOOD") || tags.Contains("MEDICAL") || tags.Contains("DRINK") || tags.Contains("ALCOHOL") || tags.Contains("SEED"))
        {
            tags.Remove("CHEMICAL");
        }

        if (tags.Count == 0) tags.Add("GENERAL");
        return tags;
    }

    private static void AddMatchingTagIds(string rawTag, HashSet<string> tags)
    {
        if (string.IsNullOrEmpty(rawTag)) return;
        string t = rawTag.Trim().ToUpperInvariant();

        // Khớp chuẩn xác 100% với danh mục hệ thống game
        if (t == "FOOD" || t == "FRESH_FOOD" || t == "PROCESSED_FOOD" || t == "TREAT" || t == "FOOD_ITEM_TAG" || t == "FRESH_FOOD_TAG" || t == "CANNED_FOOD_TAG" || t == "MEAT_FOOD_TAG" || t == "FISH_FOOD") { tags.Add("FOOD"); return; }
        if (t == "WATER" || t == "WATER_RATION" || t == "BREVAGE" || t == "FILTER" || t == "WATER_FILTER" || t == "WATER_PURIFICATION_SUPPLY" || t == "WATER_PURIFICATION_TABLET" || t == "WATER_TESTING_STRIP_TAG" || t == "WATER_BOTTLE_ITEM_TAG") { tags.Add("DRINK"); return; }
        if (t == "CHEMICAL" || t == "CHEMICAL_PRODUCT_TAG" || t == "CHEMICAL_PRODUCT_PURITY_TAG" || t == "CHEMICAL_SUPPLIES" || t == "LIQUID_ACID" || t == "LIQUID_CHLORINE" || t == "CATEGORY_CHEMICAL_PURITY" || t == "REAGENT" || t == "CHEMIST" || t == "MODULE_TYPE_CHEM" || t == "CHEMICAL_INTENT_TAG") { tags.Add("CHEMICAL"); return; }
        if (t == "ALCOHOL" || t == "CIGARETTE_TAG" || t == "TOBACCO") { tags.Add("ALCOHOL"); return; }
        if (t == "CONTRABAND" || t == "RESTRICTED_1" || t == "RESTRICTED_2" || t == "RESTRICTED_3" || t == "POISON" || t == "TRAFFICKING") { tags.Add("CONTRABAND"); return; }
        if (t == "NARCOTIC" || t == "ADDICTIVE") { tags.Add("ADDICTIVE"); return; }
        if (t == "MEDICAL" || t == "MEDICAL_TOOL" || t == "ITEM_MEDICAL_TAG" || t == "MEDICAL_ITEM_TAG" || t == "MEDICAL_SUPPLY_TAG" || t == "SUPPLIES_MEDICAL" || t == "AUTONOMOUS_MEDICAL_SYSTEM_TAG" || t == "AMENITIES_MEDICAL_TAG") { tags.Add("MEDICAL"); return; }
        if (t == "FIREARM" || t == "MODDABLE" || t == "GUN_MOD" || t == "ATTACHMENT" || (t == "WEAPON" && !tags.Contains("MELEE"))) { tags.Add("GUN"); return; }
        if (t == "MELEE_WEAPON" || t == "MELEE") { tags.Add("MELEE"); return; }
        if (t == "AMMUNITION" || t == "MUNITION" || t == "AMMO" || t == "EXPLOSIVE") { tags.Add("AMMO"); return; }
        if (t == "ARMOR") { tags.Add("ARMOR"); return; }
        if (t == "MATERIAL" || t == "BASIC_MATERIAL" || t == "REFINED_MATERIAL") { tags.Add("MATERIAL"); return; }
        if (t == "TOOL") { tags.Add("TOOL"); return; }
        if (t == "DOCUMENT" || t == "LOTTERY") { tags.Add("DOCUMENT"); return; }
        if (t == "ORGAN") { tags.Add("ORGAN"); return; }
        if (t == "MACHINE" || t == "MODULE" || t == "NODE" || t == "HYDROPONIC_MODULE" || t == "ALARM_MODULE" || t == "PRINTER_MODULE" || t == "EVAPORATOR_MODULE" || t == "ENERGY_CELL" || t == "ENERGY") { tags.Add("MODULE"); return; }
        if (t == "SEED" || t == "MODIFIED_SEED" || t == "FARMING_SUPPLIES") { tags.Add("SEED"); return; }
        if (t == "LUXURY_ITEM" || t == "HOUSEHOLD_GOOD" || t == "ART") { tags.Add("HOUSEHOLD"); return; }
        if (t == "CONTAINER" || t == "CONTAINER_TAG" || t == "STORAGE") { tags.Add("CONTAINER"); return; }
        if (t == "JUNK") { tags.Add("JUNK"); return; }
    }

    // =========================================================================
    // 6. QUÉT TẤT CẢ KHO & HÒM ĐỒ TRONG TIỆM (ID BỀN VỮNG)
    // =========================================================================
        private static MethodInfo findContentTabsMethod = null;
    private static bool searchedFindContentTabs = false;

    private static List<MachineryTabs> GetContentTabs(GameItem item)
    {
        if (item == null) return null;
        if (!searchedFindContentTabs)
        {
            findContentTabsMethod = typeof(GameItem).GetMethod("FindContentTabs", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            searchedFindContentTabs = true;
        }
        if (findContentTabsMethod != null)
        {
            try
            {
                return findContentTabsMethod.Invoke(item, null) as List<MachineryTabs>;
            }
            catch {}
        }
        return null;
    }

    private static List<GameGridInventory> GetInventoriesFromWindow(PixelWindow pw)
    {
        List<GameGridInventory> result = new List<GameGridInventory>();
        if (pw == null) return result;

        if (pw.childElement is GameGridInventory ggiDirect)
        {
            result.Add(ggiDirect);
        }

        // Hỗ trợ trực tiếp các Tab nâng cấp của game (MachineryTabs)
        if (pw.childElement is MachineryTabs mtDirect)
        {
            try
            {
                List<GraphNodeStorage> storages = new List<GraphNodeStorage>();
                mtDirect.CollectStorage(storages);
                foreach (var st in storages)
                {
                    if (st is GameGridInventory ggi && !result.Contains(ggi)) result.Add(ggi);
                }
            }
            catch {}

            for (int i = 0; i < mtDirect.count; i++)
            {
                try
                {
                    var page = mtDirect.GetPage(i);
                    if (page is GameGridInventory pageGgi && !result.Contains(pageGgi)) result.Add(pageGgi);
                    var pageStorages = MachineryContainerUtils.Collect(page);
                    if (pageStorages != null)
                    {
                        foreach (var ps in pageStorages)
                        {
                            if (ps is GameGridInventory psGgi && !result.Contains(psGgi)) result.Add(psGgi);
                        }
                    }
                }
                catch {}
            }
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

        // Kiểm tra qua parentItems của cửa sổ nếu có
        if (pw.parentItems != null)
        {
            foreach (var pItem in pw.parentItems)
            {
                if (pItem != null)
                {
                    List<MachineryTabs> tabs = GetContentTabs(pItem);
                    if (tabs != null)
                    {
                        foreach (var mt in tabs)
                        {
                            if (mt == null) continue;
                            try
                            {
                                List<GraphNodeStorage> storages = new List<GraphNodeStorage>();
                                mt.CollectStorage(storages);
                                foreach (var st in storages)
                                {
                                    if (st is GameGridInventory ggi && !result.Contains(ggi)) result.Add(ggi);
                                }
                            }
                            catch {}
                        }
                    }
                }
            }
        }

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

        // 1. TẬN DỤNG HÀM GỐC CỦA GAME: item.FindContentTabs() qua reflection
        List<MachineryTabs> contentTabs = GetContentTabs(item);
        if (contentTabs != null)
        {
            foreach (var mt in contentTabs)
            {
                if (mt == null) continue;
                try
                {
                    List<GraphNodeStorage> storages = new List<GraphNodeStorage>();
                    mt.CollectStorage(storages);
                    foreach (var st in storages)
                    {
                        if (st is GameGridInventory ggi && !result.Contains(ggi)) result.Add(ggi);
                    }
                }
                catch {}

                for (int i = 0; i < mt.count; i++)
                {
                    try
                    {
                        var page = mt.GetPage(i);
                        if (page is GameGridInventory pageGgi && !result.Contains(pageGgi)) result.Add(pageGgi);
                        var pageStorages = MachineryContainerUtils.Collect(page);
                        if (pageStorages != null)
                        {
                            foreach (var ps in pageStorages)
                            {
                                if (ps is GameGridInventory psGgi && !result.Contains(psGgi)) result.Add(psGgi);
                            }
                        }
                    }
                    catch {}
                }
            }
        }

        // 2. Nếu có contentWindow, lấy từ cửa sổ nội dung
        if (item.contentWindow != null)
        {
            var winInvs = GetInventoriesFromWindow(item.contentWindow);
            foreach (var inv in winInvs)
            {
                if (!result.Contains(inv)) result.Add(inv);
            }
        }

        // 3. Quét các children
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

    // =========================================================================
    // KIỂM TRA MỘT VẬT PHẨM CÓ PHẢI LÀ HÒM CHỨA ĐỒ ĐA NĂNG THỰC THỤ HAY KHÔNG
    // (Loại trừ các gói/hộp chứa chuyên biệt chỉ đựng 1 món như gói hạt giống, hộp viên lọc nước...)
    // =========================================================================
    public static bool IsGeneralStorageContainer(GameItem item, string fallbackName)
    {
        string name = (item != null ? item.name : fallbackName) ?? "";
        string n = name.ToLowerInvariant();
        string id = (item != null && item.identifier != null) ? item.identifier.ToLowerInvariant() : "";

        // 1. LOẠI TRỪ CÁC GÓI / HỘP CHỨA ĐỒ CHUYÊN BIỆT (RESTRICTED DISPENSERS / PACKETS)
        // a) Dựa trên Tag hệ thống game:
        if (item != null)
        {
            // PRE_CONTAINER_TAG: Hộp viên lọc nước, hộp que thử, két bia, hộp dinh dưỡng...
            if (item.IsTag("PRE_CONTAINER_TAG")) return false;
            // STORABLE_PACKAGE_TAG: Gói hạt giống nutrifruit, bao bì hạt giống...
            if (item.IsTag("STORABLE_PACKAGE_TAG")) return false;
            // AMMO_BOX_TAG: Hộp đạn chuyên dụng chỉ chứa đạn
            if (item.IsTag("AMMO_BOX_TAG")) return false;
            // WINE_CONTAINER_TAG: Chai/thùng đựng rượu
            if (item.IsTag("WINE_CONTAINER_TAG")) return false;
            // CASSETTE_PLAYER: Máy nghe nhạc chỉ cắm băng cassette
            if (item.IsTag("CASSETTE_PLAYER")) return false;
            // CAGE_TAG nếu là bẫy chuột/lồng bắt thú
            if (item.IsTag("CAGE_TAG") && !id.Contains("storage") && !id.Contains("cage_storage")) return false;
        }

        // b) Dựa trên từ khóa Identifier & Tên vật phẩm / tiêu đề cửa sổ:
        // Hạt giống: gói/túi hạt giống (seed_packet, nutrifruit_seed_packet...)
        if (id.Contains("seed_packet") || id.Contains("seed_pack") || id.Contains("seedpacket") ||
            n.Contains("gói hạt giống") || n.Contains("túi hạt giống") || (n.Contains("hạt giống") && (n.Contains("gói") || n.Contains("túi") || n.Contains("packet"))))
        {
            return false;
        }

        // Viên lọc nước & que thử: water_tablet, tablet_container, test_kit...
        if (id.Contains("water_tablet") || id.Contains("tablet_container") || id.Contains("nutrient_tablet") || id.Contains("test_kit") ||
            n.Contains("viên lọc") || n.Contains("que thử") || n.Contains("hộp viên") || n.Contains("tablet box"))
        {
            return false;
        }

        // Hộp đạn các loại: ammo_box, ammo_case...
        if (id.Contains("ammo_box") || id.Contains("ammo_case") || n.Contains("hộp đạn"))
        {
            return false;
        }

        // Két bia / thùng nước ngọt nhỏ:
        if (id.Contains("beer_case") || n.Contains("két bia"))
        {
            return false;
        }

        // Bao thuốc lá / hộp xì gà nhỏ:
        if (id.Contains("cigarette_pack") || id.Contains("cigar_case") || n.Contains("bao thuốc lá") || n.Contains("gói thuốc lá"))
        {
            return false;
        }

        // Máy nghe nhạc cassette:
        if (id.Contains("cassette_player") || n.Contains("máy nghe nhạc"))
        {
            return false;
        }

        // 2. XÁC NHẬN HÒM CHỨA ĐỒ THỰC THỤ (GENERAL STORAGE CONTAINER)
        // a) Có các Tag chứa đồ chuẩn của game:
        if (item != null)
        {
            if (item.IsTag("CONTAINER_TAG") || item.IsTag("BACKPACK_TAG") || item.IsTag("POUCH_TAG") ||
                item.IsTag("STORAGE") || item.IsTag("SAFE_TAG") || item.IsTag("SUPPLY_CRATE") ||
                item.IsTag("TOOLBOX_TAG") || item.IsTag("SMUGGLER_BAY"))
            {
                return true;
            }
        }

        // b) Tên hoặc Identifier thể hiện rõ là hòm/túi/két/ba lô lưu trữ:
        if (id.Contains("backpack") || id.Contains("satchel") || id.Contains("fannypack") || id.Contains("pouch") ||
            id.Contains("safe") || id.Contains("crate") || id.Contains("chest") || id.Contains("locker") ||
            id.Contains("toolbox") || id.Contains("storage") || id.Contains("smuggler"))
        {
            return true;
        }

        if (n.Contains("ba lô") || n.Contains("balo") || n.Contains("túi đeo") || n.Contains("túi đa năng") ||
            n.Contains("hòm") || n.Contains("két") || n.Contains("rương") || n.Contains("hộp chứa") ||
            n.Contains("thùng chứa") || n.Contains("thùng tiếp tế") || n.Contains("hộp dụng cụ") ||
            n.Contains("hộp cứu thương") || n.Contains("hốc giấu") || n.Contains("kệ") || n.Contains("tủ"))
        {
            return true;
        }

        // Nếu là vật phẩm không có tag chứa đồ rõ ràng và không phải hòm chứa -> Bỏ qua
        if (item != null && !item.IsTag("CONTAINER_TAG") && !item.IsTag("BACKPACK_TAG") && !item.IsTag("POUCH_TAG"))
        {
            return false;
        }

        return true;
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

        if (n.Contains("hóa chất") || n.Contains("hoá chất") || n.Contains("chemical") || n.Contains("chemist") || n.Contains("acid") || n.Contains("reagent") || id.Contains("chem")) return "CHEMICAL";
        if (n.Contains("súng") || n.Contains("vũ khí") || n.Contains("gun") || n.Contains("weapon") || n.Contains("firearm") || n.Contains("phụ kiện") || n.Contains("gun mod") || n.Contains("scope") || n.Contains("silencer")) return "GUN";
        if (n.Contains("cận chiến") || n.Contains("dao") || n.Contains("kiếm") || n.Contains("melee") || n.Contains("knife") || n.Contains("sword")) return "MELEE";
        if (n.Contains("đạn") || n.Contains("ammo") || n.Contains("bullet") || n.Contains("thuốc nổ") || n.Contains("lựu đạn") || n.Contains("c4") || n.Contains("bomb") || n.Contains("explosive")) return "AMMO";
        if (n.Contains("y tế") || n.Contains("cứu thương") || n.Contains("med") || n.Contains("first aid") || (n.Contains("thuốc") && !n.Contains("thuốc lá") && !n.Contains("thuốc nổ"))) return "MEDICAL";
        if (n.Contains("thực phẩm") || n.Contains("đồ ăn") || n.Contains("bánh") || n.Contains("cơm") || n.Contains("food") || n.Contains("mì")) return "FOOD";
        if (n.Contains("nước") || n.Contains("đồ uống") || n.Contains("water") || n.Contains("drink") || n.Contains("juice") || n.Contains("lọc nước") || n.Contains("purif") || n.Contains("filter") || id.Contains("water_filter") || id.Contains("water_tablet")) return "DRINK";
        if (n.Contains("rượu") || n.Contains("bia") || n.Contains("alcohol") || n.Contains("wine") || n.Contains("beer") || n.Contains("vodka") || n.Contains("thuốc lá") || n.Contains("xì gà") || n.Contains("cigar") || n.Contains("tobacco") || n.Contains("cigarette")) return "ALCOHOL";
        if (n.Contains("ma túy") || n.Contains("narcotic") || n.Contains("chất gây nghiện") || n.Contains("addictive") || n.Contains("gây nghiện")) return "ADDICTIVE";
        if (n.Contains("hàng cấm") || n.Contains("giấu hàng") || n.Contains("contraband") || n.Contains("độc dược") || n.Contains("poison") || n.Contains("hàng lậu")) return "CONTRABAND";
        if (n.Contains("giáp") || n.Contains("mũ") || n.Contains("armor") || n.Contains("vest") || n.Contains("helmet")) return "ARMOR";
        if (n.Contains("vật liệu") || n.Contains("kim loại") || n.Contains("quặng") || n.Contains("material") || n.Contains("metal")) return "MATERIAL";
        if (n.Contains("công cụ") || n.Contains("dụng cụ") || n.Contains("kỹ thuật") || n.Contains("tool") || n.Contains("sửa chữa")) return "TOOL";
        if (n.Contains("tài liệu") || n.Contains("thẻ") || n.Contains("giấy") || n.Contains("document") || n.Contains("card") || n.Contains("vé số") || n.Contains("vé cào") || n.Contains("lottery") || n.Contains("scratcher")) return "DOCUMENT";
        if (n.Contains("nội tạng") || n.Contains("sinh học") || n.Contains("organ")) return "ORGAN";
        if (n.Contains("mô-đun") || n.Contains("module") || n.Contains("chip") || id.Contains("module") || n.Contains("pin") || n.Contains("năng lượng") || n.Contains("battery") || n.Contains("power")) return "MODULE";
        if (n.Contains("hạt giống") || n.Contains("nông") || n.Contains("seed") || id.Contains("seed")) return "SEED";
        if (n.Contains("gia dụng") || n.Contains("tiện ích") || n.Contains("luxury") || n.Contains("nghệ thuật") || n.Contains("art")) return "HOUSEHOLD";
        if (n.Contains("hòm") || n.Contains("thùng") || n.Contains("crate") || n.Contains("box") || n.Contains("két") || n.Contains("túi")) return "CONTAINER";
        if (n.Contains("phế liệu") || n.Contains("rác") || n.Contains("junk") || n.Contains("trash")) return "JUNK";

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

                    GameItem parentItem = (pw.parentItems != null && pw.parentItems.Count > 0) ? pw.parentItems[0] : null;
                    string rawName = !string.IsNullOrEmpty(pw.titleString) ? pw.titleString : (parentItem != null ? parentItem.name : "Hòm Mở");

                    // 🛑 BỎ QUA GÓI HẠT GIỐNG, HỘP VIÊN LỌC NƯỚC, HỘP ĐẠN, KÉT BIA...
                    if (!IsGeneralStorageContainer(parentItem, rawName)) continue;

                    List<GameGridInventory> storages = GetInventoriesFromWindow(pw);
                    if (storages == null || storages.Count == 0) continue;

                    string defTag = DetectDefaultTagId(rawName, parentItem);
                    string id = GetContainerPersistentId(parentItem, rawName);
                    int shortUid = (parentItem != null && parentItem.uniqueId > 0) ? parentItem.uniqueId : Math.Abs(pw.GetHashCode() % 1000);

                    for (int sIdx = 0; sIdx < storages.Count; sIdx++)
                    {
                        var ggi = storages[sIdx];
                        if (ggi != null && !seenInvs.Contains(ggi))
                        {
                            string tabSuffix = storages.Count > 1 ? (" [Tab " + (sIdx + 1) + "/" + storages.Count + "]") : "";
                            string tabId = storages.Count > 1 ? (id + "_tab" + (sIdx + 1)) : id;
                            string displayName = "🔓 " + rawName + tabSuffix + " (#" + shortUid + ") [Đang Mở]";
                            AddIfValid(list, seenInvs, tabId, displayName, ggi, defTag, true);
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

                // Thu thập tất cả các container trong kho tiệm (kể cả hòm lồng trong hòm mở rộng)
                List<(GameItem item, string locationName)> allCandidateContainers = new List<(GameItem, string)>();

                foreach (var source in shopInvSources)
                {
                    if (source.inv == null || source.inv.childItems == null) continue;

                    foreach (GameItem it in source.inv.childItems)
                    {
                        if (it == null) continue;
                        allCandidateContainers.Add((it, source.locationName));

                        // TẬN DỤNG HÀM GỐC CỦA GAME: it.FindAllChildItems()
                        // Nếu trong hòm này có chứa hòm/túi khác bên trong -> tự động lấy ra quản lý!
                        try
                        {
                            List<GameItem> children = it.FindAllChildItems();
                            if (children != null)
                            {
                                foreach (var ch in children)
                                {
                                    if (ch != null && IsGeneralStorageContainer(ch, ch.name))
                                    {
                                        allCandidateContainers.Add((ch, source.locationName + " > " + it.name));
                                    }
                                }
                            }
                        }
                        catch {}
                    }
                }

                foreach (var entry in allCandidateContainers)
                {
                    GameItem it = entry.item;
                    if (it == null) continue;

                    // 🛑 BỎ QUA GÓI HẠT GIỐNG, HỘP VIÊN LỌC NƯỚC, HỘP ĐẠN, KÉT BIA...
                    if (!IsGeneralStorageContainer(it, it.name)) continue;

                    List<GameGridInventory> storages = GetInventoriesFromItem(it);
                    if (storages == null || storages.Count == 0) continue;

                    string id = GetContainerPersistentId(it, it.name);
                    string defTag = DetectDefaultTagId(it.name, it);
                    int shortUid = (it.uniqueId > 0) ? it.uniqueId : Math.Abs(it.GetHashCode() % 1000);

                    for (int sIdx = 0; sIdx < storages.Count; sIdx++)
                    {
                        var ggi = storages[sIdx];
                        if (ggi != null && !seenInvs.Contains(ggi))
                        {
                            string tabSuffix = storages.Count > 1 ? (" [Tab " + (sIdx + 1) + "/" + storages.Count + "]") : "";
                            string tabId = storages.Count > 1 ? (id + "_tab" + (sIdx + 1)) : id;
                            string displayName = "💼 " + it.name + tabSuffix + " (#" + shortUid + ") [" + entry.locationName + "]";
                            AddIfValid(list, seenInvs, tabId, displayName, ggi, defTag, true);
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
    public ContainerConfig GetConfig(string containerId, string defaultTagId, bool hasId = true)
    {
        if (string.IsNullOrEmpty(containerId)) return new ContainerConfig();

        // Chuẩn hóa tag mặc định nếu rơi vào nhóm cũ đã gộp
        if (defaultTagId == "ATTACHMENT") defaultTagId = "GUN";
        else if (defaultTagId == "EXPLOSIVE") defaultTagId = "AMMO";
        else if (defaultTagId == "WATER_FILTER") defaultTagId = "DRINK";
        else if (defaultTagId == "TOBACCO") defaultTagId = "ALCOHOL";
        else if (defaultTagId == "LOTTERY") defaultTagId = "DOCUMENT";
        else if (defaultTagId == "ENERGY") defaultTagId = "MODULE";
        else if (defaultTagId == "ART") defaultTagId = "HOUSEHOLD";

        ContainerConfig cfg;
        if (!containerConfigs.TryGetValue(containerId, out cfg))
        {
            cfg = new ContainerConfig();
            if (containerId == "hidden_stash")
            {
                cfg.IsLocked = true;
                cfg.Priority = 99;
            }
            else if (!hasId)
            {
                // Kho nền tiệm (Bàn thao tác, kệ kính, quầy thu ngân...) mặc định ưu tiên 99 để nhường đồ cho hòm riêng
                cfg.IsAuto = true;
                cfg.Priority = 99;
            }
            else
            {
                // Hòm đồ riêng (Hòm an ninh, két sắt, hộp cơm...) mặc định ưu tiên 1
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
        if (tagId == "ATTACHMENT") tagId = "GUN";
        else if (tagId == "EXPLOSIVE") tagId = "AMMO";
        else if (tagId == "WATER_FILTER") tagId = "DRINK";
        else if (tagId == "TOBACCO") tagId = "ALCOHOL";
        else if (tagId == "LOTTERY") tagId = "DOCUMENT";
        else if (tagId == "ENERGY") tagId = "MODULE";
        else if (tagId == "ART") tagId = "HOUSEHOLD";

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
