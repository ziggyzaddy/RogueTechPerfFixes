using UnityEngine;

namespace RogueTechPerfFixes.Patches;

[HarmonyPatch(typeof(Material), nameof(Material.color), MethodType.Getter)]
internal static class Material_get_color_Patch
{
    private static int s_id;
    public static void Prepare()
    {
        s_id = Shader.PropertyToID("_Color");
    }

    public static void Prefix(Material __instance, ref bool __runOriginal, ref Color __result)
    {
        __result = __instance.GetColor(s_id);
        __runOriginal = false;
    }
}
