using HarmonyLib;
using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rimworld_Animations
{
    [HarmonyPatch(typeof(PawnUtility), "GetPosture")]
    public static class HarmonyPatch_SetPawnLaying
    {

        public static bool Prefix(Pawn p, ref PawnPosture __result)
        {
            if(p.TryGetComp<CompBodyAnimator>().isAnimating)
            {
                __result = PawnPosture.LayingOnGroundNormal;
                return false;
            }

            return true;
        }

    }
}
