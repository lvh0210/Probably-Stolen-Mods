using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

public class PreciseTemperatureControlMod : IMod
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

[HarmonyPatch]
public static class HeatingPlateButtonsAlwaysShowPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        Type t = AccessTools.TypeByName("MachineHeatingPlate+<>c__DisplayClass5_0");
        return AccessTools.Method(t, "<HeatingPlate>b__4");
    }

    [HarmonyPostfix]
    public static void Postfix(object __instance)
    {
        try
        {
            FieldInfo fMinus = AccessTools.Field(__instance.GetType(), "buttonMinus");
            FieldInfo fPlus = AccessTools.Field(__instance.GetType(), "buttonPlus");
            MachineryButton btnMinus = (MachineryButton)fMinus.GetValue(__instance);
            MachineryButton btnPlus = (MachineryButton)fPlus.GetValue(__instance);
            if (btnMinus != null) btnMinus.Show();
            if (btnPlus != null) btnPlus.Show();
        }
        catch {}
    }
}

[HarmonyPatch]
public static class HeatingPlateMinusPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        Type t = AccessTools.TypeByName("MachineHeatingPlate+<>c__DisplayClass5_0");
        return AccessTools.Method(t, "<HeatingPlate>b__5");
    }

    [HarmonyPrefix]
    public static bool Prefix(object __instance)
    {
        try
        {
            FieldInfo fMachine = AccessTools.Field(__instance.GetType(), "machine");
            FieldInfo fTempControl = AccessTools.Field(__instance.GetType(), "tempControl");
            GameItem machine = (GameItem)fMachine.GetValue(__instance);
            MachineryText tempControl = (MachineryText)fTempControl.GetValue(__instance);

            int step = 1;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) step = 5;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) step = 10;

            int current = machine.GetTagReadonly("CURRENT_TARGET_TEMP").GetInt();
            int next = Math.Max(26, current - step);

            machine.ModifyTag("CURRENT_TARGET_TEMP", new Action<TagState>(delegate(TagState state) {
                state.Enable();
                state.SetInt(next);
            }), false);

            if (tempControl != null)
            {
                tempControl.SetText(MetaHelper.FormatTemperature(next));
            }
            return false;
        }
        catch
        {
            return true;
        }
    }
}

[HarmonyPatch]
public static class HeatingPlatePlusPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod()
    {
        Type t = AccessTools.TypeByName("MachineHeatingPlate+<>c__DisplayClass5_0");
        return AccessTools.Method(t, "<HeatingPlate>b__6");
    }

    [HarmonyPrefix]
    public static bool Prefix(object __instance)
    {
        try
        {
            FieldInfo fMachine = AccessTools.Field(__instance.GetType(), "machine");
            FieldInfo fTempControl = AccessTools.Field(__instance.GetType(), "tempControl");
            GameItem machine = (GameItem)fMachine.GetValue(__instance);
            MachineryText tempControl = (MachineryText)fTempControl.GetValue(__instance);

            int step = 1;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) step = 5;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) step = 10;

            int current = machine.GetTagReadonly("CURRENT_TARGET_TEMP").GetInt();
            int max = 300;
            int next = Math.Min(max, current + step);

            machine.ModifyTag("CURRENT_TARGET_TEMP", new Action<TagState>(delegate(TagState state) {
                state.Enable();
                state.SetInt(next);
            }), false);

            if (tempControl != null)
            {
                tempControl.SetText(MetaHelper.FormatTemperature(next));
            }
            return false;
        }
        catch
        {
            return true;
        }
    }
}
