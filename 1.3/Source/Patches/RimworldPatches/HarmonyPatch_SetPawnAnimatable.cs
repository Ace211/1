using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Rimworld_Animations
{
	[HarmonyPatch(typeof(PawnRenderer), "RenderPawnAt")]
	public static class PawnRenderer_RenderPawnAt_Patch
	{
		static bool ClearCache(Pawn pawn)
		{
			return pawn.IsInvisible() || pawn.TryGetComp<CompBodyAnimator>().isAnimating;
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var list = instructions.ToList();

			foreach (CodeInstruction i in instructions)
			{
				if (i.OperandIs(AccessTools.Method(typeof(PawnUtility), "IsInvisible")))
				{

					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_RenderPawnAt_Patch), "ClearCache"));
				}
				else
				{
					yield return i;
				}
			}
		}
	}

}
