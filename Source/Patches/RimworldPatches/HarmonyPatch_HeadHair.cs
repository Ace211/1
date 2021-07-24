using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace Rimworld_Animations
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawHeadHair")]
    public static class HarmonyPatch_HeadHair
    {
        public static void Prefix(ref Vector3 headOffset, ref float angle)
        {

        }

    }
}
