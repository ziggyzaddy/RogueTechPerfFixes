using Mono.Cecil;

namespace RogueTechPerfFixesInjector
{
    public interface IInjector
    {
        void Inject(IAssemblyResolver resolver);
    }
}