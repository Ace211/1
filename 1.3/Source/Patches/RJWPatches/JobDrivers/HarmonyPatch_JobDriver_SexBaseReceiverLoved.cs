using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using rjw;
using System.Reflection.Emit;

namespace Rimworld_Animations
{

    [HarmonyPatch(typeof(JobDriver_SexBaseRecieverLoved), "MakeSexToil")]
    public static class HarmonyPatch_JobDriver_SexBaseReceiverLoved
    {

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {

            var ins = codeInstructions.ToList();
            for(int i = 0; i < ins.Count; i++)
            {
                if(i + 13 < ins.Count && ins[i + 13].opcode == OpCodes.Call && ins[i + 13].OperandIs(AccessTools.DeclaredMethod(typeof(Toils_LayDown), "LayDown"))) {

                    ins.RemoveRange(i, 14);

                }

                else
                {
                    yield return ins[i];
                }
            }

        }

    }
}
