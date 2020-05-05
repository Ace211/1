using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using rjw;

namespace Rimworld_Animations {

    [HarmonyPatch(typeof(JobDriver_SexBaseInitiator), "Start")]
    static class HarmonyPatch_JobDriver_SexBaseInitiator_Start {
        public static void Postfix(ref JobDriver_SexBaseInitiator __instance) {

			/*
			 These particular jobs need special code
			 don't play anim for now
			 */
			if(__instance is JobDriver_Masturbate_Bed || __instance is JobDriver_Masturbate_Quick || __instance is JobDriver_ViolateCorpse) {
				return;
			}

			if(__instance is JobDriver_JoinInBed) {
				Log.Warning("Tried to start wrong JobDriver with Rimworld-Animations installed. If you see this warning soon after installing this mod, it's fine and animated sex will start soon. If you see this a long time after installing, that's a problem.");	
				return;
			}

			Pawn pawn = __instance.pawn;

			Building_Bed bed = __instance.Bed;

			if (__instance is JobDriver_BestialityForFemale)
				bed = (__instance as JobDriver_BestialityForFemale).Bed;
			else if (__instance is JobDriver_WhoreIsServingVisitors) {
				bed = (__instance as JobDriver_WhoreIsServingVisitors).Bed;
			}
			else if (__instance is JobDriver_SexCasualForAnimation) {
				bed = (__instance as JobDriver_SexCasualForAnimation).Bed;
			}
			else if (__instance is JobDriver_Masturbate_Bed)
				bed = (__instance as JobDriver_Masturbate_Bed).Bed;
			else if (__instance is JobDriver_Rape)
				bed = (__instance?.Partner?.jobs?.curDriver as JobDriver_Sex)?.Bed;

			if ((__instance.Target as Pawn)?.jobs?.curDriver is JobDriver_SexBaseReciever) {

				Pawn Target = __instance.Target as Pawn;

				if (!(Target.jobs.curDriver as JobDriver_SexBaseReciever).parteners.Contains(pawn)) {
					(Target.jobs.curDriver as JobDriver_SexBaseReciever).parteners.Add(pawn);
				}

				bool quickie = (__instance is JobDriver_SexQuick) && AnimationSettings.fastAnimForQuickie;

				if (bed != null) {
					RerollAnimations(Target, __instance.duration, bed as Thing, __instance.sexType, quickie);
				}
				else {
					RerollAnimations(Target, __instance.duration, sexType: __instance.sexType, fastAnimForQuickie: quickie);
				}
			}
		}

		public static void RerollAnimations(Pawn pawn, int duration, Thing bed = null, xxx.rjwSextype sexType = xxx.rjwSextype.None, bool fastAnimForQuickie = false) {

			if(pawn == null || !(pawn.jobs?.curDriver is JobDriver_SexBaseReciever)) {
				Log.Message("Error: Tried to reroll animations when pawn isn't sexing");
				return;
			}

			List<Pawn> pawnsToAnimate = (pawn.jobs.curDriver as JobDriver_SexBaseReciever).parteners.ToList();

			if (!pawnsToAnimate.Contains(pawn)) {
				pawnsToAnimate = pawnsToAnimate.Append(pawn).ToList();
			}

			AnimationDef anim = AnimationUtility.tryFindAnimation(ref pawnsToAnimate, sexType);

			if (anim != null) {

				bool mirror = GenTicks.TicksGame % 2 == 0;

				Log.Message("Now playing " + anim.defName + (mirror ? " mirrored" : ""));

				IntVec3 pos = pawn.Position;

				for (int i = 0; i < pawnsToAnimate.Count; i++) {

					if (bed != null)
						pawnsToAnimate[i].TryGetComp<CompBodyAnimator>().setAnchor(bed);
					else {

						pawnsToAnimate[i].TryGetComp<CompBodyAnimator>().setAnchor(pos);
					}

					bool shiver = pawnsToAnimate[i].jobs.curDriver is JobDriver_SexBaseRecieverRaped;
					pawnsToAnimate[i].TryGetComp<CompBodyAnimator>().StartAnimation(anim, i, mirror, shiver, fastAnimForQuickie);
					(pawnsToAnimate[i].jobs.curDriver as JobDriver_Sex).ticks_left = anim.animationTimeTicks;
					(pawnsToAnimate[i].jobs.curDriver as JobDriver_Sex).ticksLeftThisToil = anim.animationTimeTicks;
					(pawnsToAnimate[i].jobs.curDriver as JobDriver_Sex).duration = anim.animationTimeTicks;
					(pawnsToAnimate[i].jobs.curDriver as JobDriver_Sex).ticks_remaining = anim.animationTimeTicks;
					if(!AnimationSettings.hearts) {
						(pawnsToAnimate[i].jobs.curDriver as JobDriver_Sex).ticks_between_hearts = Int32.MaxValue;
					}

				} 
			}
			else {
				Log.Message("Anim not found");
				//if pawn isn't already animating,
				if (!pawn.TryGetComp<CompBodyAnimator>().isAnimating) {
					(pawn.jobs.curDriver as JobDriver_SexBaseReciever).increase_time(duration);
					//they'll just do the thrusting anim
				}
			}


		}
	}

	[HarmonyPatch(typeof(JobDriver_SexBaseInitiator), "End")]
	static class HarmonyPatch_JobDriver_SexBaseInitiator_End {

		public static void Postfix(ref JobDriver_SexBaseInitiator __instance) {

			if ((__instance.Target as Pawn)?.jobs?.curDriver is JobDriver_SexBaseReciever) {
				if (__instance.pawn.TryGetComp<CompBodyAnimator>().isAnimating) {

					List<Pawn> parteners = ((__instance.Target as Pawn)?.jobs.curDriver as JobDriver_SexBaseReciever).parteners;

					for (int i = 0; i < parteners.Count; i++) {

						//prevents pawns who started a new anim from stopping their new anim
						if (!((parteners[i].jobs.curDriver as JobDriver_SexBaseInitiator) != null && (parteners[i].jobs.curDriver as JobDriver_SexBaseInitiator).Target != __instance.pawn))
							parteners[i].TryGetComp<CompBodyAnimator>().isAnimating = false;

					}

					__instance.Target.TryGetComp<CompBodyAnimator>().isAnimating = false;

					if (xxx.is_human((__instance.Target as Pawn))) {
						(__instance.Target as Pawn)?.Drawer.renderer.graphics.ResolveApparelGraphics();
						PortraitsCache.SetDirty((__instance.Target as Pawn));
					}
				}

				((__instance.Target as Pawn)?.jobs.curDriver as JobDriver_SexBaseReciever).parteners.Remove(__instance.pawn);

			}

			if (xxx.is_human(__instance.pawn)) {
				__instance.pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
				PortraitsCache.SetDirty(__instance.pawn);
			}
		}
	}
}
