using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace RogueTechPerfFixesInjector;

/// <summary>
/// Add a field to store the InstanceId from Unity engine. InstanceId is used in sorting BTLights in the
/// <see cref="BattleTech.Rendering.BTLightController"/>
/// </summary>
public class I_BTLight : IInjector
{
    private const string _targetType = "BattleTech.Rendering.BTLight";

    private static FieldDefinition InstanceId;

    private static MethodDefinition GetInstanceIdLazy;

    private static Instruction _getInstanceID;

    public void Inject(IAssemblyResolver resolver)
    {
        var assembly = resolver.Resolve(new AssemblyNameReference("Assembly-CSharp", null));
        var type = assembly.MainModule.GetType(_targetType) ?? throw new Exception($"Can't find target type: {_targetType}");

        var unityEngineAssembly = resolver.Resolve(new AssemblyNameReference("UnityEngine.CoreModule", null));
        var objectType = unityEngineAssembly.MainModule.GetType("UnityEngine.Object");
        var getInstanceIdMethod = objectType.Methods.Single(x => x.Name == "GetInstanceID");
        var methodTypeReference = assembly.MainModule.ImportReference(getInstanceIdMethod);
        _getInstanceID = Instruction.Create(OpCodes.Callvirt, methodTypeReference);

        InjectField(type, assembly);
        InjectIL(type);
    }

    private static void InjectField(TypeDefinition type, AssemblyDefinition assembly)
    {
        var intReference = assembly.MainModule.ImportReference(typeof(int));

        InstanceId = new FieldDefinition(
            nameof(InstanceId)
            , FieldAttributes.Private
            , intReference);

        type.Fields.Add(InstanceId);

        GetInstanceIdLazy = new MethodDefinition("GetInstanctIdLazy", MethodAttributes.Public, intReference);
        var ilProcessor = GetInstanceIdLazy.Body.GetILProcessor();
        var branchTarget = ilProcessor.Create(OpCodes.Nop);
        ilProcessor.Emit(OpCodes.Ldarg_0);
        ilProcessor.Emit(OpCodes.Ldfld, InstanceId);
        ilProcessor.Emit(OpCodes.Ldc_I4_0);
        ilProcessor.Emit(OpCodes.Ceq);
        ilProcessor.Emit(OpCodes.Brfalse_S, branchTarget);
        ilProcessor.Emit(OpCodes.Ldarg_0);
        ilProcessor.Emit(OpCodes.Ldarg_0);
        ilProcessor.Emit(_getInstanceID.OpCode, _getInstanceID.Operand as MethodReference);
        ilProcessor.Emit(OpCodes.Stfld, InstanceId);
        ilProcessor.Append(branchTarget);
        ilProcessor.Emit(OpCodes.Ldarg_0);
        ilProcessor.Emit(OpCodes.Ldfld, InstanceId);
        ilProcessor.Emit(OpCodes.Ret);

        type.Methods.Add(GetInstanceIdLazy);
    }

    private static bool InitField(TypeDefinition type)
    {
        var constructors = type.GetConstructors().ToList();
        if (constructors.Count == 0)
        {
            throw new Exception("Can't find constructor for BTLight");
        }

        foreach (var constructor in constructors)
        {
            var ilProcessor = constructor.Body.GetILProcessor();
            var ctorEnd = constructor.Body.Instructions.Last();

            ilProcessor.InsertBefore(
                ctorEnd
                , Instruction.Create(OpCodes.Ldarg_0));

            ilProcessor.InsertBefore(
                ctorEnd
                , Instruction.Create(OpCodes.Ldarg_0));

            ilProcessor.InsertBefore(ctorEnd, _getInstanceID);

            ilProcessor.InsertBefore(
                ctorEnd
                , Instruction.Create(OpCodes.Stfld, InstanceId));
        }

        return true;
    }

    private static void InjectIL(TypeDefinition type)
    {
        var method = type.GetMethods().FirstOrDefault(m => m.Name == "CompareTo");
        if (method == null)
        {
            throw new Exception("Can't find target method: BTLight.CompareTo");
        }

        var GetId = Instruction.Create(OpCodes.Call, GetInstanceIdLazy);

        var loadFieldPosition = new List<int>(2);
        for (var i = 0; i < method.Body.Instructions.Count; i++)
        {
            var instruction = method.Body.Instructions[i];

            if (instruction.Operand is MethodReference reference1
                && _getInstanceID.Operand is MethodReference reference2
                && reference1.FullName == reference2.FullName)
            {
                loadFieldPosition.Add(i);
            }
        }

        if (loadFieldPosition.Count != 2)
        {
            throw new Exception("Can't patch BTLight.CompareTo");
        }

        foreach (var i in loadFieldPosition)
            method.Body.Instructions[i] = GetId;
    }
}