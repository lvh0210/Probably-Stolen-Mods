using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

public class SmartInventorySortMod : IMod
{
    private static Harmony harmony;

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
                Debug.Log("[SmartInventorySort] Đã kích hoạt tối ưu hóa sắp xếp kho (PackFirstFit - Loại bỏ khoảng trống thừa).");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[SmartInventorySort] Lỗi kích hoạt Harmony: " + ex);
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
                Debug.Log("[SmartInventorySort] Đã hủy kích hoạt.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[SmartInventorySort] Lỗi hủy kích hoạt: " + ex);
        }
    }
}

[HarmonyPatch(typeof(InventorySortHelper), "PackShelves")]
public static class DisablePackShelvesPatch
{
    public static bool Prefix(ref List<ValueTuple<GameItem, GridShape>> __result)
    {
        // Luôn trả về null để InventorySortHelper.Sort tự động chuyển sang PackFirstFit (quét từng ô)
        // Giải quyết triệt để lỗi khoảng trống chết do chia kệ ngang (shelves) của game gốc
        __result = null;
        return false;
    }
}
