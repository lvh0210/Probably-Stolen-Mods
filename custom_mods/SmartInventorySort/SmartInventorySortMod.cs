using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

// =========================================================================
// SMART INVENTORY SORT & CATEGORY STORAGE MOD (v1.4.0)
// Tác giả: Antigravity AI (VibeCode)
// Tính năng nâng cấp:
// 1. PackFirstFit thay vì PackShelves (loại bỏ hoàn toàn khoảng trống chết)
// 2. ForceSortAutoRotatePatch: Luôn tự động xoay ngang/dọc (Auto-Rotate 90°)
// 3. Hệ thống 21 Danh Mục Phân Loại bao quát 100% Item & Tag trong Game
// 4. Menu F2 Card UI hiện đại, thanh lịch, dễ nhìn, lọc hòm có ID (#UID)
// 5. Popup Selector (Nút bấm [🎯 Chọn Tag ▾] mở bảng danh mục chọn trực tiếp)
// 6. Phân phối 2 tầng ưu tiên: Hòm chỉ định -> Hòm tự động -> Kho chung
// 7. Bảo vệ đóng băng 100% kho bị khóa (chặn cả rút đồ lẫn nhét đồ)
// 8. Định danh bền vững (Persistent Container ID) lưu cấu hình vĩnh viễn
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
                Debug.Log("[SmartInventorySort] Đã kích hoạt hệ thống Sắp Xếp Kho Tối Ưu (PackFirstFit + AutoRotate 90°).");
            }

            if (uiObject == null)
            {
                uiObject = new GameObject("SmartCategorySortUI");
                uiObject.AddComponent<SmartCategorySortController>();
                UnityEngine.Object.DontDestroyOnLoad(uiObject);
                Debug.Log("[SmartInventorySort] Đã khởi tạo Controller Giao Diện Menu F2 & Sắp Xếp Đa Hòm v1.4.0.");
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
// 1. HARMONY PATCHES: Tối ưu hóa thuật toán lấp đầy & Tự động xoay ngang/dọc
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
// 2. CONTROLLER & UI: Phân loại theo Danh Mục & Menu F2 Card UI Chuyên Nghiệp
// =========================================================================
public class SmartCategorySortController : MonoBehaviour
{
    public static SmartCategorySortController Instance;

    private bool showMenu = false;
    private bool showAllStorages = false; // Mặc định FALSE: chỉ hiển thị những hòm có ID rõ ràng
    private Rect windowRect = new Rect(50, 30, 860, 640);
    private Vector2 scrollPos;
    private string statusMessage = "Bấm F2 để ẩn/hiện bảng. Bấm F3 để gom và phân loại toàn bộ kho.";

    // Modal Popup chọn Tag / Danh mục
    private string activePopupContainerId = null;
    private string activePopupContainerName = "";
    private Rect popupRect = new Rect(140, 80, 680, 540);
    private Vector2 popupScrollPos;

    // Danh sách 21 danh mục hoàn chỉnh của toàn bộ game
    public static readonly string[] CategoryNames = new string[]
    {
        "⛔ BỎ QUA / KHÓA (Đóng Băng)",               // 0
        "⚔️ Súng & Vũ Khí",                          // 1
        "🎯 Đạn Dược & Bom Mìn",                      // 2
        "🔫 Phụ Kiện Súng (Gun Mods)",                // 3
        "💊 Y Tế & Thuốc Men",                       // 4
        "🍞 Thực Phẩm & Đồ Ăn",                       // 5
        "💧 Nước Uống & Lọc Nước",                   // 6
        "🍾 Rượu Bia & Đồ Uống Cồn",                  // 7
        "🚬 Thuốc Lá, Hàng Cấm & Độc Dược",           // 8
        "🛡️ Áo Giáp & Đồ Bảo Hộ",                    // 9
        "🔩 Vật Liệu & Kim Loại Chế Tạo",             // 10
        "🛠️ Dụng Cụ & Công Cụ Kỹ Thuật",              // 11
        "📄 Tài Liệu, Thẻ Từ & Vé Số",                // 12
        "🧬 Nội Tạng & Mẫu Sinh Học",                 // 13
        "⚡ Mô-Đun, Chip & Máy Móc",                  // 14
        "🔋 Pin & Nguồn Năng Lượng",                 // 15
        "🌱 Hạt Giống & Nông Nghiệp",                 // 16
        "🧴 Đồ Gia Dụng & Nước Hoa Xa Xỉ",            // 17
        "💼 Hòm, Két & Hộp Chứa Đồ",                 // 18
        "🎨 Nghệ Thuật & Quà Lưu Niệm",               // 19
        "🗑️ Phế Liệu & Rác Thải",                     // 20
        "📦 Kho Chung (Mọi Đồ Khác)"                 // 21
    };

    // Cấu hình người dùng: key = Persistent Container ID
    // -1 = Tự động nhận diện theo hòm
    // 0 = Khóa / Bỏ qua (Đóng băng 100%)
    // 1..21 = Danh mục ưu tiên cụ thể
    private Dictionary<string, int> customAssignments = new Dictionary<string, int>();

    private void Awake()
    {
        Instance = this;
        LoadSavedAssignments();
    }

    private void Update()
    {
        // F2: Bật/Tắt Menu Giao Diện Cấu Hình
        if (Input.GetKeyDown(KeyCode.F2))
        {
            showMenu = !showMenu;
            if (!showMenu) activePopupContainerId = null;
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

        // Window chính
        windowRect = GUI.Window(889911, windowRect, DrawMainWindow, "📦 BẢNG ĐIỀU KHIỂN & PHÂN LOẠI HÒM ĐỒ (SMART INVENTORY)");

        // Window popup chọn tag (nếu đang kích hoạt)
        if (!string.IsNullOrEmpty(activePopupContainerId))
        {
            GUI.BringWindowToFront(889912);
            popupRect = GUI.Window(889912, popupRect, DrawTagPopup, "🎯 CHỌN LOẠI VẬT PHẨM CHO HÒM");
        }
    }

    // =========================================================================
    // GIAO DIỆN CHÍNH (F2) - THIẾT KẾ CARD HIỆN ĐẠI, THOÁNG ĐÃNG
    // =========================================================================
    private void DrawMainWindow(int windowId)
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
        GUILayout.Label("Danh Sách Hòm Chứa: Bấm [Chọn Tag ▾] để gán trực tiếp loại đồ nhận ưu tiên.");
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        List<NamedInventory> allInventories = GetAvailableInventories();
        List<NamedInventory> displayInventories = new List<NamedInventory>();

        foreach (var ni in allInventories)
        {
            if (ni.HasId || showAllStorages)
            {
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

                int currentSetting = -1;
                if (customAssignments.ContainsKey(id))
                {
                    currentSetting = customAssignments[id];
                }
                else if (id == "hidden_stash")
                {
                    currentSetting = 0; // Kho lậu luôn mặc định khóa
                }

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

                // Dòng 2: Badge trạng thái + Nhóm nút bấm thao tác
                GUILayout.BeginHorizontal();

                // Trạng thái hiện tại
                if (currentSetting == 0)
                {
                    GUI.backgroundColor = new Color(0.95f, 0.35f, 0.35f);
                    GUILayout.Label("⛔ ĐÃ KHÓA (Đóng băng 100%)", EditorStyles_BadgeBox(), GUILayout.Width(310), GUILayout.Height(26));
                }
                else if (currentSetting == -1)
                {
                    int autoCat = ni.DefaultCategory;
                    string catName = (autoCat >= 0 && autoCat < CategoryNames.Length) ? CategoryNames[autoCat] : "Chung";
                    GUI.backgroundColor = new Color(0.35f, 0.65f, 0.95f);
                    GUILayout.Label("🤖 Tự Động: " + catName, EditorStyles_BadgeBox(), GUILayout.Width(310), GUILayout.Height(26));
                }
                else
                {
                    string catName = (currentSetting >= 0 && currentSetting < CategoryNames.Length) ? CategoryNames[currentSetting] : "Kho Chung";
                    GUI.backgroundColor = new Color(0.95f, 0.75f, 0.15f);
                    GUILayout.Label("⭐ Ưu Tiên: " + catName, EditorStyles_BadgeBox(), GUILayout.Width(310), GUILayout.Height(26));
                }
                GUI.backgroundColor = Color.white;

                GUILayout.Space(10);

                // NÚT CHÍNH: MỞ POPUP DANH SÁCH TAG ĐỂ CHỌN TRỰC TIẾP
                GUI.backgroundColor = new Color(0.3f, 0.85f, 0.5f);
                if (GUILayout.Button("🎯 Chọn Tag ▾", GUILayout.Width(140), GUILayout.Height(26)))
                {
                    activePopupContainerId = id;
                    activePopupContainerName = ni.DisplayName;
                    popupRect.x = windowRect.x + 90;
                    popupRect.y = windowRect.y + 50;
                }
                GUI.backgroundColor = Color.white;

                // Nút nhanh: Tự động
                if (GUILayout.Button("🤖 Auto", GUILayout.Width(75), GUILayout.Height(26)))
                {
                    customAssignments[id] = -1;
                    SaveAssignments();
                }

                // Nút nhanh: Khóa / Mở khóa
                if (currentSetting == 0)
                {
                    GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
                    if (GUILayout.Button("🔓 Mở", GUILayout.Width(75), GUILayout.Height(26)))
                    {
                        customAssignments[id] = -1;
                        SaveAssignments();
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
                    if (GUILayout.Button("⛔ Khóa", GUILayout.Width(75), GUILayout.Height(26)))
                    {
                        customAssignments[id] = 0;
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
            activePopupContainerId = null;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndHorizontal();

        GUI.DragWindow(new Rect(0, 0, 860, 28));
    }

    // =========================================================================
    // MODAL POPUP: BẢNG CHỌN DANH MỤC & TAG TRỰC TIẾP (1 CLICK)
    // =========================================================================
    private void DrawTagPopup(int windowId)
    {
        GUILayout.Space(6);

        GUI.color = new Color(1.0f, 0.9f, 0.4f);
        GUILayout.Label("Hòm đang chọn: " + activePopupContainerName, EditorStyles_BoldLabel());
        GUI.color = Color.white;
        GUILayout.Label("Click trực tiếp vào danh mục hoặc tag dưới đây để gán ngay cho hòm:");

        GUILayout.Space(6);

        // Các tùy chọn nhanh hàng đầu
        GUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.4f, 0.7f, 1.0f);
        if (GUILayout.Button("🤖 Tự Động (Theo Hòm)", GUILayout.Height(32)))
        {
            SelectCategoryForActiveContainer(-1);
        }

        GUI.backgroundColor = new Color(0.95f, 0.4f, 0.4f);
        if (GUILayout.Button("⛔ Bỏ Qua / Khóa Hòm", GUILayout.Height(32)))
        {
            SelectCategoryForActiveContainer(0);
        }

        GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f);
        if (GUILayout.Button("📦 Kho Chung (Đồ Khác)", GUILayout.Height(32)))
        {
            SelectCategoryForActiveContainer(21);
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        // Danh sách dạng lưới 2 cột các danh mục chi tiết
        popupScrollPos = GUILayout.BeginScrollView(popupScrollPos, GUILayout.Height(350));

        // 20 danh mục (từ 1 đến 20)
        for (int i = 1; i <= 20; i += 2)
        {
            GUILayout.BeginHorizontal();

            // Cột 1
            if (GUILayout.Button(CategoryNames[i], GUILayout.Height(30), GUILayout.Width(305)))
            {
                SelectCategoryForActiveContainer(i);
            }

            // Cột 2
            if (i + 1 <= 20)
            {
                if (GUILayout.Button(CategoryNames[i + 1], GUILayout.Height(30), GUILayout.Width(305)))
                {
                    SelectCategoryForActiveContainer(i + 1);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        GUILayout.EndScrollView();

        GUILayout.Space(8);

        // Nút hủy / đóng popup
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        if (GUILayout.Button("❌ Hủy / Đóng Bảng", GUILayout.Width(180), GUILayout.Height(28)))
        {
            activePopupContainerId = null;
        }
        GUI.backgroundColor = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUI.DragWindow(new Rect(0, 0, 680, 28));
    }

    private void SelectCategoryForActiveContainer(int catIndex)
    {
        if (!string.IsNullOrEmpty(activePopupContainerId))
        {
            customAssignments[activePopupContainerId] = catIndex;
            SaveAssignments();
            string catName = (catIndex == -1) ? "Tự Động" : (catIndex >= 0 && catIndex < CategoryNames.Length ? CategoryNames[catIndex] : "Kho Chung");
            statusMessage = "Đã gán [" + catName + "] cho " + activePopupContainerName;
            activePopupContainerId = null;
        }
    }

    // =========================================================================
    // 3. THUẬT TOÁN PHÂN PHỐI & LẤP ĐẦY ƯU TIÊN THEO 2 TẦNG
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

            // Xây dựng danh sách hòm theo thứ tự ưu tiên cho từng Category (1..21)
            Dictionary<int, List<GameGridInventory>> priority1ByCategory = new Dictionary<int, List<GameGridInventory>>();
            Dictionary<int, List<GameGridInventory>> priority2ByCategory = new Dictionary<int, List<GameGridInventory>>();

            for (int i = 1; i <= 21; i++)
            {
                priority1ByCategory[i] = new List<GameGridInventory>();
                priority2ByCategory[i] = new List<GameGridInventory>();
            }

            foreach (var ni in inventories)
            {
                int setting = -1;
                if (!customAssignments.TryGetValue(ni.Id, out setting))
                {
                    if (ni.Id == "hidden_stash") setting = 0;
                    else setting = -1;
                }

                if (setting == 0)
                {
                    // Kho bị KHÓA / BỎ QUA -> TUYỆT ĐỐI KHÔNG NHẬN ĐỒ VÀO!
                    continue;
                }
                else if (setting >= 1 && setting <= 21)
                {
                    // Người dùng CHỈ ĐỊNH nhận nhóm này -> Ưu tiên 1
                    priority1ByCategory[setting].Add(ni.Inventory);
                }
                else
                {
                    // Chế độ Tự Động theo loại hòm
                    int autoCat = ni.DefaultCategory;
                    if (autoCat >= 1 && autoCat <= 21)
                    {
                        priority2ByCategory[autoCat].Add(ni.Inventory); // Ưu tiên 2
                    }
                }
            }

            int totalMoved = 0;

            // Quét từng kho nguồn
            foreach (var sourceNI in inventories)
            {
                GameGridInventory sourceInv = sourceNI.Inventory;
                if (sourceInv == null || sourceInv.childItems == null) continue;

                // 🛑 QUAN TRỌNG: NẾU KHO NÀY ĐANG BỊ KHÓA (setting == 0) -> ĐÓNG BĂNG 100%, TUYỆT ĐỐI KHÔNG RÚT ĐỒ RA!
                int srcSetting = -1;
                if (!customAssignments.TryGetValue(sourceNI.Id, out srcSetting))
                {
                    if (sourceNI.Id == "hidden_stash") srcSetting = 0;
                    else srcSetting = -1;
                }

                if (srcSetting == 0)
                {
                    // Kho bị khóa: Giữ nguyên 100%, bỏ qua kho này
                    continue;
                }

                // Lấy bản sao danh sách item để duyệt an toàn
                List<GameItem> itemsToProcess = new List<GameItem>(sourceInv.childItems);

                foreach (var item in itemsToProcess)
                {
                    if (item == null) continue;
                    // Bỏ qua đồ đã ghim, rác bẩn, hoặc đồ là hòm/túi đựng đồ (không nhét hòm vào trong hòm)
                    if (ItemPinHelper.IsPinned(item) || DebrisHelper.IsDebris(item)) continue;
                    if (item.contentWindow != null || item.IsTag("CONTAINER") || item.IsTag("POUCH") || item.IsTag("BACKPACK") || item.IsTag("LOCKED_CONTAINER_TAG")) continue;

                    // Lấy Category Index của món đồ
                    int itemCat = GetItemCategoryIndex(item);
                    if (itemCat <= 0) continue;

                    List<GameGridInventory> p1List = priority1ByCategory.ContainsKey(itemCat) ? priority1ByCategory[itemCat] : null;
                    List<GameGridInventory> p2List = priority2ByCategory.ContainsKey(itemCat) ? priority2ByCategory[itemCat] : null;

                    if ((p1List == null || p1List.Count == 0) && (p2List == null || p2List.Count == 0)) continue;

                    bool moved = false;

                    // BƯỚC 1: Thử đưa vào các hòm được người dùng CHỈ ĐỊNH ƯU TIÊN (Priority 1)
                    if (p1List != null && !p1List.Contains(sourceInv))
                    {
                        foreach (var destInv in p1List)
                        {
                            if (destInv == sourceInv) continue;
                            if (!CanContainerAcceptItem(destInv, item)) continue;

                            sourceInv.Expel(item);
                            destInv.UncheckedAccept(item);

                            if (InventorySortHelper.Sort(destInv, true, false))
                            {
                                totalMoved++;
                                moved = true;
                                break;
                            }
                            else
                            {
                                // Hòm này đã đầy, hoàn trả lại hòm nguồn và thử hòm tiếp theo
                                destInv.Expel(item);
                                sourceInv.UncheckedAccept(item);
                            }
                        }
                    }

                    // BƯỚC 2: Nếu chưa chuyển được (Priority 1 đầy hoặc không có) -> Đưa vào hòm TỰ ĐỘNG (Priority 2)
                    if (!moved && p2List != null && !p2List.Contains(sourceInv))
                    {
                        foreach (var destInv in p2List)
                        {
                            if (destInv == sourceInv) continue;
                            if (!CanContainerAcceptItem(destInv, item)) continue;

                            sourceInv.Expel(item);
                            destInv.UncheckedAccept(item);

                            if (InventorySortHelper.Sort(destInv, true, false))
                            {
                                totalMoved++;
                                moved = true;
                                break;
                            }
                            else
                            {
                                // Hòm mặc định đã đầy, hoàn trả lại hòm nguồn
                                destInv.Expel(item);
                                sourceInv.UncheckedAccept(item);
                            }
                        }
                    }
                }
            }

            // Sau khi phân phối xong, tự động sắp xếp nén khít toàn bộ các kho & hòm (TRỪ CÁC KHO BỊ KHÓA)
            // Nhờ ForceSortAutoRotatePatch, toàn bộ item sẽ được tự động xoay ngang/dọc để lấp khít hoàn hảo
            foreach (var ni in inventories)
            {
                int invSetting = -1;
                if (!customAssignments.TryGetValue(ni.Id, out invSetting))
                {
                    if (ni.Id == "hidden_stash") invSetting = 0;
                    else invSetting = -1;
                }

                if (invSetting == 0) continue;

                if (ni.Inventory != null && ni.Inventory.childItems != null && ni.Inventory.childItems.Count > 0)
                {
                    InventorySortHelper.Sort(ni.Inventory, true, false);
                    ni.Inventory.Validate();
                }
            }

            statusMessage = "✅ Đã phân loại & sắp xếp khít chặt toàn bộ kho! (Đã chuyển " + totalMoved + " món đồ theo ưu tiên)";
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
                int invSetting = -1;
                if (!customAssignments.TryGetValue(ni.Id, out invSetting))
                {
                    if (ni.Id == "hidden_stash") invSetting = 0;
                    else invSetting = -1;
                }

                if (invSetting == 0) continue;

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

    // =========================================================================
    // 4. KIỂM TRA ĐIỀU KIỆN CHẤP NHẬN CỦA HÒM CHỨA (An toàn logic game)
    // =========================================================================
    private static bool CanContainerAcceptItem(GameGridInventory destInv, GameItem item)
    {
        if (destInv == null || item == null) return false;
        if (destInv.IsInsertLocked()) return false;

        if (destInv.mayInventoryAddItemFunc != null)
        {
            try
            {
                if (!destInv.mayInventoryAddItemFunc(item, destInv))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        return true;
    }

    // =========================================================================
    // 5. BỘ NHẬN DIỆN DANH MỤC 100% TOÀN DIỆN VẬT PHẨM VÀ TAG
    // =========================================================================
    public static int GetItemCategoryIndex(GameItem item)
    {
        if (item == null) return 21; // 21 = Kho chung

        // 1. Kiểm tra tất cả GameItemType của item
        List<string> types = item.GetGameItemType();
        if (types != null && types.Count > 0)
        {
            foreach (string t in types)
            {
                if (string.IsNullOrEmpty(t)) continue;
                int cat = MapTagToCategoryIndex(t.ToUpperInvariant());
                if (cat > 0) return cat;
            }
        }

        // 2. Kiểm tra các Tag trong item.state
        if (item.state != null && item.state.dict != null)
        {
            foreach (var key in item.state.dict.Keys)
            {
                if (key == null) continue;
                int cat = MapTagToCategoryIndex(key.ToString().ToUpperInvariant());
                if (cat > 0) return cat;
            }
        }

        // 3. Kiểm tra IsTag của GameItem
        if (item.IsTag("FIREARM") || item.IsTag("WEAPON") || item.IsTag("MELEE_WEAPON") || item.IsTag("ADV_GUN_TAG")) return 1;
        if (item.IsTag("AMMUNITION") || item.IsTag("MUNITION") || item.IsTag("AMMO_BOX_TAG") || item.IsTag("AMMO_TYPE_TAG")) return 2;
        if (item.IsTag("GUN_MOD") || item.IsTag("GUN_CALIBER_TAG") || item.IsTag("GUN_SUPPRESSED_TAG")) return 3;
        if (item.IsTag("MEDICAL") || item.IsTag("MEDICAL_TOOL") || item.IsTag("ITEM_MEDICAL_TAG")) return 4;
        if (item.IsTag("FOOD") || item.IsTag("PROCESSED_FOOD") || item.IsTag("TREAT") || item.IsTag("MEAT_FOOD_TAG") || item.IsTag("FRESH_FOOD_TAG") || item.IsTag("CANNED_FOOD_TAG")) return 5;
        if (item.IsTag("WATER") || item.IsTag("WATER_RATION") || item.IsTag("BREVAGE") || item.IsTag("WATER_PURIFICATION_SUPPLY") || item.IsTag("WATER_PURIFICATION_TABLET") || item.IsTag("FILTER") || item.IsTag("WATER_FILTER")) return 6;
        if (item.IsTag("ALCOHOL") || item.IsTag("WINE_CONTAINER_TAG")) return 7;
        if (item.IsTag("NARCOTIC") || item.IsTag("SUBSTANCE") || item.IsTag("POISON") || item.IsTag("CONTRABAND") || item.IsTag("CIGARETTE_TAG")) return 8;
        if (item.IsTag("ARMOR") || item.IsTag("TACTICAL_EQUIPMENT_TAG") || item.IsTag("ARMORED")) return 9;
        if (item.IsTag("MATERIAL") || item.IsTag("BASIC_MATERIAL") || item.IsTag("CONSTRUCTION_MATERIAL_TAG") || item.IsTag("SCRAP_METAL") || item.IsTag("ORE_TAG") || item.IsTag("CHEMICAL_SUPPLIES")) return 10;
        if (item.IsTag("TOOL") || item.IsTag("TECHNICAL_EQUIPMENT_TAG") || item.IsTag("TOOLBOX_TAG") || item.IsTag("MAGNIFYING_GLASS") || item.IsTag("LABELER")) return 11;
        if (item.IsTag("DOCUMENT") || item.IsTag("ACCESS_CARD") || item.IsTag("LOTTERY") || item.IsTag("LOTTERY_PAPER_TAG") || item.IsTag("GUIDE_PAPER_TAG") || item.IsTag("NAV_DISK_TAG")) return 12;
        if (item.IsTag("ORGAN") || item.IsTag("ORGAN_ITEM_TAG") || item.IsTag("ORGANIC_ORGAN_TAG") || item.IsTag("ORGAN_TRANSPORT_CONTAINER_TAG")) return 13;
        if (item.IsTag("MODULE") || item.IsTag("MACHINE") || item.IsTag("PRINTER_MODULE") || item.IsTag("HYDROPONIC_MODULE") || item.IsTag("EVAPORATOR_MODULE") || item.IsTag("ALARM_MODULE") || item.IsTag("NODE")) return 14;
        if (item.IsTag("ENERGY_CELL") || item.IsTag("HYPERTECH_BATTERY_TAG")) return 15;
        if (item.IsTag("SEED") || item.IsTag("SEED_ITEM_TAG") || item.IsTag("FARMING_SUPPLIES")) return 16;
        if (item.IsTag("HOUSEHOLD_GOOD") || item.IsTag("LUXURY_ITEM")) return 17;
        if (item.IsTag("CONTAINER_TAG") || item.IsTag("SUPPLY_CRATE") || item.IsTag("STORAGE") || item.IsTag("SAFE_TAG") || item.IsTag("POUCH_TAG") || item.IsTag("STORABLE_PACKAGE_TAG") || item.IsTag("LIQUID_CONTAINER_TAG") || item.IsTag("CAGE_TAG")) return 18;
        if (item.IsTag("ART")) return 19;
        if (item.IsTag("JUNK")) return 20;

        // 4. Fallback nhận diện theo identifier / tên
        string id = (item.identifier ?? "").ToLowerInvariant();
        string name = (item.name ?? "").ToLowerInvariant();

        if (id.Contains("gun") || id.Contains("pistol") || id.Contains("smg") || id.Contains("shotgun") || id.Contains("rifle") || id.Contains("knife") || id.Contains("sword") || id.Contains("bat") || id.Contains("shiv") || id.Contains("weapon")) return 1;
        if (id.Contains("ammo") || id.Contains("round") || id.Contains("bullet") || id.Contains("grenade") || id.Contains("c4") || id.Contains("bomb")) return 2;
        if (id.Contains("mod") || id.Contains("sight") || id.Contains("silencer") || id.Contains("mag_ext") || id.Contains("scope")) return 3;
        if (id.Contains("med") || id.Contains("pill") || id.Contains("bandage") || id.Contains("firstaid") || id.Contains("heal") || id.Contains("stim") || id.Contains("cure") || id.Contains("phagi") || id.Contains("bicarsole")) return 4;
        if (id.Contains("food") || id.Contains("bread") || id.Contains("meat") || id.Contains("cheese") || id.Contains("fish") || id.Contains("snack") || id.Contains("icecream") || id.Contains("egg")) return 5;
        if (id.Contains("water") || id.Contains("drink") || id.Contains("juice") || id.Contains("milk") || id.Contains("soda") || id.Contains("pitcher") || id.Contains("bottle")) return 6;
        if (id.Contains("wine") || id.Contains("beer") || id.Contains("vodka") || id.Contains("nudka") || id.Contains("alcohol")) return 7;
        if (id.Contains("drug") || id.Contains("narcotic") || id.Contains("fentanyl") || id.Contains("cigar") || id.Contains("smoke") || id.Contains("poison") || id.Contains("zyanide") || id.Contains("contraband")) return 8;
        if (id.Contains("armor") || id.Contains("helmet") || id.Contains("vest") || id.Contains("shield") || id.Contains("plate")) return 9;
        if (id.Contains("material") || id.Contains("ingot") || id.Contains("metal") || id.Contains("ore") || id.Contains("wire") || id.Contains("plastic") || id.Contains("scrap") || id.Contains("nut") || id.Contains("screw")) return 10;
        if (id.Contains("tool") || id.Contains("cutter") || id.Contains("wrench") || id.Contains("scanner") || id.Contains("analyser") || id.Contains("pipe") || id.Contains("crowbar")) return 11;
        if (id.Contains("card") || id.Contains("keycard") || id.Contains("ticket") || id.Contains("doc") || id.Contains("book") || id.Contains("paper") || id.Contains("newspaper") || id.Contains("intel") || id.Contains("disk") || id.Contains("chip")) return 12;
        if (id.Contains("organ") || id.Contains("heart") || id.Contains("liver") || id.Contains("kidney") || id.Contains("eye") || id.Contains("tissue")) return 13;
        if (id.Contains("module") || id.Contains("machine") || id.Contains("engine") || id.Contains("pump") || id.Contains("fan") || id.Contains("filter_module")) return 14;
        if (id.Contains("cell") || id.Contains("battery") || id.Contains("power")) return 15;
        if (id.Contains("seed") || id.Contains("plant") || id.Contains("crop")) return 16;
        if (id.Contains("tampon") || id.Contains("shampoo") || id.Contains("towel") || id.Contains("soap") || id.Contains("cream") || id.Contains("perfume") || id.Contains("condom")) return 17;
        if (id.Contains("box") || id.Contains("case") || id.Contains("crate") || id.Contains("safe") || id.Contains("pouch") || id.Contains("bag") || id.Contains("container")) return 18;
        if (id.Contains("art") || id.Contains("cassette") || id.Contains("music") || id.Contains("statue") || id.Contains("toy")) return 19;
        if (id.Contains("junk") || id.Contains("waste") || id.Contains("trash") || id.Contains("rubbish")) return 20;

        return 21; // Kho chung
    }

    private static int MapTagToCategoryIndex(string tag)
    {
        switch (tag)
        {
            // 1. Súng & Vũ Khí
            case "WEAPON":
            case "FIREARM":
            case "MELEE_WEAPON":
            case "ADV_GUN_TAG":
            case "LOWTECH":
            case "MIDLINE":
            case "MODDABLE":
                return 1;

            // 2. Đạn Dược & Bom
            case "AMMUNITION":
            case "MUNITION":
            case "AMMO_BOX_TAG":
            case "AMMO_TYPE_TAG":
                return 2;

            // 3. Phụ Kiện Súng
            case "GUN_MOD":
            case "GUN_CALIBER_TAG":
            case "GUN_SUPPRESSED_TAG":
            case "MAKESHIFT":
                return 3;

            // 4. Y Tế & Dược Phẩm
            case "MEDICAL":
            case "MEDICAL_TOOL":
            case "ITEM_MEDICAL_TAG":
            case "MEDICAL_PROFICIENCY":
                return 4;

            // 5. Thực Phẩm & Đồ Ăn
            case "FOOD":
            case "PROCESSED_FOOD":
            case "TREAT":
            case "MEAT_FOOD_TAG":
            case "FRESH_FOOD_TAG":
            case "CANNED_FOOD_TAG":
                return 5;

            // 6. Nước Uống & Lọc Nước
            case "WATER":
            case "WATER_RATION":
            case "BREVAGE":
            case "WATER_PURIFICATION_SUPPLY":
            case "WATER_PURIFICATION_TABLET":
            case "WATER_TESTING_STRIP_TAG":
            case "FILTER":
            case "WATER_FILTER":
                return 6;

            // 7. Rượu Bia & Đồ Uống Cồn
            case "ALCOHOL":
            case "RED_BEER":
            case "BEER_CASE_0":
            case "WINE_CONTAINER_TAG":
                return 7;

            // 8. Thuốc Lá & Hàng Cấm
            case "NARCOTIC":
            case "SUBSTANCE":
            case "POISON":
            case "CONTRABAND":
            case "CIGARETTE_TAG":
            case "RESTRICTED_1":
            case "RESTRICTED_2":
            case "RESTRICTED_3":
                return 8;

            // 9. Áo Giáp & Đồ Bảo Hộ
            case "ARMOR":
            case "TACTICAL_EQUIPMENT_TAG":
            case "ARMORED":
                return 9;

            // 10. Vật Liệu Chế Tạo
            case "MATERIAL":
            case "BASIC_MATERIAL":
            case "CONSTRUCTION_MATERIAL_TAG":
            case "SCRAP_METAL":
            case "ORE_TAG":
            case "CHEMICAL_SUPPLIES":
            case "PRINTER_MATERIAL_TAG":
                return 10;

            // 11. Công Cụ & Dụng Cụ
            case "TOOL":
            case "TECHNICAL_EQUIPMENT_TAG":
            case "TOOLBOX_TAG":
            case "MAGNIFYING_GLASS":
            case "LABELER":
            case "LABELER_BLUE":
            case "MARKING_PEN_TAG":
                return 11;

            // 12. Tài Liệu, Thẻ Từ & Vé Số
            case "DOCUMENT":
            case "ACCESS_CARD":
            case "LOTTERY":
            case "LOTTERY_PAPER_TAG":
            case "LOTTERY_IS_WINNING_TAG":
            case "GUIDE_PAPER_TAG":
            case "NAV_DISK_TAG":
            case "TUTORIAL_PAPER_TAG":
                return 12;

            // 13. Nội Tạng & Mẫu Sinh Học
            case "ORGAN":
            case "ORGAN_ITEM_TAG":
            case "ORGANIC_ORGAN_TAG":
            case "ORGAN_TRANSPORT_CONTAINER_TAG":
                return 13;

            // 14. Mô-Đun & Máy Móc
            case "MODULE":
            case "MACHINE":
            case "PRINTER_MODULE":
            case "HYDROPONIC_MODULE":
            case "EVAPORATOR_MODULE":
            case "ALARM_MODULE":
            case "NODE":
            case "BROKEN_MACHINE_TAG":
            case "MODULE_TYPE_ALARM":
            case "MODULE_TYPE_CHEM":
            case "MODULE_TYPE_EVAPORATOR":
            case "MODULE_TYPE_FURNACE":
            case "MODULE_TYPE_HULL":
            case "MODULE_TYPE_HYDROPONIC":
            case "MODULE_TYPE_NAVIGATION":
            case "MODULE_TYPE_PRINTER":
            case "MODULE_TYPE_UNIVERSAL":
                return 14;

            // 15. Năng Lượng & Pin
            case "ENERGY_CELL":
            case "POWERBLOCK":
            case "HYPERTECH_CHARGE_RATE_TAG":
                return 15;

            // 16. Hạt Giống & Nông Nghiệp
            case "SEED":
            case "SEED_ITEM_TAG":
            case "FARMING_SUPPLIES":
            case "MODIFIED_SEED":
                return 16;

            // 17. Đồ Gia Dụng & Xa Xỉ
            case "HOUSEHOLD_GOOD":
            case "LUXURY_ITEM":
                return 17;

            // 18. Hòm & Hộp Chứa
            case "SUPPLY_CRATE":
            case "CONTAINER_TAG":
            case "STORAGE":
            case "SAFE_TAG":
            case "POUCH_TAG":
            case "STORABLE_PACKAGE_TAG":
            case "LIQUID_CONTAINER_TAG":
            case "CAGE_TAG":
                return 18;

            // 19. Nghệ Thuật & Quà
            case "ART":
                return 19;

            // 20. Phế Liệu & Rác
            case "JUNK":
            case "HAZARDOUS_WASTE_TAG":
                return 20;

            default:
                return 0;
        }
    }

    private int DetectDefaultCategory(string rawName, GameItem item)
    {
        string n = (rawName ?? "").ToLowerInvariant();
        string id = (item != null && item.identifier != null) ? item.identifier.ToLowerInvariant() : "";

        if (n.Contains("sơ cứu") || n.Contains("y tế") || n.Contains("thuốc") || n.Contains("first aid") || n.Contains("med") || id.Contains("med")) return 4;
        if (n.Contains("đạn") || n.Contains("ammo") || id.Contains("ammo")) return 2;
        if (n.Contains("vũ khí") || n.Contains("súng") || n.Contains("an ninh") || n.Contains("weapon") || n.Contains("sec") || n.Contains("security") || id.Contains("sec")) return 1;
        if (n.Contains("phụ kiện") || n.Contains("gun mod") || id.Contains("gun_mod")) return 3;
        if (n.Contains("cơm") || n.Contains("ăn") || n.Contains("thực phẩm") || n.Contains("bếp") || n.Contains("lunch") || n.Contains("food") || n.Contains("ration") || id.Contains("lunch")) return 5;
        if (n.Contains("nước") || n.Contains("water") || n.Contains("pitcher") || id.Contains("water")) return 6;
        if (n.Contains("rượu") || n.Contains("bia") || n.Contains("wine") || n.Contains("beer")) return 7;
        if (n.Contains("hàng cấm") || n.Contains("thuốc lá") || n.Contains("lậu") || n.Contains("contraband")) return 8;
        if (n.Contains("giáp") || n.Contains("armor") || n.Contains("vest")) return 9;
        if (n.Contains("vật liệu") || n.Contains("kim loại") || n.Contains("material") || n.Contains("ore")) return 10;
        if (n.Contains("dụng cụ") || n.Contains("công cụ") || n.Contains("sửa") || n.Contains("tool") || id.Contains("tool")) return 11;
        if (n.Contains("két") || n.Contains("tài liệu") || n.Contains("thẻ") || n.Contains("safe") || n.Contains("doc")) return 12;
        if (n.Contains("nội tạng") || n.Contains("sinh học") || n.Contains("organ") || id.Contains("organ")) return 13;
        if (n.Contains("mô-đun") || n.Contains("module") || n.Contains("chip") || id.Contains("module")) return 14;
        if (n.Contains("pin") || n.Contains("năng lượng") || n.Contains("battery") || n.Contains("power")) return 15;
        if (n.Contains("hạt giống") || n.Contains("nông") || n.Contains("seed") || id.Contains("seed")) return 16;
        if (n.Contains("gia dụng") || n.Contains("tiện ích") || n.Contains("luxury")) return 17;
        if (n.Contains("hòm") || n.Contains("thùng") || n.Contains("crate") || n.Contains("box")) return 18;

        return 21; // Kho chung
    }

    // =========================================================================
    // 6. QUÉT TẤT CẢ KHO & HÒM ĐỒ (KÈM VỊ TRÍ NGUỒN GỐC & ID BỀN VỮNG)
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
                    if (ggi != null && !result.Contains(ggi))
                    {
                        result.Add(ggi);
                    }
                }
            }
        }
        catch {}

        if (pw.children != null)
        {
            foreach (var ch in pw.children)
            {
                GameGridInventory ggi = ch as GameGridInventory;
                if (ggi != null && !result.Contains(ggi))
                {
                    result.Add(ggi);
                }
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
                if (ggi != null && !result.Contains(ggi))
                {
                    result.Add(ggi);
                }
            }
        }

        return result;
    }

    private static string GetContainerPersistentId(GameItem item, string fallbackName)
    {
        if (item != null)
        {
            string baseId = !string.IsNullOrEmpty(item.identifier) ? item.identifier : item.name;
            if (item.uniqueId > 0)
            {
                return "box_" + baseId + "_" + item.uniqueId;
            }
            if (!string.IsNullOrEmpty(item.identifier))
            {
                return "box_ident_" + item.identifier;
            }
        }
        return "box_hash_" + Math.Abs((fallbackName ?? "container").GetHashCode() % 100000);
    }

    public List<NamedInventory> GetAvailableInventories()
    {
        List<NamedInventory> list = new List<NamedInventory>();
        HashSet<GameGridInventory> seenInvs = new HashSet<GameGridInventory>();

        EmporiumEntry emporium = EmporiumEntry.Instance;
        if (emporium != null)
        {
            AddIfValid(list, seenInvs, "main_inv", "🏢 Kho Tiệm Chính (Bàn Thao Tác)", emporium.invElement, 21, false);
            AddIfValid(list, seenInvs, "showcase", "✨ Kệ Kính Trưng Bày", emporium.showcaseElement, 21, false);
            AddIfValid(list, seenInvs, "front_box", "📦 Hòm Đồ Trước Quầy", emporium.frontInvinvElement, 21, false);
            AddIfValid(list, seenInvs, "back_box", "📦 Hòm Đồ Sau Quầy", emporium.backInvinvElement, 21, false);
            AddIfValid(list, seenInvs, "counter_box", "🗄️ Hòm Quầy Thu Ngân", emporium.backInvinvElementCounter, 21, false);
            AddIfValid(list, seenInvs, "bazar_left", "🏪 Hòm Chợ Trái", emporium.bazarLeftinvElement, 21, false);
            AddIfValid(list, seenInvs, "afterhour", "🌙 Kho Ngoài Giờ", emporium.afterhourInventory, 21, false);

            // Kho Lậu / Hòm Ẩn: Mặc định luôn là 0 (Khóa / Bỏ qua)
            AddIfValid(list, seenInvs, "hidden_stash", "🕵️ Kho Lậu / Hòm Ẩn", emporium.hiddenElement, 0, false);
        }

        // 1. Quét tất cả cửa sổ hòm đang mở (WindowsHandler.current.visibleWindows)
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
                    int defCat = DetectDefaultCategory(rawName, parentItem);

                    string id = GetContainerPersistentId(parentItem, rawName);
                    int shortUid = (parentItem != null && parentItem.uniqueId > 0) ? parentItem.uniqueId : Math.Abs(pw.GetHashCode() % 1000);

                    foreach (var ggi in storages)
                    {
                        if (ggi != null && !seenInvs.Contains(ggi))
                        {
                            string displayName = "🔓 " + rawName + " (#" + shortUid + ") [Đang Mở]";
                            AddIfValid(list, seenInvs, id, displayName, ggi, defCat, true);
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
                        int defCat = DetectDefaultCategory(it.name, it);
                        int shortUid = (it.uniqueId > 0) ? it.uniqueId : Math.Abs(it.GetHashCode() % 1000);

                        foreach (var ggi in storages)
                        {
                            if (ggi != null && !seenInvs.Contains(ggi))
                            {
                                string displayName = "💼 " + it.name + " (#" + shortUid + ") [" + source.locationName + "]";
                                AddIfValid(list, seenInvs, id, displayName, ggi, defCat, true);
                            }
                        }
                    }
                }
            }
        }
        catch {}

        return list;
    }

    private void AddIfValid(List<NamedInventory> list, HashSet<GameGridInventory> seen, string id, string name, GameGridInventory inv, int defCat, bool hasId)
    {
        if (inv != null && inv.inventoryShape != null && inv.inventoryShape.width > 0 && inv.inventoryShape.height > 0)
        {
            if (seen.Contains(inv)) return;
            seen.Add(inv);

            NamedInventory ni = new NamedInventory();
            ni.Id = id;
            ni.DisplayName = name;
            ni.Inventory = inv;
            ni.DefaultCategory = defCat;
            ni.HasId = hasId;
            list.Add(ni);
        }
    }

    // =========================================================================
    // 7. LƯU & TẢI CẤU HÌNH GÁN HÒM (PERSISTENCE)
    // =========================================================================
    private string GetConfigPath()
    {
        try
        {
            string dir = Path.Combine(Application.persistentDataPath, "ModConfigs");
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            return Path.Combine(dir, "SmartInventoryCategoryConfig.txt");
        }
        catch
        {
            return "SmartInventoryCategoryConfig.txt";
        }
    }

    private void SaveAssignments()
    {
        try
        {
            string path = GetConfigPath();
            List<string> lines = new List<string>();
            foreach (var kvp in customAssignments)
            {
                lines.Add(kvp.Key + "=" + kvp.Value);
            }
            File.WriteAllLines(path, lines.ToArray());
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
            string path = GetConfigPath();
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        int val;
                        if (int.TryParse(parts[1].Trim(), out val))
                        {
                            customAssignments[parts[0].Trim()] = val;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[SmartInventorySort] Không thể đọc cấu hình: " + ex.Message);
        }
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

public class NamedInventory
{
    public string Id;
    public string DisplayName;
    public GameGridInventory Inventory;
    public int DefaultCategory;
    public bool HasId;
}
