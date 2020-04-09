using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using rjw;
using Verse.Sound;

namespace Rimworld_Animations {

    [HarmonyPatch(typeof(xxx), "sexTick")]
    public static class HarmonyPatch_SexTick {

        public static bool Prefix(ref Pawn pawn, ref Pawn partner, ref bool enablerotation, ref bool pawnnude, ref bool partnernude) {


			if (enablerotation) {
				pawn.rotationTracker.Face(((Thing)partner).DrawPos);
				partner.rotationTracker.Face(((Thing)pawn).DrawPos);
			}
			if (RJWSettings.sounds_enabled && !pawn.TryGetComp<CompBodyAnimator>().isAnimating) {
				SoundDef.Named("Sex").PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
			}
			pawn.Drawer.Notify_MeleeAttackOn((Thing)(object)partner);
			if (enablerotation) {
				pawn.rotationTracker.FaceCell(partner.Position);
			}
			if (pawnnude && !xxx.has_quirk(pawn, "Endytophile")) {
				xxx.DrawNude(pawn);
			}
			if (partnernude && !xxx.has_quirk(pawn, "Endytophile")) {
				xxx.DrawNude(partner);
			}

			return false;
		}

    }
}
