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

        public override Vector2 RequestedTabSize => new Vector2(505, 380);
        public override void DoWindowContents(Rect inRect) {

            Rect position = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);


            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(position);

            listingStandard.Label("Animation Manager");

            listingStandard.GapLine();


            if (Find.Selector.SingleSelectedThing is Pawn) {

                Pawn curPawn = Find.Selector.SingleSelectedThing as Pawn;

                if (curPawn.TryGetComp<CompBodyAnimator>().isAnimating) {

                    AnimationDef def = curPawn.TryGetComp<CompBodyAnimator>().CurrentAnimation;
                    int ActorIndex = curPawn.TryGetComp<CompBodyAnimator>().ActorIndex;
                    float offsetX = 0, offsetZ = 0, rotation = 0;

                    if (AnimationSettings.offsets.ContainsKey(def.defName + curPawn.def.defName + ActorIndex)) {
                        offsetX = AnimationSettings.offsets[def.defName + curPawn.def.defName + ActorIndex].x;
                        offsetZ = AnimationSettings.offsets[def.defName + curPawn.def.defName + ActorIndex].y;
                    } else {
                        AnimationSettings.offsets.Add(def.defName + curPawn.def.defName + ActorIndex, new Vector2(0, 0));
                    }

                    if (AnimationSettings.rotation.ContainsKey(def.defName + curPawn.def.defName + ActorIndex)) {
                        rotation = AnimationSettings.rotation[def.defName + curPawn.def.defName + ActorIndex];
                    }
                    else {
                        AnimationSettings.rotation.Add(def.defName + curPawn.def.defName + ActorIndex, 0);
                    }

                    listingStandard.Label("Name: " + curPawn.Name + " Race: " + curPawn.def.defName + " Actor Index: " + curPawn.TryGetComp<CompBodyAnimator>().ActorIndex + " Animation: " + def.label + (curPawn.TryGetComp<CompBodyAnimator>().Mirror ? " mirrored" : ""));

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

                    listingStandard.GapLine();

                    if(listingStandard.ButtonText("Shift Actors")) {
                        
                        if(AnimationSettings.debugMode) {
                            Log.Message("Shifting actors in animation...");
                        }

                        for(int i = 0; i < curPawn.TryGetComp<CompBodyAnimator>().actorsInCurrentAnimation.Count; i++) {
                            curPawn.TryGetComp<CompBodyAnimator>().actorsInCurrentAnimation[i].TryGetComp<CompBodyAnimator>()?.shiftActorPositionAndRestartAnimation();

                            //reset the clock time of every pawn in animation
                            if(curPawn.TryGetComp<CompBodyAnimator>().actorsInCurrentAnimation[i].jobs.curDriver is rjw.JobDriver_Sex) {
                                (curPawn.TryGetComp<CompBodyAnimator>().actorsInCurrentAnimation[i].jobs.curDriver as rjw.JobDriver_Sex).ticks_left = def.animationTimeTicks;
                                (curPawn.TryGetComp<CompBodyAnimator>().actorsInCurrentAnimation[i].jobs.curDriver as rjw.JobDriver_Sex).ticksLeftThisToil = def.animationTimeTicks;
                                (curPawn.TryGetComp<CompBodyAnimator>().actorsInCurrentAnimation[i].jobs.curDriver as rjw.JobDriver_Sex).duration = def.animationTimeTicks;
                                (curPawn.TryGetComp<CompBodyAnimator>().actorsInCurrentAnimation[i].jobs.curDriver as rjw.JobDriver_Sex).ticks_remaining = def.animationTimeTicks;
                            }

                        }

                    }

                    if (offsetX != AnimationSettings.offsets[def.defName + curPawn.def.defName + ActorIndex].x || offsetZ != AnimationSettings.offsets[def.defName + curPawn.def.defName + ActorIndex].y) {
                        AnimationSettings.offsets[def.defName + curPawn.def.defName + ActorIndex] = new Vector2(offsetX, offsetZ);

                    }

                    if(rotation != AnimationSettings.rotation[def.defName + curPawn.def.defName + ActorIndex]) {
                        AnimationSettings.rotation[def.defName + curPawn.def.defName + ActorIndex] = rotation;
                    }

                }

            }
            else {
                listingStandard.Label("Select a pawn currently in an animation to change their offsets");
            }

            listingStandard.End();

            base.DoWindowContents(inRect);

        }

        public override void PreOpen() {
            base.PreOpen();
            if(AnimationSettings.offsets == null) {
                if (AnimationSettings.debugMode)
                    Log.Message("New offsets");
                AnimationSettings.offsets = new Dictionary<string, Vector2>();
            }

            if(AnimationSettings.rotation == null) {
                if (AnimationSettings.debugMode)
                    Log.Message("New rotation");
                AnimationSettings.rotation = new Dictionary<string, float>();
            }
        }

        public override void PostClose() {
            base.PostClose();

            LoadedModManager.GetMod<RJW_Animations>().WriteSettings();
        }
    }
}
