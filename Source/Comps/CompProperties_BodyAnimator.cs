 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Rimworld_Animations {
    class CompProperties_BodyAnimator : CompProperties
    {
        public CompProperties_BodyAnimator() {

            base.compClass = typeof(CompBodyAnimator);
        }
    }
}
