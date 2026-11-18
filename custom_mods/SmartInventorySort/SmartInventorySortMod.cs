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
                Debug.Log("[SmartInventorySort] Đã kích hoạt giao diện Phân Loại Đa Hòm (F2: Menu, F3: Gom & Phân loại nhanh).");
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
// 2. CONTROLLER & UI: Phân loại theo Danh Mục & Sắp Xếp Đa Hòm Đồ
// =========================================================================
public class SmartCategorySortController : MonoBehaviour
{
    public static SmartCategorySortController Instance;

    private bool showMenu = false;
    private Rect windowRect = new Rect(40, 40, 520, 460);
    private Vector2 scrollPos;
    private string statusMessage = "Bấm F2 để ẩn/hiện bảng. Bấm F3 để gom và phân loại nhanh.";

    // Danh sách các danh mục hỗ trợ
    public static readonly string[] CategoryNames = new string[]
    {
        "Bỏ Qua (Không Chuyển)",
        "⚔️ Vũ Khí & Đạn",
        "💊 Y Tế & Dược Phẩm",
        "🍞 Thực Phẩm & Nước",
        "🛡️ Giáp & Trang Bị",
        "⚡ Linh Kiện & Module",
        "🔧 Vật Liệu & Công Cụ",
        "📄 Tài Liệu & Quý Giá"
    };

    // Lưu gán danh mục cho từng hòm: key = Inventory Identifier, value = Category Index (0..7)
    private Dictionary<string, int> containerAssignments = new Dictionary<string, int>();

    private void Awake()
    {
        Instance = this;
        LoadSavedAssignments();
    }

