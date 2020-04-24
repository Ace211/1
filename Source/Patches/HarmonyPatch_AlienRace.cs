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
    public static class HarmonyPatch_AlienRace {
        static HarmonyPatch_AlienRace () {
			try {
				((Action)(() => {
					if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "Humanoid Alien Races 2.0")) {

						(new Harmony("rjw")).Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics"),
							prefix: new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatch_AlienRace), "Prefix_StopResolveAllGraphicsWhileSex")));

						(new Harmony("rjw")).Patch(AccessTools.Method(AccessTools.TypeByName("AlienRace.HarmonyPatches"), "DrawAddons"),
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

		public static bool Prefix_AnimateHeadAddons(bool portrait, Vector3 vector, Pawn pawn, Quaternion quat, Rot4 rotation, bool invisible) {

			if (portrait || pawn.TryGetComp<CompBodyAnimator>() == null || !pawn.TryGetComp<CompBodyAnimator>().isAnimating) return true;
			if (!(pawn.def is AlienRace.ThingDef_AlienRace alienProps) || invisible) return false;

			List<AlienRace.AlienPartGenerator.BodyAddon> addons = alienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons;
			AlienRace.AlienPartGenerator.AlienComp alienComp = pawn.GetComp<AlienRace.AlienPartGenerator.AlienComp>();
			CompBodyAnimator pawnAnimator = pawn.TryGetComp<CompBodyAnimator>();

			for (int i = 0; i < addons.Count; i++) {
				AlienRace.AlienPartGenerator.BodyAddon ba = addons[index: i];
				if (!ba.CanDrawAddon(pawn: pawn)) continue;

				AlienRace.AlienPartGenerator.RotationOffset offset;
				if (ba.drawnInBed) {

					offset = pawnAnimator.headFacing == Rot4.South ?
														ba.offsets.south :
														pawnAnimator.headFacing == Rot4.North ?
															ba.offsets.north :
															pawnAnimator.headFacing == Rot4.East ?
															ba.offsets.east :
															ba.offsets.west;

				} else {

					offset = rotation == Rot4.South ?
										ba.offsets.south :
										rotation == Rot4.North ?
											ba.offsets.north :
											rotation == Rot4.East ?
											ba.offsets.east :
											ba.offsets.west;

				}

				

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
				float moffsetY = ba.inFrontOfBody ? 0.3f + ba.layerOffset : -0.3f - ba.layerOffset;
				float num = ba.angle;

				if ((ba.drawnInBed ? pawnAnimator.headFacing : rotation) == Rot4.North) {
					moffsetX = 0f;
					if (ba.layerInvert)
						moffsetY = -moffsetY;

					moffsetZ = -0.55f;
					num = 0;
				}

				moffsetX += bodyOffset.x + crownOffset.x;
				moffsetZ += bodyOffset.y + crownOffset.y;

				if ((ba.drawnInBed ? pawnAnimator.headFacing : rotation) == Rot4.East) {
					moffsetX = -moffsetX;
					num = -num; //Angle
				}

				Vector3 offsetVector = new Vector3(x: moffsetX, y: moffsetY, z: moffsetZ);

				//Angle calculation to not pick the shortest, taken from Quaternion.Angle and modified

				//assume drawnInBed means headAddon
				if (ba.drawnInBed && pawn.TryGetComp<CompBodyAnimator>() != null && pawn.TryGetComp<CompBodyAnimator>().isAnimating) {

					Quaternion headQuatInAnimation = Quaternion.AngleAxis(pawnAnimator.headAngle, Vector3.up);
					Rot4 headRotInAnimation = pawnAnimator.headFacing;
					Vector3 headPositionInAnimation = pawnAnimator.getPawnHeadPosition() - pawn.Drawer.renderer.BaseHeadOffsetAt(pawnAnimator.headFacing).RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: headQuatInAnimation)) * 2f * 57.29578f);


					GenDraw.DrawMeshNowOrLater(mesh: alienComp.addonGraphics[index: i].MeshAt(rot: headRotInAnimation), loc: headPositionInAnimation + offsetVector.RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: headQuatInAnimation)) * 2f * 57.29578f),
						quat: Quaternion.AngleAxis(angle: num, axis: Vector3.up) * headQuatInAnimation, mat: alienComp.addonGraphics[index: i].MatAt(rot: pawnAnimator.headFacing), drawNow: portrait);

				}
				else {

					GenDraw.DrawMeshNowOrLater(mesh: alienComp.addonGraphics[index: i].MeshAt(rot: rotation), loc: vector + offsetVector.RotatedBy(angle: Mathf.Acos(f: Quaternion.Dot(a: Quaternion.identity, b: quat)) * 2f * 57.29578f),
					quat: Quaternion.AngleAxis(angle: num, axis: Vector3.up) * quat, mat: alienComp.addonGraphics[index: i].MatAt(rot: rotation), drawNow: portrait);

				}

				
			}

			return false;
		}
	}
}
