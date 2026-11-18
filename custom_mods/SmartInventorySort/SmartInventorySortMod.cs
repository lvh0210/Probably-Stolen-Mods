using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

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
                Debug.Log("[SmartInventorySort] Đã kích hoạt hệ thống Sắp Xếp Kho Tối Ưu (PackFirstFit).");
            }

            if (uiObject == null)
            {
                uiObject = new GameObject("SmartCategorySortUI");
                UnityEngine.Object.DontDestroyOnLoad(uiObject);
                uiObject.AddComponent<SmartCategorySortController>();
                Debug.Log("[SmartInventorySort] Đã kích hoạt giao diện Phân Loại Đa Hòm (F2: Menu cấu hình & ưu tiên, F3: Gom & Phân loại nhanh).");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[SmartInventorySort] Lỗi kích hoạt: " + ex);
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
            Debug.Log("[SmartInventorySort] Đã hủy kích hoạt.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[SmartInventorySort] Lỗi hủy kích hoạt: " + ex);
        }
    }
}

// =========================================================================
// 1. HARMONY PATCH: Vô hiệu hóa PackShelves để kích hoạt PackFirstFit chuẩn
// =========================================================================
[HarmonyPatch(typeof(InventorySortHelper), "PackShelves")]
public static class DisablePackShelvesPatch
{
    public static bool Prefix(ref List<ValueTuple<GameItem, GridShape>> __result)
    {
        // Trả về null để ép game sử dụng thuật toán PackFirstFit (quét từng ô)
        // Loại bỏ hoàn toàn khoảng trống chết trên kệ mà không gây đè chồng item
        __result = null;
        return false;
    }
}

// =========================================================================
// 2. CONTROLLER & UI: Phân loại theo Danh Mục & Ưu Tiên Lấp Đầy Đa Hòm
// =========================================================================
public class SmartCategorySortController : MonoBehaviour
{
    public static SmartCategorySortController Instance;

    private bool showMenu = false;
    private Rect windowRect = new Rect(40, 40, 580, 530);
    private Vector2 scrollPos;
    private string statusMessage = "Bấm F2 để ẩn/hiện bảng. Bấm F3 để gom và phân loại theo ưu tiên.";

    // Danh sách 7 danh mục chính + Kho chung / Khóa
    public static readonly string[] CategoryNames = new string[]
    {
        "📦 Kho Chung / Bỏ Qua",
        "⚔️ Vũ Khí & Đạn",
        "💊 Y Tế & Dược Phẩm",
        "🍞 Thực Phẩm & Nước",
        "🛡️ Giáp & Trang Bị",
        "⚡ Linh Kiện & Module",
        "🔧 Vật Liệu & Công Cụ",
        "📄 Tài Liệu & Quý Giá"
    };

    // Cấu hình người dùng: key = Container Identifier
    // -1 = Tự động nhận diện theo tên hòm
    // 0 = Bỏ qua / Khóa (KHÔNG RÚT ĐỒ RA, KHÔNG NHÉT ĐỒ VÀO)
    // 1..7 = Người dùng CHỈ ĐỊNH nhận danh mục cụ thể (Priority 1)
    private Dictionary<string, int> customAssignments = new Dictionary<string, int>();

    private void Awake()
    {
        Instance = this;
        LoadSavedAssignments();
    }

