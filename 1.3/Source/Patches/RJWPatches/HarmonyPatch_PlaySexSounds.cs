using HarmonyLib;
using rjw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimworld_Animations
{
    [HarmonyPatch(typeof(JobDriver_Sex), "PlaySexSound")]
    class HarmonyPatch_PlaySexSounds
    {
        public static bool Prefix(JobDriver_Sex __instance)
        {
            if (__instance.pawn.TryGetComp<CompBodyAnimator>().isAnimating)
            {
                return false;
            }

            return true;
        }
    }
}
