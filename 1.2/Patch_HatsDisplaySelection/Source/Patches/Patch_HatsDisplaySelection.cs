using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using HatDisplaySelection;
using Rimworld_Animations;
using UnityEngine;
using Verse;

namespace Patch_HatsDisplaySelection
{
	[HarmonyBefore(new string[] { "velc.HatsDisplaySelection" })]
    [HarmonyPatch(typeof(HatDisplaySelection.Patch), "Patch_PawnRenderer_RenderPawnInternal_Initialize")]
    public class Patch_HatsDisplaySelectionInitialize

    {

		public static void Prefix(PawnRenderer __instance, ref Pawn ___pawn, ref Vector3 rootLoc, ref float angle, ref Rot4 bodyFacing, ref Rot4 headFacing)
        {

			CompBodyAnimator bodyAnim = ___pawn.TryGetComp<CompBodyAnimator>();
			bodyAnim.animatePawn(ref rootLoc, ref angle, ref bodyFacing, ref headFacing);
		}

        public static void Postfix(PawnRenderer __instance)
        {
			PawnGraphicSet graphics = __instance.graphics;
			Pawn pawn = graphics.pawn;
			CompBodyAnimator bodyAnim = pawn.TryGetComp<CompBodyAnimator>();

			if (!graphics.AllResolved)
			{
				graphics.ResolveAllGraphics();
			}


			if (bodyAnim != null && bodyAnim.isAnimating && pawn.Map == Find.CurrentMap)
			{
				bodyAnim.tickGraphics(graphics);
				

			}
		}
    }
}
