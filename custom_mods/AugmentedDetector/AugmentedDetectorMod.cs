using System;
using HarmonyLib;
using UnityEngine;
using TMPro;

public class AugmentedDetectorMod : IMod
{
    public void Init(ModManifest manifest)
    {
    }

    public void OnEnable()
    {
    }

    public void OnDisable()
    {
    }
}

[HarmonyPatch(typeof(DialogUIManager), "DisplayClientText")]
public static class DialogClientTextAugPatch
{
    public static void Postfix(DialogUIManager __instance, Dialogue dialogue)
    {
        try
        {
            if (PlayerStore.Instance == null || PlayerStore.Instance.currentClientInstance == null) return;
            StoreClient client = PlayerStore.Instance.currentClientInstance.GetClientBlueprint();
            if (client == null || __instance.clientNameTMP == null) return;

            string baseName = client.displayName;
            string prefix = "";

            if (client.isAug)
            {
                prefix += "<color=#FF0055>[⚠️ DỊ NHÂN CẢI TẠO - BẬC " + client.augTier + "]</color> ";
            }
            else
            {
                prefix += "<color=#00FF88>[NGƯỜI THƯỜNG]</color> ";
            }

            if (client.isWanted)
            {
                prefix += "<color=#FFAA00>[🚨 TRUY NÃ: " + (string.IsNullOrEmpty(client.wantedPosterId) ? "CẤP CAO" : client.wantedPosterId) + "]</color> ";
            }

            __instance.clientNameTMP.text = prefix + baseName;
        }
        catch {}
    }
}

[HarmonyPatch(typeof(DialogUIManager), "SetClientBubbleColor")]
public static class DialogBubbleColorAugPatch
{
    public static void Postfix(DialogUIManager __instance)
    {
        try
        {
            if (PlayerStore.Instance == null || PlayerStore.Instance.currentClientInstance == null) return;
            StoreClient client = PlayerStore.Instance.currentClientInstance.GetClientBlueprint();
            if (client == null || !client.isAug) return;

            if (__instance.clientBubble != null && __instance.clientBubble.backgroundImage != null)
            {
                __instance.clientBubble.backgroundImage.color = new Color(1.0f, 0.15f, 0.45f, 1.0f);
            }
            if (__instance.clientNameBubbleImage != null)
            {
                __instance.clientNameBubbleImage.color = new Color(0.9f, 0.05f, 0.3f, 1.0f);
            }
        }
        catch {}
    }
}

[HarmonyPatch(typeof(StoreClientMono), "PlayIdleForCurrentClient")]
public static class StoreClientIdleAugPatch
{
    public static void Postfix(StoreClientMono __instance)
    {
        try
        {
            if (PlayerStore.Instance == null || PlayerStore.Instance.currentClientInstance == null) return;
            StoreClient client = PlayerStore.Instance.currentClientInstance.GetClientBlueprint();
            if (client == null) return;

            Transform existing = __instance.transform.Find("AugIndicatorTag");
            if (client.isAug)
            {
                GameObject tagObj;
                if (existing == null)
                {
                    tagObj = new GameObject("AugIndicatorTag");
                    tagObj.transform.SetParent(__instance.transform, false);
                    tagObj.transform.localPosition = new Vector3(0f, 2.7f, 0f);
                    TextMesh tm = tagObj.AddComponent<TextMesh>();
                    tm.text = "⚠️ [DỊ NHÂN - BẬC " + client.augTier + "]";
                    tm.fontSize = 36;
                    tm.characterSize = 0.06f;
                    tm.color = new Color(1f, 0.1f, 0.35f, 1f);
                    tm.fontStyle = FontStyle.Bold;
                    tm.alignment = TextAlignment.Center;
                    tm.anchor = TextAnchor.MiddleCenter;

                    MeshRenderer mr = tagObj.GetComponent<MeshRenderer>();
                    if (__instance.image != null && mr != null)
                    {
                        mr.sortingLayerID = __instance.image.sortingLayerID;
                        mr.sortingOrder = __instance.image.sortingOrder + 20;
                    }
                }
                else
                {
                    tagObj = existing.gameObject;
                    tagObj.SetActive(true);
                    TextMesh tm = tagObj.GetComponent<TextMesh>();
                    if (tm != null)
                    {
                        tm.text = "⚠️ [DỊ NHÂN - BẬC " + client.augTier + "]";
                    }
                }
            }
            else
            {
                if (existing != null)
                {
                    existing.gameObject.SetActive(false);
                }
            }
        }
        catch {}
    }
}

[HarmonyPatch(typeof(StoreClientMono), "OnLeft")]
public static class StoreClientOnLeftPatch
{
    public static void Postfix(StoreClientMono __instance)
    {
        try
        {
            Transform existing = __instance.transform.Find("AugIndicatorTag");
            if (existing != null)
            {
                UnityEngine.Object.Destroy(existing.gameObject);
            }
        }
        catch {}
    }
}
