using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimworld_Animations {
    [StaticConstructorOnStartup]
    public static class HarmonyPatch_AlienRace {
        static HarmonyPatch_AlienRace () {
			try {
				((Action)(() => {
					if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Humanoid Alien Races 2.0")) {
						(new Harmony("rjw")).Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics"),
							prefix: new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatch_AlienRace), "Prefix_StopResolveAllGraphicsWhileSex")));
					}
				}))();
			}
			catch (TypeLoadException ex) {

			}
		}

		public static bool Prefix_StopResolveAllGraphicsWhileSex(ref Pawn ___pawn) {

			if(___pawn.TryGetComp<CompBodyAnimator>() != null && ___pawn.TryGetComp<CompBodyAnimator>().isAnimating) {
				return false;
			}
			return true;
		}

    }
}
