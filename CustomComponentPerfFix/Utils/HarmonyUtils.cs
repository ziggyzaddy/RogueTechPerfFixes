using System;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;

namespace RogueTechPerfFixes.Utils
{
    public static class HarmonyUtils
    {
        public const string HarmonyId = "NotooShabby.RogueTechPerfFixes";

        public static HarmonyInstance Harmony = HarmonyInstance.Create(HarmonyId);

        public delegate ref U RefGetter<U>();

        /// <summary>
        /// Create a pointer for static field <paramref name="s_field"/>
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="s_field"></param>
        /// <returns> Pointer to <paramref name="s_field"/></returns>
        /// <remarks> Source: https://stackoverflow.com/a/45046664/13073994 </remarks>
        public static RefGetter<U> CreateStaticFieldRef<U>(Type type, String s_field)
        {
            const BindingFlags bf = BindingFlags.NonPublic |
                                    BindingFlags.Static |
                                    BindingFlags.DeclaredOnly;

            var fi = type.GetField(s_field, bf);
            if (fi == null)
                throw new MissingFieldException(type.Name, s_field);

            var s_name = "__refget_" + type.Name + "_fi_" + fi.Name;

            // workaround for using ref-return with DynamicMethod:
            //   a.) initialize with dummy return value
            var dm = new DynamicMethod(s_name, typeof(U), null, type, true);

            //   b.) replace with desired 'ByRef' return value
            dm.GetType().GetField("returnType", AccessTools.all).SetValue(dm, typeof(U).MakeByRefType());

            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldsflda, fi);
            il.Emit(OpCodes.Ret);

            return (RefGetter<U>)dm.CreateDelegate(typeof(RefGetter<U>));
        }
    }
}
