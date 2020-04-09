using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rjw;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

namespace Rimworld_Animations {
    [HarmonyPatch(typeof(JobGiver_JoinInBed), "TryGiveJob")]
    public static class HarmonyPatch_JoinInBedGiveJob {

        public static bool Prefix(ref Job __result, ref Pawn pawn) {

			__result = null;

			if (!RJWHookupSettings.HookupsEnabled)
				return false;

			if (pawn.Drafted)
				return false;

			if (!SexUtility.ReadyForHookup(pawn))
				return false;

			// We increase the time right away to prevent the fairly expensive check from happening too frequently
			SexUtility.IncreaseTicksToNextHookup(pawn);

			// If the pawn is a whore, or recently had sex, skip the job unless they're really horny
			if (!xxx.is_frustrated(pawn) && (xxx.is_whore(pawn) || !SexUtility.ReadyForLovin(pawn)))
				return false;

			// This check attempts to keep groups leaving the map, like guests or traders, from turning around to hook up
			if (pawn.mindState?.duty?.def == DutyDefOf.TravelOrLeave) {
				// TODO: Some guest pawns keep the TravelOrLeave duty the whole time, I think the ones assigned to guard the pack animals.
				// That's probably ok, though it wasn't the intention.
				if (RJWSettings.DebugLogJoinInBed) Log.Message($"[RJW] JoinInBed.TryGiveJob:({xxx.get_pawnname(pawn)}): has TravelOrLeave, no time for lovin!");
				return false;
			}

			if (pawn.CurJob == null || pawn.CurJob.def == JobDefOf.LayDown) {
				//--Log.Message("   checking pawn and abilities");
				if (xxx.can_fuck(pawn) || xxx.can_be_fucked(pawn)) {
					//--Log.Message("   finding partner");
					Pawn partner = JobGiver_JoinInBed.find_pawn_to_fuck(pawn, pawn.Map);

					//--Log.Message("   checking partner");
					if (partner == null)
						return false;

					// Can never be null, since find checks for bed.
					Building_Bed bed = partner.CurrentBed();

					// Interrupt current job.
					if (pawn.CurJob != null && pawn.jobs.curDriver != null)
						pawn.jobs.curDriver.EndJobWith(JobCondition.InterruptForced);

					__result = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("JoinInBedAnimation", true), partner, bed);
					return false;
				}
			}

			return false;

		}

	}
}
