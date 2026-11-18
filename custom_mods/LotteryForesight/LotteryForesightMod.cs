using System;
using HarmonyLib;
using UnityEngine;

public class LotteryForesightMod : IMod
{
    private static Action<RichTextBuilder, GameItem> onTooltipDelegate;

    public void Init(ModManifest manifest)
    {
    }

    public void OnEnable()
    {
        if (onTooltipDelegate == null)
        {
            onTooltipDelegate = new Action<RichTextBuilder, GameItem>(AddLotteryTooltip);
        }
        ModHook.OnCreateTooltipLate += onTooltipDelegate;
    }

    public void OnDisable()
    {
        if (onTooltipDelegate != null)
        {
            ModHook.OnCreateTooltipLate -= onTooltipDelegate;
        }
    }

    private static void AddLotteryTooltip(RichTextBuilder builder, GameItem item)
    {
        if (builder == null || item == null) return;

        try
        {
            // Chỉ soi trước kết quả vé cào (Scratcher) để biết trước khi mua
            if (item.IsTag("SCRATCHER_TAG"))
            {
                int s1 = item.GetTagReadonly("SCRATCHER_SYMBOL1").GetInt();
                int s2 = item.GetTagReadonly("SCRATCHER_SYMBOL2").GetInt();
                int s3 = item.GetTagReadonly("SCRATCHER_SYMBOL3").GetInt();

                int prize = 0;
                if (s1 > 0 && s2 > 0 && s3 > 0)
                {
                    if (s1 == s2 && s2 == s3)
                    {
                        prize = (s1 == 1) ? 350 : 75;
                    }
                    else if (s1 == s2 || s1 == s3)
                    {
                        prize = (s1 == 1) ? 30 : 10;
                    }
                    else if (s2 == s3)
                    {
                        prize = (s2 == 1) ? 30 : 10;
                    }
                }

                if (prize > 0)
                {
                    builder.AddLine("★ [SOI VÉ CÀO] VÉ TRÚNG THƯỞNG! Giải: " + prize + " credits", true, RenderHandler.ColorPalette.Green, true, false, false, false, RenderHandler.ColorPalette.None, RenderHandler.ColorPalette.None, RenderHandler.ColorPalette.None);
                }
                else
                {
                    builder.AddLine("✕ [SOI VÉ CÀO] Vé trượt (Không trúng giải)", true, RenderHandler.ColorPalette.Red, false, false, false, false, RenderHandler.ColorPalette.None, RenderHandler.ColorPalette.None, RenderHandler.ColorPalette.None);
                }
            }
            // Vé số quay đêm (LOTTERY_TICKET_TAG) để nguyên mặc định của game!
        }
        catch {}
    }
}

// Phím tắt tùy chọn: Trong giao diện cào, người chơi có thể cào chuột bình thường, hoặc nhấn Space/Enter để cào nhanh
[HarmonyPatch(typeof(ScratcherUIManager), "Update")]
public static class ScratcherQuickScratchPatch
{
    public static void Postfix(ScratcherUIManager __instance)
    {
        try
        {
            if (__instance == null || __instance.ui == null || !__instance.ui.activeSelf) return;

            // Nếu muốn cào nhanh thì nhấn Space hoặc Enter
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                for (int i = 1; i <= 3; i++)
                {
                    __instance.OnScratchComplete(i);
                }

                if (__instance.scratchInputs != null)
                {
                    foreach (var input in __instance.scratchInputs)
                    {
                        if (input != null)
                        {
                            input.RevealAll();
                        }
                    }
                }
            }
        }
        catch {}
    }
}
