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
using Verse.AI;

namespace Rimworld_Animations
{

    [HarmonyPatch(typeof(Bed_Utility), "in_same_bed")]
    public static class HarmonyPatch_JobDriver_InSameBedPatch
    {

        public static bool Prefix(Pawn partner, ref bool __result)
        {

            if(partner != null && partner.CurJobDef == xxx.casual_sex)
            {
                __result = true;
                return false;
            }

            return true;

        }



    }

    [HarmonyPatch(typeof(JobDriver_JoinInBed), "MakeNewToils")]
    public static class HarmonyPatch_JobDriver_JoinInBed
    {

        public static void Postfix(JobDriver_JoinInBed __instance, ref IEnumerable<Toil> __result)
        {

            var toils = __result.ToList();

            Toil goToPawnInBed = Toils_Goto.GotoThing(__instance.iTarget, PathEndMode.OnCell);
            goToPawnInBed.FailOn(() => !RestUtility.InBed(__instance.Partner) && __instance.Partner.CurJobDef != xxx.gettin_loved && !Bed_Utility.in_same_bed(__instance.Partner, __instance.pawn));

            toils[1] = goToPawnInBed;


            Toil startPartnerSex = new Toil();
            startPartnerSex.initAction = delegate {


                if (!(__instance.Partner.jobs.curDriver is JobDriver_SexBaseReciever)) // allows threesomes
                {
                    Job gettinLovedJob = JobMaker.MakeJob(xxx.gettin_loved, __instance.pawn, __instance.Bed); // new gettin loved toil that wakes up the pawn goes here
                    __instance.Partner.jobs.jobQueue.EnqueueFirst(gettinLovedJob);
                    __instance.Partner.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }

            };

            toils[2] = startPartnerSex;

            toils[3].AddPreTickAction(() =>
            {
                if (!__instance.Partner.TryGetComp<CompBodyAnimator>().isAnimating)
                {
                    __instance.pawn.TryGetComp<CompBodyAnimator>().isAnimating = false;
                }
            });


            __result = toils.AsEnumerable();


        }



    }
}