    private void Update()
    {
        // F2: Bật/Tắt Menu Giao Diện Cấu Hình & Ưu Tiên Hòm Đồ
        if (Input.GetKeyDown(KeyCode.F2))
        {
            showMenu = !showMenu;
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
        windowRect = GUI.Window(889911, windowRect, DrawWindow, "📦 PHÂN LOẠI & SẮP XẾP HÒM ĐỒ ƯU TIÊN");
    }

    private void DrawWindow(int windowId)
    {
        GUILayout.Space(8);
        GUI.color = Color.yellow;
        GUILayout.Label(statusMessage);
        GUI.color = Color.white;
        GUILayout.Space(4);

        GUILayout.Label("Cấu hình Hòm Đồ (Click nút để đổi loại đồ nhận hoặc để Tự Động):", EditorStyles_BoldLabel());
        GUILayout.Label("💡 [⭐ Ưu Tiên] được lấp đầy trước. Kho bị [⛔ BỎ QUA / KHÓA] tuyệt đối không bị đụng tới.", EditorStyles_SmallItalic());

        List<NamedInventory> inventories = GetAvailableInventories();

        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
        foreach (var ni in inventories)
        {
            GUILayout.BeginHorizontal("box");

            // Tên hòm, kích thước và số lượng món hiện tại
            int count = (ni.Inventory.childItems != null) ? ni.Inventory.childItems.Count : 0;
            string labelText = ni.DisplayName + " (" + count + " món)";
            GUILayout.Label(labelText, GUILayout.Width(250));

            // Lấy trạng thái hiện tại của hòm
            string id = ni.Id;
            int currentSetting = -1;
            if (customAssignments.ContainsKey(id))
            {
                currentSetting = customAssignments[id];
            }
            else if (id == "hidden_stash")
            {
                // Kho lậu mặc định luôn là KHÓA (0) để bảo vệ tuyệt đối
                currentSetting = 0;
            }

            // Hiển thị nút bấm với nhãn tương ứng
            string btnText;
            if (currentSetting == -1)
            {
                int autoCat = ni.DefaultCategory;
                btnText = "🤖 Tự Động: " + CategoryNames[autoCat];
                GUI.backgroundColor = new Color(0.85f, 0.95f, 1.0f);
            }
            else if (currentSetting == 0)
            {
                btnText = "⛔ BỎ QUA / KHÓA";
                GUI.backgroundColor = new Color(0.95f, 0.45f, 0.45f);
            }
            else
            {
                btnText = "⭐ Ưu Tiên: " + CategoryNames[currentSetting];
                GUI.backgroundColor = new Color(1.0f, 0.9f, 0.4f);
            }

            // Click nút để xoay vòng cấu hình cho hòm này
            if (GUILayout.Button(btnText, GUILayout.Width(260), GUILayout.Height(26)))
            {
                // Chu kỳ: -1 (Tự động) -> 1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 0 (Khóa) -> -1 (Tự động)
                if (currentSetting == -1)
                    currentSetting = 1;
                else if (currentSetting >= 1 && currentSetting < 7)
                    currentSetting++;
                else if (currentSetting == 7)
                    currentSetting = 0;
                else
                    currentSetting = -1;

                customAssignments[id] = currentSetting;
                SaveAssignments();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        GUILayout.Space(8);
        GUILayout.BeginHorizontal();

        // Nút gom và phân loại tất cả theo ưu tiên
        GUI.backgroundColor = new Color(0.2f, 0.9f, 0.4f);
        if (GUILayout.Button("⚡ GOM & PHÂN LOẠI ƯU TIÊN (F3)", GUILayout.Height(36)))
        {
            ExecuteCategorySort();
        }

        // Nút nén khít chặt tất cả hòm
        GUI.backgroundColor = new Color(0.4f, 0.7f, 1.0f);
        if (GUILayout.Button("🧹 SẮP XẾP KHÍT HẾT KHO", GUILayout.Height(36)))
        {
            CompactAllInventories();
        }

        // Nút khôi phục toàn bộ về Tự Động
        GUI.backgroundColor = new Color(0.9f, 0.7f, 0.4f);
        if (GUILayout.Button("🔄 ĐẶT LẠI TỰ ĐỘNG", GUILayout.Height(36)))
        {
            customAssignments.Clear();
            customAssignments["hidden_stash"] = 0; // Luôn giữ kho lậu khóa an toàn
            SaveAssignments();
            statusMessage = "🔄 Đã đặt lại toàn bộ hòm về Tự Động (Kho Lậu vẫn được khóa an toàn)!";
        }

        // Nút đóng
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("ĐÓNG (F2)", GUILayout.Height(36), GUILayout.Width(75)))
        {
            showMenu = false;
        }

        GUILayout.EndHorizontal();
        GUI.DragWindow();
    }

    private GUIStyle EditorStyles_BoldLabel()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontStyle = FontStyle.Bold;
        return style;
    }

    private GUIStyle EditorStyles_SmallItalic()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontStyle = FontStyle.Italic;
        style.fontSize = 11;
        return style;
    }

