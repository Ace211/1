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
    [StaticConstructorOnStartup]
    public static class HarmonyPatch_AlienRace {
        static HarmonyPatch_AlienRace () {
			try {
				((Action)(() => {
					if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Humanoid Alien Races 2.0")) {

						(new Harmony("rjw")).Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics"),
							prefix: new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatch_AlienRace), "Prefix_StopResolveAllGraphicsWhileSex")));

						(new Harmony("rjw")).Patch(AccessTools.Method(AccessTools.TypeByName("AlienRace.HarmonyPatches"), "DrawAddons"),
							//transpiler: new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatch_AlienRace), "Transpiler_HeadRotation")),
							prefix: new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatch_AlienRace), "Prefix_AnimateHeadAddons")));
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

		public static float RotateHeadAddon(float initialRotation, Pawn pawn)
        {
			return pawn.TryGetComp<CompBodyAnimator>().headAngle + initialRotation;
        }

		public static IEnumerable<CodeInstruction> Transpiler_HeadRotation(IEnumerable<CodeInstruction> instructions)
        {
			List<CodeInstruction> ins = instructions.ToList();
			for (int i = 0; i < ins.Count; i++)
            {

				yield return ins[i];

				if(ins[i].opcode == OpCodes.Stloc_S && ins[i].OperandIs((object)6))
                {
					yield return new CodeInstruction(OpCodes.Ldloc_S, (object)6);
					yield return new CodeInstruction(OpCodes.Ldarg_S, (object)4);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatch_AlienRace), "RotateHeadAddon"));
					yield return new CodeInstruction(OpCodes.Stloc_S, (object)6);

                }
            }
        }

		public static bool Prefix_AnimateHeadAddons(PawnRenderFlags renderFlags, ref Vector3 vector, ref Vector3 headOffset, Pawn pawn, ref Quaternion quat, ref Rot4 rotation) {
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
				quat = Quaternion.AngleAxis(anim.headAngle * 5, Vector3.up);
				rotation = anim.headFacing;
            }

			return true;


		}
	}
}