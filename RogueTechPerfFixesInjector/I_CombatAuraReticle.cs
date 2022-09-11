using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace RogueTechPerfFixesInjector
{
    public class I_CombatAuraReticle : IInjector
    {
        private const string _targetType = "BattleTech.UI.CombatAuraReticle";

        private static FieldDefinition _counter;

        private static FieldDefinition _interval;

        private const int _intervalValue = 10;

        public void Inject(IAssemblyResolver resolver)
        {
            var assembly = resolver.Resolve(new AssemblyNameReference("Assembly-CSharp", null));
            var type = assembly.MainModule.GetType(_targetType) ?? throw new Exception($"Can't find target type: {_targetType}");

            InjectField(type, assembly);
            InitField(type);
            InjectIL(type);
        }

        private static void InjectField(TypeDefinition type, AssemblyDefinition assembly)
        {
            var unsignedInt = assembly.MainModule.ImportReference(typeof(uint));

            _counter = new FieldDefinition(
                "_counter"
                , FieldAttributes.Private
                , unsignedInt);

            _interval = new FieldDefinition(
                "_updateInterval"
                , FieldAttributes.Private | FieldAttributes.Static
                , unsignedInt);

            type.Fields.Add(_counter);
            type.Fields.Add(_interval);
        }

        private static void InitField(TypeDefinition type)
        {
            var staticCtor = type.GetStaticConstructor();
            var ilProcessor = staticCtor.Body.GetILProcessor();
            var ctorStart = staticCtor.Body.Instructions[0];

            ilProcessor.InsertBefore(ctorStart, Instruction.Create(OpCodes.Ldc_I4, _intervalValue));
            ilProcessor.InsertBefore(ctorStart, Instruction.Create(OpCodes.Stsfld, _interval));
        }

        private static void InjectIL(TypeDefinition type)
        {
            const string targetMethod = "LateUpdate";

            var method =
                type.GetMethods().FirstOrDefault(m => m.Name == targetMethod);

            if (method == null)
            {
                throw new Exception($"Can't find method: {targetMethod}\n");
            }

            var ilProcessor = method.Body.GetILProcessor();
            var methodStart = method.Body.Instructions[0];

            var newInstructions = CreateInstructions(ilProcessor, methodStart);
            newInstructions.Reverse();

            foreach (var instruction in newInstructions)
            {
                ilProcessor.InsertBefore(method.Body.Instructions[0], instruction);
            }
        }

        private static List<Instruction> CreateInstructions(ILProcessor ilProcessor, Instruction branchTarget)
        {
            var instructions = new List<Instruction>
            {
                // int remainder = _counter % _interval;
                ilProcessor.Create(OpCodes.Ldarg_0),
                ilProcessor.Create(OpCodes.Ldfld, _counter),
                ilProcessor.Create(OpCodes.Ldsfld, _interval),
                ilProcessor.Create(OpCodes.Rem_Un),

                // _counter++;
                ilProcessor.Create(OpCodes.Ldarg_0),
                ilProcessor.Create(OpCodes.Ldarg_0),
                ilProcessor.Create(OpCodes.Ldfld, _counter),
                ilProcessor.Create(OpCodes.Ldc_I4_1),
                ilProcessor.Create(OpCodes.Add),
                ilProcessor.Create(OpCodes.Stfld, _counter),

                // if (equal) goto branchTarget;
                ilProcessor.Create(OpCodes.Brfalse, branchTarget),

                // return;
                ilProcessor.Create(OpCodes.Ret),
            };

            return instructions;
        }
    }
}
