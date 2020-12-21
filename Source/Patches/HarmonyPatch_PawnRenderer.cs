using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;

namespace Rimworld_Animations {

	[HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new Type[]
		{
			typeof(Vector3),
			typeof(float),
			typeof(bool),
			typeof(Rot4),
			typeof(Rot4),
			typeof(RotDrawMode),
			typeof(bool),
			typeof(bool),
			typeof(bool)
		}
	)]
	public static class HarmonyPatch_PawnRenderer {

		[HarmonyBefore(new string[] { "showhair.kv.rw", "erdelf.HumanoidAlienRaces", "Nals.FacialAnimation" })]
		public static void Prefix(PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, bool renderBody, ref Rot4 bodyFacing, ref Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump, bool invisible) {
			PawnGraphicSet graphics = __instance.graphics;
			Pawn pawn = graphics.pawn;
			CompBodyAnimator bodyAnim = pawn.TryGetComp<CompBodyAnimator>();

			if (!graphics.AllResolved) {
				graphics.ResolveAllGraphics();
			}


			if (bodyAnim != null && bodyAnim.isAnimating && !portrait && pawn.Map == Find.CurrentMap) {
				bodyAnim.tickGraphics(graphics);
				bodyAnim.animatePawn(ref rootLoc, ref angle, ref bodyFacing, ref headFacing);

			}
		}
	}

	[StaticConstructorOnStartup]
	public static class HarmonyPatch_Animate
    {

		static HarmonyPatch_Animate() {

			if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Hats Display Selection")) {
				HarmonyPatch_HatsDisplaySelection.PatchHatsDisplaySelectionArgs();
			}
			else {
				PatchRimworldFunctionsNormally();
			}
		}

		static void PatchRimworldFunctionsNormally() {
			(new Harmony("rjw")).Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", parameters: new Type[]
				{
					typeof(Vector3),
					typeof(float),
					typeof(bool),
					typeof(Rot4),
					typeof(Rot4),
					typeof(RotDrawMode),
					typeof(bool),
					typeof(bool),
					typeof(bool)
				}),
				transpiler: new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatch_Animate), "Transpiler")));
		}

		[HarmonyAfter(new string[] { "showhair.kv.rw", "erdelf.HumanoidAlienRaces", "Nals.FacialAnimation" })]
		[HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {

			MethodInfo drawMeshNowOrLater = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater");
			FieldInfo headGraphic = AccessTools.Field(typeof(PawnGraphicSet), "headGraphic");


			List<CodeInstruction> codes = instructions.ToList();
			bool forHead = true;
			for(int i = 0; i < codes.Count(); i++) {

				//Instead of calling drawmeshnoworlater, add pawn to the stack and call my special static method
				if (codes[i].OperandIs(drawMeshNowOrLater) && forHead) {

					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.DeclaredField(typeof(PawnRenderer), "pawn"));
					yield return new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(AnimationUtility), nameof(AnimationUtility.RenderPawnHeadMeshInAnimation), new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool), typeof(Pawn) }));

				}
				//checking for if(graphics.headGraphic != null)
				else if (codes[i].opcode == OpCodes.Ldfld && codes[i].OperandIs(headGraphic)) {
					forHead = true;
					yield return codes[i];
				} 
				//checking for if(renderbody)
				else if(codes[i].opcode == OpCodes.Ldarg_3) {
					forHead = false;
					yield return codes[i];
				}
				else {
					yield return codes[i];
				}
			}
		}
    }
}
