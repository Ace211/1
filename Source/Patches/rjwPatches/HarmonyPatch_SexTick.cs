using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using rjw;
using Verse.Sound;

namespace Rimworld_Animations {

	[HarmonyPatch(typeof(JobDriver_Sex), "SexTick")]
	public static class HarmonyPatch_SexTick {

		public static bool Prefix(ref JobDriver_Sex __instance, ref Pawn pawn, ref Thing target, ref bool pawnnude, ref bool partnernude) {

			Pawn pawn2 = target as Pawn;
			if (pawn.IsHashIntervalTick(__instance.ticks_between_thrusts)) {

				__instance.Animate(pawn, (Thing)pawn2);

				if (!AnimationSettings.soundOverride || !pawn.TryGetComp<CompBodyAnimator>().isAnimating) {
					__instance.PlaySexSound();
				}

				if (!__instance.isRape) {
					pawn.GainComfortFromCellIfPossible();
					pawn2?.GainComfortFromCellIfPossible();
				}
			}
			if (!xxx.has_quirk(pawn, "Endytophile")) {
				if (pawnnude) {
					SexUtility.DrawNude(pawn);
				}
				if (pawn2 != null && partnernude) {
					SexUtility.DrawNude(pawn2);
				}
			}

			return false;
		}
	}
}
