using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace RogueTechPerfFixesInjector
{
    /// <summary>
    /// Adds an id field based on int to reduce expensive .Equals calls on complex objects later on.
    /// <see cref="DOTween.Tween"/>
    /// <see cref="DOTween.Core.TweenManager"/>
    /// </summary>
    public class I_DOTween : IInjector
    {
        public void Inject(IAssemblyResolver resolver)
        {
            var assembly = resolver.Resolve(new AssemblyNameReference("DOTween", null));
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
            var equalsMethod = (MethodReference)FindInstructionAtOffset(0x0065).Operand;
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
			IL_005F: bne.un.s  IL_0082
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

            /* if (gameObjectId != 0)
			IL_006A: ldloc.s   gameObjectId
			IL_006C: brfalse.s IL_0082
			*/
            Add(Instruction.Create(OpCodes.Ldloc_S, gameObjectId));
            Add(Instruction.Create(OpCodes.Brfalse_S, originalFirstInstruction));

            /* flag2 = id.Equals(targetId);
			IL_006E: ldarg.2
			IL_006F: ldloc.s   gameObjectId
			IL_0071: box       [mscorlib]System.Int32
			IL_0076: callvirt  instance bool [mscorlib]System.Object::Equals(object)
			IL_007B: stloc.s   flag2
			*/
            Add(Instruction.Create(OpCodes.Ldarg_2));
            Add(Instruction.Create(OpCodes.Ldloca_S, gameObjectId));
            Add(Instruction.Create(OpCodes.Box, gameObjectIdType));
            Add(Instruction.Create(OpCodes.Callvirt, equalsMethod));
            Add(Instruction.Create(OpCodes.Stloc_S, flag2Var));

            /* break
            IL_007D: br        IL_010C
             */
            Add(Instruction.Create(OpCodes.Br, originalBreakTargetInstruction));

			/*
			// flag2 = id.Equals(tween.id) || id.Equals(tween.target);
			IL_0082: ldarg.2
			IL_0083: ldloc.s   tween
			IL_0085: ldfld     object DG.Tweening.Tween::id
			IL_008A: callvirt  instance bool [mscorlib]System.Object::Equals(object)
			IL_008F: brtrue.s  IL_00A0
			IL_0091: ldarg.2
			IL_0092: ldloc.s   tween
			IL_0094: ldfld     object DG.Tweening.Tween::target
			IL_0099: callvirt  instance bool [mscorlib]System.Object::Equals(object)
			IL_00AA: ldc.i4.1
			IL_00AB: stloc.s   flag2

			// break;
			IL_00A3: br.s      IL_010C
			*/
        }
    }
}