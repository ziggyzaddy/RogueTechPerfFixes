using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace RogueTechPerfFixesInjector;

/// <summary>
/// Adds an id field based on int to reduce expensive .Equals calls on complex objects later on.
/// <see cref="DOTween.Tween"/>
/// <see cref="DOTween.Core.TweenManager"/>
/// </summary>
public class I_DOTween : IInjector
{
    public void Inject(IAssemblyResolver resolver)
    {
        var assembly = resolver.Resolve(new AssemblyNameReference("DOTween", null), new ReaderParameters { ReadWrite = true} );
        TypeDefinition GetTypeDefinition(string fullName)
        {
            return assembly.MainModule.GetType(fullName) ?? throw new Exception($"Can't find type: {fullName} in assembly {assembly.FullName}");
        }

        var tweenType = GetTypeDefinition("DG.Tweening.Tween");
        var gameObjectIdType = assembly.MainModule.ImportReference(typeof(int));
        var gameObjectIdField = new FieldDefinition("gameObjectId", FieldAttributes.Public, gameObjectIdType);
        tweenType.Fields.Add(gameObjectIdField);

        var tweenManagerType = GetTypeDefinition("DG.Tweening.Core.TweenManager");
        var filteredOperationMethod = tweenManagerType.Methods.Single(x => x.Name == "FilteredOperation");

        Instruction FindInstructionAtOffset(int offset)
        {
            foreach (var instruction in filteredOperationMethod.Body.Instructions)
            {
                if (instruction.Offset == offset)
                {
                    return instruction;
                }
            }
            throw new Exception($"Can't find {offset}");
        }

        /* original
        // switch...
        IL_003B: switch    (IL_0055, IL_005D, IL_0080, IL_00A8)
        // flag2 = id.Equals(tween.id) || id.Equals(tween.target);
        IL_005D: ldarg.2
        IL_005E: ldloc.s   V_4 (tween)
        IL_0060: ldfld     object DG.Tweening.Tween::id
        IL_0065: callvirt  instance bool [mscorlib]System.Object::Equals(object)
        IL_006A: brtrue.s  IL_007B
        IL_006C: ldarg.2
        IL_006D: ldloc.s   V_4 (tween)
        IL_006F: ldfld     object DG.Tweening.Tween::target
        IL_0074: callvirt  instance bool [mscorlib]System.Object::Equals(object)
        IL_0079: br.s      IL_007C
        IL_007B: ldc.i4.1
        IL_007C: stloc.s   V_5 (flag2)
        // break;
        IL_007E: br.s      IL_00E7
        */
        var switchInstructions = (Instruction[])FindInstructionAtOffset(0x003B).Operand;
        var originalFirstInstruction = FindInstructionAtOffset(0x005D);
        var tweenVar = (VariableDefinition)FindInstructionAtOffset(0x005E).Operand;
        var flag2Var = (VariableDefinition)FindInstructionAtOffset(0x007C).Operand;
        var originalBreakTargetInstruction = (Instruction)FindInstructionAtOffset(0x007E).Operand;

        var processor = filteredOperationMethod.Body.GetILProcessor();
        void Add(Instruction instruction)
        {
            processor.InsertBefore(originalFirstInstruction, instruction);
        }

        var gameObjectId = new VariableDefinition(gameObjectIdType);
        processor.Body.Variables.Add(gameObjectId);

        /* if (operationType == OperationType.Despawn)
        IL_005D: ldarg.0
        IL_005E: ldc.i4.1
        IL_005F: bne.un.s  IL_0087
        */
        var first = Instruction.Create(OpCodes.Ldarg_0);
        switchInstructions[1] = first;
        Add(first);
        Add(Instruction.Create(OpCodes.Ldc_I4_1));
        Add(Instruction.Create(OpCodes.Bne_Un_S, originalFirstInstruction));

        /* int gameObjectId = tween.gameObjectId;
        IL_0061: ldloc.s   tween
        IL_0063: ldfld     int32 DG.Tweening.Tween::gameObjectId
        IL_0068: stloc.s   gameObjectId
        */
        Add(Instruction.Create(OpCodes.Ldloc_S, tweenVar));
        Add(Instruction.Create(OpCodes.Ldfld, gameObjectIdField));
        Add(Instruction.Create(OpCodes.Stloc_S, gameObjectId));

        /* if (gameObjectId != 0 && id is int)
        IL_006A: ldloc.s   gameObjectId
        IL_006C: brfalse.s IL_0087

        IL_006E: ldarg.2
        IL_006F: isinst    [mscorlib]System.Int32
        IL_0074: brfalse.s IL_0087
        */
        Add(Instruction.Create(OpCodes.Ldloc_S, gameObjectId));
        Add(Instruction.Create(OpCodes.Brfalse_S, originalFirstInstruction));

        Add(Instruction.Create(OpCodes.Ldarg_2));
        Add(Instruction.Create(OpCodes.Isinst, gameObjectIdType));
        Add(Instruction.Create(OpCodes.Brfalse_S, originalFirstInstruction));

        /* flag2 = (int)id == gameObjectId;
        IL_0076: ldarg.2
        IL_0077: unbox.any [mscorlib]System.Int32
        IL_007C: ldloc.s   gameObjectId
        IL_007E: ceq
        IL_0080: stloc.s   flag2
        */
        Add(Instruction.Create(OpCodes.Ldarg_2));
        Add(Instruction.Create(OpCodes.Unbox_Any, gameObjectIdType));
        Add(Instruction.Create(OpCodes.Ldloc_S, gameObjectId));
        Add(Instruction.Create(OpCodes.Ceq));
        Add(Instruction.Create(OpCodes.Stloc_S, flag2Var));

        /* break
        IL_0082: br        IL_0111
         */
        Add(Instruction.Create(OpCodes.Br, originalBreakTargetInstruction));
    }
}