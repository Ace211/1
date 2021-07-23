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

		

		public static void RenderHeadAddonInAnimation(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Graphic graphic, AlienPartGenerator.BodyAddon bodyAddon, Vector3 v, float num, Vector3 headOffset, Pawn pawn)
        {

			CompBodyAnimator pawnAnimator = pawn.TryGetComp<CompBodyAnimator>();
			if (pawnAnimator.isAnimating && (bodyAddon.drawnInBed || bodyAddon.alignWithHead))
            {

				Quaternion headQuatInAnimation = Quaternion.AngleAxis(pawnAnimator.headAngle, Vector3.up);
				Rot4 headRotInAnimation = pawnAnimator.headFacing;
				Vector3 headPositionInAnimation = pawnAnimator.getPawnHeadPosition() - pawn.Drawer.renderer.BaseHeadOffsetAt(pawnAnimator.headFacing).RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: headQuatInAnimation)) * 2f * 57.29578f);

				Log.Message(bodyAddon.path + " " + bodyAddon.inFrontOfBody.ToStringSafe());
				headPositionInAnimation.y += bodyAddon.inFrontOfBody ? 1f : -1f;

				GenDraw.DrawMeshNowOrLater(mesh: graphic.MeshAt(rot: headRotInAnimation), loc: headPositionInAnimation + (bodyAddon.alignWithHead ? headOffset : Vector3.zero) + v.RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: headQuatInAnimation)) * 2f * 57.29578f),
					quat: Quaternion.AngleAxis(angle: num, axis: Vector3.up) * headQuatInAnimation, mat: graphic.MatAt(rot: pawnAnimator.headFacing), drawNow: drawNow);
			}

			else
            {
				GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
			}
		}
		public static Rot4 AdjustRotationValueForHeadAddon(Rot4 rotation, AlienPartGenerator.BodyAddon bodyAddon, Pawn pawn)
        {
			CompBodyAnimator anim = pawn.TryGetComp<CompBodyAnimator>();

			if (anim.isAnimating && (bodyAddon.alignWithHead || bodyAddon.drawnInBed))
            {
				return anim.headFacing;
            }
			else
            {
				return rotation;
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

					yield return new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(HarmonyPatch_AlienRace), "RenderHeadAddonInAnimation"));

                }
				else if(ins[i].opcode == OpCodes.Ldarg_S && ins[i].OperandIs((object)5)) //rotation
                {
					yield return ins[i];
					yield return new CodeInstruction(OpCodes.Ldloc, (object)4); //bodyAddon
					yield return new CodeInstruction(OpCodes.Ldarg, (object)3); //pawn
					yield return new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(HarmonyPatch_AlienRace), "AdjustRotationValueForHeadAddon"));

                }
				
				else
                {
					yield return ins[i];
				}


            }
        }

		public static bool Prefix(PawnRenderFlags renderFlags, ref Vector3 vector, ref Vector3 headOffset, Pawn pawn, ref Quaternion quat, ref Rot4 rotation) {
			/* old patch for 1.2
			if (portrait || pawn.TryGetComp<CompBodyAnimator>() == null || !pawn.TryGetComp<CompBodyAnimator>().isAnimating) return true;
			if (!(pawn.def is ThingDef_AlienRace alienProps) || invisible) return false;

			List<AlienPartGenerator.BodyAddon> addons = alienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons;
			AlienPartGenerator.AlienComp alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
			CompBodyAnimator pawnAnimator = pawn.TryGetComp<CompBodyAnimator>();

			for (int i = 0; i < addons.Count; i++) {
				AlienPartGenerator.BodyAddon ba = addons[index: i];
				if (!ba.CanDrawAddon(pawn: pawn)) continue;

				AlienPartGenerator.RotationOffset offset = ba.defaultOffsets.GetOffset((ba.drawnInBed || ba.alignWithHead) ? pawnAnimator.headFacing : rotation);
				Vector3 a = (offset != null) ? offset.GetOffset(renderFlags.FlagSet(PawnRenderFlags.Portrait), pawn.story.bodyType, comp.crownType) : Vector3.zero;


				Vector2 bodyOffset = (portrait ? offset?.portraitBodyTypes ?? offset?.bodyTypes : offset?.bodyTypes)?.FirstOrDefault(predicate: to => to.bodyType == pawn.story.bodyType)
                                   ?.offset ?? Vector2.zero;
                Vector2 crownOffset = (portrait ? offset?.portraitCrownTypes ?? offset?.crownTypes : offset?.crownTypes)?.FirstOrDefault(predicate: to => to.crownType == alienComp.crownType)
                                    ?.offset ?? Vector2.zero;

				//Defaults for tails 
				//south 0.42f, -0.3f, -0.22f
				//north     0f,  0.3f, -0.55f
				//east -0.42f, -0.3f, -0.22f   

				float moffsetX = 0.42f;
				float moffsetZ = -0.22f;

				float layerOffset = offset?.layerOffset ?? ba.layerOffset;

				float moffsetY = ba.inFrontOfBody ? 0.3f + ba.layerOffset : -0.3f - ba.layerOffset;
				float num = ba.angle;

				if (((ba.drawnInBed || ba.alignWithHead) ? pawnAnimator.headFacing : rotation) == Rot4.North) {
					moffsetX = 0f;
					if (ba.layerInvert)
						moffsetY = -moffsetY;

					moffsetZ = -0.55f;
					num = 0;
				}

				moffsetX += bodyOffset.x + crownOffset.x;
				moffsetZ += bodyOffset.y + crownOffset.y;

				if (((ba.drawnInBed || ba.alignWithHead) ? pawnAnimator.headFacing : rotation) == Rot4.East) {
					moffsetX = -moffsetX;
					num = -num; //Angle
				}

				Vector3 offsetVector = new Vector3(x: moffsetX, y: moffsetY, z: moffsetZ);

				//Angle calculation to not pick the shortest, taken from Quaternion.Angle and modified

				//assume drawnInBed means headAddon
				Graphic addonGraphic = alienComp.addonGraphics[i];
				addonGraphic.drawSize = (portrait && ba.drawSizePortrait != Vector2.zero ? ba.drawSizePortrait : ba.drawSize) * (ba.scaleWithPawnDrawsize ? alienComp.customDrawSize : Vector2.one) * 1.5f;

				if ((ba.drawnInBed || ba.alignWithHead) && pawn.TryGetComp<CompBodyAnimator>() != null && pawn.TryGetComp<CompBodyAnimator>().isAnimating) {

					Quaternion headQuatInAnimation = Quaternion.AngleAxis(pawnAnimator.headAngle, Vector3.up);
					Rot4 headRotInAnimation = pawnAnimator.headFacing;
					Vector3 headPositionInAnimation = pawnAnimator.getPawnHeadPosition() - pawn.Drawer.renderer.BaseHeadOffsetAt(pawnAnimator.headFacing).RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: headQuatInAnimation)) * 2f * 57.29578f);


					GenDraw.DrawMeshNowOrLater(mesh: addonGraphic.MeshAt(rot: headRotInAnimation), loc: headPositionInAnimation + (ba.alignWithHead ? headOffset : Vector3.zero) + offsetVector.RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: headQuatInAnimation)) * 2f * 57.29578f),
						quat: Quaternion.AngleAxis(angle: num, axis: Vector3.up) * headQuatInAnimation, mat: addonGraphic.MatAt(rot: pawnAnimator.headFacing), drawNow: portrait);

				}

				else {

					Quaternion addonRotation = quat;
					if (AnimationSettings.controlGenitalRotation && pawnAnimator.controlGenitalAngle && ba?.hediffGraphics != null && ba.hediffGraphics.Count != 0 && ba.hediffGraphics[0]?.path != null && (ba.hediffGraphics[0].path.Contains("Penis") || ba.hediffGraphics[0].path.Contains("penis"))) {
						addonRotation = Quaternion.AngleAxis(angle: pawnAnimator.genitalAngle, axis: Vector3.up);
					}

					GenDraw.DrawMeshNowOrLater(mesh: addonGraphic.MeshAt(rot: rotation), loc: vector + offsetVector.RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: quat)) * 2f * 57.29578f),
					quat: Quaternion.AngleAxis(angle: num, axis: Vector3.up) * addonRotation, mat: addonGraphic.MatAt(rot: rotation), drawNow: portrait);

				}

				
			}
			

			CompBodyAnimator pawnAnimator = pawn.TryGetComp<CompBodyAnimator>();

			ThingDef_AlienRace thingDef_AlienRace = pawn.def as ThingDef_AlienRace;
			if (thingDef_AlienRace == null || renderFlags.FlagSet(PawnRenderFlags.Invisible))
			{
				return false;
			}
			List<AlienPartGenerator.BodyAddon> bodyAddons = thingDef_AlienRace.alienRace.generalSettings.alienPartGenerator.bodyAddons;
			AlienPartGenerator.AlienComp comp = pawn.GetComp<AlienPartGenerator.AlienComp>();
			for (int i = 0; i < bodyAddons.Count; i++)
			{
				AlienPartGenerator.BodyAddon bodyAddon = bodyAddons[i];
				if (bodyAddon.CanDrawAddon(pawn))
				{
					AlienPartGenerator.RotationOffset offset = bodyAddon.defaultOffsets.GetOffset((bodyAddon.drawnInBed || bodyAddon.alignWithHead) ? pawnAnimator.headFacing : rotation);
					Vector3 a = (offset != null) ? offset.GetOffset(renderFlags.FlagSet(PawnRenderFlags.Portrait), pawn.story.bodyType, comp.crownType) : Vector3.zero;
					AlienPartGenerator.RotationOffset offset2 = bodyAddon.offsets.GetOffset((bodyAddon.drawnInBed || bodyAddon.alignWithHead) ? pawnAnimator.headFacing : rotation);
					Vector3 vector2 = a + ((offset2 != null) ? offset2.GetOffset(renderFlags.FlagSet(PawnRenderFlags.Portrait), pawn.story.bodyType, comp.crownType) : Vector3.zero);
					vector2.y = (bodyAddon.inFrontOfBody ? (0.3f + vector2.y) : (-0.3f - vector2.y));
					float num = bodyAddon.angle;
					if (rotation == Rot4.North)
					{
						if (bodyAddon.layerInvert)
						{
							vector2.y = -vector2.y;
						}
						num = 0f;
					}
					if (rotation == Rot4.East)
					{
						num = -num;
						vector2.x = -vector2.x;
					}
					Graphic graphic = comp.addonGraphics[i];
					graphic.drawSize = ((renderFlags.FlagSet(PawnRenderFlags.Portrait) && bodyAddon.drawSizePortrait != Vector2.zero) ? bodyAddon.drawSizePortrait : bodyAddon.drawSize) * (bodyAddon.scaleWithPawnDrawsize ? (bodyAddon.alignWithHead ? (renderFlags.FlagSet(PawnRenderFlags.Portrait) ? comp.customPortraitHeadDrawSize : comp.customHeadDrawSize) : (renderFlags.FlagSet(PawnRenderFlags.Portrait) ? comp.customPortraitDrawSize : comp.customDrawSize)) : Vector2.one) * 1.5f;
					GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + (bodyAddon.alignWithHead ? headOffset : Vector3.zero) + vector2.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quat)) * 2f * 57.29578f), Quaternion.AngleAxis(num, Vector3.up) * quat, graphic.MatAt(rotation, null), renderFlags.FlagSet(PawnRenderFlags.DrawNow));
				}
			}

			*/

			
			CompBodyAnimator anim = pawn.TryGetComp<CompBodyAnimator>();
			if (anim.isAnimating)
            {
				headOffset = anim.getPawnHeadOffset();
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