    // =========================================================================
    // 3. LOGIC GOM & PHÂN PHỐI THEO THỨ TỰ ƯU TIÊN (2-TIER PRIORITY FILLING)
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

            // Xây dựng danh sách hòm theo thứ tự ưu tiên cho từng Category (1..7)
            // Priority 1: Hòm được người dùng CHỈ ĐỊNH cụ thể cho Category này
            // Priority 2: Hòm TỰ ĐỘNG nhận diện theo tên/loại hòm
            Dictionary<int, List<GameGridInventory>> priority1ByCategory = new Dictionary<int, List<GameGridInventory>>();
            Dictionary<int, List<GameGridInventory>> priority2ByCategory = new Dictionary<int, List<GameGridInventory>>();

            for (int i = 1; i < CategoryNames.Length; i++)
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
                else if (setting >= 1 && setting <= 7)
                {
                    // Người dùng CHỈ ĐỊNH nhận nhóm này -> Ưu tiên 1
                    priority1ByCategory[setting].Add(ni.Inventory);
                }
                else
                {
                    // Chế độ Tự Động theo tên hòm
                    int autoCat = ni.DefaultCategory;
                    if (autoCat >= 1 && autoCat <= 7)
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
                    if (item.contentWindow != null || item.IsTag("CONTAINER") || item.IsTag("POUCH") || item.IsTag("BACKPACK")) continue;

                    // Lấy Category Index của món đồ theo TAG ĐẦU TIÊN (Quy tắc người dùng)
                    int itemCat = GetItemCategoryIndex(item);
                    if (itemCat <= 0) continue; // Không thuộc 7 nhóm cần gom

                    List<GameGridInventory> p1List = priority1ByCategory[itemCat];
                    List<GameGridInventory> p2List = priority2ByCategory[itemCat];

                    // Nếu không có hòm nào (cả P1 lẫn P2) nhận loại đồ này -> Bỏ qua
                    if (p1List.Count == 0 && p2List.Count == 0) continue;

                    // KIỂM TRA ĐÃ Ở HÒM ĐÚNG CHƯA:
                    // 1. Nếu món đồ ĐÃ Ở trong một hòm Priority 1 -> GIỮ NGUYÊN (vì đã ở hòm ưu tiên cao nhất)
                    if (p1List.Contains(sourceInv)) continue;

                    // 2. Nếu món đồ đang ở trong một hòm Priority 2, NHƯNG còn hòm Priority 1:
                    // Ta vẫn thử chuyển vào Priority 1 để lấp đầy hòm được chọn trước!
                    // Nếu không có Priority 1 hoặc Priority 1 đã đầy, và món đồ đã ở Priority 2 -> GIỮ NGUYÊN.
                    if (p1List.Count == 0 && p2List.Contains(sourceInv)) continue;

                    bool moved = false;

                    // BƯỚC 1: Thử lấp đầy vào các hòm CHỈ ĐỊNH (Priority 1) trước
                    foreach (var destInv in p1List)
                    {
                        if (destInv == sourceInv) continue;

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

                    // BƯỚC 2: Nếu chưa chuyển được (Priority 1 đầy hoặc không có) và hiện tại chưa ở Priority 2
                    if (!moved && !p2List.Contains(sourceInv))
                    {
                        foreach (var destInv in p2List)
                        {
                            if (destInv == sourceInv) continue;

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
            foreach (var ni in inventories)
            {
                int invSetting = -1;
                if (!customAssignments.TryGetValue(ni.Id, out invSetting))
                {
                    if (ni.Id == "hidden_stash") invSetting = 0;
                    else invSetting = -1;
                }

                // Không xáo trộn kho đã bị khóa
                if (invSetting == 0) continue;

                if (ni.Inventory != null && ni.Inventory.childItems != null && ni.Inventory.childItems.Count > 0)
                {
                    InventorySortHelper.Sort(ni.Inventory, true, false);
                    ni.Inventory.Validate();
                }
            }

            statusMessage = "✅ Đã phân loại & sắp xếp lại toàn bộ kho! (Đã chuyển " + totalMoved + " món đồ theo ưu tiên)";
            Debug.Log("[SmartCategorySort] " + statusMessage);
        }
        catch (Exception ex)
        {
            statusMessage = "❌ Lỗi khi phân loại: " + ex.Message;
            Debug.LogError("[SmartCategorySort] " + ex);
        }
    }

    public void CompactAllInventories()
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

                // Không xáo trộn kho đã bị khóa
                if (invSetting == 0) continue;

                if (ni.Inventory != null && ni.Inventory.childItems != null && ni.Inventory.childItems.Count > 0)
                {
                    InventorySortHelper.Sort(ni.Inventory, true, false);
                    ni.Inventory.Validate();
                    count++;
                }
            }
            statusMessage = "✅ Đã nén khít chặt toàn bộ " + count + " kho & hòm đồ thành công!";
        }
        catch (Exception ex)
        {
            statusMessage = "❌ Lỗi: " + ex.Message;
        }
    }

    // =========================================================================
    // 4. NHẬN DIỆN DANH MỤC DỰA VÀO TAG ĐẦU TIÊN (Theo yêu cầu người dùng)
    // =========================================================================
    public static int GetItemCategoryIndex(GameItem item)
    {
        if (item == null) return 0;

        // 1. Kiểm tra GetGameItemType(): Lấy TAG ĐẦU TIÊN
        List<string> types = item.GetGameItemType();
        if (types != null && types.Count > 0)
        {
            string firstTag = types[0].ToUpperInvariant();
            int cat = MapTagToCategoryIndex(firstTag);
            if (cat > 0) return cat;
        }

        // 2. Fallback kiểm tra các tag phổ biến của game
        if (item.IsTag("WEAPON") || item.IsTag("FIREARM") || item.IsTag("MELEE_WEAPON") || item.IsTag("AMMUNITION") || item.IsTag("GUN_MOD"))
            return 1;
        if (item.IsTag("MEDICAL") || item.IsTag("ITEM_MEDICAL_TAG") || item.IsTag("NARCOTIC") || item.IsTag("POISON") || item.IsTag("ORGAN"))
            return 2;
        if (item.IsTag("FOOD") || item.IsTag("PROCESSED_FOOD") || item.IsTag("WATER") || item.IsTag("ALCOHOL") || item.IsTag("BREVAGE"))
            return 3;
        if (item.IsTag("ARMOR"))
            return 4;
        if (item.IsTag("MODULE") || item.IsTag("ENERGY_CELL"))
            return 5;
        if (item.IsTag("MATERIAL") || item.IsTag("TOOL") || item.IsTag("CHEMICAL_SUPPLIES"))
            return 6;
        if (item.IsTag("DOCUMENT") || item.IsTag("LUXURY_ITEM") || item.IsTag("ACCESS_CARD") || item.IsTag("ART"))
            return 7;

        return 0; // Kho chung / Bỏ qua
    }

    private static int MapTagToCategoryIndex(string tag)
    {
        switch (tag)
        {
            case "WEAPON":
            case "FIREARM":
            case "MELEE_WEAPON":
            case "AMMUNITION":
            case "MUNITION":
            case "GUN_MOD":
            case "GRENADE":
            case "EXPLOSIVE":
                return 1; // ⚔️ Vũ Khí & Đạn

            case "MEDICAL":
            case "MEDICAL_TOOL":
            case "NARCOTIC":
            case "POISON":
            case "ORGAN":
            case "DRUG":
            case "CHEMICAL":
                return 2; // 💊 Y Tế & Dược Phẩm

            case "FOOD":
            case "PROCESSED_FOOD":
            case "WATER":
            case "WATER_RATION":
            case "TREAT":
            case "ALCOHOL":
            case "BREVAGE":
            case "DRINK":
                return 3; // 🍞 Thực Phẩm & Nước

            case "ARMOR":
            case "HELMET":
            case "VEST":
            case "EQUIPMENT":
                return 4; // 🛡️ Giáp & Trang Bị

            case "MODULE":
            case "HYDROPONIC_MODULE":
            case "EVAPORATOR_MODULE":
            case "PRINTER_MODULE":
            case "ALARM_MODULE":
            case "ENERGY_CELL":
            case "BATTERY":
            case "ELECTRONIC":
                return 5; // ⚡ Linh Kiện & Module

            case "MATERIAL":
            case "BASIC_MATERIAL":
            case "TOOL":
            case "CHEMICAL_SUPPLIES":
            case "FILTER":
            case "SEED":
            case "MODIFIED_SEED":
                return 6; // 🔧 Vật Liệu & Công Cụ

            case "LUXURY_ITEM":
            case "DOCUMENT":
            case "ACCESS_CARD":
            case "ART":
            case "MONEY":
            case "VALUABLE":
            case "KEY":
                return 7; // 📄 Tài Liệu & Quý Giá

            default:
                return 0;
        }
    }

    // =========================================================================
    // 5. NHẬN DIỆN MẶC ĐỊNH CHO HÒM ĐỒ DỰA VÀO TÊN & TAG CỦA HÒM (Xử lý đa ngữ & khoảng trắng)
    // =========================================================================
    public static int DetectDefaultCategory(string containerName, GameItem item = null)
    {
        string upper = (containerName ?? "").ToUpperInvariant().Replace("_", " ");

        // 1. Kiểm tra tag của GameItem nếu có
        if (item != null)
        {
            if (item.IsTag("WEAPON") || item.IsTag("AMMUNITION") || item.IsTag("FIREARM")) return 1;
            if (item.IsTag("MEDICAL") || item.IsTag("ITEM_MEDICAL_TAG") || item.IsTag("NARCOTIC") || item.IsTag("ORGAN")) return 2;
            if (item.IsTag("FOOD") || item.IsTag("PROCESSED_FOOD") || item.IsTag("WATER")) return 3;
            if (item.IsTag("ARMOR")) return 4;
            if (item.IsTag("MODULE") || item.IsTag("ENERGY_CELL")) return 5;
            if (item.IsTag("TOOL") || item.IsTag("MATERIAL")) return 6;
            if (item.IsTag("DOCUMENT") || item.IsTag("LUXURY_ITEM") || item.IsTag("ACCESS_CARD")) return 7;
        }

        // 2. Nhóm 2: Y Tế & Dược Phẩm
        if (upper.Contains("FIRST AID") || upper.Contains("FIRSTAID") || upper.Contains("MED") || 
            upper.Contains("HEALTH") || upper.Contains("ORGAN") || upper.Contains("PILL") || 
            upper.Contains("Y TẾ") || upper.Contains("THUỐC") || upper.Contains("DƯỢC") || 
            upper.Contains("SƠ CỨU") || upper.Contains("CỨU THƯƠNG") || upper.Contains("BĂNG GẠC"))
            return 2;

        // Nhóm 1: Vũ Khí & Đạn
        if (upper.Contains("SEC") || upper.Contains("WEAPON") || upper.Contains("GUN") || 
            upper.Contains("AMMO") || upper.Contains("MUNITION") || upper.Contains("AN NINH") || 
            upper.Contains("SÚNG") || upper.Contains("ĐẠN") || upper.Contains("VŨ KHÍ") || 
            upper.Contains("BẢO MẬT") || upper.Contains("GUNSAFE"))
            return 1;

        // Nhóm 3: Thực Phẩm & Nước
        if (upper.Contains("LUNCH") || upper.Contains("FOOD") || upper.Contains("BEER") || 
            upper.Contains("WATER") || upper.Contains("FRIDGE") || upper.Contains("RATION") || 
            upper.Contains("THỰC PHẨM") || upper.Contains("HỘP CƠM") || upper.Contains("NƯỚC") || 
            upper.Contains("ĐỒ ĂN") || upper.Contains("LƯƠNG KHÔ") || upper.Contains("BÁNH"))
            return 3;

        // Nhóm 4: Giáp & Trang Bị
        if (upper.Contains("ARMOR") || upper.Contains("VEST") || upper.Contains("HELMET") || 
            upper.Contains("GIÁP") || upper.Contains("MŨ") || upper.Contains("ÁO GIÁP"))
            return 4;

        // Nhóm 5: Linh Kiện & Module
        if (upper.Contains("ENG") || upper.Contains("MODULE") || upper.Contains("ENERGY") || 
            upper.Contains("BATTERY") || upper.Contains("TECH") || upper.Contains("KỸ THUẬT") || 
            upper.Contains("LINH KIỆN") || upper.Contains("PIN") || upper.Contains("MÔ-ĐUN"))
            return 5;

        // Nhóm 6: Vật Liệu & Công Cụ
        if (upper.Contains("TOOL") || upper.Contains("MATERIAL") || upper.Contains("SERVICE") || 
            upper.Contains("CÔNG CỤ") || upper.Contains("VẬT LIỆU") || upper.Contains("DỤNG CỤ") || 
            upper.Contains("ĐỒ NGHỀ"))
            return 6;

        // Nhóm 7: Tài Liệu & Quý Giá
        if (upper.Contains("EVIDENCE") || upper.Contains("DOSSIER") || upper.Contains("SAFE") || 
            upper.Contains("DOC") || upper.Contains("TÀI LIỆU") || upper.Contains("BẰNG CHỨNG") || 
            upper.Contains("KÉT SẮT") || upper.Contains("QUÝ GIÁ") || upper.Contains("HỒ SƠ") || 
            upper.Contains("SATCHEL"))
            return 7;

        return 0; // Mặc định: Kho chung / Bỏ qua
    }

    // =========================================================================
    // 6. TRUY XUẤT TOÀN BỘ CÁC HÒM ĐỒ TRONG TIỆM / CỬA SỔ / HÒM TRONG KHO
    // =========================================================================
    public struct NamedInventory
    {
        public string Id;
        public string DisplayName;
        public GameGridInventory Inventory;
        public int DefaultCategory;
    }

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

    private List<NamedInventory> GetAvailableInventories()
    {
        List<NamedInventory> list = new List<NamedInventory>();
        HashSet<GameGridInventory> seenInvs = new HashSet<GameGridInventory>();

        EmporiumEntry emporium = EmporiumEntry.Instance;
        if (emporium != null)
        {
            AddIfValid(list, seenInvs, "main_inv", "🏢 Kho Tiệm Chính (Bàn Thao Tác)", emporium.invElement, 0);
            AddIfValid(list, seenInvs, "showcase", "✨ Kệ Kính Trưng Bày", emporium.showcaseElement, 0);
            AddIfValid(list, seenInvs, "front_box", "📦 Hòm Đồ Trước Quầy", emporium.frontInvinvElement, 0);
            AddIfValid(list, seenInvs, "back_box", "📦 Hòm Đồ Sau Quầy", emporium.backInvinvElement, 0);
            AddIfValid(list, seenInvs, "counter_box", "🗄️ Hòm Quầy Thu Ngân", emporium.backInvinvElementCounter, 0);
            AddIfValid(list, seenInvs, "bazar_left", "🏪 Hòm Chợ Trái", emporium.bazarLeftinvElement, 0);
            AddIfValid(list, seenInvs, "afterhour", "🌙 Kho Ngoài Giờ", emporium.afterhourInventory, 0);

            // 🕵️ Kho Lậu / Hòm Ẩn: Mặc định là 0 (Khóa / Bỏ qua) để bảo vệ tuyệt đối hàng lậu
            AddIfValid(list, seenInvs, "hidden_stash", "🕵️ Kho Lậu / Hòm Ẩn", emporium.hiddenElement, 0);
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

                    string id = (parentItem != null) ? ("box_" + parentItem.name + "_" + parentItem.GetHashCode()) : ("win_" + rawName + "_" + pw.GetHashCode());

                    foreach (var ggi in storages)
                    {
                        if (ggi != null && !seenInvs.Contains(ggi))
                        {
                            string displayName = "🔓 " + rawName;
                            AddIfValid(list, seenInvs, id, displayName, ggi, defCat);
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
                GameGridInventory[] allShopInvs = new GameGridInventory[]
                {
                    emporium.invElement,
                    emporium.backInvinvElement,
                    emporium.frontInvinvElement,
                    emporium.backInvinvElementCounter,
                    emporium.showcaseElement,
                    emporium.bazarLeftinvElement,
                    emporium.afterhourInventory
                };

                foreach (var shopInv in allShopInvs)
                {
                    if (shopInv == null || shopInv.childItems == null) continue;

                    foreach (GameItem it in shopInv.childItems)
                    {
                        if (it == null) continue;
                        List<GameGridInventory> storages = GetInventoriesFromItem(it);
                        if (storages == null || storages.Count == 0) continue;

                        string id = "box_" + it.name + "_" + it.GetHashCode();
                        int defCat = DetectDefaultCategory(it.name, it);

                        foreach (var ggi in storages)
                        {
                            if (ggi != null && !seenInvs.Contains(ggi))
                            {
                                string displayName = "💼 " + it.name + " (#" + Math.Abs(it.GetHashCode() % 1000) + ")";
                                AddIfValid(list, seenInvs, id, displayName, ggi, defCat);
                            }
                        }
                    }
                }
            }
        }
        catch {}

        return list;
    }

    private void AddIfValid(List<NamedInventory> list, HashSet<GameGridInventory> seen, string id, string name, GameGridInventory inv, int defCat)
    {
        if (inv != null && inv.inventoryShape != null && inv.inventoryShape.width > 0 && inv.inventoryShape.height > 0)
        {
            if (seen.Contains(inv)) return;
            seen.Add(inv);

            NamedInventory ni;
            ni.Id = id;
            ni.DisplayName = name + " [" + inv.inventoryShape.width + "x" + inv.inventoryShape.height + "]";
            ni.Inventory = inv;
            ni.DefaultCategory = defCat;
            list.Add(ni);
        }
    }

    // =========================================================================
    // 7. LƯU & TẢI CẤU HÌNH GÁN HÒM (PERSISTENCE)
    // =========================================================================
    private void SaveAssignments()
    {
        try
        {
            foreach (var kvp in customAssignments)
            {
                PlayerPrefs.SetInt("SmartCategoryPriority_" + kvp.Key, kvp.Value);
            }
            PlayerPrefs.Save();
        }
        catch {}
    }

    private void LoadSavedAssignments()
    {
        try
        {
            string[] knownIds = new string[] { "main_inv", "showcase", "front_box", "back_box", "counter_box", "bazar_left", "afterhour", "hidden_stash" };
            foreach (var id in knownIds)
            {
                if (PlayerPrefs.HasKey("SmartCategoryPriority_" + id))
                {
                    customAssignments[id] = PlayerPrefs.GetInt("SmartCategoryPriority_" + id, -1);
                }
            }
            // Kho lậu mặc định luôn là KHÓA (0) nếu chưa có cấu hình
            if (!customAssignments.ContainsKey("hidden_stash"))
            {
                customAssignments["hidden_stash"] = 0;
            }
        }
        catch {}
    }
}
