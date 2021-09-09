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

			if ((target is Pawn) && 
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
					if (!__instance.Sexprops.isRape)
					{
						pawn.GainComfortFromCellIfPossible(false);
						if (target is Pawn)
						{
							(target as Pawn).GainComfortFromCellIfPossible(false);
						}
					}
					if(!__instance.isEndytophile)
                    {
						SexUtility.DrawNude(pawn, false);
                    }
				}

				return false;
            }

            return true;
        }

    }

}
