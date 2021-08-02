/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using rjw;
using Verse;
using RimWorld;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;

namespace Rimworld_Animations {
	[StaticConstructorOnStartup]
	public static class HarmonyPatch_CSL {
		static HarmonyPatch_CSL() {
			try {
				((Action)(() => {
					if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Children, school and learning")) {

						(new Harmony("rjw")).Patch(AccessTools.Method(AccessTools.TypeByName("Children.PawnRenderer_RenderPawnInternal_Patch"), "RenderPawnInternalScaled"),
							prefix: new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatch_CSL), "Prefix_CSL")),
							transpiler: new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatch_CSL), "Transpiler_CSL")));
					}
				}))();
			}
			catch (TypeLoadException ex) {

			}
		}

		public static void Prefix_CSL(PawnRenderer __instance, Pawn pawn, ref Vector3 rootLoc, ref float angle, bool renderBody, ref Rot4 bodyFacing, ref Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump, bool invisible) {

			PawnGraphicSet graphics = __instance.graphics;
			CompBodyAnimator bodyAnim = pawn.TryGetComp<CompBodyAnimator>();

			if (!graphics.AllResolved) {
				graphics.ResolveAllGraphics();
			}


			if (bodyAnim != null && bodyAnim.isAnimating && !portrait) {
				bodyAnim.tickGraphics(graphics);
				pawn.TryGetComp<CompBodyAnimator>().animatePawn(ref rootLoc, ref angle, ref bodyFacing, ref headFacing);

			}
		}

		public static IEnumerable<CodeInstruction> Transpiler_CSL(IEnumerable<CodeInstruction> instructions) {

			MethodInfo drawMeshNowOrLater = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater");
			FieldInfo headGraphic = AccessTools.Field(typeof(PawnGraphicSet), "headGraphic");


			List<CodeInstruction> codes = instructions.ToList();
			bool forHead = true;
			for (int i = 0; i < codes.Count(); i++) {

				//Instead of calling drawmeshnoworlater, add pawn to the stack and call my special static method
				if (codes[i].OperandIs(drawMeshNowOrLater) && forHead) {

					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.DeclaredField(typeof(PawnRenderer), "pawn"));
					yield return new CodeInstruction(OpCodes.Ldarg_2);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(AnimationUtility), nameof(AnimationUtility.RenderPawnHeadMeshInAnimation), new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool), typeof(Pawn), typeof(float) }));

				}
				//checking for if(graphics.headGraphic != null)
				else if (codes[i].opcode == OpCodes.Ldfld && codes[i].OperandIs(headGraphic)) {
					forHead = true;
					yield return codes[i];
				}
				//checking for if(renderbody)
				else if (codes[i].opcode == OpCodes.Ldarg_3) {
					forHead = false;
					yield return codes[i];
				}
				else {
					yield return codes[i];
				}
			}
		}

	}
}*/
