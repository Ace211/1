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
using AlienRace;

namespace Rimworld_Animations {

    [HarmonyPatch(typeof(AlienRace.HarmonyPatches), "DrawAddons")]
    public static class HarmonyPatch_AlienRace {

		public static void RenderHeadAddonInAnimation(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Graphic graphic, AlienPartGenerator.BodyAddon bodyAddon, Vector3 v, float num, Vector3 headOffset, Pawn pawn, PawnRenderFlags renderFlags)
        {

			CompBodyAnimator pawnAnimator = pawn.TryGetComp<CompBodyAnimator>();
			if (pawnAnimator != null && !renderFlags.FlagSet(PawnRenderFlags.Portrait) && pawnAnimator.isAnimating && (bodyAddon.drawnInBed || bodyAddon.alignWithHead))
            {

				Quaternion headQuatInAnimation = Quaternion.AngleAxis(pawnAnimator.headAngle, Vector3.up);
				Rot4 headRotInAnimation = pawnAnimator.headFacing;
				Vector3 headPositionInAnimation = pawnAnimator.getPawnHeadPosition() - pawn.Drawer.renderer.BaseHeadOffsetAt(pawnAnimator.headFacing).RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: headQuatInAnimation)) * 2f * 57.29578f);

				headPositionInAnimation.y = loc.y;

				GenDraw.DrawMeshNowOrLater(mesh: graphic.MeshAt(rot: headRotInAnimation), loc: headPositionInAnimation + (bodyAddon.alignWithHead ? headOffset : Vector3.zero) + v.RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: headQuatInAnimation)) * 2f * 57.29578f),
					quat: Quaternion.AngleAxis(angle: num, axis: Vector3.up) * headQuatInAnimation, mat: graphic.MatAt(rot: pawnAnimator.headFacing), drawNow: drawNow);
			}

			else
            {
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
			

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
			List<CodeInstruction> ins = instructions.ToList();
			for (int i = 0; i < ins.Count; i++)
            {

				Type[] type = new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) };


				if (ins[i].OperandIs(AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", type)))
                {
					
					yield return new CodeInstruction(OpCodes.Ldloc, (object)7); //graphic
					yield return new CodeInstruction(OpCodes.Ldloc, (object)4); //bodyAddon
					yield return new CodeInstruction(OpCodes.Ldloc, (object)5); //offsetVector/AddonOffset (v)
					yield return new CodeInstruction(OpCodes.Ldloc, (object)6); //num
					yield return new CodeInstruction(OpCodes.Ldarg, (object)2); //headOffset
					yield return new CodeInstruction(OpCodes.Ldarg, (object)3); //pawn
					yield return new CodeInstruction(OpCodes.Ldarg, (object)0); //renderflags

					yield return new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(HarmonyPatch_AlienRace), "RenderHeadAddonInAnimation"));

                }
				
				else
                {
					yield return ins[i];
				}
            }
        }

		public static bool Prefix(PawnRenderFlags renderFlags, ref Vector3 vector, ref Vector3 headOffset, Pawn pawn, ref Quaternion quat, ref Rot4 rotation)
		{
			if(pawn == null)
            {
				return true;
			}
				
			CompBodyAnimator anim = pawn.TryGetComp<CompBodyAnimator>();

			if(anim == null)
            {
				return true;
            }

			if (anim != null && !renderFlags.FlagSet(PawnRenderFlags.Portrait) && anim.isAnimating)
			{
				quat = Quaternion.AngleAxis(anim.bodyAngle, Vector3.up);
			}

			return true;

		}
	}

	[HarmonyPatch(typeof(PawnGraphicSet), "ResolveApparelGraphics")]
	public static class HarmonyPatch_ResolveApparelGraphics
	{
		public static bool Prefix(ref Pawn ___pawn)
		{

			if (___pawn.TryGetComp<CompBodyAnimator>() != null && ___pawn.TryGetComp<CompBodyAnimator>().isAnimating)
			{
				return false;
			}
			return true;
		}
	}
}