    private void Update()
    {
        // F2: Bật/Tắt Menu Giao Diện Trực Quan
        if (Input.GetKeyDown(KeyCode.F2))
        {
            showMenu = !showMenu;
        }

        // F3: Phím Tắt Nhanh - Gom & Phân Loại Tất Cả Ngay Lập Tức
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ExecuteCategorySort();
        }
    }

    private void OnGUI()
    {
        if (!showMenu) return;

        GUI.skin.window.fontSize = 14;
        windowRect = GUI.Window(889911, windowRect, DrawWindow, "📦 PHÂN LOẠI & SẮP XẾP HÒM ĐỒ TỰ ĐỘNG");
    }

    private void DrawWindow(int windowId)
    {
        GUILayout.Space(8);
        GUI.color = Color.yellow;
        GUILayout.Label(statusMessage);
        GUI.color = Color.white;
        GUILayout.Space(4);

        GUILayout.Label("Danh sách các hòm đồ trong tiệm (Click nút để đổi loại đồ nhận):", EditorStyles_BoldLabel());

        List<NamedInventory> inventories = GetAvailableInventories();

        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(280));
        foreach (var ni in inventories)
        {
            GUILayout.BeginHorizontal("box");

            // Tên hòm và số lượng món hiện tại
            int count = (ni.Inventory.childItems != null) ? ni.Inventory.childItems.Count : 0;
            string labelText = ni.DisplayName + " (" + count + " món)";
            GUILayout.Label(labelText, GUILayout.Width(220));

            // Lấy category hiện tại của hòm
            string id = ni.Id;
            if (!containerAssignments.ContainsKey(id))
            {
                containerAssignments[id] = 0; // Mặc định bỏ qua
            }
            int currentCat = containerAssignments[id];

            // Nút bấm đổi danh mục (chu kỳ click qua các loại)
            string btnText = CategoryNames[currentCat];
            if (GUILayout.Button(btnText, GUILayout.Width(240), GUILayout.Height(26)))
            {
                currentCat = (currentCat + 1) % CategoryNames.Length;
                containerAssignments[id] = currentCat;
                SaveAssignments();
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        // Nút gom và phân loại tất cả
        GUI.backgroundColor = new Color(0.2f, 0.9f, 0.4f);
        if (GUILayout.Button("⚡ GOM & PHÂN LOẠI TẤT CẢ (F3)", GUILayout.Height(36)))
        {
            ExecuteCategorySort();
        }

        // Nút nén khít chặt tất cả hòm
        GUI.backgroundColor = new Color(0.4f, 0.7f, 1.0f);
        if (GUILayout.Button("🧹 SẮP XẾP KHÍT HÒM", GUILayout.Height(36)))
        {
            CompactAllInventories();
        }

        // Nút đóng
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("ĐÓNG (F2)", GUILayout.Height(36), GUILayout.Width(90)))
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

    // =========================================================================
    // 3. LOGIC GOM & PHÂN PHỐI ĐỒ VÀO CÁC HÒM MỤC TIÊU
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

            // Bản đồ: Category Index -> Danh sách Hòm nhận category đó
            Dictionary<int, List<GameGridInventory>> targetsByCategory = new Dictionary<int, List<GameGridInventory>>();
            for (int i = 1; i < CategoryNames.Length; i++)
            {
                targetsByCategory[i] = new List<GameGridInventory>();
            }

            foreach (var ni in inventories)
            {
                int cat = 0;
                if (containerAssignments.TryGetValue(ni.Id, out cat) && cat > 0)
                {
                    targetsByCategory[cat].Add(ni.Inventory);
                }
            }

            int totalMoved = 0;

            // Quét từng kho nguồn
            foreach (var sourceNI in inventories)
            {
                GameGridInventory sourceInv = sourceNI.Inventory;
                if (sourceInv == null || sourceInv.childItems == null) continue;

                // Lấy bản sao danh sách item để tránh lỗi duyệt khi di chuyển
                List<GameItem> itemsToProcess = new List<GameItem>(sourceInv.childItems);

                foreach (var item in itemsToProcess)
                {
                    if (item == null) continue;
                    if (ItemPinHelper.IsPinned(item) || DebrisHelper.IsDebris(item)) continue; // Giữ nguyên đồ ghim và rác

                    // Lấy Category Index của món đồ theo TAG ĐẦU TIÊN
                    int itemCat = GetItemCategoryIndex(item);
                    if (itemCat <= 0) continue; // Không thuộc nhóm cần gom

                    List<GameGridInventory> targetInvs = targetsByCategory[itemCat];
                    if (targetInvs == null || targetInvs.Count == 0) continue; // Không có hòm nào nhận nhóm này

                    // Nếu món đồ đã ở trong một trong các hòm nhận nhóm đó rồi -> Giữ nguyên
                    if (targetInvs.Contains(sourceInv)) continue;

                    // Thử chuyển sang hòm đích
                    foreach (var destInv in targetInvs)
                    {
                        if (destInv == sourceInv) continue;

                        // Chuyển thử
                        sourceInv.Expel(item);
                        destInv.UncheckedAccept(item);

                        // Kích hoạt thuật toán sắp xếp để kiểm tra xem có vừa hòm đích không
                        if (InventorySortHelper.Sort(destInv, true, false))
                        {
                            totalMoved++;
                            break;
                        }
                        else
                        {
                            // Hòm đích đã đầy, hoàn trả lại hòm nguồn
                            destInv.Expel(item);
                            sourceInv.UncheckedAccept(item);
                        }
                    }
                }
            }

            // Sau khi phân phối xong, tự động sắp xếp nén khít toàn bộ các hòm
            foreach (var ni in inventories)
            {
                if (ni.Inventory != null && ni.Inventory.childItems != null && ni.Inventory.childItems.Count > 0)
                {
                    InventorySortHelper.Sort(ni.Inventory, true, false);
                    ni.Inventory.Validate();
                }
            }

            statusMessage = "✅ Đã phân loại và sắp xếp thành công " + totalMoved + " món đồ vào các hòm!";
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
                if (ni.Inventory != null && ni.Inventory.childItems != null && ni.Inventory.childItems.Count > 0)
                {
                    InventorySortHelper.Sort(ni.Inventory, true, false);
                    ni.Inventory.Validate();
                    count++;
                }
            }
            statusMessage = "✅ Đã nén khít chặt " + count + " hòm đồ thành công!";
        }
        catch (Exception ex)
        {
            statusMessage = "❌ Lỗi: " + ex.Message;
        }
    }

    // =========================================================================
    // 4. NHẬN DIỆN DANH MỤC DỰA VÀO TAG ĐẦU TIÊN (Theo yêu cầu)
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
        if (item.IsTag("MEDICAL") || item.IsTag("ITEM_MEDICAL_TAG") || item.IsTag("NARCOTIC") || item.IsTag("POISON"))
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

        return 0; // Bỏ qua
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
                return 1; // ⚔️ Vũ Khí & Đạn

            case "MEDICAL":
            case "MEDICAL_TOOL":
            case "NARCOTIC":
            case "POISON":
            case "ORGAN":
                return 2; // 💊 Y Tế & Dược Phẩm

            case "FOOD":
            case "PROCESSED_FOOD":
            case "WATER":
            case "WATER_RATION":
            case "TREAT":
            case "ALCOHOL":
            case "BREVAGE":
                return 3; // 🍞 Thực Phẩm & Nước

            case "ARMOR":
                return 4; // 🛡️ Giáp & Trang Bị

            case "MODULE":
            case "HYDROPONIC_MODULE":
            case "EVAPORATOR_MODULE":
            case "PRINTER_MODULE":
            case "ALARM_MODULE":
            case "ENERGY_CELL":
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
                return 7; // 📄 Tài Liệu & Quý Giá

            default:
                return 0; // Chưa rõ / Bỏ qua
        }
    }

    // =========================================================================
    // 5. TRUY XUẤT CÁC HÒM ĐỒ HIỆN CÓ TRONG TIỆM / PHÒNG
    // =========================================================================
    public struct NamedInventory
    {
        public string Id;
        public string DisplayName;
        public GameGridInventory Inventory;
    }

    private List<NamedInventory> GetAvailableInventories()
    {
        List<NamedInventory> list = new List<NamedInventory>();
        EmporiumEntry emporium = EmporiumEntry.Instance;
        if (emporium == null) return list;

        AddIfValid(list, "main_inv", "🏢 Kho Tiệm Chính (Bàn Thao Tác)", emporium.invElement);
        AddIfValid(list, "showcase", "✨ Kệ Kính Trưng Bày", emporium.showcaseElement);
        AddIfValid(list, "front_box", "📦 Hòm Đồ Trước Quầy", emporium.frontInvinvElement);
        AddIfValid(list, "back_box", "📦 Hòm Đồ Sau Quầy", emporium.backInvinvElement);
        AddIfValid(list, "counter_box", "🗄️ Hòm Quầy Thu Ngân", emporium.backInvinvElementCounter);
        AddIfValid(list, "bazar_left", "🏪 Hòm Chợ Trái", emporium.bazarLeftinvElement);
        AddIfValid(list, "afterhour", "🌙 Kho Ngoài Giờ", emporium.afterhourInventory);

        // Quét thêm hòm từ ItemContextHandler nếu có container đang mở riêng
        try
        {
            FieldInfo fCurrentInv = AccessTools.Field(typeof(ItemContextHandler), "currentInventory");
            if (fCurrentInv != null && ItemContextHandler.current != null)
            {
                GameGridInventory cur = fCurrentInv.GetValue(ItemContextHandler.current) as GameGridInventory;
                if (cur != null)
                {
                    bool exists = false;
                    foreach (var item in list)
                    {
                        if (item.Inventory == cur) { exists = true; break; }
                    }
                    if (!exists)
                    {
                        AddIfValid(list, "current_open", "🎒 Hòm / Ba Lô Đang Mở", cur);
                    }
                }
            }
        }
        catch {}

        return list;
    }

    private void AddIfValid(List<NamedInventory> list, string id, string name, GameGridInventory inv)
    {
        if (inv != null && inv.inventoryShape != null && inv.inventoryShape.width > 0 && inv.inventoryShape.height > 0)
        {
            NamedInventory ni;
            ni.Id = id;
            ni.DisplayName = name + " [" + inv.inventoryShape.width + "x" + inv.inventoryShape.height + "]";
            ni.Inventory = inv;
            list.Add(ni);
        }
    }

    // =========================================================================
    // 6. LƯU & TẢI CẤU HÌNH GÁN HÒM (PERSISTENCE)
    // =========================================================================
    private void SaveAssignments()
    {
        try
        {
            foreach (var kvp in containerAssignments)
            {
                PlayerPrefs.SetInt("SmartCategorySort_" + kvp.Key, kvp.Value);
            }
            PlayerPrefs.Save();
        }
        catch {}
    }

    private void LoadSavedAssignments()
    {
        try
        {
            string[] knownIds = new string[] { "main_inv", "showcase", "front_box", "back_box", "counter_box", "bazar_left", "afterhour", "current_open" };
            foreach (var id in knownIds)
            {
                if (PlayerPrefs.HasKey("SmartCategorySort_" + id))
                {
                    containerAssignments[id] = PlayerPrefs.GetInt("SmartCategorySort_" + id, 0);
                }
            }
        }
        catch {}
    }
}
