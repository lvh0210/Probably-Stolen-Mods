using System;
using HarmonyLib;

public class CustomerDemandHighlightMod : IMod
{
    private static Action<RichTextBuilder, GameItem> onTooltipDelegate;

    public void Init(ModManifest manifest)
    {
    }

    public void OnEnable()
    {
        if (onTooltipDelegate == null)
        {
            onTooltipDelegate = new Action<RichTextBuilder, GameItem>(AddDemandTooltip);
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

    private static void AddDemandTooltip(RichTextBuilder builder, GameItem item)
    {
        if (builder == null || item == null) return;

        try
        {
            PlayerStore store = PlayerStore.Instance;
            if (store == null || store.currentClientInstance == null) return;

            StoreClient client = store.currentClientInstance.GetClientBlueprint();
            if (client == null) return;

            if (client.IsClientBuyingThisItem(item))
            {
                string info = "★ [NHU CẦU] KHÁCH HÀNG ĐANG CẦN MUA MÓN NÀY!";
                if (client.buyPriceModifier > 0)
                {
                    info += " (Mua cao hơn +" + client.buyPriceModifier + "%)";
                }
                builder.AddLine(info, true, RenderHandler.ColorPalette.Yellow, true, false, false, false, RenderHandler.ColorPalette.None, RenderHandler.ColorPalette.None, RenderHandler.ColorPalette.None);
            }
        }
        catch
        {
        }
    }
}
