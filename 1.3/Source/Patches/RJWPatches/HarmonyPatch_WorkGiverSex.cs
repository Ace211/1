using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rjw;
using HarmonyLib;
using Verse;
using RimWorld;
using Verse.AI;

namespace Rimworld_Animations {

    [HarmonyPatch(typeof(WorkGiver_Sex), "JobOnThing")]
    public static class HarmonyPatch_WorkGiverSex {

        public static bool Prefix(ref Job __result, ref Thing t) {

            Building_Bed bed = RestUtility.CurrentBed(t as Pawn);
            if (bed == null) {
                return false;
            }
            __result = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("JoinInBedAnimation", true), t as Pawn, bed);
            return false;

        }

    }
}
