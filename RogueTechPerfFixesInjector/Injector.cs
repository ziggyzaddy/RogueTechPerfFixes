using System.Collections.Generic;
using Mono.Cecil;

namespace RogueTechPerfFixesInjector
{
    public static class Injector
    {
        public static void Inject(IAssemblyResolver resolver)
        {
            foreach (var injector in GetInjectors())
            {
                injector.Inject(resolver);
            }
        }

        private static IEnumerable<IInjector> GetInjectors()
        {
            yield return new I_CombatAuraReticle();
            yield return new I_BTLight();
            yield return new I_BTLightController();
        }
    }
}