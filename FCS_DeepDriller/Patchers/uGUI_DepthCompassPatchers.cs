﻿using FCS_DeepDriller.Configuration;
using HarmonyLib;

namespace FCS_DeepDriller.Patchers
{
    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch("LateUpdate")]
    internal class uGUI_DepthCompass_Patch
    {
        [HarmonyPostfix]
        internal static void Postfix(uGUI_DepthCompass __instance)
        {

        }
    }
}
