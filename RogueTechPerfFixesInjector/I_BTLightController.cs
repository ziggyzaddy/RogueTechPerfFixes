using System;
using Mono.Cecil;

namespace RogueTechPerfFixesInjector;

public class I_BTLightController : IInjector
{
    private const string _targetType = "BattleTech.Rendering.BTLightController";

    private static FieldDefinition InBatchProcess;

    private static FieldDefinition LightAdded;

    public void Inject(IAssemblyResolver resolver)
    {
        var assembly = resolver.Resolve(new AssemblyNameReference("Assembly-CSharp", null));
        var type = assembly.MainModule.GetType(_targetType) ?? throw new Exception($"Can't find target type: {_targetType}");

        InjectField(type, assembly);
    }

    private static void InjectField(TypeDefinition type, AssemblyDefinition assembly)
    {
        var boolReference = assembly.MainModule.ImportReference(typeof(bool));

        InBatchProcess = new FieldDefinition(
            nameof(InBatchProcess)
            , FieldAttributes.Public | FieldAttributes.Static
            , boolReference);

        LightAdded = new FieldDefinition(
            nameof(LightAdded)
            , FieldAttributes.Public | FieldAttributes.Static
            , boolReference);

        type.Fields.Add(InBatchProcess);
        type.Fields.Add(LightAdded);
    }
}