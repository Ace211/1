using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse.AI;
using rjw;
using HarmonyLib;
using Verse;

namespace Rimworld_Animations {

    [HarmonyPatch(typeof(Pawn_JobTracker), "TryTakeOrderedJob")]
    class HarmonyPatch_PlayAnimJoinInBedRMB {
        public static void Prefix(ref Job job) {
            if(job.def == xxx.casual_sex) {
                if (AnimationSettings.debugMode || RJWSettings.DevMode)
                    Log.Message("Replacing vanilla RJW JoinInBed JobDriver for animation JobDriver");
                job = new Job(DefDatabase<JobDef>.GetNamed("JoinInBedAnimation", true), job.targetA, job.targetB, job.targetC);
            }

        }

    }
}
