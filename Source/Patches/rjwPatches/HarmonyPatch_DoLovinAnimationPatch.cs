using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using rjw;
using Verse.AI;

namespace Rimworld_Animations {

    [HarmonyPatch(typeof(JobGiver_DoLovin), "TryGiveJob")]
    public static class HarmonyPatch_DoLovinAnimationPatch {

        public static void Postfix(ref Pawn pawn, ref Job __result) {

            if(__result != null) {

                RestUtility.WakeUp(pawn);
                Pawn partnerInMyBed = LovePartnerRelationUtility.GetPartnerInMyBed(pawn);
                __result = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("JoinInBedAnimation", true), partnerInMyBed, partnerInMyBed.CurrentBed());
            }

        }
    }
}
