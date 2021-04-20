using HarmonyLib;
using RimWorld;
using rjw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Rimworld_Animations
{
    [HarmonyPatch(typeof(JobDriver_Sex), "SexTick")]
    public class HarmonyPatch_SexTick
    {
        public static bool Prefix(JobDriver_Sex __instance, Pawn pawn, Thing target)
        {

			if (!(target is Pawn) || 
				!(
				(target as Pawn)?.jobs?.curDriver is JobDriver_SexBaseReciever 
				&& 
				((target as Pawn).jobs.curDriver as JobDriver_SexBaseReciever).parteners.Any() 
				&& 
				((target as Pawn).jobs.curDriver as JobDriver_SexBaseReciever).parteners[0] == pawn))
            {

				__instance.ticks_left--;
                __instance.sex_ticks--;
                __instance.Orgasm();


				if (pawn.IsHashIntervalTick(__instance.ticks_between_thrusts))
				{
					__instance.ChangePsyfocus(pawn, target);
					__instance.Animate(pawn, target);
					__instance.PlaySexSound();
					if (!__instance.isRape)
					{
						pawn.GainComfortFromCellIfPossible(false);
						if (target is Pawn)
						{
							(target as Pawn).GainComfortFromCellIfPossible(false);
						}
					}
				}

				return false;
            }

            return true;
        }

    }

    [HarmonyPatch(typeof(JobDriver_Sex), "Orgasm")]
    public class HarmonyPatch_Orgasm
    {
        public static bool Prefix(JobDriver_Sex __instance)
        {
			//todo: remove this code on next update
			if (__instance.pawn.jobs.curDriver is JobDriver_SexBaseRecieverLoved ||
				__instance.pawn.jobs.curDriver is JobDriver_SexBaseRecieverRaped) return true;

			if (__instance.sex_ticks > __instance.orgasmstick)
			{
				return false;
			}
			__instance.orgasms++;
			__instance.PlayCumSound();
			__instance.PlayOrgasmVoice();
			__instance.CalculateSatisfactionPerTick();

			if (__instance.pawn?.jobs?.curDriver is JobDriver_SexBaseInitiator)
			{
				SexUtility.SatisfyPersonal(__instance.pawn, __instance.Partner, __instance.sexType, __instance.isRape, true, __instance.satisfaction);
			}
			else
			{
				if (__instance.pawn?.jobs?.curDriver is JobDriver_SexBaseReciever)
				{
					Pawn pawn = __instance.pawn;
					Pawn_JobTracker jobs3 = __instance.pawn.jobs;
					SexUtility.SatisfyPersonal(pawn, (__instance.pawn.jobs.curDriver as JobDriver_SexBaseReciever).parteners.FirstOrFallback(null), __instance.sexType, false, false, __instance.satisfaction);
				}

			}
			Log.Message(xxx.get_pawnname(__instance.pawn) + " Orgasmed", false);
			__instance.sex_ticks = __instance.Roll_Orgasm_Duration_Reset();
			if (__instance.neverendingsex)
			{
				__instance.ticks_left = __instance.duration;
			}

			return false;
		}

    }
}
