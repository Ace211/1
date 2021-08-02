using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimworld_Animations {
    [StaticConstructorOnStartup]
    public static class Patch_ShowHairWithHats {

		static Patch_ShowHairWithHats() {
			try {
				((Action)(() =>
				{
					if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "[KV] Show Hair With Hats or Hide All Hats - 1.1")) {
						(new Harmony("rjw")).Patch(AccessTools.Method(AccessTools.TypeByName("ShowHair.Patch_PawnRenderer_RenderPawnInternal"), "Postfix"), //typeof(ShowHair.Patch_PawnRenderer_RenderPawnInternal), nameof(ShowHair.Patch_PawnRenderer_RenderPawnInternal.Postfix)),
							transpiler: new HarmonyMethod(AccessTools.Method(typeof(Patch_ShowHairWithHats), "Transpiler")));
					}
				}))();
			}
			catch (TypeLoadException ex) { }
		}


		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {

			MethodInfo drawMeshNowOrLater = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater");

			List<CodeInstruction> codes = instructions.ToList();
			for (int i = 0; i < codes.Count(); i++) {

				//Instead of calling drawmeshnoworlater, add pawn to the stack and call my special static method
				if (codes[i].OperandIs(drawMeshNowOrLater)) {

					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.DeclaredField(typeof(PawnRenderer), "pawn"));
					yield return new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(AnimationUtility), nameof(AnimationUtility.RenderPawnHeadMeshInAnimation), new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool), typeof(Pawn) }));

				}
				else {
					yield return codes[i];
				}
			}
		}
	}
}
