using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace Rimworld_Animations {
    class MainTabWindow_OffsetConfigure : MainTabWindow
    {

        public override Vector2 RequestedTabSize => new Vector2(505, 300);
        public override void DoWindowContents(Rect inRect) {

            Rect position = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);


            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(position);

            listingStandard.Label("Offset Controller");

            if (Find.Selector.SingleSelectedThing is Pawn) {

                Pawn curPawn = Find.Selector.SingleSelectedThing as Pawn;

                if (curPawn.TryGetComp<CompBodyAnimator>().isAnimating) {

                    Actor curActor = curPawn.TryGetComp<CompBodyAnimator>().CurrentAnimation.actors[curPawn.TryGetComp<CompBodyAnimator>().ActorIndex];

                    float offsetX = 0, offsetZ = 0;

                    if (curActor.offsetsByDefName.ContainsKey(curPawn.def.defName)) {
                        offsetX = curActor.offsetsByDefName[curPawn.def.defName].x;
                        offsetZ = curActor.offsetsByDefName[curPawn.def.defName].y;
                    } else {
                        curActor.offsetsByDefName.Add(curPawn.def.defName, new Vector2(0, 0));
                    }

                    listingStandard.GapLine();

                    listingStandard.Label("Offset for race " + curPawn.def.defName + " in actor position " + curPawn.TryGetComp<CompBodyAnimator>().ActorIndex);

                    if(curPawn.def.defName == "Human") {
                        listingStandard.Label("Warning--You generally don't want to change human offsets, only alien offsets");
                    }
                    

                    listingStandard.Label("X Offset: " + offsetX);
                    offsetX = listingStandard.Slider(offsetX, -10, 10);

                    listingStandard.Label("Z Offset: " + offsetZ);
                    offsetZ = listingStandard.Slider(offsetZ, -10, 10);

                    if (offsetX != curActor.offsetsByDefName[curPawn.def.defName].x || offsetZ != curActor.offsetsByDefName[curPawn.def.defName].y) {
                        curActor.offsetsByDefName[curPawn.def.defName] = new Vector2(offsetX, offsetZ);
                    }
                }


            }

            listingStandard.End();

            base.DoWindowContents(inRect);

        }
    }
}
