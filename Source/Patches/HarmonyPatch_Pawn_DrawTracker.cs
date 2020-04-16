using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Rimworld_Animations {

    [HarmonyPatch(typeof(Pawn_DrawTracker), "DrawPos", MethodType.Getter)]
    public static class HarmonyPatch_Pawn_DrawTracker {
        public static bool Prefix(ref Pawn ___pawn, ref Vector3 __result) {

            if(___pawn.TryGetComp<CompBodyAnimator>() != null) {

                if(___pawn.TryGetComp<CompBodyAnimator>().isAnimating) {
                    __result = ___pawn.TryGetComp<CompBodyAnimator>().anchor + ___pawn.TryGetComp<CompBodyAnimator>().deltaPos;
                    ___pawn.Position = __result.ToIntVec3();
                    return false;
                } else if(___pawn.TryGetComp<CompBodyAnimator>().resetPosition) {
                    //resetting position to anchor
                    __result = ___pawn.TryGetComp<CompBodyAnimator>().anchor;
                    ___pawn.Position = ___pawn.TryGetComp<CompBodyAnimator>().positionReset;
                    return false;

                }
                
            }
            return true;

        }
    }

    [HarmonyPatch(typeof(Pawn_PathFollower), "StartPath")]
    public static class HarmonyPatch_ResetPositionIfPathing {
        public static void Prefix(ref Pawn ___pawn) {

            if(___pawn.TryGetComp<CompBodyAnimator>() != null) {
                if(___pawn.TryGetComp<CompBodyAnimator>().isAnimating || ___pawn.TryGetComp<CompBodyAnimator>().resetPosition) {
                    ___pawn.TryGetComp<CompBodyAnimator>().isAnimating = false;
                    ___pawn.Position = ___pawn.TryGetComp<CompBodyAnimator>().positionReset;
                }
            }
        }
    }
}
