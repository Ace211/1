using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;

namespace Rimworld_Animations {
    public static class HarmonyPatch_HatsDisplaySelection {

        public static void PatchHatsDisplaySelectionArgs() {
            (new Harmony("rjw")).Patch(AccessTools.Method(AccessTools.TypeByName("HatDisplaySelection.Patch"), "DrawHatCEWithHair"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatch_HatsDisplaySelection), "ReplaceDrawMeshOrLaterWithAnimate")));

            (new Harmony("rjw")).Patch(AccessTools.Method(AccessTools.TypeByName("HatDisplaySelection.Patch"), "DrawHatWithHair"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatch_HatsDisplaySelection), "ReplaceDrawMeshOrLaterWithAnimate")));
        }

        public static IEnumerable<CodeInstruction> ReplaceDrawMeshOrLaterWithAnimate(IEnumerable<CodeInstruction> instructions) {

            MethodInfo drawMeshNowOrLater = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater");
            List<CodeInstruction> codes = instructions.ToList();
            for (int i = 0; i < instructions.Count(); i++) {

                
                if (codes[i].OperandIs(drawMeshNowOrLater)) {

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.DeclaredField(AccessTools.TypeByName("HatDisplaySelection.Patch"), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(AnimationUtility), nameof(AnimationUtility.RenderPawnHeadMeshInAnimation), new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool), typeof(Pawn) }));

                }
                else {
                    yield return codes[i];
                }

            }

        }

    }
}
