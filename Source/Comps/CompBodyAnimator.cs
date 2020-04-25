﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using rjw;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Rimworld_Animations {
    class CompBodyAnimator : ThingComp
    {
        public Pawn pawn => base.parent as Pawn;
        public PawnGraphicSet Graphics;
        
        public CompProperties_BodyAnimator Props => (CompProperties_BodyAnimator)(object)base.props;

        public bool isAnimating {
            get {
                return Animating;
            }
            set {
                Animating = value;

                if(value == true) {
                    xxx.DrawNude(pawn);
                } else {
                    pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                }

                PortraitsCache.SetDirty(pawn);
            }
        }
        private bool Animating;
        private bool mirror = false, quiver = false, shiver = false;
        private int actor;

        private int animTicks = 0, stageTicks = 0, clipTicks = 0;
        private int curStage = 0;
        private float clipPercent = 0;

        public Vector3 anchor, deltaPos, headBob;
        public float bodyAngle, headAngle;
        public Rot4 headFacing, bodyFacing;

        private AnimationDef anim;
        private AnimationStage stage => anim.animationStages[curStage];
        private PawnAnimationClip clip => (PawnAnimationClip)stage.animationClips[actor];

        public void setAnchor(IntVec3 pos)
        {
            anchor = pos.ToVector3Shifted();
        }
        public void setAnchor(Thing thing) {
            
            //center on bed
            if(thing is Building_Bed) {
                anchor = thing.Position.ToVector3();
                if (((Building_Bed)thing).SleepingSlotsCount == 2) {
                    if (thing.Rotation.AsInt == 0) {
                        anchor.x += 1;
                        anchor.z += 1;
                    }
                    else if (thing.Rotation.AsInt == 1) {
                        anchor.x += 1;
                    }
                    else if(thing.Rotation.AsInt == 3) {
                        anchor.z += 1;
                    }

                }
                else {
                    if(thing.Rotation.AsInt == 0) {
                        anchor.x += 0.5f;
                        anchor.z += 1f;
                    }
                    else if(thing.Rotation.AsInt == 1) {
                        anchor.x += 1f;
                        anchor.z += 0.5f;
                    }
                    else if(thing.Rotation.AsInt == 2) {
                        anchor.x += 0.5f;
                    } else {
                        anchor.z += 0.5f;
                    }
                }
            }
            else {
                anchor = thing.Position.ToVector3Shifted();
            }
        }
        public void StartAnimation(AnimationDef anim, int actor, bool mirror = false, bool shiver = false) {

            isAnimating = true;

            AlienRaceOffset offset = anim?.actors[actor]?.raceOffsets?.Find(x => x.defName == pawn.def.defName);

            if (offset != null) {
                anchor.x += mirror ? offset.x * -1f : offset.x;
                anchor.z += offset.z;
            }

            pawn.jobs.posture = PawnPosture.Standing;

            this.actor = actor;
            this.anim = anim;
            this.mirror = mirror;

            curStage = 0;
            animTicks = 0;
            stageTicks = 0;
            clipTicks = 0;

            quiver = false;
            this.shiver = shiver && AnimationSettings.rapeShiver;

            //tick once for initialization
            tickAnim();

        }
        public override void CompTick() {

            base.CompTick();

            if(isAnimating) {
                if (pawn?.jobs?.curDriver == null || (pawn?.jobs?.curDriver != null && !(pawn?.jobs?.curDriver is rjw.JobDriver_Sex))) {
                    isAnimating = false;
                }
                else {
                    tickAnim();
                }
            }
        }
        public void animatePawn(ref Vector3 rootLoc, ref float angle, ref Rot4 bodyFacing, ref Rot4 headFacing) {

            if(!isAnimating) {
                return;
            }
            rootLoc = anchor + deltaPos;
            angle = bodyAngle;
            bodyFacing = this.bodyFacing;
            headFacing = this.headFacing;

        }

        public void tickGraphics(PawnGraphicSet graphics) {
            this.Graphics = graphics;
        }

        public void tickAnim() {

            if (!isAnimating) return;

            animTicks++;
            if (animTicks < anim.animationTimeTicks) {
                tickStage();
            } else {

                isAnimating = false;
            }
        }

        public void tickStage()
        {
            stageTicks++;

            if(stageTicks >= stage.playTimeTicks) {

                curStage++;

                stageTicks = 0;
                clipTicks = 0;
                clipPercent = 0;
            }

            if(curStage >= anim.animationStages.Count && animTicks < anim.animationTimeTicks && pawn.jobs.curDriver is JobDriver_SexBaseInitiator) {
                pawn.jobs.curDriver.ReadyForNextToil();
            }

            tickClip();
            
        }

        public void tickClip() {

            clipTicks++;

            //play sound effect
            if(rjw.RJWSettings.sounds_enabled && clip.SoundEffects.ContainsKey(clipTicks) && AnimationSettings.soundOverride) {
                SoundDef.Named(clip.SoundEffects[clipTicks]).PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            }
            if(AnimationSettings.orgasmQuiver && clip.quiver.ContainsKey(clipTicks)) {
                quiver = clip.quiver[clipTicks];
            }
            
            //loop animation if possible
            if (clipPercent >= 1 && stage.isLooping) {
                clipTicks = 1;//warning: don't set to zero or else calculations go wrong
            }
            clipPercent = (float)clipTicks / (float)clip.duration;

            calculateDrawValues();
        }

        public void calculateDrawValues() {

            deltaPos = new Vector3(clip.BodyOffsetX.Evaluate(clipPercent) * (mirror ? -1 : 1), clip.layer.AltitudeFor(), clip.BodyOffsetZ.Evaluate(clipPercent));
            bodyAngle = (clip.BodyAngle.Evaluate(clipPercent) + (quiver || shiver ? ((Rand.Value * AnimationSettings.shiverIntensity) - (AnimationSettings.shiverIntensity / 2f)) : 0f)) * (mirror ? -1 : 1);

            //don't go past 360 or less than 0
            if (bodyAngle < 0) bodyAngle = 360 - ((-1f*bodyAngle) % 360);
            if (bodyAngle > 360) bodyAngle %= 360;

            headAngle = clip.HeadAngle.Evaluate(clipPercent) * (mirror ? -1 : 1);
            if (headAngle < 0) headAngle = 360 - ((-1f * headAngle) % 360);
            if (headAngle > 360) headAngle %= 360;


            bodyFacing = mirror ? new Rot4((int)clip.BodyFacing.Evaluate(clipPercent)).Opposite : new Rot4((int)clip.BodyFacing.Evaluate(clipPercent));

            bodyFacing = new Rot4((int)clip.BodyFacing.Evaluate(clipPercent));
            if(bodyFacing.IsHorizontal && mirror) {
                bodyFacing = bodyFacing.Opposite;
            }

            headFacing = new Rot4((int)clip.HeadFacing.Evaluate(clipPercent));
            if(headFacing.IsHorizontal && mirror) {
                headFacing = headFacing.Opposite;
            }
            headBob = new Vector3(0, 0, clip.HeadBob.Evaluate(clipPercent));
        }

        public Vector3 getPawnHeadPosition() {

            return anchor + deltaPos + Quaternion.AngleAxis(bodyAngle, Vector3.up) * (pawn.Drawer.renderer.BaseHeadOffsetAt(headFacing) + headBob);

        }

        public override void PostExposeData() {
            base.PostExposeData();
            
            Scribe_Defs.Look(ref anim, "anim");

            Scribe_Values.Look(ref animTicks, "animTicks", 1);
            Scribe_Values.Look(ref stageTicks, "stageTicks", 1);
            Scribe_Values.Look(ref clipTicks, "clipTicks", 1);
            Scribe_Values.Look(ref clipPercent, "clipPercent", 1);

            Scribe_Values.Look(ref mirror, "mirror");

            Scribe_Values.Look(ref curStage, "curStage", 0);
            Scribe_Values.Look(ref actor, "actor");

            Scribe_Values.Look(ref Animating, "Animating");
            Scribe_Values.Look(ref anchor, "anchor");
            Scribe_Values.Look(ref deltaPos, "deltaPos");
            Scribe_Values.Look(ref headBob, "headBob");
            Scribe_Values.Look(ref bodyAngle, "bodyAngle");
            Scribe_Values.Look(ref headAngle, "headAngle");

            Scribe_Values.Look(ref headFacing, "headFacing");
            Scribe_Values.Look(ref headFacing, "bodyFacing");

            Scribe_Values.Look(ref quiver, "orgasmQuiver");                             
        }

    }
}
