using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimworld_Animations {
    class HarmonyPatch_DontShaveYourHead {

		[StaticConstructorOnStartup]
		public static class Patch_DontShaveYourHead {

			static Patch_DontShaveYourHead() {
				try {
					((Action)(() =>
					{
						if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Don't Shave Your Head 1.0")) {
							(new Harmony("rjw")).Patch(AccessTools.Method(AccessTools.TypeByName("DontShaveYourHead.Harmony_PawnRenderer"), "DrawHairReroute"), //typeof(ShowHair.Patch_PawnRenderer_RenderPawnInternal), nameof(ShowHair.Patch_PawnRenderer_RenderPawnInternal.Postfix)),
								transpiler: new HarmonyMethod(AccessTools.Method(typeof(Patch_ShowHairWithHats), "Transpiler")));
						}
					}))();
				}
				catch (TypeLoadException ex) { }
			}
		}
	}
}
