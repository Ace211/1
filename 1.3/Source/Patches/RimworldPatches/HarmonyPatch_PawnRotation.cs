using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimworld_Animations {
    [HarmonyPatch(typeof(Thing), "Rotation", MethodType.Getter)]
    public static class HarmonyPatch_PawnRotation {

        public static bool Prefix(Thing __instance, ref Rot4 __result) {

            if(!(__instance is Pawn) || (__instance as Pawn)?.TryGetComp<CompBodyAnimator>() == null || !(__instance as Pawn).TryGetComp<CompBodyAnimator>().isAnimating) {
                return true;
            }

            Pawn pawn = (__instance as Pawn);
            __result = pawn.TryGetComp<CompBodyAnimator>().bodyFacing;

            return false;


        }

    }

}
