using HarmonyLib;
using UnityEngine;

namespace AgriHarvestPriority.Patches
{
    [HarmonyPatch(typeof(FilteredDragTool))]
    public static class DeconstructToolTravelTubePatch
    {
        private const string TRAVELTUBE_FILTER = "TRAVELTUBE";
        private const string BUILDINGS_FILTER  = "BUILDINGS";
        private const string TRAVELTUBE_PREFAB = "TravelTube";

        // ---------- 1. 追加过滤器 ----------
        [HarmonyPostfix]
        [HarmonyPatch("GetDefaultFilters")]
        public static void GetDefaultFilters_Postfix(
            FilteredDragTool __instance,
            ref ToolParameterMenu.ToggleData[] filters)
        {
            if (!(__instance is DeconstructTool)) return;
            if (filters == null) return;

            foreach (var f in filters)
                if (f != null && f.name == TRAVELTUBE_FILTER) return;

            var newFilters = new ToolParameterMenu.ToggleData[filters.Length + 1];
            System.Array.Copy(filters, newFilters, filters.Length);
            newFilters[filters.Length] = new ToolParameterMenu.ToggleData(
                TRAVELTUBE_FILTER, ToolParameterMenu.ToggleState.Off, false);

            filters = newFilters;
        }

        // ---------- 2. 运载管道命中 TRAVELTUBE ----------
        [HarmonyPrefix]
        [HarmonyPatch("GetFilterLayerFromGameObject")]
        public static bool GetFilterLayerFromGameObject_Prefix(
            FilteredDragTool __instance,
            GameObject input,
            ref string __result)
        {
            if (!(__instance is DeconstructTool)) return true;
            if (!IsTravelTube(input)) return true;

            if (__instance.IsActiveLayer(TRAVELTUBE_FILTER))
            {
                __result = TRAVELTUBE_FILTER;
                return false;
            }
            if (__instance.IsActiveLayer(BUILDINGS_FILTER))
            {
                __result = "Buildings";
                return false;
            }

            __result = TRAVELTUBE_FILTER;
            return false;
        }

        private static bool IsTravelTube(GameObject go)
        {
            if (go == null) return false;

            var bc = go.GetComponent<BuildingComplete>();
            if (bc != null && bc.Def != null && bc.Def.PrefabID == TRAVELTUBE_PREFAB)
                return true;

            var buc = go.GetComponent<BuildingUnderConstruction>();
            if (buc != null && buc.Def != null && buc.Def.PrefabID == TRAVELTUBE_PREFAB)
                return true;

            return false;
        }
    }
}