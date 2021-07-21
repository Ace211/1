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
					typeof(RotDrawMode),
					typeof(PawnRenderFlags)
		}
	)]
	public static class HarmonyPatch_PawnRenderer
	{

		[HarmonyBefore(new string[] { "showhair.kv.rw", "erdelf.HumanoidAlienRaces", "Nals.FacialAnimation" })]
		public static void Prefix(PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, bool renderBody, ref Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
		{
			PawnGraphicSet graphics = __instance.graphics;
			Pawn pawn = graphics.pawn;
			CompBodyAnimator bodyAnim = pawn.TryGetComp<CompBodyAnimator>();


			if (bodyAnim != null && bodyAnim.isAnimating && pawn.Map == Find.CurrentMap)
			{
				bodyAnim.animatePawnBody(ref rootLoc, ref angle, ref bodyFacing);

			}
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			bool forHead = false;

			foreach (CodeInstruction i in instructions)
			{



				
				if (i.opcode == OpCodes.Ldfld && i.OperandIs(AccessTools.Field(typeof(PawnGraphicSet), "headGraphic")))
				{

					forHead = true;
					yield return i;
				}

				else if (forHead && i.operand == (object)7)
				{

					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
					yield return new CodeInstruction(OpCodes.Ldloc_S, operand: 7);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AnimationUtility), "PawnHeadRotInAnimation"));
				}

				else
                {
					yield return i;
				}
				
				
			}

		}

	}


	

}
