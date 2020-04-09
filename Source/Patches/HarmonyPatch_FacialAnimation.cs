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
	public static class Patch_FacialAnimation {

		static Patch_FacialAnimation() {
			try {
				((Action)(() => {
					if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "[NL] Facial Animation - WIP")) {
						(new Harmony("rjw")).Patch(AccessTools.Method(AccessTools.TypeByName("FacialAnimation.DrawFaceGraphicsComp"), "DrawGraphics"),
							prefix: new HarmonyMethod(AccessTools.Method(typeof(Patch_FacialAnimation), "Prefix")));
					}
				}))();
			}
			catch (TypeLoadException ex) {
				
			}
		}

		public static bool Prefix(ref Pawn ___pawn, ref Rot4 headFacing, ref Vector3 headOrigin, ref Quaternion quaternion, ref bool portrait) {

			CompBodyAnimator bodyAnim = ___pawn.TryGetComp<CompBodyAnimator>();

			if (bodyAnim.isAnimating && !portrait) {

				headFacing = bodyAnim.headFacing;
				headOrigin = new Vector3(bodyAnim.getPawnHeadPosition().x, headOrigin.y, bodyAnim.getPawnHeadPosition().z);
				quaternion = Quaternion.AngleAxis(bodyAnim.headAngle, Vector3.up);
			}

			return true;
		}
	}
}
