﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimworld_Animations {

    [HarmonyPatch(typeof(Pawn_DrawTracker), "DrawPos", MethodType.Getter)]
    public static class HarmonyPatch_Pawn_DrawTracker {
        public static bool Prefix(ref Pawn ___pawn, ref Vector3 __result) {

            CompBodyAnimator bodyAnim = ___pawn.TryGetComp<CompBodyAnimator>();

            if (bodyAnim != null && bodyAnim.isAnimating) {
                __result = ___pawn.TryGetComp<CompBodyAnimator>().anchor + ___pawn.TryGetComp<CompBodyAnimator>().deltaPos;

                if (bodyAnim.CurrentAnimation?.actors[bodyAnim.ActorIndex]?.offsetsByDefName != null && bodyAnim.CurrentAnimation.actors[bodyAnim.ActorIndex].offsetsByDefName.ContainsKey(___pawn.def.defName)) {
                    __result.x += bodyAnim.CurrentAnimation.actors[bodyAnim.ActorIndex].offsetsByDefName[___pawn.def.defName].x;
                    __result.z += bodyAnim.CurrentAnimation.actors[bodyAnim.ActorIndex].offsetsByDefName[___pawn.def.defName].y;
                }

                return false;
            }
            return true;

        }
    }
}
