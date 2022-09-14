using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BattleTech.Rendering.UI;
using Harmony;
using UnityEngine.Rendering;

namespace RogueTechPerfFixes.Patches
{
    [HarmonyPatch(typeof(ElementManager), nameof(ElementManager.RefreshCommandBuffer))]
    public static class H_ElementManager_RefreshCommandBuffer
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilGenerator)
        {
            var uiCommandBuffer = typeof(ElementManager).GetField("_uiCommandBuffer", AccessTools.all);
            var notNullLabel = ilGenerator.DefineLabel();
            var code = new List<CodeInstruction>();

            code.Add(new CodeInstruction(OpCodes.Ldarg_0));
            code.Add(new CodeInstruction(OpCodes.Ldfld, uiCommandBuffer));
            code.Add(new CodeInstruction(OpCodes.Brtrue_S, notNullLabel));

            code.Add(new CodeInstruction(OpCodes.Ldarg_0));
            code.Add(new CodeInstruction(OpCodes.Newobj, typeof(CommandBuffer).GetConstructor(Type.EmptyTypes)));
            code.Add(new CodeInstruction(OpCodes.Stfld, uiCommandBuffer));

            code.Add(new CodeInstruction(OpCodes.Ldarg_0));
            code.Add(new CodeInstruction(OpCodes.Ldfld, uiCommandBuffer));
            code.Add(new CodeInstruction(OpCodes.Ldstr, "UI Command Buffer"));
            code.Add(new CodeInstruction(OpCodes.Callvirt, typeof(CommandBuffer).GetMethod("set_name")));

            var branch = new CodeInstruction(OpCodes.Ldarg_0);
            branch.labels.Add(notNullLabel);
            code.Add(branch);
            code.Add(new CodeInstruction(OpCodes.Ldfld, uiCommandBuffer));
            code.Add(new CodeInstruction(OpCodes.Callvirt, typeof(CommandBuffer).GetMethod(nameof(CommandBuffer.Clear))));

            code.Add(new CodeInstruction(OpCodes.Ldarg_0));
            code.Add(new CodeInstruction(OpCodes.Call, typeof(ElementManager).GetMethod("RefreshCommandBufferInt", AccessTools.all)));

            code.Add(new CodeInstruction(OpCodes.Ret));

            return code;
        }
    }
}
