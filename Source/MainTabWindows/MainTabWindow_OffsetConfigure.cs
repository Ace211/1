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

        public override Vector2 RequestedTabSize => new Vector2(505, 340);
        public override void DoWindowContents(Rect inRect) {

            Rect position = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);


            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(position);

            listingStandard.Label("Offset Manager");

            listingStandard.GapLine();


            if (Find.Selector.SingleSelectedThing is Pawn) {

                Pawn curPawn = Find.Selector.SingleSelectedThing as Pawn;

                if (curPawn.TryGetComp<CompBodyAnimator>().isAnimating) {

                    Actor curActor = curPawn.TryGetComp<CompBodyAnimator>().CurrentAnimation.actors[curPawn.TryGetComp<CompBodyAnimator>().ActorIndex];

                    float offsetX = 0, offsetZ = 0, rotation = 0;

                    if (curActor.offsetsByDefName.ContainsKey(curPawn.def.defName)) {
                        offsetX = curActor.offsetsByDefName[curPawn.def.defName].x;
                        offsetZ = curActor.offsetsByDefName[curPawn.def.defName].y;
                    } else {
                        curActor.offsetsByDefName.Add(curPawn.def.defName, new Vector2(0, 0));
                    }

                    if (curActor.rotationByDefName.ContainsKey(curPawn.def.defName)) {
                        rotation = curActor.rotationByDefName[curPawn.def.defName];
                    }
                    else {
                        curActor.rotationByDefName.Add(curPawn.def.defName, 180);
                    }


                    listingStandard.Label("Offset for race " + curPawn.def.defName + " in actor position " + curPawn.TryGetComp<CompBodyAnimator>().ActorIndex + (curPawn.TryGetComp<CompBodyAnimator>().Mirror ? " mirrored" : ""));

                    if(curPawn.def.defName == "Human") {
                        listingStandard.Label("Warning--You generally don't want to change human offsets, only alien offsets");
                    }
                    

                    listingStandard.Label("X Offset: " + offsetX);
                    offsetX = listingStandard.Slider(offsetX, -10, 10);

                    listingStandard.Label("Z Offset: " + offsetZ);
                    offsetZ = listingStandard.Slider(offsetZ, -10, 10);

                    listingStandard.Label("Rotation: " + rotation);
                    rotation = listingStandard.Slider(rotation, -180, 180);

                    if(listingStandard.ButtonText("Reset All")) {
                        offsetX = 0;
                        offsetZ = 0;
                        rotation = 0;
                    }

                    if (offsetX != curActor.offsetsByDefName[curPawn.def.defName].x || offsetZ != curActor.offsetsByDefName[curPawn.def.defName].y) {
                        curActor.offsetsByDefName[curPawn.def.defName] = new Vector2(offsetX, offsetZ);
                    }

                    if(rotation != curActor.rotationByDefName[curPawn.def.defName]) {
                        curActor.rotationByDefName[curPawn.def.defName] = rotation;
                    }

                }


            }
            else {
                listingStandard.Label("Select a pawn currently in an animation to change their offsets");
            }

            listingStandard.End();

            base.DoWindowContents(inRect);

        }
    }
}
