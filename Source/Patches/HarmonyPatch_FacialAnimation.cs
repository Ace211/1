using HarmonyLib;
using RimWorld;
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


						(new Harmony("rjw")).Patch(AccessTools.Method(AccessTools.TypeByName("FacialAnimation.FaceAnimationDef"), "IsSame", new Type[] { typeof(JobDef) }),
							prefix: new HarmonyMethod(AccessTools.Method(typeof(Patch_FacialAnimation), "Prefix_IsSameA")));

						(new Harmony("rjw")).Patch(AccessTools.Method(AccessTools.TypeByName("FacialAnimation.FaceAnimationDef"), "IsSame", new Type[] { typeof(string) }),
							prefix: new HarmonyMethod(AccessTools.Method(typeof(Patch_FacialAnimation), "Prefix_IsSameB")));
					}
				}))();
			}
			catch (TypeLoadException ex) {
				
			}
		}

		public static bool Prefix(ref Pawn ___pawn, ref Rot4 headFacing, ref Vector3 headOrigin, ref Quaternion quaternion, ref bool portrait) {

			CompBodyAnimator bodyAnim = ___pawn.TryGetComp<CompBodyAnimator>();

			if (bodyAnim != null && bodyAnim.isAnimating && !portrait) {

				headFacing = bodyAnim.headFacing;
				headOrigin = new Vector3(bodyAnim.getPawnHeadPosition().x, headOrigin.y, bodyAnim.getPawnHeadPosition().z);
				quaternion = Quaternion.AngleAxis(bodyAnim.headAngle, Vector3.up);
			}

			return true;
		}

		public static List<string> rjwLovinDefNames = new List<string>{
			"JoinInBed",
			"GettinLoved",
			"GettinLicked",
			"GettinSucked",
			"GettinRaped",
			"ViolateCorpse",
			"Masturbate_Bed",
			"Masturbate_Quick",
			"GettinBred",
			"Bestiality",
			"BestialityForFemale",
			"StruggleInBondageGear",
			"WhoreIsServingVisitors",
			"UseFM"
		};


		public static bool Prefix_IsSameA(JobDef job, string ___jobDef, ref bool __result) {

			if(___jobDef != null && ___jobDef == "Lovin" && job?.defName != null && rjwLovinDefNames.Contains(job?.defName)) {
				__result = true;
				return false;
			}

			return true;
		}

		public static bool Prefix_IsSameB(string jobName, string ___jobDef, ref bool __result) {

			if (___jobDef != null && ___jobDef == "Lovin" && jobName != null && rjwLovinDefNames.Contains(jobName)) {
				__result = true;
				return false;
			}

			return true;
		}
	}
}
